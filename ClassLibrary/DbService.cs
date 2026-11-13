
using Npgsql;
using NpgsqlTypes;
using static ClassLibrary.Models;

namespace ClassLibrary
{
    public static class DbService
    {
        /// <summary>
        /// Создание схемы
        /// </summary>
        /// <param name="conn">стркоа соединения</param>
        /// <returns></returns>
        public static async Task EnsureSchemaAsync(string conn)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Schema, c);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Загрузка факультетов для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<FacultyItem>> LoadFacultiesAsync(string conn)
        {
            var list = new List<FacultyItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Faculties, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new FacultyItem { Id = r.GetGuid(0), Name = r.GetString(1) });
            return list;
        }

        /// <summary>
        /// Загрузка кафедр для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<DeptItem>> LoadDepartmentsAsync(string conn)
        {
            var list = new List<DeptItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Departments, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new DeptItem { Id = r.GetGuid(0), Name = r.GetString(1), FacultyName = r.GetString(2) });
            return list;
        }

        /// <summary>
        /// Загрузка направлений для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<ProgramItem>> LoadProgramsAsync(string conn)
        {
            var list = new List<ProgramItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Programs, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new ProgramItem{Id = r.GetGuid(0), Code = r.GetString(1), Profile = r.GetString(2), DepartmentName = r.GetString(3)});
            return list;
        }

        /// <summary>
        /// Загрузка студентов для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<StudentItem>> LoadStudentsAsync(string conn)
        {
            var list = new List<StudentItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Students, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new StudentItem{Id = r.GetGuid(0),FullName = r.GetString(1),GroupNumber = r.GetString(2),EnrollmentYear = r.GetInt32(3),Profile = r.GetString(5)});
            return list;
        }

        /// <summary>
        /// Загрузка оценок для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<GradeItem>> LoadGradesAsync(string conn)
        {
            var list = new List<GradeItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Grades, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new GradeItem{Id = r.GetGuid(0), Date = r.GetDateTime(1), StudentName = r.GetString(2), Subject = r.GetString(3), Grade = r.GetInt32(4), Teacher = r.GetString(5)});
            return list;
        }

        /// <summary>
        /// Загрузка приказов для UI с учетом is_deleted
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<OrderItem>> LoadOrdersAsync(string conn)
        {
            var list = new List<OrderItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Orders, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new OrderItem{Id = r.GetGuid(0), Date = r.GetDateTime(1),Text = r.GetString(2), Scope = r.GetString(3), Status = r.GetString(4)});
            return list;
        }

        /// <summary>
        /// Добавление факультета
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="name">название факультеат</param>
        /// <returns></returns>
        public static async Task AddFacultyAsync(string conn, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertFaculty, c);
            cmd.Parameters.AddWithValue("n", name);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление кафедры
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="facultyId">ID факультета</param>
        /// <param name="name">название</param>
        /// <returns></returns>
        public static async Task AddDepartmentAsync(string conn, Guid facultyId, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertDepartment, c);
            cmd.Parameters.AddWithValue("fid", facultyId);
            cmd.Parameters.AddWithValue("n", name);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление направления
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="deptId">ID кафедры</param>
        /// <param name="code">код группы</param>
        /// <param name="profile">название направлени</param>
        /// <returns></returns>
        public static async Task AddProgramAsync(string conn, Guid deptId, string code, string profile)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertProgram, c);
            cmd.Parameters.AddWithValue("did", deptId);
            cmd.Parameters.AddWithValue("c", code);
            cmd.Parameters.AddWithValue("p", profile);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление студента 
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="programId">ID направления</param>
        /// <param name="fullName">ФИО студента</param>
        /// <param name="groupNumber">номер группы</param>
        /// <param name="enrollmentYear">гож поступления</param>
        /// <returns></returns>
        public static async Task AddStudentAsync(string conn, Guid programId, string fullName, string groupNumber, int enrollmentYear)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertStudent, c);
            cmd.Parameters.AddWithValue("pid", programId);
            cmd.Parameters.AddWithValue("fn", fullName);
            cmd.Parameters.AddWithValue("gn", groupNumber);
            cmd.Parameters.AddWithValue("ey", enrollmentYear);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление оценки
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="studentId">ID студента</param>
        /// <param name="subject">предмет</param>
        /// <param name="grade">оценка</param>
        /// <param name="date">дата</param>
        /// <param name="teacher">преподаватель</param>
        /// <returns></returns>
        public static async Task AddGradeAsync(string conn, Guid studentId, string subject, int grade, DateTime date, string teacher)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertGrade, c);
            cmd.Parameters.AddWithValue("sid", studentId);
            cmd.Parameters.AddWithValue("s", subject);
            cmd.Parameters.AddWithValue("g", grade);
            var paramD = cmd.Parameters.AddWithValue("d", date);
            paramD.NpgsqlDbType = NpgsqlDbType.Date;
            cmd.Parameters.AddWithValue("t", teacher);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление приказа
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="text">текст приказа</param>
        /// <param name="date">дата</param>
        /// <param name="scope">адрессат</param>
        /// <param name="status">статус</param>
        /// <returns></returns>
        public static async Task AddOrderAsync(string conn, string text, DateTime date, string scope, string status)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertOrder, c);
            cmd.Parameters.AddWithValue("t", text);
            var paramD = cmd.Parameters.AddWithValue("d", date);
            paramD.NpgsqlDbType = NpgsqlDbType.Date;
            cmd.Parameters.AddWithValue("sc", scope);
            cmd.Parameters.AddWithValue("st", status);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление факультеат
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID факультета</param>
        /// <returns></returns>
        public static async Task DeleteFacultyAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteFaculty, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление кафедры
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID кафедры</param>
        /// <returns></returns>
        public static async Task DeleteDepartmentAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteDepartment, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление направления
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID направления</param>
        /// <returns></returns>
        public static async Task DeleteProgramAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteProgram, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление приказа
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID приказа</param>
        /// <returns></returns>
        public static async Task DeleteOrderAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteOrder, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Мягкое удаление студента (ставит флаг is_deleted = true)
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID студента</param>
        /// <returns></returns>
        public static async Task SoftDeleteStudentAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.SoftDeleteStudent, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Мягкое удаление оценки
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID оценки</param>
        /// <returns></returns>
        public static async Task SoftDeleteGradeAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.SoftDeleteGrade, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Физическое удаление студента
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID студента</param>
        /// <returns></returns>
        public static async Task HardDeleteStudentAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.HardDeleteStudent, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Физическое удаление оценки
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID оценки</param>
        /// <returns></returns>
        public static async Task HardDeleteGradeAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.HardDeleteGrade, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Загрузка для строгой структуры вниз фвкультетов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<FacultyItem>> LoadAllFacultiesForSyncAsync(string conn)
        {
            var list = new List<FacultyItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyFaculties, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new FacultyItem { Id = r.GetGuid(0), Name = r.GetString(1) });
            return list;
        }

        /// <summary>
        /// Загрузка для строгой синхронизации вниз кафедр
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<DeptItem>> LoadAllDepartmentsForSyncAsync(string conn)
        {
            var list = new List<DeptItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyDept, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new DeptItem { Id = r.GetGuid(0), FacultyId = r.GetGuid(1), Name = r.GetString(2) });
            return list;
        }

        /// <summary>
        /// Загрузка для строгой синхронизации вниз направлений
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static async Task<List<ProgramItem>> LoadAllProgramsForSyncAsync(string conn)
        {
            var list = new List<ProgramItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyPrograms, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new ProgramItem { Id = r.GetGuid(0), DepartmentId = r.GetGuid(1), Code = r.GetString(2), Profile = r.GetString(3) });
            return list;
        }

        /// <summary>
        /// Загрузка для строгой синхронизации вниз приказов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<OrderItem>> LoadOrdersForSyncAsync(string conn)
        {
            var list = new List<OrderItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyOrders, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new OrderItem{Id = r.GetGuid(0), Text = r.GetString(1), Date = r.GetDateTime(2), Scope = r.GetString(3), Status = r.GetString(4)});
            return list;
        }

        /// <summary>
        /// Загрузка для синхронизации вверх студентов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="since">время последней синхронизации</param>
        /// <returns></returns>
        public static async Task<List<StudentItem>> LoadStudentsForSyncAsync(string conn, DateTime since)
        {
            var list = new List<StudentItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadStudentsForSync, c);
            cmd.Parameters.AddWithValue("since", since);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new StudentItem{Id = r.GetGuid(0), ProgramId = r.GetGuid(1), FullName = r.GetString(2), GroupNumber = r.GetString(3), EnrollmentYear = r.GetInt32(4), IsDeleted = r.GetBoolean(5)});
            return list;
        }

        /// <summary>
        /// Загрузка для синхронизации вверх студентов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="since">время последней синхронизации</param>
        /// <returns></returns>
        public static async Task<List<GradeItem>> LoadGradesForSyncAsync(string conn, DateTime since)
        {
            var list = new List<GradeItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadGradesForSync, c);
            cmd.Parameters.AddWithValue("since", since);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new GradeItem{Id = r.GetGuid(0), StudentId = r.GetGuid(1), Subject = r.GetString(2), Grade = r.GetInt32(3), Date = r.GetDateTime(4), Teacher = r.GetString(5), IsDeleted = r.GetBoolean(6)});
            return list;
        }

        /// <summary>
        /// Обновление факультетов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID факультета</param>
        /// <param name="name">название факультета</param>
        /// <returns></returns>
        public static async Task UpsertFacultyAsync(string conn, Guid id, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertFaculty, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("n", name);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление кафедры
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID кафедры</param>
        /// <param name="facultyId">ID факультета</param>
        /// <param name="name">название кафедры</param>
        /// <returns></returns>
        public static async Task UpsertDepartmentAsync(string conn, Guid id, Guid facultyId, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertDepartment, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("fid", facultyId);
            cmd.Parameters.AddWithValue("n", name);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }
        /// <summary>
        /// Обновление направления
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID направления</param>
        /// <param name="deptId">ID кафедры</param>
        /// <param name="code">код направления</param>
        /// <param name="profile">название направления</param>
        /// <returns></returns>
        public static async Task UpsertProgramAsync(string conn, Guid id, Guid deptId, string code, string profile)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertProgram, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("did", deptId);
            cmd.Parameters.AddWithValue("c", code);
            cmd.Parameters.AddWithValue("p", profile);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление приказа
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID приказа</param>
        /// <param name="text">текст приказа</param>
        /// <param name="date">дата</param>
        /// <param name="scope">адрессат</param>
        /// <param name="status">статус выполнения</param>
        /// <returns></returns>
        public static async Task UpsertOrderAsync(string conn, Guid id, string text, DateTime date, string scope, string status)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertOrder, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("t", text);
            var paramD = cmd.Parameters.AddWithValue("d", date);
            paramD.NpgsqlDbType = NpgsqlDbType.Date;
            cmd.Parameters.AddWithValue("sc", scope);
            cmd.Parameters.AddWithValue("st", status);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление студентов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID студента</param>
        /// <param name="programId">ID направления</param>
        /// <param name="fullName">ФИО</param>
        /// <param name="groupNumber">номер группы</param>
        /// <param name="enrollmentYear">год поступления</param>
        /// <param name="isDeleted">флаг удаления</param>
        /// <returns></returns>
        public static async Task UpsertStudentAsync(string conn, Guid id, Guid programId, string fullName, string groupNumber, int enrollmentYear, bool isDeleted = false)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertStudent, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("pid", programId);
            cmd.Parameters.AddWithValue("fn", fullName);
            cmd.Parameters.AddWithValue("gn", groupNumber);
            cmd.Parameters.AddWithValue("ey", enrollmentYear);
            cmd.Parameters.AddWithValue("del", isDeleted);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление оценки
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID оценки</param>
        /// <param name="studentId">ID студента</param>
        /// <param name="subject">предмет</param>
        /// <param name="grade">оценка</param>
        /// <param name="date">дата</param>
        /// <param name="teacher">преподаватель</param>
        /// <param name="isDeleted">флаг удаления</param>
        /// <returns></returns>
        public static async Task UpsertGradeAsync(string conn, Guid id, Guid studentId, string subject, int grade, DateTime date, string teacher, bool isDeleted = false)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertGrade, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("sid", studentId);
            cmd.Parameters.AddWithValue("s", subject);
            cmd.Parameters.AddWithValue("g", grade);
            var paramD = cmd.Parameters.AddWithValue("d", date);
            paramD.NpgsqlDbType = NpgsqlDbType.Date;
            cmd.Parameters.AddWithValue("t", teacher);
            cmd.Parameters.AddWithValue("del", isDeleted);
            var paramU = cmd.Parameters.AddWithValue("u", DateTime.Now);
            paramU.NpgsqlDbType = NpgsqlDbType.Timestamp;
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление осиротевших записей факультета
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="validIds">ID существующих записей</param>
        /// <returns></returns>
        public static async Task DeleteOrphanedFacultiesAsync(string conn, List<Guid> validIds)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = c;
            if (validIds.Count == 0)
                cmd.CommandText = SqlQueries.DeleteAllFaculties;
            else
            {
                cmd.CommandText = SqlQueries.DeleteOrphanedFaculties;
                var p = cmd.Parameters.AddWithValue("validIds", validIds.ToArray());
                p.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            }
            int n = await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление осиротевших записей кафедры
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="facultyId">ID факультета</param>
        /// <param name="validIds">ID существующих записей</param>
        /// <returns></returns>
        public static async Task DeleteOrphanedDepartmentsAsync(string conn, Guid facultyId, List<Guid> validIds)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = c;
            if (validIds.Count == 0)
            {
                cmd.CommandText = SqlQueries.DeleteAllDepartmentsByFaculty;
                cmd.Parameters.AddWithValue("fid", facultyId);
            }
            else
            {
                cmd.CommandText = SqlQueries.DeleteOrphanedDepartments;
                cmd.Parameters.AddWithValue("fid", facultyId);
                var p = cmd.Parameters.AddWithValue("validIds", validIds.ToArray());
                p.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            }
            int n = await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление осиротевших записей направлений
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="deptId">ID кафедры</param>
        /// <param name="validIds">ID существующих записей</param>
        /// <returns></returns>
        public static async Task DeleteOrphanedProgramsAsync(string conn, Guid deptId, List<Guid> validIds)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = c;
            if (validIds.Count == 0)
            {
                cmd.CommandText = SqlQueries.DeleteAllProgramsByDept;
                cmd.Parameters.AddWithValue("did", deptId);
            }
            else
            {
                cmd.CommandText = SqlQueries.DeleteOrphanedPrograms;
                cmd.Parameters.AddWithValue("did", deptId);
                var p = cmd.Parameters.AddWithValue("validIds", validIds.ToArray());
                p.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            }
            int n = await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление осиротевших записей приказов
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="validIds">ID существующих записей</param>
        /// <returns></returns>
        public static async Task DeleteOrphanedOrdersAsync(string conn, List<Guid> validIds)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = c;
            if (validIds.Count == 0)
                cmd.CommandText = SqlQueries.DeleteAllLocalOrders;
            else
            {
                cmd.CommandText = SqlQueries.DeleteOrphanedOrders;
                var p = cmd.Parameters.AddWithValue("validIds", validIds.ToArray());
                p.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            }
            int n = await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Получение времени последней синхронизации
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="syncId">ID синхронизации (вверх/вниз)</param>
        /// <returns></returns>
        public static async Task<DateTime> GetLastSyncAsync(string conn, int syncId = 1)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.GetSync, c);
            cmd.Parameters.AddWithValue("id", syncId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? (DateTime)result : DateTime.MinValue;
        }

        /// <summary>
        /// Установление нового времени синхронизации
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="ts">время синхронизации</param>
        /// <param name="syncId">ID синхронизации (вверх/вниз)</param>
        /// <returns></returns>
        public static async Task SetLastSyncAsync(string conn, DateTime ts, int syncId = 1)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.SetSync, c);
            var p = cmd.Parameters.AddWithValue("ts", ts);
            p.NpgsqlDbType = NpgsqlDbType.Timestamp;
            cmd.Parameters.AddWithValue("id", syncId);
            await cmd.ExecuteNonQueryAsync();
        }


    }
}