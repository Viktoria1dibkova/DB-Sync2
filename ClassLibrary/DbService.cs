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
        /// Синхронизация вниз (от родителя к ребенку)
        /// </summary>
        /// <param name="parentConn">строка соединения родителя</param>
        /// <param name="childConn">строка соединения ребёнка</param>
        /// <returns></returns>
        /// <exception cref="Exception">Ошибка синхронизации</exception>
        public static async Task SyncDownAsync(string parentConn, string childConn)
        {
            try
            {
                // Находим ID нашего Факультета
                Guid facultyId = Guid.Empty;
                using (var c = new NpgsqlConnection(parentConn))
                {
                    await c.OpenAsync();
                    using var cmd = new NpgsqlCommand(SqlQueries.FindFacultyByName, c);
                    cmd.Parameters.AddWithValue("name", DbConfig.MyFacultyName);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) facultyId = (Guid)result;
                }

                if (facultyId == Guid.Empty) throw new Exception($"Факультет '{DbConfig.MyFacultyName}' не найден!");

                // Находим ID нашей Кафедры
                Guid deptId = Guid.Empty;
                using (var c = new NpgsqlConnection(parentConn))
                {
                    await c.OpenAsync();
                    using var cmd = new NpgsqlCommand(SqlQueries.FindDeptByName, c);
                    cmd.Parameters.AddWithValue("fid", facultyId);
                    cmd.Parameters.AddWithValue("name", DbConfig.MyDeptName);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) deptId = (Guid)result;
                }

                if (deptId == Guid.Empty) throw new Exception($"Кафедра '{DbConfig.MyDeptName}' не найдена!");

                // Скачиваем направления
                var programs = new List<(Guid Id, string Code, string Profile)>();
                using (var c = new NpgsqlConnection(parentConn))
                {
                    await c.OpenAsync();
                    using var cmd = new NpgsqlCommand(SqlQueries.LoadMyPrograms, c);
                    cmd.Parameters.AddWithValue("deptName", DbConfig.MyDeptName);
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync()) programs.Add((r.GetGuid(0), r.GetString(2), r.GetString(3)));
                }

                // Скачиваем приказы
                var orders = new List<(Guid Id, string Text, DateTime Date, string Scope, string Status)>();
                using (var c = new NpgsqlConnection(parentConn))
                {
                    await c.OpenAsync();
                    using var cmd = new NpgsqlCommand(SqlQueries.LoadMyOrders, c);
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync()) orders.Add((r.GetGuid(0), r.GetString(1),r.GetDateTime(2),r.GetString(3), r.GetString(4)));
                }

                // Вставляем в базу ребенка
                using (var c = new NpgsqlConnection(childConn))
                {
                    await c.OpenAsync();

                    // Вставляем направления
                    foreach (var p in programs)
                    {
                        using var cmd = new NpgsqlCommand(SqlQueries.UpsertProgram, c);
                        cmd.Parameters.AddWithValue("id", p.Id);
                        cmd.Parameters.AddWithValue("did", deptId);
                        cmd.Parameters.AddWithValue("c", p.Code);
                        cmd.Parameters.AddWithValue("p", p.Profile);
                        cmd.Parameters.AddWithValue("u", DateTime.Now);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Вставляем приказы
                    foreach (var o in orders)
                    {
                        using var cmd = new NpgsqlCommand(SqlQueries.UpsertOrder, c);
                        cmd.Parameters.AddWithValue("id", o.Id);
                        cmd.Parameters.AddWithValue("t", o.Text);
                        cmd.Parameters.AddWithValue("d", o.Date);
                        cmd.Parameters.AddWithValue("sc", o.Scope);
                        cmd.Parameters.AddWithValue("st", o.Status);
                        cmd.Parameters.AddWithValue("u", DateTime.Now);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка синхронизации: " + ex.Message);
            }

        }
    }
}
