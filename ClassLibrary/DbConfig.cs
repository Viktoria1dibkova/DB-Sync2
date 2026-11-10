using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class DbConfig
    {
        public const string UniversityConn = "Host=localhost;Port=5432;Database=university_db;Username=postgres";

        public const string FacultyConn = "Host=localhost;Port=5432;Database=faculty_it_db;Username=postgres";

        public const string DeptConn = "Host=localhost;Port=5432;Database=dept_po_db;Username=postgres";
        
        /// <summary>
        /// Названия для фильтрации
        /// </summary>
        public const string MyFacultyName = "Факультет информационных технологий";
        public const string MyDeptName = "Кафедра программного обеспечения";
    }
}
