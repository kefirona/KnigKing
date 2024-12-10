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
    /// Логика взаимодействия для DobavTovar.xaml
    /// </summary>
    public partial class DobavTovar : Window
    {
        GlEntities gl = new GlEntities();
        public DobavTovar()
        {
            InitializeComponent();

            // Заполняем ComboBox производителями
            autor.ItemsSource = gl.Proizvoditels.ToList();
            autor.DisplayMemberPath = "Name"; // Замените на имя поля, которое вы хотите показывать в ComboBox
            autor.SelectedValuePath = "ID";  // Это будет значение, которое будет использоваться в коде
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Name = nameTextBox.Text;
            string Opis = descriptionTextBlock.Text;

            // Проверка на пустые значения
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Opis) || string.IsNullOrEmpty(priceTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            decimal Prise;
            if (!decimal.TryParse(priceTextBox.Text, out Prise))
            {
                MessageBox.Show("Пожалуйста, введите корректную цену.");
                return;
            }

            // Получаем выбранного производителя из ComboBox
            var selectedProizvoditel = (Proizvoditel)autor.SelectedItem;
            if (selectedProizvoditel == null)
            {
                MessageBox.Show("Пожалуйста, выберите производителя.");
                return;
            }

            // Загружаем путь к изображению с использованием метода GetFullImagePath
            string imagePath = GetFullImagePath(image.Source != null ? image.Source.ToString() : string.Empty);

            // Создаем новый товар
            var newTovar = new Tovar
            {
                Name = Name,
                Opisanie = Opis,
                Prise = Prise,
                ID_Status = 3,  // предполагаем, что статус 3 означает "в наличии"
                Ostatok = 0,  // количество товара
                ID_Proizvoditel = selectedProizvoditel.ID, // Устанавливаем производителя
                Image = imagePath  // Используем полученный путь к изображению
            };

            // Добавляем товар в базу данных
            gl.Tovars.Add(newTovar);
            gl.SaveChanges(); // Сохраняем изменения в базе данных

            MessageBox.Show("Товар успешно добавлен.");
            this.Close(); // Закрываем окно после добавления товара
        }

        private string GetFullImagePath(string imagePath)
        {
            if (System.IO.File.Exists(imagePath))
            {
                return imagePath;  // Если файл существует, возвращаем полный путь к файлу
            }
            else
            {
                return "C:\\Users\\usersql\\Pictures\\дфьи.png";  // Укажите путь к изображению по умолчанию, если файл не найден
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора изображения
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";  // фильтр для изображений

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                // Устанавливаем путь к изображению
                string imagePath = dlg.FileName;
                image.Source = new BitmapImage(new Uri(imagePath)); // Загружаем картинку в Image
            }
        }
    }
}
