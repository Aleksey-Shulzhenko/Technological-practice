using System;
using System.Windows;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz
{
    public partial class EmployeeEditDialog : Window
    {
        public Employee Employee { get; private set; }

        public EmployeeEditDialog(Employee? employee = null)
        {
            InitializeComponent();
            Employee = employee ?? new Employee();
            LoadData();
        }

        private void LoadData()
        {
            txtFullName.Text = Employee.FullName;
            txtPosition.Text = Employee.Position;
            dpHireDate.SelectedDate = Employee.HireDate;
            txtSalary.Text = Employee.Salary.ToString("0.00");
            txtPhone.Text = Employee.Phone ?? "";
            txtEmail.Text = Employee.Email ?? "";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введіть ПІБ працівника", "Помилка");
                return;
            }

            Employee.FullName = txtFullName.Text;
            Employee.Position = txtPosition.Text;
            Employee.HireDate = dpHireDate.SelectedDate ?? DateTime.Now;

            if (decimal.TryParse(txtSalary.Text, out decimal salary))
                Employee.Salary = salary;

            Employee.Phone = txtPhone.Text;
            Employee.Email = txtEmail.Text;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}