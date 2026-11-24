using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ClassLibrary;
using Npgsql;

namespace UnitTest
{
    [TestClass]
    public class DbServiceTests
    {
        public static class TestDbConfig
        {
            public const string TestConn = "Host=localhost;Port=5432;Database=university_test_db;Username=postgres";
        }
        private string conn;

        [TestInitialize]
        public async Task Setup()
        {
            conn = TestDbConfig.TestConn;
            await DbService.EnsureSchemaAsync(conn);
            await CleanDatabaseAsync();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await CleanDatabaseAsync();
        }

        private async Task CleanDatabaseAsync()
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(@"DELETE FROM grades;
                                                       DELETE FROM students;
                                                       DELETE FROM orders;
                                                       DELETE FROM programs;
                                                       DELETE FROM departments;
                                                       DELETE FROM faculties;
                                                       UPDATE sync_state SET last_sync = '2000-01-01';", c);
            await cmd.ExecuteNonQueryAsync();
        }

        // Тесты создания

        [TestMethod]
        public async Task AddFaculty_ShouldCreateFaculty()
        {
            await DbService.AddFacultyAsync(conn, "Тестовый факультет");
            var faculties = await DbService.LoadFacultiesAsync(conn);

            Assert.AreEqual(1, faculties.Count);
            Assert.AreEqual("Тестовый факультет", faculties[0].Name);
        }

        [TestMethod]
        public async Task AddDepartment_ShouldCreateDepartment()
        {
            await DbService.AddFacultyAsync(conn, "Факультет");
            var faculty = (await DbService.LoadFacultiesAsync(conn)).First();

            await DbService.AddDepartmentAsync(conn, faculty.Id, "Кафедра ПО");
            var departments = await DbService.LoadDepartmentsAsync(conn);

            Assert.AreEqual(1, departments.Count);
            Assert.AreEqual("Кафедра ПО", departments[0].Name);
        }

        [TestMethod]
        public async Task AddProgram_ShouldCreateProgram()
        {
            await DbService.AddFacultyAsync(conn, "Факультет");
            var faculty = (await DbService.LoadFacultiesAsync(conn)).First();
            await DbService.AddDepartmentAsync(conn, faculty.Id, "Кафедра");
            var dept = (await DbService.LoadDepartmentsAsync(conn)).First();

            await DbService.AddProgramAsync(conn, dept.Id, "09.03.04", "РИС");
            var programs = await DbService.LoadProgramsAsync(conn);

            Assert.AreEqual(1, programs.Count);
            Assert.AreEqual("09.03.04", programs[0].Code);
            Assert.AreEqual("РИС", programs[0].Profile);
        }

        [TestMethod]
        public async Task AddStudent_ShouldCreateStudent()
        {
            var programId = await CreateTestProgramAsync();

            await DbService.AddStudentAsync(conn, programId, "Иванов И.И.", "Группа-1", 2025);
            var students = await DbService.LoadStudentsAsync(conn);

            Assert.AreEqual(1, students.Count);
            Assert.AreEqual("Иванов И.И.", students[0].FullName);
            Assert.AreEqual("Группа-1", students[0].GroupNumber);
        }

        [TestMethod]
        public async Task AddGrade_ShouldCreateGrade()
        {
            var (studentId, _) = await CreateTestStudentWithGradeAsync();

            await DbService.AddGradeAsync(conn, studentId, "БД", 5, DateTime.Now, "Петрова", 3);
            var grades = await DbService.LoadGradesAsync(conn);

            Assert.AreEqual(2, grades.Count);
            Assert.AreEqual("БД", grades[0].Subject);
            Assert.AreEqual(5, grades[0].Grade);
        }

        [TestMethod]
        public async Task AddOrder_ShouldCreateOrder()
        {
            await DbService.AddOrderAsync(conn, "Тестовый приказ", DateTime.Now, "ALL", "active");
            var orders = await DbService.LoadOrdersAsync(conn);

            Assert.AreEqual(1, orders.Count);
            Assert.AreEqual("Тестовый приказ", orders[0].Text);
        }

        // Тесты мягкого удаления

