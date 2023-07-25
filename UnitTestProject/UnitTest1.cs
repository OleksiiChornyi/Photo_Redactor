using Microsoft.VisualStudio.TestTools.UnitTesting;
using Academic_year_project;
using Image = System.Windows.Controls.Image;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        Scale_handler sh = new Scale_handler();
        [TestMethod]
        public void Size()
        {
            Image im1 = new Image();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri("data.jpg");
            bmp.EndInit();
            im1.Source = bmp;
            im1.Height = 200;
            im1.Width = 200;
            long excepted = (long)im1.ActualHeight;
            System.Drawing.Image im2 = sh.Drawing(im1.Source);
            long actual = (long)im2.Height;
            CollectionAssert.Equals(excepted, actual);
        }

        //[TestMethod]
        public void Transforme()
        {
            Image im1 = new Image();
            im1.Height = 200;
            im1.Width = 200;
            ScaleTransform scalet = new ScaleTransform(2, 2, im1.Width / 2, im1.Height / 2);
            SkewTransform skewt = new SkewTransform(23, 23, im1.Margin.Left + im1.Width / 2, im1.Margin.Top + im1.Height / 2);
            RotateTransform rott = new RotateTransform(5, im1.Margin.Left + im1.Width / 2, im1.Margin.Top + im1.Height / 2);
            TranslateTransform transl = new TranslateTransform((0 - 0), (0 - 0));
            TransformGroup excepted = new TransformGroup();
            excepted.Children.Add(transl);
            excepted.Children.Add(scalet);
            excepted.Children.Add(skewt);
            excepted.Children.Add(rott);
            Image im2 = new Image();
            im2.Height = 200;
            im2.Width = 200;
            //TransformGroup actual = operations.Transforme(im2.Width / 2, im2.Height / 2, im2, 2, 23, 5);
            //CollectionAssert.Equals(excepted, actual);
        }
    }
}
