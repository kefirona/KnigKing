using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KnigKing
{
    public partial class MainWindow : Window
    {
        private GlEntities gl = new GlEntities();

        public MainWindow()
        {
            InitializeComponent();

            // Загрузка товаров из базы данных
            LoadTovars();
            UpdateCartButtonVisibility();
        }
        private void UpdateCartButtonVisibility()
        {
            // Проверяем, есть ли хотя бы один товар в корзине
            var userCart = gl.Korzinas.FirstOrDefault(k => k.ID_Polz == CurrentUser.UserId && k.ID_Status == 1);

            if (userCart != null && userCart.Zakazs.Any())
            {
                // Показываем кнопку "Корзина", если она еще не видна
                if (KorzinaButton.Visibility != Visibility.Visible)
                {
                    KorzinaButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Скрываем кнопку "Корзина", если товаров нет
                if (KorzinaButton.Visibility != Visibility.Hidden)
                {
                    KorzinaButton.Visibility = Visibility.Hidden;
                }
            }
        }
        private void LoadTovars()
        {
            try
            {
                var tovars = gl.Tovars
                    .Where(t => t.ID_Status == 2 || t.ID_Status == 3)
                    .OrderBy(t => t.Name) // Сортируем товары по алфавиту
                    .ToList();

                // Создаем Canvas для каждого товара
                foreach (var tovar in tovars)
                {
                    AddTovarToUI(tovar);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
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

        private void AddTovarToUI(Tovar tovar)
        {
            // Создаем Canvas для товара
            Canvas tovarCanvas = new Canvas
            {
                Margin = new Thickness(0, 10, 0, 0),
                Width = 497,
                Height = 79,
                Background = Brushes.White
            };
            tovarCanvas.SetValue(BorderBrushProperty, Brushes.DarkGreen);

            // Добавляем изображение товара
            string imagePath = GetFullImagePath(tovar.Image);
            Image image = new Image
            {
                Width = 62,
                Height = 53,
                Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute))
            };

            Canvas.SetLeft(image, 10);
            Canvas.SetTop(image, 8);
            tovarCanvas.Children.Add(image);

            // Добавляем название товара
            Label nameLabel = new Label
            {
                Content = tovar.Name,
                Width = 225
            };
            Canvas.SetLeft(nameLabel, 82);
            Canvas.SetTop(nameLabel, 8);
            tovarCanvas.Children.Add(nameLabel);

            // Добавляем автора товара
            Label authorLabel = new Label
            {
                Content = tovar.Proizvoditel?.Name ?? "Не указан",
                Width = 225
            };
            Canvas.SetLeft(authorLabel, 82);
            Canvas.SetTop(authorLabel, 40);
            tovarCanvas.Children.Add(authorLabel);

            // Добавляем метку для "Цена"
            Label priceLabel = new Label
            {
                Content = "Цена",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Canvas.SetLeft(priceLabel, 400);
            Canvas.SetTop(priceLabel, 8);
            tovarCanvas.Children.Add(priceLabel);

            // Добавляем цену товара с учетом скидки
            Label priceValueLabel = new Label
            {
                Content = CalculateDiscountedPrice(tovar.Prise, tovar.Sale),
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Canvas.SetLeft(priceValueLabel, 400);
            Canvas.SetTop(priceValueLabel, 40);
            tovarCanvas.Children.Add(priceValueLabel);

            // Обработчик левого клика (открытие подробной информации о товаре)
            tovarCanvas.MouseLeftButtonUp += (sender, e) =>
            {
                TovarMnogo tovarMnogoWindow = new TovarMnogo(tovar);
                tovarMnogoWindow.Show();
            };

            // Обработчик ПКМ (добавление товара в корзину)
            tovarCanvas.MouseRightButtonUp += (sender, e) =>
            {
                AddToCart(tovar); // Вызываем метод добавления в корзину
            };

            // Добавление выделения при наведении
            tovarCanvas.MouseEnter += (sender, e) =>
            {
                tovarCanvas.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                tovarCanvas.Cursor = Cursors.Hand;
            };

            tovarCanvas.MouseLeave += (sender, e) =>
            {
                tovarCanvas.Background = Brushes.White;
                tovarCanvas.Cursor = Cursors.Arrow;
            };

            // Добавляем Canvas в контейнер на экране
            TovarsContainer.Children.Add(tovarCanvas);
        }
        private int GenerateRandomCode()
        {
            Random random = new Random();
            // Генерируем случайное трехзначное число (от 100 до 999)
            return random.Next(100, 1000);  // Возвращаем как int
        }
        private void AddToCart(Tovar tovar)
        {
            if (tovar == null)
            {
                MessageBox.Show("Ошибка: товар не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем текущего пользователя
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
                    Sum = 0, // Начальная сумма
                    Sale = 0, // Начальная скидка
                    SrokVidach = 0, // Срок не определен
                    Kod = GenerateRandomCode(), // Генерируем случайный код
                    Zakazs = new List<Zakaz>() // Инициализация коллекции Zakazs
                };
                gl.Korzinas.Add(korzina);
                gl.SaveChanges(); // Сохраняем корзину в базе данных
            }

            // Проверяем, есть ли уже этот товар в корзине
            var existingZakaz = korzina.Zakazs.FirstOrDefault(z => z.ID_Tovar == tovar.ID);

            // Получаем цену с учетом скидки
            decimal discountedPrice = CalculateDiscountedPrice(tovar.Prise, tovar.Sale);

            // Приводим цену к целому числу
            int discountedPriceAsInt = Convert.ToInt32(Math.Round(discountedPrice));

            if (existingZakaz != null)
            {
                // Если товар уже есть в корзине, увеличиваем его количество и пересчитываем сумму
                existingZakaz.KolVoTovar += 1;
                existingZakaz.Sum = existingZakaz.KolVoTovar * discountedPriceAsInt;
            }
            else
            {
                // Если товара нет в корзине, добавляем его как новый заказ
                var zakaz = new Zakaz
                {
                    ID_Tovar = tovar.ID, // ID товара
                    ID_Korzina = korzina.ID, // ID корзины
                    KolVoTovar = 1, // Количество товара
                    Sum = discountedPriceAsInt // Сумма с учетом скидки
                };

                // Добавляем заказ в корзину
                korzina.Zakazs.Add(zakaz);
            }

            // Сохраняем изменения в базе данных
            gl.SaveChanges();

            // Обновляем видимость кнопки "Корзина"
            UpdateCartButtonVisibility();

            MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Очищаем текущие товары на UI
            TovarsContainer.Children.Clear();

            // Получаем текст для поиска
            string searchText = searchTextBox.Text.ToLower();

            // Загружаем товары, фильтруем их по введенному тексту в поисковом поле
            var filteredTovars = gl.Tovars
                .Where(t => (t.Name.ToLower().Contains(searchText)) && (t.ID_Status == 2 || t.ID_Status == 3)) // Фильтруем по названию
                .OrderBy(t => t.Name) // Сортируем по алфавиту
                .ToList();

            // Добавляем товары на UI
            foreach (var tovar in filteredTovars)
            {
                AddTovarToUI(tovar);
            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Autoriz autoriz = new Autoriz();
            autoriz.Show(); 
            this.Close();
        }
        public decimal CalculateDiscountedPrice(decimal originalPrice, decimal? discount)
        {
            if (discount.HasValue && discount.Value > 0)
            {
                // Возвращаем цену с учетом скидки как decimal
                return originalPrice * (1 - discount.Value / 100);
            }
            else
            {
                // Если скидки нет, возвращаем исходную цену
                return originalPrice;
            }
        }

        private void KorzinaButton_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Ваша корзина пуста!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Открываем окно с корзиной и передаем туда данные
            KorzinaWindow korzinaWindow = new KorzinaWindow(korzina);
            korzinaWindow.Show();
            this.Close();
        }
    }
}
