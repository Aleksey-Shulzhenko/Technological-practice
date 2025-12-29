using System;
using System.Linq;
using System.Windows;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz
{
    public partial class SimpleEmployeesWindow : Window
    {
        private AppDbContext? _dbContext;

        public SimpleEmployeesWindow()
        {
            InitializeComponent();
            try
            {
                _dbContext = new AppDbContext();
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації: {ex.Message}");
            }
        }

        private void LoadEmployees()
        {
            try
            {
                if (_dbContext == null) return;

                var employees = _dbContext.Employees.ToList();
                EmployeesList.ItemsSource = employees;

                if (StatusText != null)
                    StatusText.Text = $"Знайдено {employees.Count} працівників";

                // РАХУЄМО СУМУ ЗАРПЛАТ
                decimal totalSalary = 0;
                foreach (var employee in employees.Where(e => e.IsActive))
                {
                    totalSalary += employee.Salary;
                }

                // ПЕРЕВІРКА txtTotalSalary
                if (txtTotalSalary != null)
                    txtTotalSalary.Text = $"Сума зарплат: {totalSalary:N2} грн";
                else
                {
                    // Якщо txtTotalSalary немає в XAML, створюємо
                    txtTotalSalary = new System.Windows.Controls.TextBlock();
                    txtTotalSalary.Text = $"Сума зарплат: {totalSalary:N2} грн";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
                if (txtTotalSalary != null)
                    txtTotalSalary.Text = "Сума зарплат: 0.00 грн";
            }
        }

        private void SearchEmployees()
        {
            try
            {
                if (_dbContext == null) return;

                var searchText = txtSearch.Text.ToLower().Trim();

                if (string.IsNullOrWhiteSpace(searchText) ||
                    searchText == "пошук за піб, посадою, телефоном...")
                {
                    LoadEmployees();
                    return;
                }

                // Простий пошук по текстовим полям
                var employees = _dbContext.Employees
                    .Where(e => e.FullName.ToLower().Contains(searchText) ||
                               e.Position.ToLower().Contains(searchText) ||
                               e.Phone.ToLower().Contains(searchText) ||
                               e.Email.ToLower().Contains(searchText))
                    .ToList();

                EmployeesList.ItemsSource = employees;

                if (StatusText != null)
                    StatusText.Text = $"Знайдено {employees.Count} працівників за запитом '{searchText}'";

                // РАХУЄМО СУМУ ДЛЯ ЗНАЙДЕНИХ
                decimal totalSalary = 0;
                foreach (var employee in employees.Where(e => e.IsActive))
                {
                    totalSalary += employee.Salary;
                }

                if (txtTotalSalary != null)
                    txtTotalSalary.Text = $"Сума зарплат: {totalSalary:N2} грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка пошуку: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeEditDialog(null);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.Employees.Add(dialog.Employee);
                        _dbContext.SaveChanges();
                        LoadEmployees();
                        MessageBox.Show("Працівника додано!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = EmployeesList.SelectedItem as Employee;
            if (selected == null)
            {
                MessageBox.Show("Виберіть працівника", "Увага");
                return;
            }

            var dialog = new EmployeeEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.SaveChanges();
                        LoadEmployees();
                        MessageBox.Show("Дані оновлено!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = EmployeesList.SelectedItem as Employee;
            if (selected == null)
            {
                MessageBox.Show("Виберіть працівника", "Увага");
                return;
            }

            if (MessageBox.Show($"Видалити {selected.FullName}?", "Підтвердження",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.Employees.Remove(selected);
                        _dbContext.SaveChanges();
                        LoadEmployees();
                        MessageBox.Show("Працівника видалено!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SearchEmployees();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchEmployees();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
            if (txtSearch != null)
                txtSearch.Text = "Пошук за ПІБ, посадою, телефоном...";
            MessageBox.Show("Список оновлено!", "Оновлення");
        }

        private void EmployeesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedCountText != null)
                SelectedCountText.Text = $"Вибрано: {EmployeesList.SelectedItems.Count}";
        }

        private void EmployeesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
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
    }
}