        [TestMethod]
        public async Task SoftDeleteStudent_ShouldSetIsDeletedFlag()
        {
            var (studentId, _) = await CreateTestStudentWithGradeAsync();

            await DbService.SoftDeleteStudentAsync(conn, studentId);
            var visibleStudents = await DbService.LoadStudentsAsync(conn);
            Assert.AreEqual(0, visibleStudents.Count);
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT is_deleted FROM students WHERE id = @id", c);
            cmd.Parameters.AddWithValue("id", studentId);
            var result = (bool)await cmd.ExecuteScalarAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SoftDeleteGrade_ShouldSetIsDeletedFlag()
        {
            var (studentId, gradeId) = await CreateTestStudentWithGradeAsync();

            await DbService.SoftDeleteGradeAsync(conn, gradeId);

            var visibleGrades = await DbService.LoadGradesAsync(conn);
            Assert.AreEqual(0, visibleGrades.Count);
        }

        // Тесты Upsert с приоритетом

        [TestMethod]
        public async Task UpsertGrade_HigherPriority_ShouldOverride()
        {
            var (studentId, gradeId) = await CreateTestStudentWithGradeAsync(sourceLevel: 3);
            await DbService.UpsertGradeAsync(conn, gradeId, studentId, "БД", 4, DateTime.Now, "Сидоров", isDeleted: false, sourceLevel: 2);

            var grades = await DbService.LoadGradesAsync(conn);
            Assert.AreEqual(1, grades.Count);
            Assert.AreEqual(4, grades[0].Grade);
        }

        [TestMethod]
        public async Task UpsertGrade_LowerPriority_ShouldNotOverride()
        {
            var (studentId, gradeId) = await CreateTestStudentWithGradeAsync(sourceLevel: 2);
            await DbService.UpsertGradeAsync(conn, gradeId, studentId, "БД", 3, DateTime.Now, "Петров", isDeleted: false, sourceLevel: 3);

            var grades = await DbService.LoadGradesAsync(conn);
            Assert.AreEqual(1, grades.Count);
            Assert.AreEqual(5, grades[0].Grade);
        }

        // Тесты синхронизации

        [TestMethod]
        public async Task SyncDown_ShouldCopyStructure()
        {
            var parentConn = TestDbConfig.TestConn;
            var childConn = TestDbConfig.TestConn;

            await DbService.AddFacultyAsync(parentConn, "Факультет Тест");
            var faculty = (await DbService.LoadFacultiesAsync(parentConn)).First();
            await DbService.AddDepartmentAsync(parentConn, faculty.Id, "Кафедра Тест");

            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 2);
            await SyncService.SyncDownAsync(parentConn, childConn);

            var faculty_ = (await DbService.LoadFacultiesAsync(childConn)).First();
            Assert.IsTrue(faculty_.Name == "Факультет Тест");
        }

        [TestMethod]
        public async Task SyncUp_ShouldSendStudentsToParent()
        {
            var childConn = TestDbConfig.TestConn;
            var parentConn = TestDbConfig.TestConn;

            var programId = await CreateTestProgramAsync();
            await DbService.AddStudentAsync(childConn, programId, "Петров П.П.", "Г-2", 2024);

            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(childConn, parentConn);

            var student_ = (await DbService.LoadStudentsAsync(parentConn)).First();
            Assert.IsTrue(student_.FullName == "Петров П.П.");
        }

        [TestMethod]
        public async Task SyncUp_WithDeletedStudent_ShouldHardDeleteInParent()
        {
            var conn = TestDbConfig.TestConn;

            var programId = await CreateTestProgramAsync();
            await DbService.AddStudentAsync(conn, programId, "Сидоров С.С.", "Г-3", 2023);
            var student = (await DbService.LoadStudentsAsync(conn)).First();
            await DbService.SoftDeleteStudentAsync(conn, student.Id);
            await DbService.SetLastSyncAsync(conn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(conn, conn);
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students", c);
            var count = (long)await cmd.ExecuteScalarAsync();
            Assert.AreEqual(0, count);
        }

        // Тесты разграничения прав

        [TestMethod]
        public async Task LoadStudents_ShouldFilterDeleted()
        {
            var programId = await CreateTestProgramAsync();
            await DbService.AddStudentAsync(conn, programId, "Активный", "Г-1", 2025);
            await DbService.AddStudentAsync(conn, programId, "Удалённый", "Г-2", 2025);
            var deleted = (await DbService.LoadStudentsAsync(conn)).Last();
            await DbService.SoftDeleteStudentAsync(conn, deleted.Id);

            var visible = await DbService.LoadStudentsAsync(conn);
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("Активный", visible[0].FullName);
        }

        [TestMethod]
        public async Task LoadGradesForSync_ShouldIncludeDeleted()
        {
            var (studentId, gradeId) = await CreateTestStudentWithGradeAsync();
            await DbService.SoftDeleteGradeAsync(conn, gradeId);

            var grades = await DbService.LoadGradesForSyncAsync(conn, DateTime.MinValue);
            Assert.AreEqual(1, grades.Count);
            Assert.IsTrue(grades[0].IsDeleted);
        }

        // Вспомогательные методы

        private async Task<Guid> CreateTestProgramAsync()
        {
            await DbService.AddFacultyAsync(conn, "Факультет");
            var faculty = (await DbService.LoadFacultiesAsync(conn)).First();
            await DbService.AddDepartmentAsync(conn, faculty.Id, "Кафедра");
            var dept = (await DbService.LoadDepartmentsAsync(conn)).First();
            await DbService.AddProgramAsync(conn, dept.Id, "09.03.04", "РИС");
            return (await DbService.LoadProgramsAsync(conn)).First().Id;
        }

        private async Task<(Guid studentId, Guid gradeId)> CreateTestStudentWithGradeAsync(int sourceLevel = 3)
        {
            var programId = await CreateTestProgramAsync();
            await DbService.AddStudentAsync(conn, programId, "Иванов И.И.", "Г-1", 2025);
            var student = (await DbService.LoadStudentsAsync(conn)).First();

            await DbService.AddGradeAsync(conn, student.Id, "БД", 5, DateTime.Now, "Петров",  sourceLevel);
            var grade = (await DbService.LoadGradesAsync(conn)).First();

            return (student.Id, grade.Id);
        }
    }
}
