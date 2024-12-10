using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System;
using System.Data.Entity;  // Для метода Include

namespace KnigKing
{
    public partial class RedachZakaz : Window
    {
        private GlEntities gl = new GlEntities();
        public ObservableCollection<Zakaz> ZakazList { get; set; }

        public RedachZakaz()
        {
            InitializeComponent();
            LoadZakazData();
        }

        private void LoadZakazData()
        {
            // Используем Include для загрузки связанных объектов (Polz и StatusZakaz)
            var zakazData = gl.Zakazs
                .Include(z => z.Korzina)  // Загрузка связанной корзины
                .Include(z => z.Tovar)    // Загрузка связанного товара
                .Include(z => z.Korzina.Polz)        // Загрузка пользователя
                .Include(z => z.Korzina.StatusZakaz) // Загрузка статуса заказа
                .ToList();

            ZakazList = new ObservableCollection<Zakaz>(zakazData);
            ZakazDataGrid.DataContext = this;
        }

        private void SaveChanges()
        {
            // Проверяем каждую запись в коллекции ZakazList на изменения
            foreach (var item in ZakazList)
            {
                gl.Entry(item).State = System.Data.Entity.EntityState.Modified;
            }

            // Сохраняем изменения в базу данных
            try
            {
                gl.SaveChanges();
                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveChanges();
        }
    }
}
