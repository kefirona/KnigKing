using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KnigKing
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        public ManagerWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow n = new MainWindow();    
            n.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RedachZakaz redachZakaz = new RedachZakaz();
            redachZakaz.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OstatokWindow ostatok = new OstatokWindow();
            ostatok.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DobavTovar dobavTovar = new DobavTovar();
            dobavTovar.Show();
        }
    }
}
