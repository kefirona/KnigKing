using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace KnigKing
{
    public partial class RedachTovarWindow : Window
    {
        private GlEntities gl = new GlEntities();
        public ObservableCollection<Tovar> TovarList { get; set; }

        public RedachTovarWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            TovarList = new ObservableCollection<Tovar>(gl.Tovars.Where(t => t.ID_Status == 2 || t.ID_Status == 3).ToList());
            TovarDataGrid.ItemsSource = TovarList; 
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var tovar = (Tovar)((Button)sender).DataContext;
            // Изменяем статус товара на удалено
            tovar.ID_Status = 1;
            gl.SaveChanges();
            LoadData();
            MessageBox.Show("Товар был удален.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DobavTovar dobavTovar = new DobavTovar();
            dobavTovar.Show();
            this.Close();
        }
    }
}
