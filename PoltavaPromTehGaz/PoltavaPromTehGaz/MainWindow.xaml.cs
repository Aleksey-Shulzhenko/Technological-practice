using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PoltavaPromTehGaz.Data;

namespace PoltavaPromTehGaz
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateDatabaseAndTables();
            UpdateStatistics();
        }

        private void CreateDatabaseAndTables()
        {
            try
            {
                CreateTablesWithSql();

                using (var db = new AppDbContext())
                {
                    if (!db.Employees.Any())
                    {
                        AddTestData(db);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення бази: {ex.Message}", "Помилка");
            }
        }

        private void CreateTablesWithSql()
        {
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=PoltavaPromTehGazDB;Trusted_Connection=True;TrustServerCertificate=True;";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string createEmployeesTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
                    BEGIN
                        CREATE TABLE Employees (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            FullName NVARCHAR(200) NOT NULL,
                            Position NVARCHAR(100),
                            HireDate DATETIME NOT NULL DEFAULT GETDATE(),
                            Salary DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Phone NVARCHAR(50),
                            Email NVARCHAR(100),
                            IsActive BIT NOT NULL DEFAULT 1
                        );
                    END";

                string createSalariesTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Salaries')
                    BEGIN
                        CREATE TABLE Salaries (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            EmployeeId INT NOT NULL,
                            Month DATETIME NOT NULL,
                            Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
                            CalculatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                            FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
                        );
                    END";

                string createCylindersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cylinders')
                    BEGIN
                        CREATE TABLE Cylinders (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            SerialNumber NVARCHAR(100) NOT NULL,
                            GasType NVARCHAR(50) NOT NULL,
                            Status INT NOT NULL DEFAULT 0,
                            Location INT NOT NULL DEFAULT 0,
                            ManufactureDate DATETIME NOT NULL,
                            LastCheckDate DATETIME NOT NULL,
                            NextCheckDate DATETIME NOT NULL,
                            PurchasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                            CurrentValue DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Notes NVARCHAR(MAX),
                            CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                            UpdatedDate DATETIME NULL
                        );
                    END";

                using (var command = new SqlCommand(createEmployeesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand(createSalariesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand(createCylindersTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddTestData(AppDbContext db)
        {
            var admin = new Models.Employee
            {
                FullName = "Адміністратор",
                Position = "Адміністратор",
                HireDate = DateTime.Now,
                Salary = 25000,
                Phone = "+380501234567",
                Email = "admin@company.com",
                IsActive = true
            };

            db.Employees.Add(admin);
            db.SaveChanges();

            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var salary = new Models.Salary
            {
                EmployeeId = admin.Id,
                Month = currentMonth,
                Amount = admin.Salary,
                CalculatedDate = DateTime.Now
            };

            db.Salaries.Add(salary);
            db.SaveChanges();

            var cylinder = new Models.Cylinder
            {
                SerialNumber = "ARG-001",
                GasType = "Аргон",
                Status = Models.CylinderStatus.Повний,
                Location = Models.CylinderLocation.Склад,
                ManufactureDate = DateTime.Now.AddYears(-1),
                LastCheckDate = DateTime.Now,
                NextCheckDate = DateTime.Now.AddYears(1),
                PurchasePrice = 1500,
                CurrentValue = 1800,
                Notes = "Тестовий балон"
            };

            db.Cylinders.Add(cylinder);
            db.SaveChanges();

            MessageBox.Show("✅ Базу даних успішно створено з тестовими даними!", "Успіх");
        }

        private void UpdateStatistics()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var employeeCount = db.Employees.Count();
                    var cylinderCount = db.Cylinders.Count();
                    var totalValue = db.Cylinders.Sum(c => (decimal?)c.CurrentValue) ?? 0;

                    var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var totalSalary = 0m;

                    try
                    {
                        totalSalary = db.Salaries
                            .Where(s => s.Month == currentMonth)
                            .Sum(s => (decimal?)s.Amount) ?? 0;
                    }
                    catch
                    {
                        totalSalary = 0;
                    }

                    txtEmployeeCount.Text = $"Працівників: {employeeCount}";
                    txtCylinderCount.Text = $"Балонів: {cylinderCount}";
                    txtTotalValue.Text = $"Загальна вартість: {totalValue:N2} грн";

                    UpdateSalaryDisplay(totalSalary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення статистики: {ex.Message}", "Помилка");
                txtEmployeeCount.Text = "Працівників: 0";
                txtCylinderCount.Text = "Балонів: 0";
                txtTotalValue.Text = "Загальна вартість: 0 грн";
            }
        }

        private void UpdateSalaryDisplay(decimal totalSalary)
        {
            var container = FindName("MainContentArea") as Grid;
            if (container != null)
            {
                var border = container.Children.OfType<Border>()
                    .FirstOrDefault(b => b.Child is StackPanel);

                if (border != null)
                {
                    var stackPanel = border.Child as StackPanel;
                    if (stackPanel != null)
                    {
                        var salaryText = stackPanel.Children
                            .OfType<TextBlock>()
                            .FirstOrDefault(t => t.Name == "txtSalary");

                        if (salaryText == null)
                        {
                            salaryText = new TextBlock
                            {
                                Name = "txtSalary",
                                Text = $"Зарплата за місяць: {totalSalary:N2} грн",
                                Foreground = Brushes.Blue,
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(0, 10, 0, 0)
                            };
                            stackPanel.Children.Add(salaryText);
                        }
                        else
                        {
                            salaryText.Text = $"Зарплата за місяць: {totalSalary:N2} грн";
                        }
                    }
                }
            }
        }

        private void BtnCylinders_Click(object sender, RoutedEventArgs e)
        {
            var window = new CylindersWindow();
            window.Owner = this;
            window.ShowDialog();
            UpdateStatistics();
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            var window = new SimpleEmployeesWindow();
            window.Owner = this;
            window.ShowDialog();
            UpdateStatistics();
        }

        private void BtnSalary_Click(object sender, RoutedEventArgs e)
        {
            var window = new SalaryWindow();
            window.Owner = this;
            window.ShowDialog();
            UpdateStatistics();
        }

        private void BtnViewSalaries_Click(object sender, RoutedEventArgs e)
        {
            BtnSalary_Click(sender, e);
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var employeeCount = db.Employees.Count();
                    var cylinderCount = db.Cylinders.Count();
                    var totalValue = db.Cylinders.Sum(c => (decimal?)c.CurrentValue) ?? 0;
                    var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var totalSalary = 0m;

                    try
                    {
                        totalSalary = db.Salaries
                            .Where(s => s.Month == currentMonth)
                            .Sum(s => (decimal?)s.Amount) ?? 0;
                    }
                    catch
                    {
                        totalSalary = 0;
                    }

                    var report = $"📊 ЗВІТ ПО СИСТЕМІ\n" +
                                $"========================\n" +
                                $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                                $"Працівників: {employeeCount}\n" +
                                $"Балонів: {cylinderCount}\n" +
                                $"Загальна вартість: {totalValue:N2} грн\n" +
                                $"Зарплата за місяць: {totalSalary:N2} грн\n" +
                                $"========================\n";

                    MessageBox.Show(report, "Звіт по системі");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка формування звіту: {ex.Message}", "Помилка");
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "⚙️ Налаштування системи",
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var mainGrid = new Grid();
            mainGrid.Margin = new Thickness(10);

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerBorder = new Border
            {
                Background = (Brush?)new BrushConverter().ConvertFrom("#3498DB") ?? Brushes.Blue,
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(5)
            };
            Grid.SetRow(headerBorder, 0);

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
            headerStack.Children.Add(new TextBlock
            {
                Text = "⚙️",
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            });
            headerStack.Children.Add(new TextBlock
            {
                Text = " НАЛАШТУВАННЯ СИСТЕМИ",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            });
            headerBorder.Child = headerStack;

            var scrollViewer = new ScrollViewer();
            Grid.SetRow(scrollViewer, 1);

            var contentStack = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };

            var dbGroupBox = new GroupBox
            {
                Header = "🗄️ База даних",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var dbStack = new StackPanel();
            dbStack.Children.Add(new TextBlock
            {
                Text = $"Сервер: (localdb)\\MSSQLLocalDB",
                Margin = new Thickness(0, 5, 0, 2)
            });
            dbStack.Children.Add(new TextBlock
            {
                Text = $"База даних: PoltavaPromTehGazDB",
                Margin = new Thickness(0, 2, 0, 10)
            });

            var testButton = new Button
            {
                Content = "🔍 Перевірити з'єднання",
                Width = 180,
                Height = 30,
                Background = (Brush?)new BrushConverter().ConvertFrom("#17A2B8") ?? Brushes.Cyan,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 0)
            };
            testButton.Click += (s, args) =>
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var employees = db.Employees.Count();
                        var cylinders = db.Cylinders.Count();
                        MessageBox.Show($"✅ З'єднання успішне!\n\nПрацівників: {employees}\nБалонів: {cylinders}", "Перевірка з'єднання");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Помилка з'єднання:\n{ex.Message}", "Помилка");
                }
            };
            dbStack.Children.Add(testButton);

            dbGroupBox.Content = dbStack;
            contentStack.Children.Add(dbGroupBox);

            var backupGroupBox = new GroupBox
            {
                Header = "💾 Резервне копіювання",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var backupStack = new StackPanel();

            var backupButton = new Button
            {
                Content = "📥 Створити резервну копію",
                Width = 180,
                Height = 30,
                Background = (Brush?)new BrushConverter().ConvertFrom("#28A745") ?? Brushes.Green,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 5)
            };
            backupButton.Click += (s, args) =>
            {
                try
                {
                    var backupPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        $"Backup_PoltavaPromTehGaz_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

                    using (var connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                    {
                        connection.Open();

                        var backupCommand = $"BACKUP DATABASE PoltavaPromTehGazDB TO DISK = '{backupPath}' WITH FORMAT, MEDIANAME = 'PoltavaBackup', NAME = 'Full Backup';";

                        using (var cmd = new SqlCommand(backupCommand, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show($"✅ Резервну копію створено:\n{backupPath}", "Успіх");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Помилка створення резервної копії:\n{ex.Message}", "Помилка");
                }
            };
            backupStack.Children.Add(backupButton);

            var restoreButton = new Button
            {
                Content = "📤 Відновити з бекапу",
                Width = 180,
                Height = 30,
                Background = (Brush?)new BrushConverter().ConvertFrom("#FFC107") ?? Brushes.Yellow,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 0)
            };
            restoreButton.Click += (s, args) =>
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "SQL Backup files (*.sql)|*.sql|All files (*.*)|*.*",
                    Title = "Виберіть файл резервної копії"
                };

                if (openDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                        {
                            connection.Open();

                            var restoreCommand = $"USE master; RESTORE DATABASE PoltavaPromTehGazDB FROM DISK = '{openDialog.FileName}' WITH REPLACE;";

                            using (var cmd = new SqlCommand(restoreCommand, connection))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("✅ Базу даних відновлено з резервної копії!\n\nПерезапустіть програму для застосування змін.", "Успіх");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Помилка відновлення:\n{ex.Message}", "Помилка");
                    }
                }
            };
            backupStack.Children.Add(restoreButton);

            backupGroupBox.Content = backupStack;
            contentStack.Children.Add(backupGroupBox);

            // Блок системної інформації з ПРАВИЛЬНОЮ ДАТОЮ
            var sysGroupBox = new GroupBox
            {
                Header = "⚡ Інформація про систему",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var sysStack = new StackPanel();

            DateTime buildDate;
            try
            {
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(assemblyLocation))
                {
                    buildDate = System.IO.File.GetLastWriteTime(assemblyLocation);
                }
                else
                {
                    buildDate = DateTime.Now;
                }
            }
            catch
            {
                buildDate = DateTime.Now;
            }

            sysStack.Children.Add(new TextBlock
            {
                Text = $"Версія програми: 1.0.1",
                Margin = new Thickness(0, 5, 0, 2)
            });
            sysStack.Children.Add(new TextBlock
            {
                Text = $"Дата збірки: {buildDate:dd.MM.yyyy HH:mm}",
                Margin = new Thickness(0, 2, 0, 2)
            });
            sysStack.Children.Add(new TextBlock
            {
                Text = $"Фреймворк: .NET 8.0",
                Margin = new Thickness(0, 2, 0, 2)
            });
            sysStack.Children.Add(new TextBlock
            {
                Text = $"Користувач: {Environment.UserName}",
                Margin = new Thickness(0, 2, 0, 2)
            });
            sysStack.Children.Add(new TextBlock
            {
                Text = $"Комп'ютер: {Environment.MachineName}",
                Margin = new Thickness(0, 2, 0, 2)
            });
            sysStack.Children.Add(new TextBlock
            {
                Text = $"ОС: Windows {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}",
                Margin = new Thickness(0, 2, 0, 5)
            });

            sysGroupBox.Content = sysStack;
            contentStack.Children.Add(sysGroupBox);

            scrollViewer.Content = contentStack;

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(buttonPanel, 2);

            var closeButton = new Button
            {
                Content = "✕ Закрити",
                Width = 100,
                Height = 35,
                Background = (Brush?)new BrushConverter().ConvertFrom("#95A5A6") ?? Brushes.Gray,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };
            closeButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(closeButton);

            mainGrid.Children.Add(headerBorder);
            mainGrid.Children.Add(scrollViewer);
            mainGrid.Children.Add(buttonPanel);

            dialog.Content = mainGrid;
            dialog.ShowDialog();
        }

        private void BtnQuickAddCylinder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CylinderEditDialog(null);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        db.Cylinders.Add(dialog.Cylinder);
                        db.SaveChanges();
                        UpdateStatistics();
                        MessageBox.Show("✅ Балон додано успішно!", "Успіх");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Помилка: {ex.Message}", "Помилка");
                }
            }
        }

        private void BtnQuickSalary_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Розрахувати зарплату за поточний місяць?\n\nЦе оновить дані зарплат для всіх активних працівників.",
                "Розрахунок зарплати",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AppDbContext())
                    {
                        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                        var oldSalaries = db.Salaries
                            .Where(s => s.Month == currentMonth)
                            .ToList();

                        if (oldSalaries.Any())
                        {
                            db.Salaries.RemoveRange(oldSalaries);
                        }

                        var activeEmployees = db.Employees
                            .Where(e => e.IsActive)
                            .ToList();

                        foreach (var employee in activeEmployees)
                        {
                            var salary = new Models.Salary
                            {
                                EmployeeId = employee.Id,
                                Month = currentMonth,
                                Amount = employee.Salary,
                                CalculatedDate = DateTime.Now
                            };

                            db.Salaries.Add(salary);
                        }

                        db.SaveChanges();
                        UpdateStatistics();

                        MessageBox.Show($"✅ Зарплату розраховано для {activeEmployees.Count} працівників\n" +
                                       $"📊 Загальна сума: {activeEmployees.Sum(e => e.Salary):N2} грн",
                                       "Розрахунок завершено");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Помилка розрахунку: {ex.Message}", "Помилка");
                }
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вийти з програми?", "Підтвердження",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}