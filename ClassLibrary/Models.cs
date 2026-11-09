using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    internal class Models
    {
        /// <summary>
        /// Структура записи факультета
        /// </summary>
        public class FacultyItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Структура записи кафедры
        /// </summary>
        public class DeptItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string FacultyName { get; set; }
        }

        /// <summary>
        /// Структура записи направления
        /// </summary>
        public class ProgramItem
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
            public string Profile { get; set; }
            public string DepartmentName { get; set; }
        }

        /// <summary>
        /// Структура записи студента
        /// </summary>
        public class StudentItem
        {
            public Guid Id { get; set; }
            public Guid ProgramId { get; set; }
            public string FullName { get; set; }
            public string GroupNumber { get; set; }
            public int EnrollmentYear { get; set; }
            public string Profile { get; set; }
            public string DisplayName => $"{FullName} | {GroupNumber} | {EnrollmentYear}";
        }

        /// <summary>
        /// Структура записи оценок
        /// </summary>
        public class GradeItem
        {
            public Guid Id { get; set; }
            public Guid StudentId { get; set; }
            public DateTime Date { get; set; }
            public string StudentName { get; set; }
            public string Subject { get; set; }
            public int Grade { get; set; }
            public string Teacher { get; set; }
        }

        /// <summary>
        /// Структура записи приказов
        /// </summary>
        public class OrderItem
        {
            public Guid Id { get; set; }
            public DateTime Date { get; set; }
            public string Text { get; set; }
            public string Scope { get; set; }
            public string Status { get; set; }
        }
    }
}
