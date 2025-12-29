using System;
using System.Linq;
using System.Windows;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz
{
    public partial class SalaryWindow : Window
    {
        private readonly AppDbContext? _dbContext; // Додаємо nullable
        private DateTime _currentMonth;

        public SalaryWindow()
        {
            InitializeComponent();

            try
            {
                _dbContext = new AppDbContext();
                CreateDatabaseIfNotExists(); // Створюємо БД та таблиці

                _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                MonthPicker.SelectedDate = _currentMonth;

                LoadSalariesForCurrentMonth();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації: {ex.Message}", "Помилка");
                Close();
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            if (_dbContext == null) return;

            try
            {
                // Створюємо базу даних та таблиці
                _dbContext.Database.EnsureCreated();

                // Перевіряємо чи є дані
                if (!_dbContext.Employees.Any())
                {
                    AddTestData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка бази даних: {ex.Message}", "Помилка");
                throw;
            }
        }

        private void AddTestData()
        {
            if (_dbContext == null) return;

            // Додаємо тестового працівника
            var admin = new Employee
            {
                FullName = "Адміністратор",
                Position = "Адміністратор",
                HireDate = DateTime.Now,
                Salary = 25000,
                Phone = "+380501234567",
                Email = "admin@company.com",
                IsActive = true
            };

            _dbContext.Employees.Add(admin);
            _dbContext.SaveChanges();

            // Додаємо тестову зарплату
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var salary = new Salary
            {
                EmployeeId = admin.Id,
                Employee = admin,
                Month = currentMonth,
                Amount = admin.Salary,
                CalculatedDate = DateTime.Now
            };

            _dbContext.Salaries.Add(salary);
            _dbContext.SaveChanges();
        }

        private void LoadSalariesForCurrentMonth()
        {
            if (_dbContext == null) return;

            try
            {
                var month = MonthPicker.SelectedDate ?? _currentMonth;
                var monthStart = new DateTime(month.Year, month.Month, 1);

                // Отримуємо зарплати з обробкою можливих помилок
                var salaries = _dbContext.Salaries
                    .Where(s => s.Month == monthStart)
                    .ToList();

                // Завантажуємо пов'язаних працівників
                foreach (var salary in salaries)
                {
                    salary.Employee = _dbContext.Employees.Find(salary.EmployeeId);
                }

                // Сортуємо по ПІБ працівника
                salaries = salaries
                    .OrderBy(s => s.Employee?.FullName ?? "")
                    .ToList();

                SalaryGrid.ItemsSource = salaries;

                var totalAmount = salaries.Sum(s => s.Amount);
                TotalAmountText.Text = $"Загальна сума: {totalAmount:N2} грн";

                var statusText = $"Зарплата за {monthStart:MMMM yyyy} ({salaries.Count} працівників)";
                StatusText.Text = statusText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка");
                SalaryGrid.ItemsSource = null;
                TotalAmountText.Text = "Загальна сума: 0.00 грн";
                StatusText.Text = "Немає даних";
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dbContext == null) return;

            try
            {
                var month = MonthPicker.SelectedDate ?? DateTime.Now;
                var monthStart = new DateTime(month.Year, month.Month, 1);

                var result = MessageBox.Show(
                    $"Розрахувати зарплату за {monthStart:MMMM yyyy}?\n\n" +
                    "Примітка: старі розрахунки за цей місяць будуть перезаписані.",
                    "Підтвердження розрахунку",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Видаляємо старі розрахунки за цей місяць
                    var oldSalaries = _dbContext.Salaries
                        .Where(s => s.Month == monthStart)
                        .ToList();

                    if (oldSalaries.Any())
                    {
                        _dbContext.Salaries.RemoveRange(oldSalaries);
                    }

                    // Отримуємо активних працівників
                    var activeEmployees = _dbContext.Employees
                        .Where(e => e.IsActive)
                        .ToList();

                    // Розраховуємо зарплату для кожного
                    foreach (var employee in activeEmployees)
                    {
                        var salary = new Salary
                        {
                            EmployeeId = employee.Id,
                            Employee = employee,
                            Month = monthStart,
                            Amount = employee.Salary,
                            CalculatedDate = DateTime.Now
                        };

                        _dbContext.Salaries.Add(salary);
                    }

                    _dbContext.SaveChanges();
                    LoadSalariesForCurrentMonth();

                    MessageBox.Show(
                        $"✅ Зарплату розраховано для {activeEmployees.Count} працівників\n" +
                        $"📊 Загальна сума: {activeEmployees.Sum(e => e.Salary):N2} грн\n" +
                        $"📅 Місяць: {monthStart:MMMM yyyy}",
                        "Розрахунок завершено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Помилка розрахунку: {ex.Message}", "Помилка");
            }
        }

        private void ShowAllSalaries_Click(object sender, RoutedEventArgs e)
        {
            if (_dbContext == null) return;

            try
            {
                var allSalaries = _dbContext.Salaries
                    .ToList();

                // Завантажуємо пов'язаних працівників
                foreach (var salary in allSalaries)
                {
                    salary.Employee = _dbContext.Employees.Find(salary.EmployeeId);
                }

                // Сортуємо
                allSalaries = allSalaries
                    .OrderByDescending(s => s.Month)
                    .ThenBy(s => s.Employee?.FullName ?? "")
                    .ToList();

                if (!allSalaries.Any())
                {
                    MessageBox.Show("Немає розрахованих зарплат", "Інформація");
                    return;
                }

                SalaryGrid.ItemsSource = allSalaries;

                var totalAmount = allSalaries.Sum(s => s.Amount);
                TotalAmountText.Text = $"Загальна сума всіх зарплат: {totalAmount:N2} грн";
                StatusText.Text = $"Всі зарплати ({allSalaries.Count} записів)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Помилка: {ex.Message}", "Помилка");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var salaries = SalaryGrid.ItemsSource as System.Collections.IEnumerable;
                if (salaries == null || !salaries.Cast<object>().Any())
                {
                    MessageBox.Show("Немає даних для друку", "Інформація");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Зарплата_{DateTime.Now:yyyyMMdd_HHmm}.txt",
                    Filter = "Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*",
                    DefaultExt = ".txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("═".PadRight(50, '═'));
                        writer.WriteLine("               ЗВІТ ПРО ЗАРПЛАТУ");
                        writer.WriteLine("═".PadRight(50, '═'));
                        writer.WriteLine($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        writer.WriteLine($"Звіт згенеровано в системі: ПОЛТАВАПРОМТЕХГАЗ");
                        writer.WriteLine("─".PadRight(50, '─'));

                        decimal totalAmount = 0;
                        int employeeCount = 0;

                        foreach (Salary salary in salaries)
                        {
                            writer.WriteLine($"Працівник: {salary.Employee?.FullName ?? "Невідомий"}");
                            writer.WriteLine($"  Посада: {salary.Employee?.Position ?? "-"}");
                            writer.WriteLine($"  Місяць: {salary.Month.ToString("MMMM yyyy", new System.Globalization.CultureInfo("uk-UA"))}");
                            writer.WriteLine($"  Сума: {salary.Amount:N2} грн");
                            writer.WriteLine($"  Дата розрахунку: {salary.CalculatedDate:dd.MM.yyyy}");
                            writer.WriteLine("─".PadRight(30, '─'));

                            totalAmount += salary.Amount;
                            employeeCount++;
                        }

                        writer.WriteLine("═".PadRight(50, '═'));
                        writer.WriteLine($"РАЗОМ:");
                        writer.WriteLine($"Кількість працівників: {employeeCount}");
                        writer.WriteLine($"ЗАГАЛЬНА СУМА: {totalAmount:N2} грн");
                        writer.WriteLine("═".PadRight(50, '═'));
                    }

                    MessageBox.Show($"✅ Звіт збережено у файл:\n{saveDialog.FileName}", "Збереження успішне");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Помилка друку: {ex.Message}", "Помилка");
            }
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var salaries = SalaryGrid.ItemsSource as System.Collections.IEnumerable;
                if (salaries == null || !salaries.Cast<object>().Any())
                {
                    MessageBox.Show("Немає даних для експорту", "Інформація");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Зарплата_{DateTime.Now:yyyyMMdd}.csv",
                    Filter = "CSV файли (*.csv)|*.csv|Всі файли (*.*)|*.*"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        writer.WriteLine("ПІБ;Посада;Місяць;Зарплата (грн);Дата розрахунку");

                        // Дані
                        foreach (Salary salary in salaries)
                        {
                            writer.WriteLine($"\"{salary.Employee?.FullName}\";" +
                                          $"\"{salary.Employee?.Position}\";" +
                                          $"\"{salary.Month:MMMM yyyy}\";" +
                                          $"{salary.Amount:N2};" +
                                          $"{salary.CalculatedDate:dd.MM.yyyy}");
                        }
                    }

                    MessageBox.Show($"✅ Дані експортовано у файл:\n{saveDialog.FileName}", "Експорт завершено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Помилка експорту: {ex.Message}", "Помилка");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbContext?.Dispose();
            }
            catch { }
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try
            {
                _dbContext?.Dispose();
            }
            catch { }
        }

        private void MonthPicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadSalariesForCurrentMonth();
        }
    }
}