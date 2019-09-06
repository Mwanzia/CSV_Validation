using System;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeValidation
{
    public static class ErrorMessages
    {
        public const string INVALID_CSV_COLUMN_COUNT = "Invalid number of columns in csv file, only 3 columns per row were expected";
        public const string INVALID_EMPLOYEED_ID = "Invalid Employee Id: Input must be an integer greater than zero";
        public const string INVALID_SALARY = "Invalid Salary: Input must be an integer greater than zero";
        public const string MANAGER_CIRCULAR_REFERENCE = "Circular Reference Error: Two employees cannot be each others manager";
        public const string CEO_NOT_DEFINED = "CEO is not defined. This must be an employee or manager without a manager";
        public const string MULTIPLE_CEOS_DEFINED = "Multiple CEOS have been defined where as only one was expected";
        public const string EMPLOYEE_NOT_FOUND = "The employee/manager with the corresponding id does not exist in the list of employees";
        public const string DUPLICATE_EMPLOYEE_DEFINED = "An employee has appeared more than once in the list";
    }

    public class EmployeeHelper
    { 
        public EmployeeHelper(string employeeCSVInput)
        {
            ParseEmployeeCSV(employeeCSVInput);

            BuildEmployeeTree(GetRootNode());
        }

        #region Class Properties

        /// <summary>
        /// Represents an unordered linear list of employees that will is used to populate a hierachial employee tree
        /// </summary>
        private List<EmployeeNode> UnorderedEmployeeList { get; set; }
        #endregion

        #region Input Parsing and main validation logic
        
        /// <summary>
        /// Reads a CSV file and generates a list of employees, any parsing error will result in an exception being thrown
        /// and program will be halted
        /// </summary>
        /// <param name="input">Represents data from the CSV file with three expected columns of type integer</param>
        private void ParseEmployeeCSV(string input)
        {
            this.UnorderedEmployeeList = new List<EmployeeNode>();

            foreach (var line in input.Split(Environment.NewLine))
            {
                string[] columns = line.Split(',');
                if (columns.Length != 3)
                    throw new Exception(ErrorMessages.INVALID_CSV_COLUMN_COUNT);

                EmployeeNode node = new EmployeeNode(ParseEmployeeId(columns[0]),
                                                    ParseManagerId(columns[1]),
                                                    ParseSalary(columns[2]));

                if (this.UnorderedEmployeeList.Contains(node))
                    throw new Exception(ErrorMessages.DUPLICATE_EMPLOYEE_DEFINED);

                this.UnorderedEmployeeList.Add(node);
            }

            //validate that all managers are valid employees
            var managers = this.UnorderedEmployeeList.Where(x => x.ManagerId != null)
                                    .Select(x => x.ManagerId.Value)
                                    .Distinct()
                                    .ToList();

            if (managers.Where(x => !(this.UnorderedEmployeeList.Select(e => e.Id).Contains(x))).Any())
            {
                throw new Exception(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            //check for Circular References
            ValidateCircularReference();
        }

        /// <summary>
        /// Attempts to parse a string value into a employee id, throws exception if value is not an integer greater than zero
        /// </summary>
        /// <param name="text">String to parse</param>
        /// <returns>Parsed output as integer</returns>
        private int ParseEmployeeId(string text)
        {
            int id = 0;

            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                throw new Exception(ErrorMessages.INVALID_EMPLOYEED_ID);

            text = text.Trim().ToLower();

            //expected input format: Employee1
            if (!text.StartsWith("employee") || !int.TryParse(text.Substring(text.LastIndexOf('e') + 1), out id))
                throw new Exception(ErrorMessages.INVALID_EMPLOYEED_ID);

            return id;
        }

        private int? ParseManagerId(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text))
                return null;

            return ParseEmployeeId(text);
        }

        /// <summary>
        /// Attempts to parse a string value into an integer represeting an employee's salary. Throws an exception 
        /// if input is not a valid integer that is greater than zero. Null values are however accpeted 
        /// </summary>
        /// <param name="text">Represents input to parse</param>
        /// <returns>Parsed salary as an integer, zero is returned if the input parameter is null or empty</returns>
        private int ParseSalary(string text)
        {
            int salary = 0;
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrEmpty(text) ||
                !int.TryParse(text.Trim(), out salary) ||
                salary < 1)
            {
                throw new Exception(ErrorMessages.INVALID_SALARY);
            }
            return salary;
        }

        /// <summary>
        /// validates a manager is a valid employee
        /// </summary>
        /// <param name="managerId">The employee or manager to validate</param>
        private void ValidateManagerIsEmployee(int? managerId)
        {
            if (managerId == null)
                return; //this is valid, no manager defined for employee

            if (UnorderedEmployeeList.Count(x => x.Id == managerId) == 0)
                throw new Exception(ErrorMessages.EMPLOYEE_NOT_FOUND);
        }

        private void ValidateCircularReference()
        {
            foreach(var employee in this.UnorderedEmployeeList.Where(x=>x.ManagerId != null))
            {
                if (GetDirectReportees(employee.Id).Where(r => employee.ManagerId == r.Id).Any())
                    throw new Exception(ErrorMessages.MANAGER_CIRCULAR_REFERENCE);
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Attempts to retrieve the root node aka the CEO from the list of unstructured employees.
        /// Throws an exception of less or more than 1 CEO is found
        /// </summary>
        /// <returns>Represents the CEO which acts as the root node</returns>
        private EmployeeNode GetRootNode()
        {
            var ceos = UnorderedEmployeeList.Where(x => x.ManagerId == null).ToList();

            if (ceos.Count() < 1)
            {
                throw new Exception(ErrorMessages.CEO_NOT_DEFINED);
            }
            else if (ceos.Count() > 1)
            {
                throw new Exception(ErrorMessages.MULTIPLE_CEOS_DEFINED);
            }
            else
            {
                return ceos[0];
            }
        }
        private long TransverseSalaries(EmployeeNode managerNode,long sum)
        {
            sum += managerNode.Salary;

            if (managerNode.Reportees.Any())
            {
                foreach(var node in managerNode.Reportees)
                {
                    sum+= TransverseSalaries(node, 0);
                }
            }

            return sum;
        }
        #endregion

        #region Main Tree Data Structure Functionality

        /// <summary>
        /// Retrieves a list of employees that have the manager id set to the input parameter
        /// </summary>
        /// <param name="managerId"></param>
        /// <returns></returns>
        public List<EmployeeNode> GetDirectReportees(int managerId)
        {
            return UnorderedEmployeeList
                        .Where(x => x.ManagerId == managerId)
                        .ToList();
        }


        /// <summary>
        /// Recursively creates a hierachial tree map with the CEO as the root node
        /// </summary>
        /// <param name="node"></param>
        private void BuildEmployeeTree(EmployeeNode node)
        {
            ValidateManagerIsEmployee(node.ManagerId);

            //define node descendants i.e. subordinates to the current employee
            node.Reportees = GetDirectReportees(node.Id);

            if (node.Reportees.Count() == 0)
            {
                return;
            }
            else
            {
                foreach (EmployeeNode temp in node.Reportees)
                {
                    //further build the tree recursively by defining the reportees to each employee at this tree level
                    BuildEmployeeTree(temp);
                }
            }
        }

        public long GetManagerSalaryBudget(string employee)
        {
            int employeeId = ParseEmployeeId(employee);
            var temp = UnorderedEmployeeList.FirstOrDefault(x => x.Id == employeeId);

            if (temp == null)
            {
                throw new Exception(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            return TransverseSalaries(temp, 0);
        }

        #endregion
        public static void Main(string[] args)
        {
            string filePath = "", fileContents = "";
            try
            {
                if (args.Length <= 0)
                {
                    Console.Error.WriteLine("Ensure that the first parameter is a valid path to the employee CSV file");
                    return;
                }

                filePath = args[0];
                fileContents = System.IO.File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error In Reading File : " + ex.Message);
                return;
            }

            EmployeeHelper helper = new EmployeeHelper(fileContents);
        }
    }
}