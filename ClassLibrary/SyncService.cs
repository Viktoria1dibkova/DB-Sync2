using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public static class SyncService
    {
        /// <summary>
        /// Синхронизация вниз
        /// </summary>
        /// <param name="parentConn"> строка соединения родителя</param>
        /// <param name="childConn">строка соединения ребенка</param>
        /// <returns></returns>
        public static async Task SyncDownAsync(string parentConn, string childConn)
        {
            try
            {
                var parentFaculties = await DbService.LoadAllFacultiesForSyncAsync(parentConn);
                var parentDepts = await DbService.LoadAllDepartmentsForSyncAsync(parentConn);
                var parentPrograms = await DbService.LoadAllProgramsForSyncAsync(parentConn);
                await DbService.DeleteOrphanedFacultiesAsync(childConn, parentFaculties.Select(f => f.Id).ToList());
                foreach (var faculty in parentFaculties) 
                {
                    var deptIds = parentDepts.Where(d => d.FacultyId == faculty.Id).Select(d => d.Id).ToList();
                    await DbService.DeleteOrphanedDepartmentsAsync(childConn, faculty.Id, deptIds);
                }
                foreach (var dept in parentDepts)
                {
                    var progIds = parentPrograms.Where(p => p.DepartmentId == dept.Id).Select(p => p.Id).ToList();
                    await DbService.DeleteOrphanedProgramsAsync(childConn, dept.Id, progIds);
                }
                foreach (var f in parentFaculties) await DbService.UpsertFacultyAsync(childConn, f.Id, f.Name);
                foreach (var d in parentDepts) await DbService.UpsertDepartmentAsync(childConn, d.Id, d.FacultyId, d.Name);
                foreach (var p in parentPrograms) await DbService.UpsertProgramAsync(childConn, p.Id, p.DepartmentId, p.Code, p.Profile);

                var lastSyncDown = await DbService.GetLastSyncAsync(childConn, syncId: 2);
                var students = await DbService.LoadStudentsForSyncAsync(parentConn, lastSyncDown);
                int okStudents = 0;

                foreach (var s in students)
                {
                    if (s.IsDeleted) await DbService.HardDeleteStudentAsync(childConn, s.Id);
                    else await DbService.UpsertStudentAsync( childConn, s.Id, s.ProgramId, s.FullName, s.GroupNumber, s.EnrollmentYear, isDeleted: false);
                    okStudents++;
                }

                var orders = await DbService.LoadOrdersForSyncAsync(parentConn);
                await DbService.DeleteOrphanedOrdersAsync(childConn, orders.Select(o => o.Id).ToList());

                foreach (var o in orders) await DbService.UpsertOrderAsync(childConn, o.Id, o.Text, o.Date, o.Scope, o.Status);
                await DbService.SetLastSyncAsync(childConn, DateTime.Now, syncId: 2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка SyncDownAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Синхронизация вверх
        /// </summary>
        /// <param name="childConn">соединение ребенка</param>
        /// <param name="parentConn">строка соединения родителя</param>
        /// <returns></returns>
        public static async Task SyncUpAsync(string childConn, string parentConn, string grandparentConn = null)
        {
            try
            {
                var lastSync = await DbService.GetLastSyncAsync(childConn, syncId: 1);
                var students = await DbService.LoadStudentsForSyncAsync(childConn, lastSync);
                int okStudents = 0;

                foreach (var s in students)
                {
                    if (s.IsDeleted)
                    {
                        await DbService.HardDeleteStudentAsync(parentConn, s.Id);
                        if (grandparentConn != null) await DbService.HardDeleteStudentAsync(grandparentConn, s.Id);
                    }
                    else
                    {
                        await DbService.UpsertStudentAsync(parentConn, s.Id, s.ProgramId, s.FullName, s.GroupNumber, s.EnrollmentYear, false);
                        if (grandparentConn != null)  await DbService.UpsertStudentAsync(grandparentConn, s.Id, s.ProgramId, s.FullName, s.GroupNumber, s.EnrollmentYear, false);
                           
                    }
                    okStudents++;
                }
                var grades = await DbService.LoadGradesForSyncAsync(childConn, lastSync);
                int okGrades = 0;

                foreach (var g in grades)
                {
                    if (g.IsDeleted)
                    {
                        await DbService.HardDeleteGradeAsync(parentConn, g.Id);
                        if (grandparentConn != null) await DbService.HardDeleteGradeAsync(grandparentConn, g.Id);
                    }
                    else
                    {
                        await DbService.UpsertGradeAsync(parentConn, g.Id, g.StudentId, g.Subject, g.Grade, g.Date, g.Teacher, false);
                        if (grandparentConn != null) await DbService.UpsertGradeAsync(grandparentConn, g.Id, g.StudentId, g.Subject, g.Grade, g.Date, g.Teacher, false);
                    }
                    okGrades++;
                }
                await DbService.SetLastSyncAsync(childConn, DateTime.Now, syncId: 1);
                await DbService.SetLastSyncAsync(parentConn, DateTime.Now, syncId: 1);
                if (grandparentConn != null) await DbService.SetLastSyncAsync(grandparentConn, DateTime.Now, syncId: 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка SyncUpAsync: {ex.Message}");
                throw;
            }
        }
    }
}