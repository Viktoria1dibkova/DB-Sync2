using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace ClassLibrary
{
    internal class DbService
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task EnsureSchemaAsync(string conn)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Schema, c);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Нахождение ID факультета по названию
        /// </summary>
        /// <param name="conn">стркоа соединения</param>
        /// <param name="name">название</param>
        /// <returns></returns>
        public static async Task<Guid?> FindFacultyByNameAsync(string conn, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.FindFacultyByName, c);
            cmd.Parameters.AddWithValue("name", name);
            var result = await cmd.ExecuteScalarAsync();
            return result == null ? null : (Guid?)result;
        }

        /// <summary>
        /// Нахождение ID кафедры по ID факультета и названию кафедры
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="facultyId">ID факультета</param>
        /// <param name="name">название кафедры</param>
        /// <returns></returns>
        public static async Task<Guid?> FindDeptByNameAsync(string conn, Guid facultyId, string name)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.FindDeptByName, c);
            cmd.Parameters.AddWithValue("fid", facultyId);
            cmd.Parameters.AddWithValue("name", name);
            var result = await cmd.ExecuteScalarAsync();
            return result == null ? null : (Guid?)result;
        }

        /// <summary>
        /// Чтение факультетов
        /// </summary>
        /// <param name="conn">строка соединеия</param>
        /// <returns></returns>
        public static async Task<List<Models.FacultyItem>> LoadFacultiesAsync(string conn)
        {
            var list = new List<Models.FacultyItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Faculties, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.FacultyItem { Id = r.GetGuid(0), Name = r.GetString(1) });
            return list;
        }

        /// <summary>
        /// Чтение кафедр
        /// </summary>
        /// <param name="conn">строка соединеия</param>
        /// <returns></returns>
        public static async Task<List<Models.DeptItem>> LoadDepartmentsAsync(string conn)
        {
            var list = new List<Models.DeptItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Departments, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.DeptItem { Id = r.GetGuid(0), Name = r.GetString(1), FacultyName = r.GetString(2) });
            return list;
        }

        /// <summary>
        /// Чтение направлений
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<Models.ProgramItem>> LoadProgramsAsync(string conn)
        {
            var list = new List<Models.ProgramItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Programs, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.ProgramItem { Id = r.GetGuid(0), Code = r.GetString(1), Profile = r.GetString(2), DepartmentName = r.GetString(3) });
            return list;
        }

        /// <summary>
        /// Чтение студентов
        /// </summary>
        /// <param name="conn">строка соединеия</param>
        /// <returns></returns>
        public static async Task<List<Models.StudentItem>> LoadStudentsAsync(string conn)
        {
            var list = new List<Models.StudentItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Students, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.StudentItem{Id = r.GetGuid(0),FullName = r.GetString(1),GroupNumber = r.GetString(2),EnrollmentYear = r.GetInt32(3),Profile = r.GetString(5)});
            return list;
        }

        /// <summary>
        /// Чтение оценок
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<List<Models.GradeItem>> LoadGradesAsync(string conn)
        {
            var list = new List<Models.GradeItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Grades, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.GradeItem {Id = r.GetGuid(0), Date = r.GetDateTime(1),StudentName = r.GetString(2),Subject = r.GetString(3),Grade = r.GetInt32(4), Teacher = r.GetString(5)});
            return list;
        }

        /// <summary>
        /// Чтение приказов
        /// </summary>
        /// <param name="conn">строка соединеия</param>
        /// <returns></returns>
        public static async Task<List<Models.OrderItem>> LoadOrdersAsync(string conn)
        {
            var list = new List<Models.OrderItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.Orders, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())list.Add(new Models.OrderItem {Id = r.GetGuid(0),Date = r.GetDateTime(1),Text = r.GetString(2),Scope = r.GetString(3),Status = r.GetString(4)});
            return list;
        }

        /// <summary>
        /// Чтение для синхронизации (читаем направления для конкретной кафедры)
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="deptName">название кафедры</param>
        /// <returns></returns>
        public static async Task<List<Models.ProgramItem>> LoadProgramsForSyncAsync(string conn, string deptName)
        {
            var list = new List<Models.ProgramItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyPrograms, c);
            cmd.Parameters.AddWithValue("deptName", deptName);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.ProgramItem { Id = r.GetGuid(0), Code = r.GetString(2), Profile = r.GetString(3) });
            return list;
        }

        /// <summary>
        /// Чтение для синхронизации (читаем приказы для конкретной кафедры)
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static async Task<List<Models.OrderItem>> LoadOrdersForSyncAsync(string conn)
        {
            var list = new List<Models.OrderItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadMyOrders, c);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.OrderItem {Id = r.GetGuid(0), Text = r.GetString(1), Date = r.GetDateTime(2), Scope = r.GetString(3), Status = r.GetString(4)});
            return list;
        }

        /// <summary>
        /// Чтение для синхронизации (читаем студентов со всех направлений)
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="since">последнее время синхронизации</param>
        /// <returns></returns>
        public static async Task<List<Models.StudentItem>> LoadStudentsForSyncAsync(string conn, DateTime since)
        {
            var list = new List<Models.StudentItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadStudentsSince, c);
            cmd.Parameters.AddWithValue("since", since);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.StudentItem{Id = r.GetGuid(0), ProgramId = r.GetGuid(1), FullName = r.GetString(2), GroupNumber = r.GetString(3), EnrollmentYear = r.GetInt32(4)});
            return list;
        }

        /// <summary>
        /// Чтение для синхронизации (читаем все оценки всех студентов)
        /// </summary>
        /// <param name="conn">строка соединеиня</param>
        /// <param name="since">последнее время синхронизации</param>
        /// <returns></returns>
        public static async Task<List<Models.GradeItem>> LoadGradesForSyncAsync(string conn, DateTime since)
        {
            var list = new List<Models.GradeItem>();
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.LoadGradesSince, c);
            cmd.Parameters.AddWithValue("since", since);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(new Models.GradeItem{Id = r.GetGuid(0), StudentId = r.GetGuid(1), Subject = r.GetString(2), Grade = r.GetInt32(3), Date = r.GetDateTime(4), Teacher = r.GetString(5)});
            return list;
        }

        /// <summary>
        /// Добавление факультета
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="name">название факультета</param>
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
        /// <param name="name">название кафедры</param>
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
        /// <param name="profile">направление</param>
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
        /// <param name="fullName">ФИО</param>
        /// <param name="groupNumber">код группы</param>
        /// <param name="enrollmentYear">год поступления</param>
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
            cmd.Parameters.AddWithValue("d", date);
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
        /// <param name="status">статус (активен/архив)</param>
        /// <returns></returns>
        public static async Task AddOrderAsync(string conn, string text, DateTime date, string scope, string status)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.InsertOrder, c);
            cmd.Parameters.AddWithValue("t", text);
            cmd.Parameters.AddWithValue("d", date);
            cmd.Parameters.AddWithValue("sc", scope);
            cmd.Parameters.AddWithValue("st", status);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление факультета
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
        /// Удаление напрпавления
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
        /// Удаление студента
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID студента</param>
        /// <returns></returns>
        public static async Task DeleteStudentAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteStudent, c);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Удаление оценки
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID оценки</param>
        /// <returns></returns>
        public static async Task DeleteGradeAsync(string conn, Guid id)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.DeleteGrade, c);
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
        /// Обновление направления
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID направления</param>
        /// <param name="deptId">ID кафедры</param>
        /// <param name="code">код группы</param>
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
            cmd.Parameters.AddWithValue("u", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление приказа
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID приказа</param>
        /// <param name="text">текст приказа</param>
        /// <param name="date">дата приказа</param>
        /// <param name="scope">адрессат</param>
        /// <param name="status">стутус выполнения</param>
        /// <returns></returns>
        public static async Task UpsertOrderAsync(string conn, Guid id, string text, DateTime date, string scope, string status)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertOrder, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("t", text);
            cmd.Parameters.AddWithValue("d", date);
            cmd.Parameters.AddWithValue("sc", scope);
            cmd.Parameters.AddWithValue("st", status);
            cmd.Parameters.AddWithValue("u", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление студента
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID студента</param>
        /// <param name="programId">ID направления</param>
        /// <param name="fullName">ФИО</param>
        /// <param name="groupNumber">код группы</param>
        /// <param name="enrollmentYear">год поступления</param>
        /// <returns></returns>
        public static async Task UpsertStudentAsync(string conn, Guid id, Guid programId, string fullName, string groupNumber, int enrollmentYear)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertStudent, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("pid", programId);
            cmd.Parameters.AddWithValue("fn", fullName);
            cmd.Parameters.AddWithValue("gn", groupNumber);
            cmd.Parameters.AddWithValue("ey", enrollmentYear);
            cmd.Parameters.AddWithValue("u", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Обновление оценок
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="id">ID оценки</param>
        /// <param name="studentId">ID студента</param>
        /// <param name="subject">предмет</param>
        /// <param name="grade">оценка</param>
        /// <param name="date">дата</param>
        /// <param name="teacher">преподаватель</param>
        /// <returns></returns>
        public static async Task UpsertGradeAsync(string conn, Guid id, Guid studentId, string subject, int grade, DateTime date, string teacher)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.UpsertGrade, c);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("sid", studentId);
            cmd.Parameters.AddWithValue("s", subject);
            cmd.Parameters.AddWithValue("g", grade);
            cmd.Parameters.AddWithValue("d", date);
            cmd.Parameters.AddWithValue("t", teacher);
            cmd.Parameters.AddWithValue("u", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Получение времени последней синхронизации
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <returns></returns>
        public static async Task<DateTime> GetLastSyncAsync(string conn)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.GetSync, c);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (DateTime)result : DateTime.MinValue;
        }

        /// <summary>
        /// Обновление времени последней синхронизации
        /// </summary>
        /// <param name="conn">строка соединения</param>
        /// <param name="timestamp">время</param>
        /// <returns></returns>
        public static async Task SetLastSyncAsync(string conn, DateTime timestamp)
        {
            using var c = new NpgsqlConnection(conn);
            await c.OpenAsync();
            using var cmd = new NpgsqlCommand(SqlQueries.SetSync, c);
            cmd.Parameters.AddWithValue("ts", timestamp);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
