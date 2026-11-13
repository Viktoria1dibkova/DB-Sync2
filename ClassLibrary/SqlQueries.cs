using System.Collections.Generic;
using System.Security.Cryptography;

namespace ClassLibrary
{
    public class SqlQueries
    {
        /// <summary>
        /// Схема создания всех таблиц университета
        /// </summary>
        public const string Schema = @"CREATE TABLE IF NOT EXISTS faculties (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     name VARCHAR(255) NOT NULL UNIQUE,
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS departments (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     faculty_id UUID NOT NULL REFERENCES faculties(id),
                                     name VARCHAR(255) NOT NULL,
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS programs (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     dept_id UUID NOT NULL REFERENCES departments(id),
                                     code VARCHAR(20) NOT NULL,
                                     profile VARCHAR(255) NOT NULL,
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS students (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     program_id UUID NOT NULL REFERENCES programs(id),
                                     full_name VARCHAR(255) NOT NULL,
                                     group_number VARCHAR(50) NOT NULL,
                                     enrollment_year INTEGER NOT NULL CHECK (enrollment_year > 2000),
                                     is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS grades (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     student_id UUID NOT NULL REFERENCES students(id),
                                     subject VARCHAR(255) NOT NULL,
                                     grade INTEGER NOT NULL CHECK (grade BETWEEN 2 AND 5),
                                     date DATE NOT NULL DEFAULT CURRENT_DATE,
                                     teacher VARCHAR(255)  NOT NULL,
                                     is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS orders (
                                     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                                     text TEXT NOT NULL,
                                     date DATE NOT NULL DEFAULT CURRENT_DATE,
                                     scope VARCHAR(50) NOT NULL DEFAULT 'ALL',
                                     status VARCHAR(50) NOT NULL DEFAULT 'active',
                                     created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                                     updated_at TIMESTAMP NOT NULL DEFAULT NOW());
            
                                     CREATE TABLE IF NOT EXISTS sync_state (
                                     id INTEGER PRIMARY KEY,
                                     last_sync TIMESTAMP NOT NULL);
            
                                     INSERT INTO sync_state (id, last_sync) VALUES (1, '2000-01-01')
                                     ON CONFLICT (id) DO NOTHING;

                                     INSERT INTO sync_state (id, last_sync) VALUES (2, '2000-01-01')
                                     ON CONFLICT (id) DO NOTHING;";

        /// <summary>
        /// Чтение факультетов
        /// </summary>
        public const string Faculties = @"SELECT id, name, created_at AS ""Создан"" FROM faculties ORDER BY name";

        /// <summary>
        /// Чтение кафедр
        /// </summary>
        public const string Departments = @"SELECT d.id, d.name AS ""Название"", f.name AS ""Факультет"", d.created_at AS ""Создан""
                                          FROM departments d JOIN faculties f ON f.id = d.faculty_id ORDER BY f.name, d.name";

        /// <summary>
        /// Чтение направлений обучения
        /// </summary>
        public const string Programs = @"SELECT p.id, p.code AS ""Код"", p.profile AS ""Профиль"", d.name AS ""Кафедра"", f.name AS ""Факультет"", p.created_at AS ""Создан""
                                       FROM programs p JOIN departments d ON d.id = p.dept_id JOIN faculties f ON f.id = d.faculty_id ORDER BY f.name, d.name, p.code";

        /// <summary>
        /// Чтение студентов
        /// </summary>
        public const string Students = @"SELECT s.id, s.full_name AS ""ФИО"", s.group_number AS ""Группа"", s.enrollment_year AS ""Год поступления"",
                                       p.code AS ""Код направления"", p.profile AS ""Профиль"", s.created_at AS ""Создан"" FROM students s
                                       JOIN programs p ON p.id = s.program_id WHERE s.is_deleted = FALSE ORDER BY s.group_number, s.full_name";

        public const string Grades = @"SELECT g.id, g.date AS ""Дата"", s.full_name AS ""Студент"", g.subject AS ""Предмет"", g.grade AS ""Оценка"", 
                                     g.teacher AS ""Преподаватель"", g.created_at AS ""Создан"" FROM grades g JOIN students s ON s.id = g.student_id
                                     WHERE g.is_deleted = FALSE ORDER BY g.date DESC";

        /// <summary>
        /// Чтение приказов
        /// </summary>
        public const string Orders = @"SELECT id, date AS ""Дата"", text AS ""Текст"", scope AS ""Адресат"", status AS ""Статус"", created_at AS ""Создан"" FROM orders ORDER BY date DESC";

        /// <summary>
        /// Вставка факултета
        /// </summary>
        public const string InsertFaculty = "INSERT INTO faculties (name) VALUES (@n)";

        /// <summary>
        /// Вставка кафедры
        /// </summary>
        public const string InsertDepartment = "INSERT INTO departments (faculty_id, name) VALUES (@fid, @n)";

        /// <summary>
        /// Вставка направления
        /// </summary>
        public const string InsertProgram = "INSERT INTO programs (dept_id, code, profile) VALUES (@did, @c, @p)";

        /// <summary>
        /// Вставка студента
        /// </summary>
        public const string InsertStudent = "INSERT INTO students (program_id, full_name, group_number, enrollment_year) VALUES(@pid, @fn, @gn, @ey)";

        /// <summary>
        /// Вставка оценки
        /// </summary>
        public const string InsertGrade = "INSERT INTO grades (student_id, subject, grade, date, teacher) VALUES(@sid, @s, @g, @d, @t)";

        /// <summary>
        /// Вставка приказа
        /// </summary>
        public const string InsertOrder = "INSERT INTO orders (text, date, scope, status) VALUES (@t, @d, @sc, @st)";

        /// <summary>
        /// Методы удаления строк
        /// </summary>
        public const string DeleteFaculty = "DELETE FROM faculties WHERE id = @id";
        public const string DeleteDepartment = "DELETE FROM departments WHERE id = @id";
        public const string DeleteProgram = "DELETE FROM programs WHERE id = @id";
        public const string DeleteStudent = "DELETE FROM students WHERE id = @id";
        public const string DeleteGrade = "DELETE FROM grades WHERE id = @id";
        public const string DeleteOrder = "DELETE FROM orders WHERE id = @id";

        /// <summary>
        /// Получение времени последней синхронизации
        /// </summary>
        public const string GetSync = "SELECT last_sync FROM sync_state WHERE id = @id";

        /// <summary>
        /// Получение времени последней синхронизации
        /// </summary>
        public const string SetSync = "UPDATE sync_state SET last_sync = @ts WHERE id = @id";

        /// <summary>
        /// Методы обновления в связи с новыми данными
        /// </summary>

        public const string UpsertFaculty = @"INSERT INTO faculties (id, name, updated_at) VALUES (@id, @n, @u) ON CONFLICT (id) DO UPDATE
                                            SET name = EXCLUDED.name, updated_at = EXCLUDED.updated_at WHERE faculties.updated_at < EXCLUDED.updated_at";

        public const string UpsertDepartment = @"INSERT INTO departments (id, faculty_id, name, updated_at) VALUES (@id, @fid, @n, @u)
                                               ON CONFLICT (id) DO UPDATE SET faculty_id = EXCLUDED.faculty_id, name = EXCLUDED.name, updated_at = EXCLUDED.updated_at
                                               WHERE departments.updated_at < EXCLUDED.updated_at";

        public const string UpsertProgram = @"INSERT INTO programs (id, dept_id, code, profile, updated_at) VALUES (@id, @did, @c, @p, @u)
                                            ON CONFLICT (id) DO UPDATE SET dept_id = EXCLUDED.dept_id, code = EXCLUDED.code, profile = EXCLUDED.profile, updated_at = EXCLUDED.updated_at
                                            WHERE programs.updated_at < EXCLUDED.updated_at";

        public const string UpsertStudent = @"INSERT INTO students (id, program_id, full_name, group_number, enrollment_year, is_deleted, updated_at)
                                            VALUES (@id, @pid, @fn, @gn, @ey, @del, @u) ON CONFLICT (id) DO UPDATE SET program_id = EXCLUDED.program_id, full_name = EXCLUDED.full_name,
                                            group_number = EXCLUDED.group_number, enrollment_year = EXCLUDED.enrollment_year, is_deleted = EXCLUDED.is_deleted, updated_at = EXCLUDED.updated_at";

        public const string UpsertGrade = @"INSERT INTO grades (id, student_id, subject, grade, date, teacher, is_deleted, updated_at)
                                          VALUES (@id, @sid, @s, @g, @d, @t, @del, @u) ON CONFLICT (id) DO UPDATE SET student_id = EXCLUDED.student_id, subject = EXCLUDED.subject, grade = EXCLUDED.grade,
                                          date = EXCLUDED.date, teacher = EXCLUDED.teacher, is_deleted = EXCLUDED.is_deleted, updated_at = EXCLUDED.updated_at";

        public const string UpsertOrder = @"INSERT INTO orders (id, text, date, scope, status, updated_at) VALUES (@id, @t, @d, @sc, @st, @u)
                                          ON CONFLICT (id) DO UPDATE SET text = EXCLUDED.text, date = EXCLUDED.date, scope = EXCLUDED.scope, 
                                          status = EXCLUDED.status, updated_at = EXCLUDED.updated_at WHERE orders.updated_at < EXCLUDED.updated_at";

        /// <summary>
        /// Загрузка факульеттов
        /// </summary>
        public const string LoadMyFaculties = @"SELECT id, name FROM faculties";

        /// <summary>
        /// Загрузка кафедр
        /// </summary>
        public const string LoadMyDept = "SELECT id, faculty_id, name FROM departments";

        /// <summary>
        /// Загрузка направлений
        /// </summary>
        public const string LoadMyPrograms = "SELECT id, dept_id, code, profile FROM programs";
        /// Загрузка приказов
        /// </summary>
        public const string LoadMyOrders = @"SELECT id, text, date, scope, status, created_at, updated_at FROM orders 
                                           WHERE scope = 'ALL' OR scope = 'FACULTY_IT' OR scope = 'DEPT_PO'";

        /// <summary>
        /// Методы для получения новых данных        
        /// </summary>
        public const string LoadStudentsForSync = @"SELECT id, program_id, full_name, group_number, enrollment_year, is_deleted FROM students WHERE updated_at >= @since";
        public const string LoadGradesForSync = @"SELECT id, student_id, subject, grade, date, teacher, is_deleted FROM grades WHERE updated_at >= @since";

        /// <summary>
        /// Методы мягкого и физического удаления
        /// </summary>
        public const string SoftDeleteStudent = "UPDATE students SET is_deleted = TRUE, updated_at = NOW() WHERE id = @id";
        public const string SoftDeleteGrade = "UPDATE grades SET is_deleted = TRUE, updated_at = NOW() WHERE id = @id";
        public const string HardDeleteStudent = "DELETE FROM students WHERE id = @id";
        public const string HardDeleteGrade = "DELETE FROM grades WHERE id = @id";

        /// <summary>
        /// Удаление осиротевших записей
        /// </summary>
        public const string DeleteOrphanedFaculties = "DELETE FROM faculties WHERE id != ALL(@validIds)";
        public const string DeleteAllFaculties = "DELETE FROM faculties";

        public const string DeleteOrphanedDepartments = "DELETE FROM departments WHERE faculty_id = @fid AND id != ALL(@validIds)";
        public const string DeleteAllDepartmentsByFaculty = "DELETE FROM departments WHERE faculty_id = @fid";

        public const string DeleteOrphanedPrograms = "DELETE FROM programs WHERE dept_id = @did AND id != ALL(@validIds)";
        public const string DeleteAllProgramsByDept = "DELETE FROM programs WHERE dept_id = @did";

        public const string DeleteOrphanedOrders = "DELETE FROM orders WHERE (scope = 'ALL' OR scope = 'FACULTY_IT' OR scope = 'DEPT_PO') AND id != ALL(@validIds)";
        public const string DeleteAllLocalOrders = "DELETE FROM orders WHERE scope = 'ALL' OR scope = 'FACULTY_IT' OR scope = 'DEPT_PO'";

        /// <summary>
        /// Каскадное кдаление
        /// </summary>
        public const string DeleteProgramsByDept = "DELETE FROM programs WHERE dept_id = @did";
        public const string DeleteDepartmentById = "DELETE FROM departments WHERE id = @id";
    }
}
