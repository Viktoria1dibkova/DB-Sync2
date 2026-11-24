using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using static ClassLibrary.Models;

namespace UnitTest
{
    [TestClass]
    public class ModelsTests
    {
        [TestMethod]
        public void StudentItem_ShouldHaveCorrectProperties()
        {
            var student = new StudentItem
            {
                Id = Guid.NewGuid(),
                FullName = "Иванов И.И.",
                GroupNumber = "Г-1",
                EnrollmentYear = 2025,
                IsDeleted = false,
            };

            Assert.AreEqual("Иванов И.И.", student.FullName);
            Assert.AreEqual("Г-1", student.GroupNumber);
            Assert.AreEqual(2025, student.EnrollmentYear);
            Assert.IsFalse(student.IsDeleted);
        }

        [TestMethod]
        public void GradeItem_ShouldHaveCorrectProperties()
        {
            var grade = new GradeItem
            {
                Id = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                Subject = "Базы данных",
                Grade = 5,
                Date = DateTime.Now,
                Teacher = "Петров П.П.",
                IsDeleted = false,
                SourceLevel = 3
            };

            Assert.AreEqual("Базы данных", grade.Subject);
            Assert.AreEqual(5, grade.Grade);
            Assert.AreEqual("Петров П.П.", grade.Teacher);
            Assert.AreEqual(3, grade.SourceLevel);
        }

        [TestMethod]
        public void GradeItem_SourceLevel_ShouldReflectPriority()
        {
            var rectorateGrade = new GradeItem { SourceLevel = 1 };
            var facultyGrade = new GradeItem { SourceLevel = 2 };
            var deptGrade = new GradeItem { SourceLevel = 3 };

            Assert.IsTrue(rectorateGrade.SourceLevel < facultyGrade.SourceLevel);
            Assert.IsTrue(facultyGrade.SourceLevel < deptGrade.SourceLevel);
        }

        [TestMethod]
        public void FacultyItem_ShouldStoreName()
        {
            var faculty = new FacultyItem
            {
                Id = Guid.NewGuid(),
                Name = "Факультет ИТ"
            };

            Assert.AreEqual("Факультет ИТ", faculty.Name);
        }

        [TestMethod]
        public void ProgramItem_ShouldStoreCodeAndProfile()
        {
            var program = new ProgramItem
            {
                Id = Guid.NewGuid(),
                Code = "09.03.04",
                Profile = "Разработка информационных систем"
            };

            Assert.AreEqual("09.03.04", program.Code);
            Assert.AreEqual("Разработка информационных систем", program.Profile);
        }
    }
}
