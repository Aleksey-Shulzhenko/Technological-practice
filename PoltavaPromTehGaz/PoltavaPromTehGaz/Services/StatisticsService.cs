using System;
using System.Linq;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz.Services
{
    public class StatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public class StatisticsData
        {
            public int EmployeeCount { get; set; }
            public int ActiveEmployeeCount { get; set; }
            public int CylinderCount { get; set; }
            public decimal TotalCylinderValue { get; set; }
            public decimal CurrentMonthSalaryTotal { get; set; }
            public decimal AllSalariesTotal { get; set; }
        }

        public StatisticsData GetStatistics()
        {
            var data = new StatisticsData();

            try
            {
                // Працівники
                data.EmployeeCount = _context.Employees.Count();
                data.ActiveEmployeeCount = _context.Employees.Count(e => e.IsActive);

                // Балоні
                data.CylinderCount = _context.Cylinders.Count();
                data.TotalCylinderValue = _context.Cylinders.Sum(c => (decimal?)c.CurrentValue) ?? 0;

                // Зарплата за поточний місяць
                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                data.CurrentMonthSalaryTotal = _context.Salaries
                    .Where(s => s.Month == currentMonth)
                    .Sum(s => (decimal?)s.Amount) ?? 0;

                // Загальна сума всіх зарплат
                data.AllSalariesTotal = _context.Salaries.Sum(s => (decimal?)s.Amount) ?? 0;
            }
            catch (Exception)
            {
                // У разі помилки повертаємо нульові значення
            }

            return data;
        }

        // Отримання статистики по типах газів
        public object? GetGasTypeStatistics()
        {
            try
            {
                return _context.Cylinders
                    .GroupBy(c => c.GasType)
                    .Select(g => new
                    {
                        GasType = g.Key,
                        Count = g.Count(),
                        TotalValue = g.Sum(c => c.CurrentValue)
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        // Отримання статистики по статусах балонів
        public object? GetCylinderStatusStatistics()
        {
            try
            {
                return _context.Cylinders
                    .GroupBy(c => c.Status)
                    .Select(g => new
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}