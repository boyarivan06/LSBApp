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
using System.Windows.Navigation;
using System.IO;
using System.Windows.Shapes;
using System.Drawing;
using System.Text.Json;

namespace LSB
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Action> History = new List<Action>();
        BitmapImage BI;
        BitmapImage BI2;
        // C:\Users\apple\source\repos\LSB\LSB\img\test.png
        // Y:\Downloads
        LSBHider lsb_hider = new LSBHider();
        LSBReader lsb_reader = new LSBReader();
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!File.Exists("history.json")) File.Create("history.json");
            File.AppendAllText("history.json", JsonSerializer.Serialize(History));
        }

        private void openImage_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(image_name_box.Text))
            {
                
                BI = new BitmapImage();
                BI.BeginInit();
                BI.UriSource = new Uri(image_name_box.Text, UriKind.Absolute);
                BI.EndInit();
                working_img.Stretch = Stretch.Fill;
                working_img.Source = BI;
                working_img.Visibility = Visibility.Visible;
                warningLabel.Content = String.Empty;
            }
            else
            {
                warningLabel.Content = "Файл не найден";
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            working_img.Visibility = Visibility.Collapsed;
            image_name_box.Text = String.Empty;
            coordsBox.Text = String.Empty;
            working_textBox.Text = String.Empty;
            saved_warning_label.Content = String.Empty;
            BI = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (BI == null) return;
            string text = working_textBox.Text;
            byte[] textBytes = Encoding.Unicode.GetBytes(text);
            text = string.Empty;
            foreach (byte b in textBytes)
            {
                var add = Convert.ToString(b, 2);
                while (add.Length < 7) add = '0' + add;
                text += add;
            } 
            string res = lsb_hider.HideByCoords(text, FromBitmapImagetoBitmap(BI));
            lsb_hider.b_image.Save(image_name_box.Text.Replace(".png", $"{DateTime.Now.Minute}.png"));
            if (res == "DONE")
            {
                ok_Label.Content = $"Текст успешно вcтроен, сохранено как ...{ DateTime.Now.Minute}.png";
                string text_coords = String.Empty;
                foreach (List<int> coord in lsb_hider.coords)
                {
                    text_coords += $"({coord[0]}, {coord[1]}), ";
                }
                coordsBox.Text = text_coords;
                History.Add(new Action { time = DateTime.Now, coords = lsb_hider.coords, image_path = image_name_box.Text, type=1 });
                
                
            }
            else
            {
                warningLabel.Content = res;
            }
        }

        private Bitmap FromBitmapImagetoBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
            
                return new Bitmap(bitmap);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (save_coords_path.Text != String.Empty)
            {
                try
                {
                    string slash = "\\";
                    if (save_coords_path.Text[save_coords_path.Text.Length - 1] == '\\') slash = String.Empty;
                    string path = save_coords_path.Text + slash + $"coords_{DateTime.Now.Hour}{DateTime.Now.Minute}.json";
                    //coordsBox.Text = path;
                    //System.Threading.Thread.Sleep(10000);
                    path = path.Replace('\\', '/');
                    File.WriteAllText(path, JsonSerializer.Serialize(lsb_hider.coords));
                    saved_warning_label.Content = $"Сохранено как {path}";
                }
                catch (DirectoryNotFoundException de)
                {
                    save_file_warningBox.Text = "Введенной директории не существует";
                }

            }
        }

        private void openImageToRead(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ImageToReadPathBox.Text))
            {

                BI2 = new BitmapImage();
                BI2.BeginInit();
                BI2.UriSource = new Uri(ImageToReadPathBox.Text, UriKind.Absolute);
                BI2.EndInit();
                ReadingImg.Stretch = Stretch.Fill;
                ReadingImg.Source = BI2;
                ReadingImg.Visibility = Visibility.Visible;
                warningLabel2.Content = String.Empty;
            }
            else
            {
                warningLabel2.Content = "Файл не найден";
            }


        }
        private void ReadedImageClose(object sender, RoutedEventArgs e)
        {
            ReadingImg.Visibility = Visibility.Collapsed;
            ImageToReadPathBox.Text = String.Empty;
            coords_file_path_Box.Text = String.Empty;
            result_textBox.Text = String.Empty;
            BI2 = null;
        }
        private void ReadImage(object sender, RoutedEventArgs e)
        {

            if (BI2 == null) return;
            List<List<int>> coords_list = new List<List<int>>();
            try
            {
                coords_list = JsonSerializer.Deserialize<List<List<int>>>(File.ReadAllText(coords_file_path_Box.Text));

            }
            catch (FileNotFoundException)
            {
                warningLabel2.Content = "Файл с координатами не найден";
            } 


            string res = lsb_reader.ReadByCoords(coords_list, FromBitmapImagetoBitmap(BI2));
            if (res == "DONE")
            {
                ok_Label2.Content = "Текст успешно извлечен";


                result_textBox.Text = lsb_reader.text;
                History.Add(new Action { time = DateTime.Now, coords = lsb_hider.coords, image_path = image_name_box.Text, type=2 });


            }
            else
            {
                warningLabel2.Content = res;
            }
        }

        private void saveText(object sender, RoutedEventArgs e)
        {
            if (saveTextCoordsBox.Text != String.Empty)
            {
                try
                {
                    string slash = "\\";
                    if (saveTextCoordsBox.Text[saveTextCoordsBox.Text.Length - 1] == '\\') slash = String.Empty;
                    string path = saveTextCoordsBox.Text + slash + $"text_{DateTime.Now.Hour}{DateTime.Now.Minute}.txt";
                    
                    //System.Threading.Thread.Sleep(10000);
                    path = path.Replace('\\', '/');
                    File.WriteAllText(path, JsonSerializer.Serialize(lsb_hider.coords));
                }
                catch (DirectoryNotFoundException de)
                {
                    save_file_warningBox.Text = "Введенной директории не существует";
                }

            }
        }
    }
}
/*
 * (568, 610), (63, 83), (366, 638), (581, 875), (1017, 1188), (1055, 768), (801, 880), (878, 60), (320, 426), (304, 1063), (284, 637), (1003, 335), (669, 1092), (1074, 1213), (752, 1234), (1091, 219), (1189, 571), (578, 575), (142, 953), (1126, 997), (67, 1071), (536, 1335), (189, 667), (978, 989), (1652, 670), (560, 272), (484, 958), (1487, 1143), (1721, 1078), (1329, 541), (1688, 577), (452, 872), (544, 547), (1753, 10), (634, 61), (1824, 647), (1344, 591), (1396, 503), (341, 761), (272, 1186), (526, 1158), (18, 1244), (1140, 519), (1701, 1273), (1183, 645), (955, 393), (1867, 276), (1569, 840), (1022, 885), (1294, 958), (1050, 699), (728, 1138), (982, 1127), (915, 1128), (1807, 576), (1232, 488), (1212, 1147), (218, 67), (776, 452), (61, 1243), (935, 0), (715, 1183), (1013, 931), (908, 699), (1129, 173), (1554, 730), (1638, 606), (584, 1051), (866, 242), (730, 1068), (1684, 106), (1829, 283), (1539, 205), (1213, 208), (162, 1071), (380, 289), (1158, 1063), (384, 762), (247, 713), (554, 1149), (644, 1333), (425, 1138), (1123, 164), (917, 273), (995, 475), (1110, 320), (1777, 343), 
 */