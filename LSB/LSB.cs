using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Drawing;

namespace LSB
{
    internal class LSBHider
    
    {
        Random random = new Random();
        public List<List<int>> coords;
        public Bitmap b_image;

        public void Log(string text)
        {
            File.AppendAllText("logs.txt", text + '\n');
        }
        public string HideByCoords(string bit_text, Bitmap _b_image, List<List<int>> _coords=null)
        {
            Log($"in hide by coords: bit_text == {bit_text}");
            b_image = _b_image;
            int length = bit_text.Length;
            int width = b_image.Width;
            int height = b_image.Height;
            if (length > width * height) return "Слишком длинный текст";
            if (_coords != null && length != _coords.Count)
            {
                return "Не хватает координат";
            }
            if (_coords == null)
            {
                _coords = new List<List<int>>();
                for (int i = 0; i < length; i++)
                {
                    _coords.Add(new List<int> { random.Next(0, width), random.Next(0, height) });
                }
            }
            coords = _coords;
            int n = 0;
            Color pixel_Color;
            foreach (List<int> pair in coords)
            {
                try
                {
                    pixel_Color = b_image.GetPixel(pair[0], pair[1]);
                }
                catch (ArgumentOutOfRangeException ae)
                {
                    return "Координаты некорректны";
                }
                Color new_pixel_Color;
                if (pixel_Color.R % 2 < bit_text[n] % 2)
                {
                    new_pixel_Color = Color.FromArgb(pixel_Color.R + 1, pixel_Color.G, pixel_Color.B);
                }
                else if (pixel_Color.R % 2 > bit_text[n] % 2)
                {
                    new_pixel_Color = Color.FromArgb(pixel_Color.R - 1, pixel_Color.G, pixel_Color.B);
                }
                else
                {
                    new_pixel_Color = pixel_Color;
                }

                b_image.SetPixel(pair[0], pair[1], new_pixel_Color);
            }
            
            return "DONE";
        }
    }
    internal class LSBReader
    {
        public string text { get; set; }
        public string ReadByCoords(List<List<int>> coords, Bitmap b_img)
        {
            string byte_result = String.Empty;
            string result = String.Empty;
            foreach (List<int> pair in coords)
            {
                try
                {
                    int b = b_img.GetPixel(pair[0], pair[1]).R % 2;
                    byte_result += b.ToString();
                }
                catch (ArgumentOutOfRangeException ae)
                {
                    return "Координаты некорректны";
                }

            }

            for (int i = 0; i < byte_result.Length; i+=7)
            {
                string to_res = String.Empty;
                for (int j = i; j < i + 7; j++) to_res += byte_result[j];
                char sym = (char)Convert.ToInt64(to_res, 2);
                result += sym;
            }
            text = result;
            return "DONE";
        }

    }
}
