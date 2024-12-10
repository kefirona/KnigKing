using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Data.Entity;  // Для метода Include

namespace KnigKing
{
    public partial class OstatokWindow : Window
    {
        private GlEntities gl = new GlEntities();
        public ObservableCollection<Tovar> TovarList { get; set; }

        public OstatokWindow()
        {
            InitializeComponent();
            LoadTovarData();
        }

        private void LoadTovarData()
        {
            // Загрузка данных о товарах с остатками, включая статус
            var tovarData = gl.Tovars
                .Include(t => t.StatusTovara) // Загружаем статус товара
                .Where(t => t.Ostatok.HasValue) // Только товары с остатками
                .ToList();

            // Преобразуем данные в ObservableCollection для привязки
            TovarList = new ObservableCollection<Tovar>(tovarData);
            // Устанавливаем привязку для DataGrid
            ZakazDataGrid.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Postavka postavka = new Postavka();
            postavka.Show();
            this.Close();
        }
    }
}
