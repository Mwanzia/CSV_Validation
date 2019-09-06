using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeValidation
{
    public class EmployeeNode 
    {
        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public int Salary { get; set; }

        /// <summary>
        /// Represents employees reporting to this instance
        /// </summary>
        public List<EmployeeNode> Reportees { get; set; }

        public EmployeeNode(int id, int? managerId, int salary)
        {
            Id = id;
            ManagerId = managerId;
            Salary = salary;

            Reportees = new List<EmployeeNode>();
        }

        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(EmployeeNode) && ((EmployeeNode)obj).Id == this.Id);
        }
    }
}
