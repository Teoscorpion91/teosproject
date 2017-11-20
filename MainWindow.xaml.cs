using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace merchandise
{
    public class Product
    {
        public string Title { get; set; }
        public int Number { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public string Placement { get; set; }
        public int NumPrice => Number * Price - Discount * Number;
        public bool IsChecked { get; set; }
    }

    public partial class MainWindow : Window
    {
        
        #region 1. Основные переменные
        int cupon_all = 0, cupon_item = 0; // Купоны на весь товар, или единичный товар
        int pushcare_item = 0; // Количество вещей в корзине.
        int interact = 10; // Переменная для таймера (для интерактивного взаимодействия)
        string[] cupons; // Массив для чтения количества купонов из файла

        int[] count = new int[9999]; int step = 0; // Инициация массива и переменной для UNDO / REDO
        string[] names = new string[9999]; // Инициация массива для наименований для UNDO / REDO
        int[] pushcar = new int[9999]; // Массив для товара (на тот случай если мы хотим изменить кол-во позиций)

        Random random = new Random();

        List<Product> products; // Для списка продукции
        
        // Эффект приглушения для кнопок (в выборе товара)
        private void Merch1_MouseEnter(object sender, MouseEventArgs e) { Merch1.Opacity = 0.8; }
        private void Merch1_MouseLeave(object sender, MouseEventArgs e) { Merch1.Opacity = 0.2; }
        private void Merch2_MouseEnter(object sender, MouseEventArgs e) { Merch2.Opacity = 0.8; }
        private void Merch2_MouseLeave(object sender, MouseEventArgs e) { Merch2.Opacity = 0.2; }
        private void Merch3_MouseEnter(object sender, MouseEventArgs e) { Merch3.Opacity = 0.8; }
        private void Merch3_MouseLeave(object sender, MouseEventArgs e) { Merch3.Opacity = 0.2; }
        private void Merch4_MouseEnter(object sender, MouseEventArgs e) { Merch4.Opacity = 0.8; }
        private void Merch4_MouseLeave(object sender, MouseEventArgs e) { Merch4.Opacity = 0.2; }
        private void Merch5_MouseEnter(object sender, MouseEventArgs e) { Merch5.Opacity = 0.8; }
        private void Merch5_MouseLeave(object sender, MouseEventArgs e) { Merch5.Opacity = 0.2; }
        private void Merch6_MouseEnter(object sender, MouseEventArgs e) { Merch6.Opacity = 0.8; }
        private void Merch6_MouseLeave(object sender, MouseEventArgs e) { Merch6.Opacity = 0.2; }
        private void Merch7_MouseEnter(object sender, MouseEventArgs e) { Merch7.Opacity = 0.8; }
        private void Merch7_MouseLeave(object sender, MouseEventArgs e) { Merch7.Opacity = 0.2; }
        private void Merch8_MouseEnter(object sender, MouseEventArgs e) { Merch8.Opacity = 0.8; }
        private void Merch8_MouseLeave(object sender, MouseEventArgs e) { Merch8.Opacity = 0.2; }
        #endregion

        #region 2. Загрузка главного окна
        // Загрузка главного окна
        public MainWindow()
        {
            InitializeComponent();

            GB1_Message.Text = "Добро пожаловать в магазин товаров от Adobe";
            GB1_Message.Foreground = Brushes.Black;

            // Коллекция продуктов.
            products = new List<Product> {};

            LW.ItemsSource = products; // Привязываем к окну нашу коллекцию.

            // Запуск таймера для интерактивного взаимодействия с пользователем
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            // Две группы объектов, два из которых изначально скрываем.
            GR_2.Visibility = Visibility.Hidden;

            for (int i = 0; i < 10; i++)
            { pushcar[i] = 0; } // При запуске обнуляем количество товара.

            try
            {   // Загрузка изображений для Undo и Redo
                IMG_Undo.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/undo.png", UriKind.Absolute));
                IMG_Redo.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/redo.png", UriKind.Absolute));
                // Загрузка изображений для магазина
                Merch1.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_1.png", UriKind.Absolute));
                Merch2.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_2.png", UriKind.Absolute));
                Merch3.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_3.png", UriKind.Absolute));
                Merch4.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_4.png", UriKind.Absolute));
                Merch5.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_5.png", UriKind.Absolute));
                Merch6.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_6.png", UriKind.Absolute));
                Merch7.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_7.png", UriKind.Absolute));
                Merch8.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/merch_8.png", UriKind.Absolute));
            } catch {}

            try
            {   // Подсчет общего количества купонов из файла
                cupons = File.ReadAllLines("cupons.txt");
                cupon_all = Convert.ToInt32(cupons[1]);
                cupon_item = Convert.ToInt32(cupons[0]);

                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
            } catch { }

            AllSum.Text = "0";
        }
        #endregion

        #region 3. Таймер для интерактива
        // Таймер для активного взаимодействия с пользователем (чтобы приложение не выглядело топорно).
        private void timerTick(object sender, EventArgs e)
        {
            if (interact == 8)
            {
                chb_all_check = 0;
            }
            if (interact != 0)
            {
                interact--;
            }
            else
            {
                // Эта часть кода направлена на интерактивное взаимодействие. Её можно было упростить 
                // заставив читать специальные фразы из .txt, но я решил не усложнять себе задачу.
                int a = random.Next(5);
                switch (a)
                {
                    case 0:
                        GB1_Message.Text = "На Adobe Photoshop скидка в 50%! Спешите!";
                        GB2_Message.Text = "Обращаем внимание что скидочный купон на единичный товар дает вам 10% скидки";
                        break;
                    case 1:
                        GB1_Message.Text = "Совершая постоянные покупки вы можете приобрести скидочные купоны";
                        GB2_Message.Text = "Если у вас есть скидочный купон на весь товар вы можете получить 10% скидки на всё что выберете";
                        break;
                    case 2:
                        GB1_Message.Text = "Нашим товаром пользуется более 85% пользователей ПК";
                        GB2_Message.Text = "Вы можете сохранить набор продуктов чтобы совершить покупку позднее";
                        break;
                    case 3:
                        GB1_Message.Text = "Для организаций у нас есть особые условия скидок";
                        GB2_Message.Text = "Будьте внимательны при выборе товара, выбирайте то, что вам нужно";
                        break;
                    case 4:
                        GB1_Message.Text = "С каждым годом мы становимся совершенней и универсальней";
                        GB2_Message.Text = "На Adobe Photoshop по умолчанию скидка 50%, купон на него не подействует";
                        break;
                    case 5:
                        GB1_Message.Text = "Следить за обновлениями можно на нашем сайте";
                        GB2_Message.Text = "Если вы завершили свой выбор, можете нажать печать чека и оплатить товар";
                        break;
                    case 6:
                        GB1_Message.Text = "Наша продукция - выбор множества пользователей";
                        GB2_Message.Text = "Для оплаты товара нужно нажать \"Печать чека\"";
                        break;
                    case 7:
                        GB1_Message.Text = "Вы можете отменить своё действие нажав Ctrl+z и вернуть его нажав Ctrl+r";
                        GB2_Message.Text = "К сожалению не весь товар доступен в данный момент";
                        break;
                    case 8:
                        GB1_Message.Text = "Если вам сложно сориентироваться в приложении прочтите справку (нажмите F1)";
                        GB2_Message.Text = "Вы можете загрузить ваш предыдущий выбор если вы сохранили его ранее";
                        break;
                    case 9:
                        GB1_Message.Text = "Вы можете присоединится к нашей компании, подробная информация на нашем сайте";
                        GB2_Message.Text = "Если у вас возникают проблемы с приложением прочтите справку (нажмите F1)";
                        break;
                    default:
                        break;
                }
                AllSumCount();
                interact = 10;
                GB1_Message.Foreground = Brushes.DarkBlue;
            }
        }
        #endregion

        #region 4. Выбор товара по иконкам
        private void Merch1_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Photoshop
            interact = 10;
            count[step] = 1;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Photoshop\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[1]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch2_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка товара который не доступен (не будет куплен).
            interact = 10;
            // count[step] = 2;
            // step++; // Перекрываю в связи с отсутствием товара

            interact = 10;
            GB1_Message.Foreground = Brushes.DarkRed;
            GB1_Message.Text = "К сожалению, данный товар временно отсутствует";

            // pushcar[2]++;
            // Car_Change();
        }

        private void Merch3_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Illustrator
            interact = 10;
            count[step] = 3;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Illustrator\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[3]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch4_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Dreamviewer
            interact = 10;
            count[step] = 4;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Dreamviewer\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[4]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch5_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Audition
            interact = 10;
            count[step] = 5;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Audition\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[5]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch6_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Bridge
            interact = 10;
            count[step] = 6;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Bridge\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[6]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch7_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Builder
            interact = 10;
            count[step] = 7;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Builder\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[7]++;
            Car_Change();

            count[step + 1] = 0;
        }

        private void Merch8_MouseDown(object sender, MouseButtonEventArgs e)
        { // Покупка Adobe Lightroom
            interact = 10;
            count[step] = 8;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB1_Message.Text = "Товар \"Adobe Lightroom\" был положен в корзину";

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";

            pushcar[8]++;
            Car_Change();

            count[step + 1] = 0;
        }

        // Изменения происходящие в корзине при нажатии на товар.
        public void Car_Change()
        {
            interact = 10;
            switch (count[step - 1])
            {
                case 1:
                    if (!products.Any(p => p.Title == "Adobe Photoshop"))
                    {
                        products.Add(new Product { Title = "Adobe Photoshop", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Adobe Photoshop\"";
                        var product = products.FirstOrDefault(p => p.Title == "Adobe Photoshop");
                        if (product != null)
                        { 
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 2:
                    // Перекрыл товар в связи с тем, что его нет в наличии
                    break;
                case 3:
                    if (!products.Any(p => p.Title == "Adobe Illustrator"))
                    {
                        products.Add(new Product { Title = "Adobe Illustrator", Number = 1, Price = 15, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Adobe Illustrator\"";
                        var product = products.FirstOrDefault(p => p.Title == "Adobe Illustrator");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 4:
                    if (!products.Any(p => p.Title == "Dreamviewer"))
                    {
                        products.Add(new Product { Title = "Dreamviewer", Number = 1, Price = 50, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Dreamviewer\"";
                        var product = products.FirstOrDefault(p => p.Title == "Dreamviewer");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 5:
                    if (!products.Any(p => p.Title == "Adobe Audition"))
                    {
                        products.Add(new Product { Title = "Adobe Audition", Number = 1, Price = 5, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Adobe Audition\"";
                        var product = products.FirstOrDefault(p => p.Title == "Adobe Audition");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 6:
                    if (!products.Any(p => p.Title == "Adobe Bridge"))
                    {
                        products.Add(new Product { Title = "Adobe Bridge", Number = 1, Price = 30, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Adobe Bridge\"";
                        var product = products.FirstOrDefault(p => p.Title == "Adobe Bridge");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 7:
                    if (!products.Any(p => p.Title == "Flash Builder"))
                    {
                        products.Add(new Product { Title = "Flash Builder", Number = 1, Price = 20, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Flash Builder\"";
                        var product = products.FirstOrDefault(p => p.Title == "Flash Builder");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                case 8:
                    if (!products.Any(p => p.Title == "Adobe Lightroom"))
                    {
                        products.Add(new Product { Title = "Adobe Lightroom", Number = 1, Price = 15, Placement = "в наличии" });
                        LW.Items.Refresh();
                    }
                    else
                    {
                        GB1_Message.Text = "В корзину добавлен еще 1 продукт под названием: \"Adobe Lightroom\"";
                        var product = products.FirstOrDefault(p => p.Title == "Adobe Lightroom");
                        if (product != null)
                        {
                            product.Number++;
                            LW.Items.Refresh();
                        }
                    }
                    break;
                default:
                    break;
            }
            AllSumCount();
        }
        #endregion

        // Отображение информации о купонах
        private void CuponInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            interact = 10;
            GB1_Message.Text = "У вас в наличии " + cupon_all.ToString() + " купонов со скидкой на весь товар и " + cupon_item.ToString() + " купонов на определенный товар";
            GB2_Message.Text = "У вас в наличии " + cupon_all.ToString() + " купонов со скидкой на весь товар и " + cupon_item.ToString() + " купонов на определенный товар";
            GB1_Message.Foreground = Brushes.Black;
        }

        #region 5. Переход и работа с корзиной
        // Переход в корзину для взаимодействия с ней
        private void Car_Click(object sender, RoutedEventArgs e)
        {
            GR_1.Visibility = Visibility.Hidden;
            GR_2.Visibility = Visibility.Visible;
        }

        // Очистка корзины (удаление выбранного товара полностью).
        private void Btn_Clear_all_car_Click(object sender, RoutedEventArgs e)
        {
            products.Clear();
            LW.Items.Refresh();

            cupon_all = Convert.ToInt32(cupons[1]);
            cupon_item = Convert.ToInt32(cupons[0]);

            pushcare_item = 0;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);

            interact = 15;
            GB2_Message.Text = "Была очищена корзина, можете совершать покупки заного";
            GB1_Message.Foreground = Brushes.Black;

            for (int i = 0; i < 999; i++)
            {
                count[i] = 0;
            }

            AllSumCount();

            step = 0;
        }
        
        // Возврат к покупкам
        private void Btn_Back_to_merch_Click(object sender, RoutedEventArgs e)
        {
            GR_1.Visibility = Visibility.Visible;
            GR_2.Visibility = Visibility.Hidden;
        }
        #endregion

        #region 6. Работа с (условной) базой в корзине
        // Добавка +1 продукта
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).DataContext as Product;
            product.Number++;
            LW.Items.Refresh();

            pushcare_item++;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
            AllSumCount();

            count[step] = 101;
            switch(product.Title)
            {
                case "Adobe Photoshop":
                    names[step] = product.Title;
                    break;
                case "Adobe Illustrator":
                    names[step] = product.Title;
                    break;
                case "Dreamviewer":
                    names[step] = product.Title;
                    break;
                case "Adobe Audition":
                    names[step] = product.Title;
                    break;
                case "Adobe Bridge":
                    names[step] = product.Title;
                    break;
                case "Flash Builder":
                    names[step] = product.Title;
                    break;
                case "Adobe Lightroom":
                    names[step] = product.Title;
                    break;
                default:
                    break;
            }
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB2_Message.Text = "+1 \"" + product.Title + "\" было добавлено в корзину";

            count[step + 1] = 0;
        }

        // Отнимаем -1 продукт, или удаляем из списка если упало ниже нуля
        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).DataContext as Product;
            names[step] = product.Title;

            product.Number--;
            if (product.Number < 1)
            {
                if (product.IsChecked == true) cupon_item++;
                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                products.Remove(product);
            }
            LW.Items.Refresh();

            pushcare_item--;
            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
            AllSumCount();

            count[step] = 102;
            step++;

            GB1_Message.Foreground = Brushes.Black;
            GB2_Message.Text = "-1 \"" + product.Title + "\" было изъято из корзины";

            count[step + 1] = 0;
        }

        // Применяем скидочный купон (если он есть)
        private void CheckCupo_Checked(object sender, RoutedEventArgs e)
        {
            GB1_Message.Foreground = Brushes.Black;

            var product = (sender as CheckBox).DataContext as Product;
            if (product.Title == "Adobe Photoshop")
            {
                interact = 15;
                GB2_Message.Text = "К данному продукту не применим купон, т.к. на него и так идет скидка в 50%";
                product.IsChecked = false;
                LW.Items.Refresh();
            }
            else if (cupon_item <= 0)
            {
                interact = 15;
                GB2_Message.Text = "Сожалею, но у вас не осталось купонов для единичного товара";
                product.IsChecked = false;
                LW.Items.Refresh();
            }
            else
            {
                cupon_item--;
                product.Price = product.Price - Convert.ToInt32(product.Price / 10);
                LW.Items.Refresh();
                names[step] = product.Title;
                GB2_Message.Text = "К \"" + product.Title + "\" была применена скидка в 10%";
            }
            
            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
            AllSumCount();

            if (CBALL.IsChecked == true)
            {
                cupon_all++;
                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                AllDiscount = 0;
                AllSumCount();
                CBALL.IsChecked = false;
            }

            count[step] = 103;
            step++;
            count[step + 1] = 0;
        }
        
        // Отменяем применение скидочного купона
        private void CheckCupo_Unchecked(object sender, RoutedEventArgs e)
        {
            GB1_Message.Foreground = Brushes.Black;

            var product = (sender as CheckBox).DataContext as Product;
            cupon_item++;
            switch (product.Title)
            {
                case "Adobe Photoshop":
                    product.Price = 20;
                    break;
                case "Adobe Illustrator":
                    product.Price = 15;
                    break;
                case "Dreamviewer":
                    product.Price = 50;
                    break;
                case "Adobe Audition":
                    product.Price = 5;
                    break;
                case "Adobe Bridge":
                    product.Price = 30;
                    break;
                case "Flash Builder":
                    product.Price = 20;
                    break;
                case "Adobe Lightroom":
                    product.Price = 15;
                    break;
                default:
                    break;
            }
            LW.Items.Refresh();

            GB2_Message.Text = "К \"" + product.Title + "\" была убрана скидка в 10%";

            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
            AllSumCount();

            names[step] = product.Title;
            count[step] = 104;
            step++;
            count[step + 1] = 0;
        }

        // Важная функция считающая "ИТОГО" (в зависимости от общей скдики)
        public void AllSumCount()
        {
            if (AllDiscount == 0)
            {
                AllSum.Text = products.Sum(x => x.NumPrice).ToString();
            }
            else
            {
                AllSum.Text = (products.Sum(x => x.NumPrice) - (products.Sum(x => x.NumPrice) / 10)).ToString();
            }
        }
        
        // Применение скидки на весь товар
        int AllDiscount = 0; // Если 1 то идет скидка на весь товар.
        int[] disc = new int[8]; // Специально для Undo.
        int chb_all_check = 0;
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            GB1_Message.Foreground = Brushes.Black;

            if (cupon_all == 0)
            {
                interact = 15;
                GB2_Message.Text = "К сожалению у вас нету купона на весь товар";
                CBALL.IsChecked = false;
            }
            else
            {
                int i = 0;
                List<Product> ProdItem = (List<Product>)LW.ItemsSource;
                foreach (Product product in ProdItem)
                {
                    if (product.IsChecked == true)
                    {
                        cupon_item++;
                        disc[i] = 1;
                        i++;
                    }
                    else
                    {
                        disc[i] = 0;
                        i++;
                    }
                    product.IsChecked = false;

                    switch (product.Title)
                    {
                        case "Adobe Photoshop":
                            product.Price = 20;
                            break;
                        case "Adobe Illustrator":
                            product.Price = 15;
                            break;
                        case "Dreamviewer":
                            product.Price = 50;
                            break;
                        case "Adobe Audition":
                            product.Price = 5;
                            break;
                        case "Adobe Bridge":
                            product.Price = 30;
                            break;
                        case "Flash Builder":
                            product.Price = 20;
                            break;
                        case "Adobe Lightroom":
                            product.Price = 15;
                            break;
                        default:
                            break;
                    }

                    count[step] = 105;
                    step++;
                    count[step + 1] = 0;
                }



                cupon_all--;
                AllDiscount = 1;
                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                interact = 15;
                GB2_Message.Text = "Вы применили скидку 10% на весь товар";
                AllSumCount();
                LW.Items.Refresh();

            }
            if (chb_all_check == 1)
            {
                step--;
            }
        }

        // Отказ от скидки на весь товар
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GB1_Message.Foreground = Brushes.Black;

            cupon_all++;
            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
            interact = 15;
            GB2_Message.Text = "Вы убрали скидку в 10% на весь товар";
            AllDiscount = 0;
            AllSumCount();

            //count[step] = 106;
            //step++;
        }
        #endregion

        #region 7. Работа с UNDO и REDO
        // Кнопки для отмены и возврата действия
        private void IMG_Undo_MouseDown(object sender, MouseButtonEventArgs e) { UNDO(); }
        private void IMG_Redo_MouseDown(object sender, MouseButtonEventArgs e) { REDO(); }

        // Шпаргалка для UNDO / REDO (чтобы не забыть что отменяется)
        // ---------------------------------------------------------
        // 1 - добавление Adobe Photoshop
        // 3 - добавление Adobe Illustrator
        // 4 - добавление Dreamviewer
        // 5 - добавление Adobe Audition
        // 6 - добавление Adobe Bridge
        // 7 - добавление Flash Builder
        // 8 - добавление Adobe Lightroom
        // 101 - Кнопка "+" - увеличение кол-ва товара в корзине
        // 102 - Кнопка "-" - уменьшение кол-ва товара в корзине
        // 103 - Применение скидочного купона для ед. товара
        // 104 - Отмена применения скидочного купона для ед. товара
        // 105 - Применение скидки на весь товар
        // 106 - Отмена применения скидки на весь товар
        #region UNDO
        // Функция отмены
        public void UNDO ()
        {
            GB1_Message.Foreground = Brushes.Black;
            GB2_Message.Foreground = Brushes.Black;
            if (step < 1)
            {
                GB1_Message.Text = "Нечего отменять";
                GB2_Message.Text = "Нечего отменять";
            }
            else
            {
                switch (count[step - 1])
                {
                    case 1:
                        interact = 10;
                        step--;

                        var product1 = products.FirstOrDefault(p => p.Title == "Adobe Photoshop");

                        product1.Number--;
                        if (product1.Number < 1)
                        {
                            if (product1.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product1);
                            GB1_Message.Text = "Товар \"Adobe Photoshop\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Adobe Photoshop\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Adobe Photoshop\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Adobe Photoshop\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 3:
                        interact = 10;
                        step--;

                        var product2 = products.FirstOrDefault(p => p.Title == "Adobe Illustrator");

                        product2.Number--;
                        if (product2.Number < 1)
                        {
                            if (product2.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product2);
                            GB1_Message.Text = "Товар \"Adobe Illustrator\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Adobe Illustrator\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Adobe Illustrator\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Adobe Illustrator\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 4:
                        interact = 10;
                        step--;

                        var product3 = products.FirstOrDefault(p => p.Title == "Dreamviewer");

                        product3.Number--;
                        if (product3.Number < 1)
                        {
                            if (product3.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product3);
                            GB1_Message.Text = "Товар \"Dreamviewer\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Dreamviewer\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Dreamviewer\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Dreamviewer\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 5:
                        interact = 10;
                        step--;

                        var product4 = products.FirstOrDefault(p => p.Title == "Adobe Audition");

                        product4.Number--;
                        if (product4.Number < 1)
                        {
                            if (product4.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product4);
                            GB1_Message.Text = "Товар \"Adobe Audition\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Adobe Audition\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Adobe Audition\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Adobe Audition\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 6:
                        interact = 10;
                        step--;

                        var product5 = products.FirstOrDefault(p => p.Title == "Adobe Bridge");

                        product5.Number--;
                        if (product5.Number < 1)
                        {
                            if (product5.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product5);
                            GB1_Message.Text = "Товар \"Adobe Bridge\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Adobe Bridge\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Adobe Bridge\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Adobe Bridge\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 7:
                        interact = 10;
                        step--;

                        var product6 = products.FirstOrDefault(p => p.Title == "Flash Builder");

                        product6.Number--;
                        if (product6.Number < 1)
                        {
                            if (product6.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product6);
                            GB1_Message.Text = "Товар \"Flash Builder\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Flash Builder\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Flash Builder\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Flash Builder\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 8:
                        interact = 10;
                        step--;

                        var product7 = products.FirstOrDefault(p => p.Title == "Adobe Lightroom");

                        product7.Number--;
                        if (product7.Number < 1)
                        {
                            if (product7.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product7);
                            GB1_Message.Text = "Товар \"Adobe Lightroom\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"Adobe Lightroom\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"Adobe Lightroom\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"Adobe Lightroom\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 101:
                        interact = 10;
                        step--;

                        var product8 = products.FirstOrDefault(p => p.Title == names[step]);

                        product8.Number--;
                        if (product8.Number < 1)
                        {
                            if (product8.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product8);
                            GB1_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();
                        break;
                    case 102:
                        interact = 10;
                        step--;
                        
                        if (!products.Any(p => p.Title == names[step]))
                        {
                            products.Add(new Product { Title = names[step], Number = 1, Price = 50, Placement = "в наличии" });
                            LW.Items.Refresh();

                            pushcare_item++;
                            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                            AllSumCount();
                        }
                        else
                        {
                            var product9 = products.FirstOrDefault(p => p.Title == names[step]);
                            
                            LW.Items.Refresh();

                            pushcare_item++;
                            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                            AllSumCount();

                            switch (product9.Title)
                            {
                                case "Adobe Photoshop":
                                    product9.Number++;
                                    break;
                                case "Adobe Illustrator":
                                    product9.Number++;
                                    break;
                                case "Dreamviewer":
                                    product9.Number++;
                                    break;
                                case "Adobe Audition":
                                    product9.Number++;
                                    break;
                                case "Adobe Bridge":
                                    product9.Number++;
                                    break;
                                case "Flash Builder":
                                    product9.Number++;
                                    break;
                                case "Adobe Lightroom":
                                    product9.Number++;
                                    break;
                                default:
                                    break;
                            }

                            GB1_Message.Text = "+1 \"" + product9.Title + "\" был возвращен в корзину";
                            GB2_Message.Text = "+1 \"" + product9.Title + "\" был возвращен в корзину";
                        }
                        break;
                    case 103:
                        try
                        {
                            interact = 10;
                            step--;

                            var product10 = products.FirstOrDefault(p => p.Title == names[step]);

                            if (product10.Title != "Adobe Photoshop")
                            {
                                cupon_item++;

                                switch (product10.Title)
                                {
                                    case "Adobe Photoshop":
                                        product10.Price = 20;
                                        break;
                                    case "Adobe Illustrator":
                                        product10.Price = 15;
                                        break;
                                    case "Dreamviewer":
                                        product10.Price = 50;
                                        break;
                                    case "Adobe Audition":
                                        product10.Price = 5;
                                        break;
                                    case "Adobe Bridge":
                                        product10.Price = 30;
                                        break;
                                    case "Flash Builder":
                                        product10.Price = 20;
                                        break;
                                    case "Adobe Lightroom":
                                        product10.Price = 15;
                                        break;
                                    default:
                                        break;
                                }
                                product10.IsChecked = false;
                                LW.Items.Refresh();

                                GB1_Message.Text = "К \"" + product10.Title + "\" была убрана скидка в 10%";
                                GB2_Message.Text = "К \"" + product10.Title + "\" была убрана скидка в 10%";

                                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                                AllSumCount();
                            }
                        }
                        catch
                        {
                            interact = 10;
                            GB1_Message.Text = "404: Здесь кто-то пошалил над кодом. Желательно перезапустить приложение.";
                            GB2_Message.Text = "404: Здесь кто-то пошалил над кодом. Желательно перезапустить приложение.";
                            step--;
                        }
                        break;
                    case 104:
                        interact = 10;
                        step--;

                        var product11 = products.FirstOrDefault(p => p.Title == names[step]);

                        cupon_item--;
                        product11.Price = product11.Price - Convert.ToInt32(product11.Price / 10);
                        GB1_Message.Text = "К \"" + product11.Title + "\" была возвращена скидка в 10%";
                        GB2_Message.Text = "К \"" + product11.Title + "\" была возвращена скидка в 10%";

                        cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                        AllSumCount();

                        product11.IsChecked = true;

                        if (CBALL.IsChecked == true)
                        {
                            cupon_all++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            AllDiscount = 0;
                            AllSumCount();
                            CBALL.IsChecked = false;
                        }
                        
                        LW.Items.Refresh();
                        break;
                    case 105:
                        interact = 10;
                        step--;

                        int i = 0;
                        List<Product> ProdItem = (List<Product>)LW.ItemsSource;
                        foreach (Product product in ProdItem)
                        {
                            var product12 = products.FirstOrDefault(p => p.Title == product.Title);

                            if (disc[i] == 1)
                            {
                                product12.IsChecked = true;
                                product12.Price = product12.Price - Convert.ToInt32(product12.Price / 10);
                                i++;
                            }
                            else
                            {
                                product12.IsChecked = false;
                                i++;
                            }
                        }

                        cupon_all++;
                        AllDiscount = 0;
                        
                        cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                        GB1_Message.Text = "Скидка на весь товар в 10% была отменена";
                        GB2_Message.Text = "Скидка на весь товар в 10% была отменена";
                        LW.Items.Refresh();
                        chb_all_check = 1;
                        CBALL.IsChecked = false;
                        AllSumCount();
                        break;
                    default:
                        step--;
                        break;
                }
            }
        }
        #endregion
        #region REDO
        // Функция возврата
        public void REDO ()
        {
            GB1_Message.Foreground = Brushes.Black;
            if (count[step] == 0)
            {
                GB1_Message.Text = "Нечего возвращать";
                GB2_Message.Text = "Нечего возвращать";
            }
            else
            {
                switch (count[step])
                {
                    case 1:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Adobe Photoshop"))
                        {
                            products.Add(new Product { Title = "Adobe Photoshop", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Photoshop\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Photoshop\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Photoshop\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Photoshop\"";
                            var product = products.FirstOrDefault(p => p.Title == "Adobe Photoshop");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 3:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Adobe Illustrator"))
                        {
                            products.Add(new Product { Title = "Adobe Illustrator", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Illustrator\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Illustrator\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Illustrator\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Illustrator\"";
                            var product = products.FirstOrDefault(p => p.Title == "Adobe Illustrator");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 4:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Dreamviewer"))
                        {
                            products.Add(new Product { Title = "Dreamviewer", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Dreamviewer\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Dreamviewer\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Dreamviewer\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Dreamviewer\"";
                            var product = products.FirstOrDefault(p => p.Title == "Dreamviewer");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 5:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Adobe Audition"))
                        {
                            products.Add(new Product { Title = "Adobe Audition", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Audition\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Audition\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Audition\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Audition\"";
                            var product = products.FirstOrDefault(p => p.Title == "Adobe Audition");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 6:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Adobe Bridge"))
                        {
                            products.Add(new Product { Title = "Adobe Bridge", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Bridge\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Bridge\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Bridge\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Bridge\"";
                            var product = products.FirstOrDefault(p => p.Title == "Adobe Bridge");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 7:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Flash Builder"))
                        {
                            products.Add(new Product { Title = "Flash Builder", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Flash Builder\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Flash Builder\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Flash Builder\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Flash Builder\"";
                            var product = products.FirstOrDefault(p => p.Title == "Flash Builder");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 8:
                        interact = 10;
                        step++;

                        if (!products.Any(p => p.Title == "Adobe Lightroom"))
                        {
                            products.Add(new Product { Title = "Adobe Lightroom", Number = 1, Price = 20, Discount = 10, Placement = "в наличии (скидка 50%)" });
                            LW.Items.Refresh();
                            GB1_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Lightroom\"";
                            GB2_Message.Text = "В корзину возвращен продукт под названием: \"Adobe Lightroom\"";
                        }
                        else
                        {
                            GB1_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Lightroom\"";
                            GB2_Message.Text = "В корзину возвращен еще 1 продукт под названием: \"Adobe Lightroom\"";
                            var product = products.FirstOrDefault(p => p.Title == "Adobe Lightroom");
                            if (product != null)
                            {
                                product.Number++;
                                LW.Items.Refresh();
                            }
                        }

                        pushcare_item++;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        break;
                    case 101:
                        interact = 10;

                        if (!products.Any(p => p.Title == names[step]))
                        {
                            products.Add(new Product { Title = names[step], Number = 1, Price = 50, Placement = "в наличии" });
                            LW.Items.Refresh();

                            pushcare_item++;
                            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                            AllSumCount();
                        }
                        else
                        {
                            var product9 = products.FirstOrDefault(p => p.Title == names[step]);
                            
                            LW.Items.Refresh();

                            pushcare_item++;
                            Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                            AllSumCount();

                            count[step] = 101;
                            switch (product9.Title)
                            {
                                case "Adobe Photoshop":
                                    product9.Number++;
                                    break;
                                case "Adobe Illustrator":
                                    product9.Number++;
                                    break;
                                case "Dreamviewer":
                                    product9.Number++;
                                    break;
                                case "Adobe Audition":
                                    product9.Number++;
                                    break;
                                case "Adobe Bridge":
                                    product9.Number++;
                                    break;
                                case "Flash Builder":
                                    product9.Number++;
                                    break;
                                case "Adobe Lightroom":
                                    product9.Number++;
                                    break;
                                default:
                                    break;
                            }

                            GB1_Message.Text = "+1 \"" + product9.Title + "\" был возвращен в корзину";
                            GB2_Message.Text = "+1 \"" + product9.Title + "\" был возвращен в корзину";
                        }

                        step++;
                        break;
                    case 102:
                        interact = 10;

                        var product8 = products.FirstOrDefault(p => p.Title == names[step]);

                        product8.Number--;
                        if (product8.Number < 1)
                        {
                            if (product8.IsChecked == true) cupon_item++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            products.Remove(product8);
                            GB1_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины";
                            GB2_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины";
                        }
                        else
                        {
                            GB1_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины в количестве 1 штука";
                            GB2_Message.Text = "Товар \"" + names[step] + "\" был изъят из корзины в количестве 1 штука";
                        }
                        LW.Items.Refresh();

                        pushcare_item--;
                        Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                        AllSumCount();

                        step++;
                        break;
                    case 103:
                        interact = 10;

                        var product11 = products.FirstOrDefault(p => p.Title == names[step]);

                        cupon_item--;
                        product11.Price = product11.Price - Convert.ToInt32(product11.Price / 10);
                        GB1_Message.Text = "К \"" + product11.Title + "\" была применена скидка в 10%";
                        GB2_Message.Text = "К \"" + product11.Title + "\" была применена скидка в 10%";

                        cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                        AllSumCount();

                        if (CBALL.IsChecked == true)
                        {
                            cupon_all++;
                            cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                            AllDiscount = 0;
                            AllSumCount();
                            CBALL.IsChecked = false;
                        }

                        product11.IsChecked = true;
                        LW.Items.Refresh();

                        step++;
                        break;
                    case 104:
                        interact = 10;

                        var product10 = products.FirstOrDefault(p => p.Title == names[step]);

                        cupon_item++;

                        switch (product10.Title)
                        {
                            case "Adobe Photoshop":
                                product10.Price = 20;
                                break;
                            case "Adobe Illustrator":
                                product10.Price = 15;
                                break;
                            case "Dreamviewer":
                                product10.Price = 50;
                                break;
                            case "Adobe Audition":
                                product10.Price = 5;
                                break;
                            case "Adobe Bridge":
                                product10.Price = 30;
                                break;
                            case "Flash Builder":
                                product10.Price = 20;
                                break;
                            case "Adobe Lightroom":
                                product10.Price = 15;
                                break;
                            default:
                                break;
                        }
                        product10.IsChecked = false;
                        LW.Items.Refresh();

                        GB1_Message.Text = "К \"" + product10.Title + "\" была убрана скидка в 10%";
                        GB2_Message.Text = "К \"" + product10.Title + "\" была убрана скидка в 10%";

                        cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);
                        AllSumCount();

                        step++;
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
        #endregion   

        #region 8. Небольшое меню
        // Запись данных в своеобразную базу (файл)
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            
            saveFileDialog1.Title = "Сохранение файла";
            saveFileDialog1.Filter = "txt base files (*.txtbase)|*.txtbase|All files|*.*";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile());

                List<Product> ProdItem = (List<Product>)LW.ItemsSource;
                int i = 0;
                foreach (Product product in ProdItem)
                {
                    i++;
                }
                sw.WriteLine(i.ToString());
                foreach (Product product in ProdItem)
                {
                    sw.WriteLine(product.Title);
                    sw.WriteLine(product.Price.ToString());
                    sw.WriteLine(product.IsChecked.ToString());
                    sw.WriteLine(product.Number.ToString());
                    sw.WriteLine(product.NumPrice.ToString());
                }

                sw.Close();
            }
        }

        // Чтение файла из базы
        private void LoadAs_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadFileDialog1 = new OpenFileDialog();

            loadFileDialog1.Title = "Сохранение файла";
            loadFileDialog1.Filter = "txt base files (*.txtbase)|*.txtbase|All files|*.*";
            loadFileDialog1.ShowDialog();

            if (loadFileDialog1.FileName != "")
            {
                products.Clear();
                LW.Items.Refresh();

                cupon_all = Convert.ToInt32(cupons[1]);
                cupon_item = Convert.ToInt32(cupons[0]);

                pushcare_item = 0;
                Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                cupon_TB.Text = Convert.ToString(cupon_all + cupon_item);

                interact = 15;
                GB2_Message.Text = "Была очищена корзина";
                GB1_Message.Text = "Была очищена корзина";
                GB2_Message.Foreground = Brushes.Black;
                GB1_Message.Foreground = Brushes.Black;

                for (int i = 0; i < 999; i++)
                {
                    count[i] = 0;
                }

                step = 0;

                string[] lines = File.ReadAllLines(loadFileDialog1.FileName);

                int i_count = Convert.ToInt32(lines[0]);

                for (int i = 0; i < i_count + 1; i++)
                {
                    switch (i)
                    {
                        case 1:
                            if (lines[3] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1], Number = Convert.ToInt32(lines[4]), IsChecked = Convert.ToBoolean(lines[3]), Price = Convert.ToInt32(lines[2]) - Convert.ToInt32(lines[2]) / 10, Placement = "в наличии" });   
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1], Number = Convert.ToInt32(lines[4]), IsChecked = Convert.ToBoolean(lines[3]), Price = Convert.ToInt32(lines[2]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 2:
                            if (lines[3 + 5] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 5], Number = Convert.ToInt32(lines[4 + 5]), IsChecked = Convert.ToBoolean(lines[3 + 5]), Price = Convert.ToInt32(lines[2 + 5]) - Convert.ToInt32(lines[2 + 5]) / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1 + 5], Number = Convert.ToInt32(lines[4 + 5]), IsChecked = Convert.ToBoolean(lines[3 + 5]), Price = Convert.ToInt32(lines[2 + 5]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 3:
                            if (lines[3 + 10] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 10], Number = Convert.ToInt32(lines[4 + 10]), IsChecked = Convert.ToBoolean(lines[3 + 10]), Price = Convert.ToInt32(lines[2 + 10]) - Convert.ToInt32(lines[2 + 10]) / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1 + 10], Number = Convert.ToInt32(lines[4 + 10]), IsChecked = Convert.ToBoolean(lines[3 + 10]), Price = Convert.ToInt32(lines[2 + 10]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 4:
                            if (lines[3 + 15] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 15], Number = Convert.ToInt32(lines[4 + 15]), IsChecked = Convert.ToBoolean(lines[3 + 15]), Price = Convert.ToInt32(lines[2 + 15]) - Convert.ToInt32(lines[2 + 15]) / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1 + 15], Number = Convert.ToInt32(lines[4 + 15]), IsChecked = Convert.ToBoolean(lines[3 + 15]), Price = Convert.ToInt32(lines[2 + 15]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 5:
                            if (lines[3 + 20] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 20], Number = Convert.ToInt32(lines[4 + 20]), IsChecked = Convert.ToBoolean(lines[3 + 20]), Price = Convert.ToInt32(lines[2 + 20]) - Convert.ToInt32(lines[2 + 20])  / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1 + 20], Number = Convert.ToInt32(lines[4 + 20]), IsChecked = Convert.ToBoolean(lines[3 + 20]), Price = Convert.ToInt32(lines[2 + 20]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 6:
                            if (lines[3 + 25] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 25], Number = Convert.ToInt32(lines[4 + 25]), IsChecked = Convert.ToBoolean(lines[3 + 25]), Price = Convert.ToInt32(lines[2 + 25]) - Convert.ToInt32(lines[2 + 25])  / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 25], Number = Convert.ToInt32(lines[4 + 25]), IsChecked = Convert.ToBoolean(lines[3 + 25]), Price = Convert.ToInt32(lines[2 + 25]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        case 7:
                            if (lines[3 + 30] == "True" && cupon_item > 0)
                            {
                                cupon_item--;
                                products.Add(new Product { Title = lines[1 + 30], Number = Convert.ToInt32(lines[4 + 30]), IsChecked = Convert.ToBoolean(lines[3 + 30]), Price = Convert.ToInt32(lines[2 + 30]) - Convert.ToInt32(lines[2 + 30]) / 10, Placement = "в наличии" });
                            }
                            else
                            {
                                products.Add(new Product { Title = lines[1 + 30], Number = Convert.ToInt32(lines[4 + 30]), IsChecked = Convert.ToBoolean(lines[3 + 30]), Price = Convert.ToInt32(lines[2 + 30]), Placement = "в наличии" });
                            }
                            pushcare_item += Convert.ToInt32(lines[4]);
                            break;
                        default:
                            break;
                    }
                }

                Car.Header = "Корзина (" + pushcare_item.ToString() + ")";
                AllSumCount();
                LW.Items.Refresh();
            }
        }

        // Простейшая справка на MessageBox'е
        private void Question_Click(object sender, RoutedEventArgs e)
        {
            Questions();
        }

        public void Questions()
        {
            MessageBox.Show("В данной программе есть специальные кнопки:\n\n" +
                "Ctrl+Z - Отменяет ваше действие (или стрелочка влево)\nCtrl+R - Возвращает отмененное действие (стрелочка вправо)" +
                "\nF1 Вызывает справку (данное окно)\n\nПомимо этого у нас в магазине существуют скидки:\n" +
                "На Adobe Photoshop в данный момент скидка в 50%!\nНа прочий товар можно применять купоны\nКупоны дают скидку в 10%" +
                "\n\nЕсть купон на весь товар сразу\nЕго можно применить только по отдельности\n\n\nЕсли вы хотите совершить покупку" +
                "Выберите интересующий вас товар\nДалее перейдите в корзину\nУточните детали вашего заказа\nОтправьте ваш заказ на печать" +
                "\n\nПосле отправки заказа на печать вам выдадут чек\nПо этому чеку пройдите на кассу и оплатите заказ");
        }
        #endregion

        // Реагирование на Ctrl+Z, Ctrl+R и F1
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                Questions();
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z)
            {
                UNDO();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.R)
            {
                REDO();
            }
        }

        #region 9. Отправка файла на печать
        // Печать документа
        private void Btn_Print_a_check_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.PageRangeSelection = PageRangeSelection.AllPages;
            printDialog.UserPageRangeEnabled = true;
            
            Nullable<Boolean> print = printDialog.ShowDialog();
            if (print == true)
            {
                TextBlock visual = new TextBlock();

                // К сожалению у меня почему-то не получилось инициировать DrawLines(pen, point, point) и нарисовать нормальную таблицу.
                // Не знаю почему это произошло, час копался но не нашел решения, чтобы не затягивать сделаю таблицу в ручную.

                string s = DateTime.Now.ToString("dd MMMM yyyy");

                visual.Inlines.Add("\n");
                visual.Inlines.Add("\n");
                visual.Inlines.Add("     Товарный чек № PT-2                                                            " +
                    "                                                                                    от [" + s + "]\n");
                visual.Inlines.Add("     На сумму:      " + AllSum.Text + "$\n");
                visual.Inlines.Add("\n\n\n");
                visual.Inlines.Add("     Организаця: \"Adobe\"\n");
                visual.Inlines.Add("     ИНН: 772834818231\n");
                visual.Inlines.Add("     Магазин: \"Тестовое задание\"\n");
                visual.Inlines.Add("\n\n\n\n\n");
                visual.Inlines.Add("     Артикул:\n");
                visual.Inlines.Add("     _____________________________\n");
                visual.Inlines.Add("\n");

                List<Product> ProdItem = (List<Product>)LW.ItemsSource;
                foreach (Product product in ProdItem)
                {
                    visual.Inlines.Add("     [Товар]:   " + product.Title + "\n");
                    if (product.IsChecked == true)
                    {
                        visual.Inlines.Add("     (c учетом скидки)\n");
                    }
                    visual.Inlines.Add("     [Цена]:    " + product.Price + "\n");
                    visual.Inlines.Add("     [Кол-во]:  " + product.Number + "\n");
                    visual.Inlines.Add("     [Итого]:   " + product.NumPrice + "\n");
                    visual.Inlines.Add("\n");
                }
                visual.Inlines.Add("     _____________________________\n");
                visual.Inlines.Add("\n");
                if (CBALL.IsChecked == true)
                {
                    visual.Inlines.Add("     НА ВЕСЬ ТОВАР ДЕЙСТВУЕТ СКИДКА ПО КУПОНУ\n");
                }
                visual.Inlines.Add("\n");
                visual.Inlines.Add("     Итого к оплате [" + pushcare_item.ToString() + " наименований] на сумму: [" + AllSum.Text +"]\n");
                visual.Inlines.Add("\n\n\n\n\n\n\n");
                visual.Inlines.Add("     Кассир: Самойлов Виктор Андреевич                                            " +
                    "                                                                      _________________ (роспись)");


                new PrintDialog().PrintVisual(visual, "Print Job Description");
            }

            // new PrintDialog().PrintVisual(LW, "Print Job Description");
        }
        #endregion

    }
}
