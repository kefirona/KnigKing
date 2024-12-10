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
    /// Логика взаимодействия для TovarMnogo.xaml
    /// </summary>
    public partial class TovarMnogo : Window
    {
        GlEntities gl = new GlEntities();
        private Tovar tovar;  // Добавляем поле для хранения товара
        public TovarMnogo(Tovar tovar)
        {
            InitializeComponent();
            // Заполняем данные о товаре
            image.Source = new BitmapImage(new Uri(GetFullImagePath(tovar.Image), UriKind.Absolute));
            nameTextBox.Text = tovar.Name;
            authorTextBox.Text = tovar.Proizvoditel?.Name ?? "Не указан";
            descriptionTextBlock.Text = tovar.Opisanie;
            priceTextBox.Text = tovar.Prise.ToString("C");
            saleTextBox.Text = tovar.Sale.HasValue ? (tovar.Sale.Value / 100.0).ToString("0%") : "Нет скидки";

            // Блокируем возможность изменения
            nameTextBox.IsReadOnly = true;
            authorTextBox.IsReadOnly = true;
            priceTextBox.IsReadOnly = true;
            saleTextBox.IsReadOnly = true;
            descriptionTextBlock.IsHitTestVisible = false; // Запрещаем взаимодействие с TextBlock
        }
        private string GetFullImagePath(string imagePath)
        {
            // Проверка существования файла
            if (System.IO.File.Exists(imagePath))
            {
                return imagePath;
            }
            else
            {
                // Возвращаем путь к изображению по умолчанию, если файл не найден
                return "path_to_default_image.png";
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = gl.Polzs.FirstOrDefault(u => u.ID == CurrentUser.UserId);
            if (currentUser == null)
            {
                MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем корзину текущего пользователя
            var korzina = currentUser.Korzinas.FirstOrDefault(k => k.ID_Status == 1); // Корзина в статусе "в процессе"
            if (korzina == null)
            {
                // Если корзина не найдена, создаем новую корзину
                korzina = new Korzina
                {
                    ID_Polz = currentUser.ID,
                    DateZakaz = DateTime.Now,
                    ID_Status = 1, // Статус "в процессе"
                    Sum = 0, 
                    Sale = 0, 
                    SrokVidach = 0, 
                    Kod = GenerateRandomCode() 
                };
                gl.Korzinas.Add(korzina);
                gl.SaveChanges(); // Сохраняем корзину в базе данных
            }
            var existingZakaz = korzina.Zakazs.FirstOrDefault(z => z.ID_Tovar == tovar.ID);

            if (existingZakaz != null)
            {
                existingZakaz.KolVoTovar += 1;
                decimal discountedPrice = CalculateDiscountedPrice(tovar.Prise, tovar.Sale);
                existingZakaz.Sum = (int)(discountedPrice * existingZakaz.KolVoTovar); 
                gl.SaveChanges();

                MessageBox.Show("Количество товара в корзине увеличено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Если товара нет в корзине, добавляем его
                decimal discountedPrice = CalculateDiscountedPrice(tovar.Prise, tovar.Sale);
                int sum = (int)discountedPrice;

                var zakaz = new Zakaz
                {
                    ID_Tovar = tovar.ID,
                    ID_Korzina = korzina.ID,
                    KolVoTovar = 1, // Начинаем с 1
                    Sum = sum
                };

                // Добавляем заказ в корзину
                korzina.Zakazs.Add(zakaz);
                gl.SaveChanges(); // Сохраняем изменения в базе данных

                MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private int GenerateRandomCode()
        {
            Random random = new Random();
            // Генерируем случайное трехзначное число (от 100 до 999)
            return random.Next(100, 1000);  // Возвращаем как int
        }
        private decimal CalculateDiscountedPrice(decimal originalPrice, decimal? discount)
        {
            if (discount.HasValue && discount.Value > 0)
            {
                // Скидка в процентах, например, скидка 50% означает, что discount = 50
                decimal discountedPrice = originalPrice * (1 - discount.Value / 100);
                return discountedPrice; // Возвращаем цену с учетом скидки в виде decimal
            }
            else
            {
                // Если скидки нет, возвращаем исходную цену
                return originalPrice;
            }
        }
    }
}
