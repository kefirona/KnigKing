using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ZXing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;

namespace KnigKing
{
    /// <summary>
    /// Логика взаимодействия для KorzinaWindow.xaml
    /// </summary>
    public partial class KorzinaWindow : Window
    {
        GlEntities gl = new GlEntities();
        Korzina korzina;

        public KorzinaWindow(Korzina korzina)
        {
            InitializeComponent();
            this.korzina = korzina;

            // Загружаем товары в корзине
            foreach (var zakaz in korzina.Zakazs)
            {
                var tovar = zakaz.Tovar;

                var item = new StackPanel { Orientation = Orientation.Horizontal };

                var itemText = new TextBlock { Text = $"{tovar.Name} - {zakaz.KolVoTovar} шт. - {zakaz.Sum.ToString("C")}" };
                var increaseButton = new Button { Content = "+" };
                var decreaseButton = new Button { Content = "-" };
                var deleteButton = new Button { Content = "Удалить" };

                increaseButton.Click += (s, e) => UpdateItemQuantity(zakaz, 1);
                decreaseButton.Click += (s, e) => UpdateItemQuantity(zakaz, -1);
                deleteButton.Click += (s, e) => RemoveItemFromCart(zakaz);

                item.Children.Add(itemText);
                item.Children.Add(increaseButton);
                item.Children.Add(decreaseButton);
                item.Children.Add(deleteButton);

                ItemsListBox.Items.Add(item);
            }
        }

        private void UpdateItemQuantity(Zakaz zakaz, int delta)
        {
            // Обновляем количество товара
            zakaz.KolVoTovar += delta;

            if (zakaz.KolVoTovar <= 0)
            {
                RemoveItemFromCart(zakaz);
            }
            else
            {
                zakaz.Sum = Convert.ToInt32(CalculateDiscountedPrice(zakaz.Tovar.Prise, zakaz.Tovar.Sale) * zakaz.KolVoTovar);

                gl.SaveChanges();
                RefreshItemsList();
            }
        }

        private void RemoveItemFromCart(Zakaz zakaz)
        {
            var trackedZakaz = gl.Zakazs.Local.FirstOrDefault(z => z.ID == zakaz.ID);

            if (trackedZakaz == null)
            {
                trackedZakaz = gl.Zakazs.FirstOrDefault(z => z.ID == zakaz.ID);
            }

            if (trackedZakaz != null)
            {
                // Удаляем товар из базы данных
                gl.Zakazs.Remove(trackedZakaz);
                gl.SaveChanges();  // Сохраняем изменения в базе данных
            }
            korzina.Zakazs.Remove(zakaz);
            RefreshItemsList();
        }

        private void RefreshItemsList()
        {
            // Очищаем ListBox, если это необходимо (по желанию)
            ItemsListBox.Items.Clear();

            // Добавляем обновленные элементы обратно в ListBox
            foreach (var zakaz in korzina.Zakazs)
            {
                var tovar = zakaz.Tovar;

                // Проверяем, есть ли уже элемент для этого товара в ListBox
                var existingItem = ItemsListBox.Items.Cast<StackPanel>().FirstOrDefault(item =>
                {
                    var itemText = item.Children.OfType<TextBlock>().FirstOrDefault();
                    return itemText != null && itemText.Text.Contains(tovar.Name);
                });

                if (existingItem != null)
                {
                    // Если товар уже существует в ListBox, обновляем его текст
                    var itemText = existingItem.Children.OfType<TextBlock>().First();
                    itemText.Text = $"{tovar.Name} - {zakaz.KolVoTovar} шт. - {zakaz.Sum.ToString("C")}";
                }
                else
                {
                    // Если товар не найден в ListBox, добавляем новый элемент
                    var item = new StackPanel { Orientation = Orientation.Horizontal };

                    var itemText = new TextBlock { Text = $"{tovar.Name} - {zakaz.KolVoTovar} шт. - {zakaz.Sum.ToString("C")}" };
                    var increaseButton = new Button { Content = "+" };
                    var decreaseButton = new Button { Content = "-" };
                    var deleteButton = new Button { Content = "Удалить" };

                    // При нажатии на "+" увеличиваем количество товара в корзине
                    increaseButton.Click += (s, e) => UpdateItemQuantity(zakaz, 1);
                    // При нажатии на "-" уменьшаем количество товара в корзине
                    decreaseButton.Click += (s, e) => UpdateItemQuantity(zakaz, -1);
                    // При нажатии на "Удалить" удаляем товар из корзины
                    deleteButton.Click += (s, e) => RemoveItemFromCart(zakaz);

                    item.Children.Add(itemText);
                    item.Children.Add(increaseButton);
                    item.Children.Add(decreaseButton);
                    item.Children.Add(deleteButton);

                    ItemsListBox.Items.Add(item);
                }
            }
        }
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            // Рассчитываем итоговую цену без скидки (цена товара без учета скидки)
            decimal totalPriceWithoutDiscount = korzina.Zakazs.Sum(z => z.KolVoTovar * z.Tovar.Prise);
            // Рассчитываем итоговую цену со скидкой (цена товара с учетом скидки)
            decimal totalPriceWithDiscount = korzina.Zakazs.Sum(z => z.KolVoTovar * CalculateDiscountedPrice(z.Tovar.Prise, z.Tovar.Sale));
            // Сумма скидки
            decimal totalDiscount = (totalPriceWithoutDiscount - totalPriceWithDiscount);

