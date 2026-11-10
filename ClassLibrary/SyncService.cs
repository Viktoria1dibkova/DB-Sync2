using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    internal class SyncService
    {
        /// <summary>
        /// Синхронизации сверху вниз
        /// </summary>
        /// <param name="parentConn">строка соединения родителя(ректорат)</param>
        /// <param name="childConn">строка соединения ребенка (кафедра)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task SyncDownAsync(string parentConn, string childConn)
        {
            var facultyId = await DbService.FindFacultyByNameAsync(parentConn, DbConfig.MyFacultyName);
            if (facultyId == null) throw new Exception($"Факультет '{DbConfig.MyFacultyName}' не найден у родителя");
            var deptId = await DbService.FindDeptByNameAsync(parentConn, facultyId.Value, DbConfig.MyDeptName);
            if (deptId == null) throw new Exception($"Кафедра '{DbConfig.MyDeptName}' не найдена у родителя");

            var programs = await DbService.LoadProgramsForSyncAsync(parentConn, DbConfig.MyDeptName);
            var orders = await DbService.LoadOrdersForSyncAsync(parentConn);

            foreach (var p in programs) await DbService.UpsertProgramAsync(childConn, p.Id, deptId.Value, p.Code, p.Profile);

            foreach (var o in orders) await DbService.UpsertOrderAsync(childConn, o.Id, o.Text, o.Date, o.Scope, o.Status);
        }

        /// <summary>
        /// Синхронизация снизу вверх
        /// </summary>
        /// <param name="childConn">строка соединения ребенка</param>
        /// <param name="parentConn">строка соединения родителя</param>
        /// <returns></returns>
        public static async Task SyncUpAsync(string childConn, string parentConn)
        {
            var lastSync = await DbService.GetLastSyncAsync(childConn);
            var students = await DbService.LoadStudentsForSyncAsync(childConn, lastSync);
            var grades = await DbService.LoadGradesForSyncAsync(childConn, lastSync);

            foreach (var s in students) await DbService.UpsertStudentAsync(parentConn, s.Id, s.ProgramId, s.FullName, s.GroupNumber, s.EnrollmentYear);
            foreach (var g in grades) await DbService.UpsertGradeAsync(parentConn, g.Id,g.StudentId, g.Subject, g.Grade, g.Date, g.Teacher);
            
            await DbService.SetLastSyncAsync(childConn, DateTime.Now);
        }

        /// <summary>
        /// Полная синхронизаци
        /// </summary>
        /// <param name="parentConn">строка соединения родителя</param>
        /// <param name="childConn">строка соединения ребенка</param>
        /// <returns></returns>
        public static async Task SyncFullAsync(string parentConn, string childConn)
        {
            await SyncDownAsync(parentConn, childConn);
            await SyncUpAsync(childConn, parentConn);
        }
    }
}
