using System;
using System.Linq;
using System.Windows;
using PoltavaPromTehGaz.Data;
using PoltavaPromTehGaz.Models;

namespace PoltavaPromTehGaz
{
    public partial class CylindersWindow : Window
    {
        private AppDbContext? _dbContext;

        public CylindersWindow()
        {
            InitializeComponent();
            try
            {
                _dbContext = new AppDbContext();
                LoadCylinders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації: {ex.Message}");
            }
        }

        private void LoadCylinders()
        {
            try
            {
                if (_dbContext == null) return;

                var cylinders = _dbContext.Cylinders.ToList();
                CylindersList.ItemsSource = cylinders;

                if (StatusText != null)
                    StatusText.Text = $"Знайдено {cylinders.Count} балонів";

                decimal total = 0;
                foreach (var cylinder in cylinders)
                {
                    total += cylinder.CurrentValue;
                }

                if (txtTotalValue != null)
                    txtTotalValue.Text = $"Загальна вартість: {total:N2} грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
                if (txtTotalValue != null)
                    txtTotalValue.Text = "Загальна вартість: 0.00 грн";
            }
        }

        private void SearchCylinders()
        {
            try
            {
                if (_dbContext == null) return;

                var searchText = txtSearch.Text.ToLower().Trim();

                if (string.IsNullOrWhiteSpace(searchText) ||
                    searchText == "пошук за номером, типом газу...")
                {
                    LoadCylinders();
                    return;
                }

                var allCylinders = _dbContext.Cylinders.ToList();

                var cylinders = allCylinders
                    .Where(c => c.SerialNumber.ToLower().Contains(searchText) ||
                               c.GasType.ToLower().Contains(searchText) ||
                               (c.Notes != null && c.Notes.ToLower().Contains(searchText)))
                    .ToList();

                cylinders = cylinders
                    .Where(c =>
                        c.SerialNumber.ToLower().Contains(searchText) ||
                        c.GasType.ToLower().Contains(searchText) ||
                        (c.Notes != null && c.Notes.ToLower().Contains(searchText)) ||
                        (c.Status == CylinderStatus.Повний && "повний".Contains(searchText)) ||
                        (c.Status == CylinderStatus.Порожній && "порожній".Contains(searchText)) ||
                        (c.Status == CylinderStatus.В_ремонті && "в_ремонті".Contains(searchText) || "в ремонті".Contains(searchText)) ||
                        (c.Status == CylinderStatus.Списаний && "списаний".Contains(searchText)) ||
                        (c.Location == CylinderLocation.Склад && "склад".Contains(searchText)) ||
                        (c.Location == CylinderLocation.В_дорозі && "в_дорозі".Contains(searchText) || "в дорозі".Contains(searchText)) ||
                        (c.Location == CylinderLocation.У_клієнта && "у_клієнта".Contains(searchText) || "у клієнта".Contains(searchText)) ||
                        (c.Location == CylinderLocation.На_заправці && "на_заправці".Contains(searchText) || "на заправці".Contains(searchText)) ||
                        (c.Location == CylinderLocation.На_обміні && "на_обміні".Contains(searchText) || "на обміні".Contains(searchText)))
                    .ToList();

                CylindersList.ItemsSource = cylinders;

                if (StatusText != null)
                    StatusText.Text = $"Знайдено {cylinders.Count} балонів за запитом '{searchText}'";

                decimal total = 0;
                foreach (var cylinder in cylinders)
                {
                    total += cylinder.CurrentValue;
                }

                if (txtTotalValue != null)
                    txtTotalValue.Text = $"Загальна вартість: {total:N2} грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка пошуку: {ex.Message}");
            }
        }

        private void BtnAddCylinder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CylinderEditDialog(null);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.Cylinders.Add(dialog.Cylinder);
                        _dbContext.SaveChanges();
                        LoadCylinders();
                        MessageBox.Show("Балон додано успішно!", "Успіх");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void BtnEditCylinder_Click(object sender, RoutedEventArgs e)
        {
            var selected = CylindersList.SelectedItem as Cylinder;
            if (selected == null)
            {
                MessageBox.Show("Виберіть балон", "Увага");
                return;
            }

            var dialog = new CylinderEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.SaveChanges();
                        LoadCylinders();
                        MessageBox.Show("Дані оновлено!", "Успіх");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void BtnDeleteCylinder_Click(object sender, RoutedEventArgs e)
        {
            var selected = CylindersList.SelectedItem as Cylinder;
            if (selected == null)
            {
                MessageBox.Show("Виберіть балон", "Увага");
                return;
            }

            if (MessageBox.Show($"Видалити балон {selected.SerialNumber}?", "Підтвердження",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    if (_dbContext != null)
                    {
                        _dbContext.Cylinders.Remove(selected);
                        _dbContext.SaveChanges();
                        LoadCylinders();
                        MessageBox.Show("Балон видалено!", "Успіх");
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
            SearchCylinders();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCylinders();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCylinders();
            if (txtSearch != null)
                txtSearch.Text = "Пошук за номером, типом газу...";
            MessageBox.Show("Список оновлено!", "Оновлення");
        }

        private void CylindersList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedCountText != null)
                SelectedCountText.Text = $"Вибрано: {CylindersList.SelectedItems.Count}";
        }

        private void CylindersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEditCylinder_Click(sender, e);
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