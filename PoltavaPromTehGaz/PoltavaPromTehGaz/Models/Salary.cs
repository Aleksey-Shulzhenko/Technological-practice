using System;

namespace PoltavaPromTehGaz.Models
{
    public class Salary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime Month { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public DateTime CalculatedDate { get; set; } = DateTime.Now;
    }
}