using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using static UnitTest.DbServiceTests;

namespace UnitTest
{
    [TestClass]
    public class SyncServiceTests
    {
        private string parentConn;
        private string childConn;

        [TestInitialize]
        public async Task Setup()
        {
            parentConn = TestDbConfig.TestConn;
            childConn = TestDbConfig.TestConn;

            await DbService.EnsureSchemaAsync(parentConn);
            await DbService.EnsureSchemaAsync(childConn);

            await CleanDatabaseAsync(parentConn);
            await CleanDatabaseAsync(childConn);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await CleanDatabaseAsync(parentConn);
            await CleanDatabaseAsync(childConn);
        }

        private async Task CleanDatabaseAsync(string conn)
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

        [TestMethod]
        public async Task SyncDown_ShouldPropagateFacultyToChild()
        {
            await DbService.AddFacultyAsync(parentConn, "Факультет ИТ");
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 2);
            await SyncService.SyncDownAsync(parentConn, childConn);

            var childFaculties = await DbService.LoadFacultiesAsync(childConn);
            Assert.AreEqual(1, childFaculties.Count);
            Assert.AreEqual("Факультет ИТ", childFaculties[0].Name);
        }

        [TestMethod]
        public async Task SyncUp_ShouldSendGradesToParent()
        {
            var programId = await CreateTestProgramAsync(childConn);
            await DbService.AddStudentAsync(childConn, programId, "Иванов", "Г-1", 2025);
            var student = (await DbService.LoadStudentsAsync(childConn)).First();
            await DbService.AddGradeAsync(childConn, student.Id, "БД", 5, DateTime.Now, "Петров", 3);
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(childConn, parentConn);

            var parentGrades = await DbService.LoadGradesAsync(parentConn);
            Assert.AreEqual(1, parentGrades.Count);
            Assert.AreEqual("БД", parentGrades[0].Subject);
            Assert.AreEqual(5, parentGrades[0].Grade);
        }

        [TestMethod]
        public async Task SyncUp_CascadeDelete_ShouldRemoveFromGrandparent()
        {
            var programId = await CreateTestProgramAsync(childConn);
            await DbService.AddStudentAsync(childConn, programId, "Иванов", "Г-1", 2025);
            var student = (await DbService.LoadStudentsAsync(childConn)).First();
            await DbService.AddGradeAsync(childConn, student.Id, "БД", 5, DateTime.Now, "Петров", 3);
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(childConn, parentConn);
            var grade = (await DbService.LoadGradesAsync(childConn)).First();
            await DbService.SoftDeleteGradeAsync(childConn, grade.Id);
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(childConn, parentConn, grandparentConn: parentConn);

            var parentGrades = await DbService.LoadGradesAsync(parentConn);
            Assert.AreEqual(0, parentGrades.Count);
        }

        [TestMethod]
        public async Task SyncDown_StudentsShouldPropagateFromFacultyToDept()
        {
            var programId = await CreateTestProgramAsync(parentConn);
            await DbService.AddStudentAsync(parentConn, programId, "Петров", "Г-2", 2024);
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 2);
            await SyncService.SyncDownAsync(parentConn, childConn);

            var childStudents = await DbService.LoadStudentsAsync(childConn);
            Assert.AreEqual(1, childStudents.Count);
            Assert.AreEqual("Петров", childStudents[0].FullName);
        }

        [TestMethod]
        public async Task SyncState_ShouldUpdateAfterSync()
        {
            var beforeSync = DateTime.Now.AddMinutes(-1);
            await DbService.SetLastSyncAsync(childConn, DateTime.MinValue, syncId: 1);
            await SyncService.SyncUpAsync(childConn, parentConn);

            var lastSync = await DbService.GetLastSyncAsync(childConn, syncId: 1);
            Assert.IsTrue(lastSync > beforeSync);
        }

        // Вспомогательные методы

        private async Task<Guid> CreateTestProgramAsync(string conn)
        {
            await DbService.AddFacultyAsync(conn, "Факультет");
            var faculty = (await DbService.LoadFacultiesAsync(conn)).First();
            await DbService.AddDepartmentAsync(conn, faculty.Id, "Кафедра");
            var dept = (await DbService.LoadDepartmentsAsync(conn)).First();
            await DbService.AddProgramAsync(conn, dept.Id, "09.03.04", "РИС");
            return (await DbService.LoadProgramsAsync(conn)).First().Id;
        }
    }
}