            // Проверяем наличие товара на складе
            bool allAvailable = korzina.Zakazs.All(z => z.Tovar.Ostatok.HasValue && z.Tovar.Ostatok.Value >= z.KolVoTovar);
            int deliveryDays = korzina.Zakazs.Count >= 3 && allAvailable ? 3 : 6;
            DateTime deliveryDate = DateTime.Now.AddDays(deliveryDays);

            foreach (var zakaz in korzina.Zakazs)
            {
                var tovar = zakaz.Tovar;

                if (tovar.Ostatok.HasValue && tovar.Ostatok.Value >= zakaz.KolVoTovar)
                {
                    // Уменьшаем остаток товара
                    tovar.Ostatok -= zakaz.KolVoTovar;
                }
                else
                {
                    MessageBox.Show($"Недостаточно товара {tovar.Name} на складе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Если недостаточно товара, прерываем оформление
                }
            }

            korzina.Sum = totalPriceWithDiscount;
            korzina.Sale = totalDiscount;
            korzina.SrokVidach = deliveryDays;
            korzina.ID_Status = 2; // Статус заказа (предполагаем, что 2 — это статус, который означает подтверждение заказа)

            // Перезагружаем объект корзины из базы данных для обновления всех данных
            var reloadedKorzina = gl.Korzinas.FirstOrDefault(k => k.ID == korzina.ID);
            if (reloadedKorzina != null)
            {
                reloadedKorzina.Sum = korzina.Sum;
                reloadedKorzina.Sale = korzina.Sale;
                reloadedKorzina.SrokVidach = korzina.SrokVidach;
                reloadedKorzina.ID_Status = korzina.ID_Status;
                // Сохраняем изменения в базе данных
                gl.SaveChanges();
            }

            // Сохраняем изменения в базе данных для каждого товара (обновленные остатки)
            gl.SaveChanges();

            MessageBox.Show($"Итоговая сумма: {totalPriceWithDiscount.ToString("C")}\nСумма скидки: {totalDiscount.ToString("C")}\nДата доставки: {deliveryDate.ToShortDateString()}", "Оформление заказа", MessageBoxButton.OK, MessageBoxImage.Information);

            // Генерация PDF с QR-кодом
            GenerateOrderPdf(korzina);

            // Переходим в главное окно
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
            decimal finalSum = korzina.Zakazs.Sum(z => z.Sum);
        }

        private void GenerateOrderPdf(Korzina korzina)
        {
            string orderInfo = GetOrderInfo(korzina); // Формируем информацию для QR-кода

            // Генерация QR-кода
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300
                }
            };
            var qrCodeImage = barcodeWriter.Write(orderInfo);

            // Создаем PDF-документ
            Document doc = new Document(PageSize.A4);
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Добавляем QR-код в PDF
                iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(qrCodeImage, System.Drawing.Imaging.ImageFormat.Png);
                qrImage.ScaleToFit(100f, 100f);
                qrImage.SetAbsolutePosition(450f, 600f);  // Устанавливаем позицию QR-кода на странице
                doc.Add(qrImage);

                doc.Close();

                // Сохраняем PDF
                File.WriteAllBytes(@" C:\Users\usersql\Desktop\Задание УП02 С#\KnigKing\bin\Debug\Order_" + korzina.ID + ".pdf", ms.ToArray());

                MessageBox.Show("PDF документ с QR-кодом был успешно создан! Можете найти его по адресу"+ " C:\\Users\\usersql\\Desktop\\Задание УП02 С#\\KnigKing\\bin\\Debug\\Order_" + korzina.ID + ".pdf", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Формируем строку с информацией о заказе для QR-кода
        private string GetOrderInfo(Korzina korzina)
        {
            return $"Data order: {DateTime.Now.ToShortDateString()}\n" +
                   $"Data dostavki: {DateTime.Now.AddDays(3).ToShortDateString()}\n" +
                   $"Number order: {korzina.ID}\n" +
                   $"Sostav order: {string.Join(", ", korzina.Zakazs.Select(z => z.Tovar.Name))}\n" +
                   $"Sum order: {korzina.Sum}\n" +
                   $"Sum sale: {korzina.Sale}\n" +
                   $"Kod poluchenija: {korzina.Kod}";
        }


        private decimal CalculateDiscountedPrice(decimal originalPrice, decimal? discount)
        {
            if (discount.HasValue && discount.Value > 0)
            {
                // Скидка в процентах
                return originalPrice * (1 - discount.Value / 100);
            }
            else
            {
                return originalPrice;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
