using System;
using System.Windows;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz
{
    public partial class CylinderEditDialog : Window
    {
        public Cylinder Cylinder { get; private set; }

        public CylinderEditDialog(Cylinder? cylinder = null)
        {
            InitializeComponent();
            Cylinder = cylinder ?? new Cylinder();
            LoadData();
        }

        private void LoadData()
        {
            txtSerialNumber.Text = Cylinder.SerialNumber;
            txtPurchasePrice.Text = Cylinder.PurchasePrice.ToString("0.00");
            txtCurrentValue.Text = Cylinder.CurrentValue.ToString("0.00");
            txtNotes.Text = Cylinder.Notes ?? "";

            // Встановлюємо вибраний тип газу
            if (!string.IsNullOrEmpty(Cylinder.GasType))
            {
                foreach (var item in cmbGasType.Items)
                {
                    if (item is System.Windows.Controls.ComboBoxItem comboItem &&
                        comboItem.Content.ToString() == Cylinder.GasType)
                    {
                        cmbGasType.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                cmbGasType.SelectedIndex = 0;
            }

            // Статуси
            cmbStatus.Items.Clear();
            foreach (CylinderStatus status in Enum.GetValues(typeof(CylinderStatus)))
                cmbStatus.Items.Add(status);
            cmbStatus.SelectedItem = Cylinder.Status;

            // Розташування
            cmbLocation.Items.Clear();
            foreach (CylinderLocation location in Enum.GetValues(typeof(CylinderLocation)))
                cmbLocation.Items.Add(location);
            cmbLocation.SelectedItem = Cylinder.Location;

            // Дати
            dpManufactureDate.SelectedDate = Cylinder.ManufactureDate;
            dpLastCheckDate.SelectedDate = Cylinder.LastCheckDate;
            dpNextCheckDate.SelectedDate = Cylinder.NextCheckDate;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSerialNumber.Text))
            {
                MessageBox.Show("Введіть серійний номер", "Помилка");
                return;
            }

            if (cmbGasType.SelectedItem == null)
            {
                MessageBox.Show("Виберіть тип газу", "Помилка");
                return;
            }

            // ПЕРЕВІРКА ВАРТОСТІ
            if (!decimal.TryParse(txtCurrentValue.Text, out decimal currentValue))
            {
                MessageBox.Show("Введіть число у вартість (напр. 1500.50)", "Помилка");
                txtCurrentValue.Focus();
                txtCurrentValue.SelectAll();
                return;
            }

            // ПЕРЕВІРКА ЦІНИ
            if (!decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice))
            {
                MessageBox.Show("Введіть число у ціну придбання", "Помилка");
                txtPurchasePrice.Focus();
                txtPurchasePrice.SelectAll();
                return;
            }

            // Отримуємо вибраний тип газу
            var selectedItem = cmbGasType.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedItem != null)
            {
                Cylinder.GasType = selectedItem.Content.ToString() ?? "Аргон"; // Додали ?? для nullable
            }
            else
            {
                Cylinder.GasType = "Аргон";
            }

            Cylinder.SerialNumber = txtSerialNumber.Text;

            if (cmbStatus.SelectedItem is CylinderStatus status)
                Cylinder.Status = status;

            if (cmbLocation.SelectedItem is CylinderLocation location)
                Cylinder.Location = location;

            Cylinder.ManufactureDate = dpManufactureDate.SelectedDate ?? DateTime.Now.AddYears(-1);
            Cylinder.LastCheckDate = dpLastCheckDate.SelectedDate ?? DateTime.Now;
            Cylinder.NextCheckDate = dpNextCheckDate.SelectedDate ?? DateTime.Now.AddYears(1);

            Cylinder.PurchasePrice = purchasePrice;
            Cylinder.CurrentValue = currentValue;
            Cylinder.Notes = txtNotes.Text ?? string.Empty;

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