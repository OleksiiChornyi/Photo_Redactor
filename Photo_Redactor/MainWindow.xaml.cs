using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Drawing.Imaging;
using Size = System.Windows.Size;
using System.Drawing;
using Image = System.Windows.Controls.Image;

namespace Photo_Redactor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// ctrl - указывает, нажата ли клавиша ctrl
        /// but_down - указывает, была ли нажата левая кнопка мыши
        /// translate - указывает, была ли нажата левая кнопка мыши без кнопки ctrl (перетаскивание)
        /// operations - экземпляр класса Operations
        /// </summary>
        bool ctrl = false, but_down = false, translate = false;
        Operations operations = new Operations();
        /// <summary>
        /// Точка входа программы
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Slider_scale.Value = 1;
            label_size_im1.Content = "Изображение весит " + operations.Size_im(im1) + " KB";
            label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
            operations.mouse_left_down_x = 0;
            operations.mouse_left_down_y = 0;
            operations.mouse_left_up_x = im2.Width;
            operations.mouse_left_up_y = im2.Height;
        }
        /// <summary>
        /// Нажатие на кнопку "Открыть"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_open_Click(object sender, RoutedEventArgs e)
        {
            // диалог для выбора файла
            OpenFileDialog ofd = new OpenFileDialog();
            // фильтр форматов файлов
            ofd.Filter = "Image Files(*.JPEG;*.GIF;*.BMP;*.PNG,*.TIFF)|*.JPEG;*.GIF;*.BMP;*.PNG,*.TIFF|All files (*.*)|*.*";
            // если в диалоге была нажата кнопка Открыть
            ofd.InitialDirectory = "c:\\";
            if (ofd.ShowDialog() == true) //Если пользователь выберет файл и нажмет "Open" результатом будет True
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    if (ofd.FileName.LastIndexOf(".csv") == ofd.FileName.Length - 4)
                    {
                        MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        // загружаем изображение                    
                        // создание нового объекта типа BitmapImage
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(ofd.FileName);
                        bmp.EndInit();
                        im1.Source = bmp;
                        im2.Source = bmp;
                    }
                    label_size_im1.Content = "Изображение весит " + operations.Size_im(im1) + " KB";
                    label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
                    label_time.Content = "Время преобразования = 0 milliseconds";
                    operations.transl_x_down = operations.transl_x_up;
                    operations.transl_y_down = operations.transl_y_up;
                    operations.mouse_left_down_x = 0;
                    operations.mouse_left_down_y = 0;
                    operations.mouse_left_up_x = im2.Width;
                    operations.mouse_left_up_y = im2.Height;
                    Slider_rotate.Value = 0;
                    Slider_scale.Value = 1;
                    Slider_skew.Value = 0;
                    im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
                    but_down = false;
                }
                catch // в случае ошибки выводим MessageBox
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Нажатие на кнопку "Сохранить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Cохранение файла";
            sfd.Filter = "Image Files(*.JPEG;*.GIF;*.BMP;*.PNG,*.CSV,*.TIFF)|*.JPEG;*.GIF;*.BMP;*.PNG,*.CSV,*.TIFF|All files (*.*)|*.*";
            sfd.InitialDirectory = "c:\\";
            System.Windows.Media.PixelFormat format;
            format = PixelFormats.Pbgra32;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)im2.Width, (int)im2.Height, 96, 96, format);
            im2.Measure(new Size((int)im2.Width, (int)im2.Height));
            im2.Arrange(new Rect(new Size((int)im2.Width, (int)im2.Height)));
            rtb.Clear();
            rtb.Render(im2);
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    if (sfd.FileName.LastIndexOf(".csv") != sfd.FileName.Length - 4)
                    {
                        using (FileStream outStream = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            if (sfd.FileName.LastIndexOf(".png") == sfd.FileName.Length - 4)
                            {
                                PngBitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                encoder.Save(outStream);
                            }
                            else if (sfd.FileName.LastIndexOf(".gif") == sfd.FileName.Length - 4)
                            {
                                GifBitmapEncoder encoder = new GifBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                encoder.Save(outStream);
                            }
                            else if (sfd.FileName.LastIndexOf(".bmp") == sfd.FileName.Length - 4)
                            {
                                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                encoder.Save(outStream);
                            }
                            else if (sfd.FileName.LastIndexOf(".tiff") == sfd.FileName.Length - 5)
                            {
                                TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                encoder.Save(outStream);
                            }
                            else
                            {
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(rtb));
                                encoder.Save(outStream);
                            }
                        }
                    }
                    else if (sfd.FileName.LastIndexOf(".csv") == sfd.FileName.Length - 4)
                    {
                        operations.Save_csv(im2, sfd);
                    }
                }
                catch
                {
                    MessageBox.Show("Невозможно сохранить файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Отпускание левой кнопки мыши над полем "Border2"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!but_down) return;
            if (ctrl)
            {
                operations.mouse_left_up_x = e.GetPosition(im2).X;
                operations.mouse_left_up_y = e.GetPosition(im2).Y;
                if ((int)Slider_skew.Value == 0 && (int)Slider_rotate.Value == 0)
                {
                    im2.Source = operations.Cut(im2);
                    label2.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Для обрезки нужно чтобы изображение не было повёрнуто и скошено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                border_cut.Visibility = Visibility.Hidden;
                label_size_im1.Content = "Изображение весит " + operations.Size_im(im1) + " KB";
                label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
            }
            translate = false;
            but_down = false;
        }
        /// <summary>
        /// Перемещение мыши над полем "im"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void im2_MouseMove(object sender, MouseEventArgs e)
        {
            if (ctrl && but_down)
            {
                if (e.GetPosition(Grid1).X > border_cut.Margin.Left && e.GetPosition(Grid1).Y > border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(0);
                    border_cut.Width = e.GetPosition(Grid1).X - border_cut.Margin.Left;
                    border_cut.Height = e.GetPosition(Grid1).Y - border_cut.Margin.Top;
                }
                if (e.GetPosition(Grid1).X > border_cut.Margin.Left && e.GetPosition(Grid1).Y < border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(270);
                    border_cut.Height = e.GetPosition(Grid1).X - border_cut.Margin.Left;
                    border_cut.Width = border_cut.Margin.Top - e.GetPosition(Grid1).Y;
                }
                if (e.GetPosition(Grid1).X < border_cut.Margin.Left && e.GetPosition(Grid1).Y > border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(90);
                    border_cut.Height = border_cut.Margin.Left - e.GetPosition(Grid1).X;
                    border_cut.Width = e.GetPosition(Grid1).Y - border_cut.Margin.Top;
                }
                if (e.GetPosition(Grid1).X < border_cut.Margin.Left && e.GetPosition(Grid1).Y < border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(180);
                    border_cut.Width = border_cut.Margin.Left - e.GetPosition(Grid1).X;
                    border_cut.Height = border_cut.Margin.Top - e.GetPosition(Grid1).Y;
                }
                operations.mouse_left_up_x = e.GetPosition(im2).X;
                operations.mouse_left_up_y = e.GetPosition(im2).Y;
            }
            if (translate)
            {
                operations.transl_x_up = e.GetPosition(Border2).X;
                operations.transl_y_up = e.GetPosition(Border2).Y;
                im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
                label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
            }
        }
        /// <summary>
        /// Изменение значения Slider_rot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_rot_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            label_rotate.Content = (int)Slider_rotate.Value + "°";
            im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
            label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
        }
        /// <summary>
        /// Отпускание кнопки на клавиатуре
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrl = false;
                border_cut.Visibility = Visibility.Hidden;
            }
        }
        /// <summary>
        /// Изменение значения Slider_skew
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_skew_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            label_skew.Content = (int)Slider_skew.Value + "°";
            im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
            label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
        }
        /// <summary>
        /// Отпускание левой кнопки мыши над полем im2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void im2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!but_down) return;
            if (ctrl)
            {
                operations.mouse_left_up_x = e.GetPosition(im2).X;
                operations.mouse_left_up_y = e.GetPosition(im2).Y;
                if ((int)Slider_skew.Value == 0 && (int)Slider_rotate.Value == 0)
                {
                    im2.Source = operations.Cut(im2);
                    label2.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Для обрезки нужно чтобы изображение не было повёрнуто и скошено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                label_size_im1.Content = "Изображение весит " + operations.Size_im(im1) + " KB";
                label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
                border_cut.Visibility = Visibility.Hidden;
            }
            if (!ctrl)
            {
                translate = false;
            }
            but_down = false;
        }
        /// <summary>
        /// Нажатие левой кнопки мыши над полем Grid1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(Border2).X >= 0 && e.GetPosition(Border2).Y >= 0 && e.GetPosition(Border2).X <= Border2.Width && e.GetPosition(Border2).Y <= Border2.Height)
            {
                if (ctrl)
                {
                    border_cut.Visibility = Visibility.Visible;
                    border_cut.Margin = new Thickness(e.GetPosition(Grid1).X, e.GetPosition(Grid1).Y, 0, 0);
                    border_cut.Width = 0;
                    border_cut.Height = 0;
                    operations.mouse_left_down_x = e.GetPosition(im2).X;
                    operations.mouse_left_down_y = e.GetPosition(im2).Y;
                }
                if (!ctrl)
                {
                    translate = true;
                    operations.transl_x_down = e.GetPosition(im2).X;
                    operations.transl_y_down = e.GetPosition(im2).Y;
                }
                but_down = true;
            }
        }
        /// <summary>
        /// Перетаскивание мыши над полем Grid1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (but_down && e.GetPosition(Border2).X >= 0 && e.GetPosition(Border2).Y >= 0 && e.GetPosition(Border2).X <= Border2.Width && e.GetPosition(Border2).Y <= Border2.Height && ctrl)
            {
                if (e.GetPosition(Grid1).X > border_cut.Margin.Left && e.GetPosition(Grid1).Y > border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(0);
                    border_cut.Width = e.GetPosition(Grid1).X - border_cut.Margin.Left;
                    border_cut.Height = e.GetPosition(Grid1).Y - border_cut.Margin.Top;
                }
                if (e.GetPosition(Grid1).X > border_cut.Margin.Left && e.GetPosition(Grid1).Y < border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(270);
                    border_cut.Height = e.GetPosition(Grid1).X - border_cut.Margin.Left;
                    border_cut.Width = border_cut.Margin.Top - e.GetPosition(Grid1).Y;
                }
                if (e.GetPosition(Grid1).X < border_cut.Margin.Left && e.GetPosition(Grid1).Y > border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(90);
                    border_cut.Height = border_cut.Margin.Left - e.GetPosition(Grid1).X;
                    border_cut.Width = e.GetPosition(Grid1).Y - border_cut.Margin.Top;
                }
                if (e.GetPosition(Grid1).X < border_cut.Margin.Left && e.GetPosition(Grid1).Y < border_cut.Margin.Top)
                {
                    border_cut.RenderTransform = new RotateTransform(180);
                    border_cut.Width = border_cut.Margin.Left - e.GetPosition(Grid1).X;
                    border_cut.Height = border_cut.Margin.Top - e.GetPosition(Grid1).Y;
                }
                operations.mouse_left_up_x = e.GetPosition(im2).X;
                operations.mouse_left_up_y = e.GetPosition(im2).Y;
            }
            if (e.GetPosition(Border2).X >= 0 && e.GetPosition(Border2).Y >= 0 && e.GetPosition(Border2).X <= Border2.Width && e.GetPosition(Border2).Y <= Border2.Height && translate)
            {
                operations.transl_x_up = e.GetPosition(Border2).X;
                operations.transl_y_up = e.GetPosition(Border2).Y;
                im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
                label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
            }
        }
        /// <summary>
        /// Отпускание левой кнопки мыши над полем Grid1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!but_down) return;
            if (ctrl)
            {
                if ((int)Slider_skew.Value == 0 && (int)Slider_rotate.Value == 0)
                {
                    im2.Source = operations.Cut(im2);
                    label2.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("Для обрезки нужно чтобы изображение не было повёрнуто и скошено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                label_size_im1.Content = "Изображение весит " + operations.Size_im(im1) + " KB";
                label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
                border_cut.Visibility = Visibility.Hidden;
            }
            translate = false;
            but_down = false;
        }
        /// <summary>
        /// Изменение знчения Slider_scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Slider_scale.Value >= 1)
                label_scale.Content = Slider_scale.Value.ToString("##.##");
            else
                label_scale.Content = "0" + Slider_scale.Value.ToString("##.##");
            im2.RenderTransform = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
            label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
        }
        /// <summary>
        /// Нажатие на кнопку Изменение пксельного формата
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_change_pixel_format_Click(object sender, RoutedEventArgs e)
        {
        int ellapledTicks = DateTime.Now.Millisecond;
            im2.Source = operations.Pixel_Format(im1, cmbbox.SelectedIndex);
            ellapledTicks = DateTime.Now.Millisecond - ellapledTicks;
            label_time.Content = "Время преобразования = " + Math.Abs(ellapledTicks).ToString() + " milliseconds";
            im2.Source = operations.Cut(im2);
            label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
        }
        /// <summary>
        /// Нажатие кнопки на клавиатуре
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) ctrl = true;
        }
        /// <summary>
        /// Нажатие левой кнопки мыши над полем im2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void im2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ctrl)
            {
                border_cut.Visibility = Visibility.Visible;
                border_cut.Margin = new Thickness(e.GetPosition(Grid1).X, e.GetPosition(Grid1).Y, 0, 0);
                border_cut.Width = 0;
                border_cut.Height = 0;
                operations.mouse_left_down_x = e.GetPosition(im2).X;
                operations.mouse_left_down_y = e.GetPosition(im2).Y;
            }
            else
            {
                translate = true;
                operations.transl_x_down = e.GetPosition(im2).X;
                operations.transl_y_down = e.GetPosition(im2).Y;
            }
            but_down = true;
        }
        /// <summary>
        /// Прокрутка колёсика мыши над полем im2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void im2_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ctrl)
            {
                try
                {
                    if (e.Delta > 0)
                    {
                        Slider_scale.Value += 0.1;
                    }
                    if (e.Delta < 0)
                    {
                        Slider_scale.Value -= 0.1;
                    }
                    double e_x = e.GetPosition(im2).X;
                    double e_y = e.GetPosition(im2).Y;
                    im2.RenderTransform = operations.Transforme(e_x, e_y, im2, Slider_scale.Value, (int)Slider_skew.Value, (int)Slider_rotate.Value);
                    label_size_im2.Content = "Преобразованное изображение весит " + operations.Size_im(im2) + " KB";
                    label1.Visibility = Visibility.Hidden;
                }
                catch
                {
                }
            }
        }
    }
    public class ImageWrapper : IDisposable, IEnumerable<System.Drawing.Point>
    {
        /// <summary>
        /// Ширина изображения
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высота изображения
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Цвет по-умолачнию (используется при выходе координат за пределы изображения)
        /// </summary>
        public System.Drawing.Color DefaultColor { get; set; }

        private byte[] data;//буфер исходного изображения
        private byte[] outData;//выходной буфер
        private int stride;
        private BitmapData bmpData;
        private Bitmap bmp;

        /// <summary>
        /// Создание обертки поверх bitmap.
        /// </summary>
        /// <param name="copySourceToOutput">Копирует исходное изображение в выходной буфер</param>
        public ImageWrapper(Bitmap bmp, bool copySourceToOutput = false)////////////////////////////////
        {
            Width = bmp.Width;
            Height = bmp.Height;
            this.bmp = bmp;
            bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            stride = bmpData.Stride;

            data = new byte[stride * Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

            outData = copySourceToOutput ? (byte[])data.Clone() : new byte[stride * Height];
        }
        /// <summary>.
        /// Возвращает пиксел из исходнго изображения.
        /// Либо заносит пиксел в выходной буфер.
        /// </summary>
        public System.Drawing.Color this[int x, int y]
        {
            get
            {
                var i = GetIndex(x, y);
                return i < 0 ? DefaultColor : System.Drawing.Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
            }

            set
            {
                var i = GetIndex(x, y);
                if (i >= 0)
                {
                    outData[i] = value.B;
                    outData[i + 1] = value.G;
                    outData[i + 2] = value.R;
                    outData[i + 3] = value.A;
                };
            }
        }
        /// <summary>
        /// Возвращает пиксел из исходнго изображения.
        /// Либо заносит пиксел в выходной буфер.
        /// </summary>
        public System.Drawing.Color this[System.Drawing.Point p]
        {
            get { return this[p.X, p.Y]; }
            set { this[p.X, p.Y] = value; }
        }

        /// <summary>
        /// Заносит в выходной буфер значение цвета, заданные в double.
        /// Допускает выход double за пределы 0-255.
        /// </summary>
        public void SetPixel(System.Drawing.Point p, double r, double g, double b)////////////////////////
        {
            if (r < 0) r = 0;
            if (r >= 256) r = 255;
            if (g < 0) g = 0;
            if (g >= 256) g = 255;
            if (b < 0) b = 0;
            if (b >= 256) b = 255;

            this[p.X, p.Y] = System.Drawing.Color.FromArgb((int)r, (int)g, (int)b);
        }

        int GetIndex(int x, int y)
        {
            return (x < 0 || x >= Width || y < 0 || y >= Height) ? -1 : x * 4 + y * stride;
        }

        /// <summary>
        /// Заносит в bitmap выходной буфер и снимает лок.
        /// Этот метод обязателен к исполнению (либо явно, лмбо через using)
        /// </summary>
        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.Copy(outData, 0, bmpData.Scan0, outData.Length);
            bmp.UnlockBits(bmpData);
        }

        /// <summary>
        /// Перечисление всех точек изображения
        /// </summary>
        public IEnumerator<System.Drawing.Point> GetEnumerator()///////////////////////////
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    yield return new System.Drawing.Point(x, y);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Operations
    {
        /// <summary>
        /// transl_x_up - Координата y при натисненні лівої кнопкою миші при перетягуванні зображення
        /// transl_x_down - Координата x при натисненні лівої кнопкою миші при перетягуванні зображення
        /// transl_y_up -Координата y при піднятті лівої кнопкою миші при перетягуванні зображення 
        /// transl_y_down - Координата x при піднятті лівої кнопкою миші при перетягуванні зображення
        /// mouse_left_down_x - Координата x при натисненні лівої кнопкою миші при обрізанні зображення
        /// mouse_left_up_x - Координата x при піднятті лівої кнопкою миші при обрізанні зображення
        /// mouse_left_down_y - Координата y при натисненні лівої кнопкою миші при обрізанні зображення
        /// mouse_left_up_y - Координата y при піднятті лівої кнопкою миші при обрізанні зображення
        /// </summary>
        public double transl_x_up, transl_x_down, transl_y_up, transl_y_down, mouse_left_down_x, mouse_left_up_x, mouse_left_down_y, mouse_left_up_y;
        /// <summary>
        /// Метод обрезки изображения
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        public ImageSource Cut(Image im)
        {
            System.Drawing.Image imm = ImageWpfToGDI(im.Source);
            Bitmap bmp = new Bitmap(imm);
            using (var wr = new ImageWrapper(bmp))
            {
                if (mouse_left_down_x < mouse_left_up_x)
                    for (int x = (int)(mouse_left_down_x * (imm.Width / im.Width)); x < mouse_left_up_x * (imm.Width / im.Width); x++)
                    {
                        if (mouse_left_down_y < mouse_left_up_y)
                            for (int y = (int)(mouse_left_down_y * (imm.Width / im.Width)); y < mouse_left_up_y * (imm.Width / im.Width); y++)
                            {
                                wr[x, y] = wr[x, y];
                            }
                        else
                            for (int y = (int)(mouse_left_up_y * (imm.Width / im.Width)); y < mouse_left_down_y * (imm.Width / im.Width); y++)
                            {
                                wr[x, y] = wr[x, y];
                            }
                    }
                else
                    for (int x = (int)(mouse_left_up_x * (imm.Width / im.Width)); x < mouse_left_down_x * (imm.Width / im.Width); x++)
                    {
                        if (mouse_left_down_y < mouse_left_up_y)
                            for (int y = (int)(mouse_left_down_y * (imm.Width / im.Width)); y < mouse_left_up_y * (imm.Width / im.Width); y++)
                            {
                                wr[x, y] = wr[x, y];
                            }
                        else
                            for (int y = (int)(mouse_left_up_y * (imm.Width / im.Width)); y < mouse_left_down_y * (imm.Width / im.Width); y++)
                            {
                                wr[x, y] = wr[x, y];
                            }
                    }
            }
            imm = bmp;
            im.Source = imageToImgSource(imm);
            return im.Source;
        }
        /// <summary>
        /// Метод 
        /// </summary>
        /// <param name="e_x"></param>
        /// <param name="e_y"></param>
        /// <param name="im"></param>
        /// <param name="scale"></param>
        /// <param name="skew"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public TransformGroup Transforme(double e_x, double e_y, Image im, double scale, int skew, int rot)
        {
            ScaleTransform scalet = new ScaleTransform(scale, scale, e_x, e_y);
            SkewTransform skewt = new SkewTransform(skew, skew, im.Margin.Left + im.Width / 2, im.Margin.Top + im.Height / 2);
            RotateTransform rott = new RotateTransform(rot, im.Margin.Left + im.Width / 2, im.Margin.Top + im.Height / 2);
            TranslateTransform transl = new TranslateTransform((transl_x_up - transl_x_down), (transl_y_up - transl_y_down));
            TransformGroup transf = new TransformGroup();
            transf.Children.Add(transl);
            transf.Children.Add(scalet);
            transf.Children.Add(skewt);
            transf.Children.Add(rott);
            return transf;
        }
        /// <summary>
        /// Сохранение изображения в формате *.csv
        /// </summary>
        /// <param name="im"></param>
        /// <param name="sfd"></param>
        public void Save_csv(Image im, SaveFileDialog sfd)
        {
            System.Drawing.Image imm = ImageWpfToGDI(im.Source);
            Bitmap bmp = new Bitmap(imm);
            int i = 0;
            using (System.IO.StreamWriter file = new StreamWriter(sfd.FileName, true))
            {
                using (var wr = new ImageWrapper(bmp))
                {
                    foreach (var p in wr)
                    {
                        file.Write(" ");
                        file.Write(wr[p].R > (255 / 2) ? (byte)1 : (byte)0);

                        i++;
                        if (i >= imm.Width)
                        {
                            i = 0;
                            file.WriteLine();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Преобразование пиксельного формата
        /// </summary>
        /// <param name="im"></param>
        /// <param name="but_pixel_down"></param>
        /// <returns></returns>
        public ImageSource Pixel_Format(Image im, int but_pixel_down)
        {
            System.Drawing.Image imm = ImageWpfToGDI(im.Source);
            Bitmap bmp = new Bitmap(imm);
            switch (but_pixel_down)
            {
                case 0://24
                    using (var wr = new ImageWrapper(bmp))
                    {
                        foreach (var p in wr)
                        {
                            wr.SetPixel(p, wr[p].R, wr[p].G, wr[p].B);
                        }
                    }
                    break;
                case 1://16    
                    using (var wr = new ImageWrapper(bmp))
                    {
                        foreach (var p in wr)
                        {
                            wr.SetPixel(p, wr[p].R - (wr[p].R % 8), wr[p].G - (wr[p].G % 8), wr[p].B - (wr[p].B % 8));
                        }
                    }
                    break;
                case 2://8    
                    using (var wr = new ImageWrapper(bmp))
                    {
                        foreach (var p in wr)
                        {
                            wr.SetPixel(p, (int)(wr[p].R * 0.3) + (int)(wr[p].G * 0.59) + (int)(wr[p].B * 0.11), (int)(wr[p].R * 0.3) + (int)(wr[p].G * 0.59) + (int)(wr[p].B * 0.11), (int)(wr[p].R * 0.3) + (int)(wr[p].G * 0.59) + (int)(wr[p].B * 0.11));
                        }
                    }
                    break;
                case 3://4
                    using (var wr = new ImageWrapper(bmp))
                    {
                        foreach (var p in wr)
                        {
                            wr.SetPixel(p, (int)(wr[p].R * 0.15) + (int)(wr[p].G * 0.295) + (int)(wr[p].B * 0.55), (int)(wr[p].R * 0.15) + (int)(wr[p].G * 0.295) + (int)(wr[p].B * 0.55), (int)(wr[p].R * 0.15) + (int)(wr[p].G * 0.295) + (int)(wr[p].B * 0.55));
                        }
                    }
                    break;
                case 4://1
                    using (var wr = new ImageWrapper(bmp))
                    {
                        foreach (var p in wr)
                        {
                            wr.SetPixel(p, (wr[p].R + wr[p].G + wr[p].B) > (255 * 1.5) ? 255 : 0, (wr[p].R + wr[p].G + wr[p].B) > (255 * 1.5) ? 255 : 0, (wr[p].R + wr[p].G + wr[p].B) > (255 * 1.5) ? 255 : 0);
                        }
                    }
                    break;
                default:
                    break;
            }
            imm = bmp;
            return imageToImgSource(imm);
        }
        /// <summary>
        /// Изменение изображения из System.Windows.Media.ImageSource в System.Drawing.Image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private System.Drawing.Image ImageWpfToGDI(System.Windows.Media.ImageSource image)
        {
            MemoryStream ms = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image as System.Windows.Media.Imaging.BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }
        /// <summary>
        /// Изменение изображения из System.Drawing.Image image в System.Windows.Media.ImageSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private ImageSource imageToImgSource(System.Drawing.Image image)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                var img_source = new BitmapImage();
                img_source.BeginInit();
                img_source.UriSource = null;
                img_source.CacheOption = BitmapCacheOption.OnLoad;
                img_source.StreamSource = ms;
                img_source.EndInit();
                return img_source;
            }
        }
        /// <summary>
        /// Метод, который возвращает размер изображения
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        public long Size_im(Image im)
        {
            FileStream f_info = new FileStream(".png", FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)im.Width, (int)im.Height, 96, 96, PixelFormats.Pbgra32);
            im.Measure(new Size((int)im.Width, (int)im.Height));
            im.Arrange(new Rect(new Size((int)im.Width, (int)im.Height)));
            rtb.Clear();
            rtb.Render(im);
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(f_info);
            long lenght = f_info.Length / 1024;
            f_info.Dispose();
            return lenght;
        }
    }
}