using System.Windows;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;
using System.Linq;

namespace PoltavaPromTehGaz.Views
{
    public partial class EmployeesWindow : Window
    {
        private readonly AppDbContext _dbContext;

        public EmployeesWindow(AppDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                var employees = _dbContext.Employees.ToList();
                EmployeesGrid.ItemsSource = employees;
                StatusText.Text = $"Знайдено {employees.Count} працівників";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Помилка завантаження";
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка");
            }
        }
    }
}