// Data/DatabaseInitializer.cs
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            try
            {
                // Створюємо базу даних, якщо її немає
                if (context.Database.EnsureCreated())
                {
                    Console.WriteLine("Базу даних створено!");
                }

                // Перевіряємо чи є адміністратор
                if (!context.Employees.Any())
                {
                    // Додаємо тестові дані
                    var admin = new Employee
                    {
                        FullName = "Адміністратор Системи",
                        Position = "Адміністратор",
                        HireDate = DateTime.Now,
                        Salary = 25000,
                        Phone = "+380501234567",
                        Email = "admin@pptg.com.ua",
                        IsActive = true
                    };

                    context.Employees.Add(admin);
                    context.SaveChanges();

                    Console.WriteLine("Базу даних ініціалізовано з тестовими даними");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка ініціалізації бази даних: {ex.Message}", ex);
            }
        }
    }
}