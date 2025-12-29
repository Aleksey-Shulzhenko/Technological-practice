// Services/SalaryService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz.Services
{
    public class SalaryService
    {
        private readonly AppDbContext _context;

        public SalaryService(AppDbContext context)
        {
            _context = context;
        }

        // Прості методи без Include
        public decimal CalculateTotalSalaryForMonth(DateTime month)
        {
            var activeEmployees = _context.Employees
                .Where(e => e.IsActive)
                .ToList();

            return activeEmployees.Sum(e => e.Salary);
        }

        public List<Salary> GetMonthlySalaries(DateTime month)
        {
            var salaries = _context.Salaries
                .Where(s => s.Month == month)
                .ToList();

            // Завантажуємо працівників окремо
            foreach (var salary in salaries)
            {
                salary.Employee = _context.Employees.Find(salary.EmployeeId);
            }

            return salaries;
        }
    }
}