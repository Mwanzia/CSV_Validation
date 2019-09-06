using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmployeeValidation;
using System;
using System.Linq;

namespace Helper_UnitTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void EmployeeId_Validation_Test()
        {
            //test for empty/invalid employee id
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper(",Employee1,200"), ErrorMessages.INVALID_EMPLOYEED_ID);

            //employee id is invalid
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("-1,Employee1,200"), ErrorMessages.INVALID_EMPLOYEED_ID);

            //employee id is invalid
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("employee,Employee1,200"), ErrorMessages.INVALID_EMPLOYEED_ID);

            //manager id is invalid
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,Employee,200"), ErrorMessages.INVALID_EMPLOYEED_ID);

            //manager id is invalid
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,3,200"), ErrorMessages.INVALID_EMPLOYEED_ID);

            //NO CEO defined, also manager is not an employee
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,Employee2,200"));

            try
            {
                new EmployeeHelper("Employee1,,250");
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception but instead got: " + ex.Message);
            }
        }

        [TestMethod]
        public void Employee_Salary_Validation_Test()
        {
            //salary must be greater than 0
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,,0"), ErrorMessages.INVALID_SALARY);

            //null salary not allowed
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,,"), ErrorMessages.INVALID_SALARY);

            //only integers allowed
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,,a43!sdf94"), ErrorMessages.INVALID_SALARY);

            //only integer values allowed
            Assert.ThrowsException<Exception>(() =>
                new EmployeeHelper("Employee1,,2343.435"), ErrorMessages.INVALID_SALARY);
        }

        [TestMethod]
        public void Manager_Direct_Reports_Validation_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,250",
                "Employee2,Employee1,100",
                "Employee4,Employee2,130",
                "Employee5,Employee1,130",
                "Employee6,Employee2,130",
                "Employee7,Employee1,130"
            };

            string csvFile = string.Join(newLine, input);

            EmployeeHelper helper = new EmployeeHelper(csvFile);

            var subordinates = helper.GetDirectReportees(1).Select(x => x.Id).ToArray();
            int[] expectedSubordinates = new[] { 2, 5, 7 };

            Assert.IsTrue(Enumerable.SequenceEqual(subordinates, expectedSubordinates),
                "The expected subordinates was not correct");
        }

        [TestMethod]
        public void CEO_Not_Defined_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,Employee4,250",
                "Employee2,Employee1,100",
                "Employee4,Employee2,130",
                "Employee5,Employee1,130",
                "Employee6,Employee2,130",
                "Employee7,Employee1,130"
            };

            string csvFile = string.Join(newLine, input);
            Assert.ThrowsException<Exception>(() => new EmployeeHelper(csvFile), ErrorMessages.CEO_NOT_DEFINED);
        }

        [TestMethod]
        public void Multiple_CEO_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,250",
                "Employee2,,100",
                "Employee4,Employee2,130",
                "Employee5,Employee1,130",
                "Employee6,Employee2,130",
                "Employee7,Employee1,130"
            };

            string csvFile = string.Join(newLine, input);
            Assert.ThrowsException<Exception>(() => new EmployeeHelper(csvFile), ErrorMessages.MULTIPLE_CEOS_DEFINED);
        }

        [TestMethod]
        public void Manager_Not_Employee_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,250",
                "Employee2,Employee1,100",
                "Employee4,Employee2,130",
                "Employee5,Employee1294733,130",
                "Employee6,Employee2,130",
                "Employee7,Employee14345,130"
            };

            string csvFile = string.Join(newLine, input);

            //Employee1294733 and Employee14345 should throw an error
            Assert.ThrowsException<Exception>(() => new EmployeeHelper(csvFile), ErrorMessages.EMPLOYEE_NOT_FOUND);
        }

        [TestMethod]
        public void One_Manager_Per_Employee_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,250",
                "Employee2,Employee1,100",
                "Employee2,Employee6,130",
                "Employee6,Employee2,130",
                "Employee4,Employee1,130"
            };

            string csvFile = string.Join(newLine, input);

            //Employee1294733 and Employee14345 should throw an error
            Assert.ThrowsException<Exception>(() => new EmployeeHelper(csvFile), ErrorMessages.DUPLICATE_EMPLOYEE_DEFINED);
        }

        [TestMethod]
        public void Circular_Reference_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,250",
                "Employee2,Employee1,100",
                "Employee3,Employee4,130",
                "Employee4,Employee3,130",
            };

            string csvFile = string.Join(newLine, input);

            Assert.ThrowsException<Exception>(() => new EmployeeHelper(csvFile), ErrorMessages.MANAGER_CIRCULAR_REFERENCE);
        }

        [TestMethod]
        public void Manager_Budget_Test()
        {
            var newLine = System.Environment.NewLine;
            string[] input = new[]
            {
                "Employee1,,400",
                "Employee2,Employee1,350",
                "Employee3,Employee1,340",
                "Employee4,Employee1,320",
                "Employee5,Employee2,300",
                "Employee6,Employee2,280",
                "Employee7,Employee2,250",
                "Employee8,Employee2,230",
                "Employee9,Employee3,200",
                "Employee10,Employee3,150"
            };

            string csvFile = string.Join(newLine, input);
            var helper = new EmployeeHelper(csvFile);

            Assert.ThrowsException<Exception>(() =>
                helper.GetManagerSalaryBudget("Employee34883"), ErrorMessages.EMPLOYEE_NOT_FOUND);

            Assert.AreEqual(2820, helper.GetManagerSalaryBudget("Employee1"));
            Assert.AreEqual(1410, helper.GetManagerSalaryBudget("Employee2"));
            Assert.AreEqual(690,  helper.GetManagerSalaryBudget("Employee3"));
            Assert.AreEqual(320,  helper.GetManagerSalaryBudget("Employee4"));
            Assert.AreEqual(300,  helper.GetManagerSalaryBudget("Employee5"));
            Assert.AreEqual(280,  helper.GetManagerSalaryBudget("Employee6"));
            Assert.AreEqual(250,  helper.GetManagerSalaryBudget("Employee7"));
            Assert.AreEqual(230,  helper.GetManagerSalaryBudget("Employee8"));
            Assert.AreEqual(200,  helper.GetManagerSalaryBudget("Employee9"));
            Assert.AreEqual(150,  helper.GetManagerSalaryBudget("Employee10"));
        }
    }
}