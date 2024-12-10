using System;
using System.Linq;
using System.Windows;

namespace KnigKing
{
    public partial class Postavka : Window
    {
        private GlEntities gl = new GlEntities();

        public Postavka()
        {
            InitializeComponent();
            LoadTovarData();
        }

        private void LoadTovarData()
        {
            var tovarList = gl.Tovars.ToList();
            TovarComboBox.ItemsSource = tovarList;
            TovarComboBox.DisplayMemberPath = "Name"; // Отображаем название товара
            TovarComboBox.SelectedValuePath = "ID"; // Используем ID товара как выбранное значение
        }

        private void PostavkaButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный товар и количество из поля ввода
            var selectedTovarId = (int)TovarComboBox.SelectedValue;
            var quantity = int.Parse(QuantityTextBox.Text);

            // Находим товар в базе данных
            var tovar = gl.Tovars.SingleOrDefault(t => t.ID == selectedTovarId);

            if (tovar != null)
            {
                // Обновляем остаток товара
                tovar.Ostatok = (tovar.Ostatok ?? 0) + quantity;
                if (tovar.Ostatok > 0)
                {
                    tovar.ID_Status = 2; // статус "На складе" 
                }
                try
                {
                    gl.SaveChanges();
                    MessageBox.Show("Поставка товара успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите товар!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OstatokWindow ostatokWindow = new OstatokWindow();
            ostatokWindow.Show();
            this.Close();
        }
    }
}
