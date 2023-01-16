using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CA1416 // Validate platform compatibility
namespace CgiBin
{
    public class Raster
    {
        public static string file = "./Resources/_stats.png";
        public static string texture = "./Resources/texture.png";
        public static bool renderBackground = false;
        static readonly float ratio = 16f / 9f;
        static int width = 240;
        static int height(string text) => (int)((width * ratio) + lines(text) * fontSize);
        static int height(int count) => (int)((width * ratio) + count * fontSize);
        static int lines(string text) => text.Count(t => t == '\n');
        static double fontSize = 12d;
        private Raster() { }
        public static string RasterizeToFile(int width, List<Entry> list, string header, StatType type)
        {
            var green = Color.FromArgb(0, 255, 0);
            float fontSize = 18f;
            Raster.fontSize = fontSize + fontSize / 40d;// / 0.63265335d;
            int count = 0;
            int offset = 0;
            int output = 0;
            switch (type)
            {
                case StatType.Monthly:
                    count = list.Count(t => t.data.total > 0);
                    break;
                case StatType.Weekly:
                    count = list.Count(t => t.data.weekly > 0);
                    break;
            }
            int _height = height(count);
            using (Bitmap bitmap = new Bitmap(width, _height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.FillRectangle(new SolidBrush(green), new Rectangle(0, 0, width, _height));
                    if (renderBackground)
                    {
                        var draw = DrawHelper.ErrorResult(width, _height, 24);
                        graphics.DrawImage(draw, Point.Empty);
                    }
                    for (int i = 0; i < count; i++)
                    {
                        switch (type)
                        {
                            case StatType.Monthly:
                                output = list[i].data.total;
                                break;
                            case StatType.Weekly:
                                output = list[i].data.weekly;
                                break;
                        }
                        int size = 32;
                        var image_brush = DrawHelper.ErrorResult((int)(width * 0.6f), 16, 8);
                        var image_icon = DrawHelper.ErrorResult(size, size, 8);
                        TextureBrush texture_brush = new TextureBrush(Bitmap.FromFile(texture));
                        SolidBrush brush = null;
                        switch (list[i].faction)
                        {
                            case Faction.TR:
                                brush = new SolidBrush(Color.Red);
                                goto default;
                            case Faction.NC:
                                brush = new SolidBrush(Color.Blue);
                                goto default;
                            case Faction.VS:
                                brush = new SolidBrush(Color.Purple);
                                goto default;
                            default:
                                int offY = 4;
                                int top = 40;
                                int headerTop = top / 5;
                                float marginIcon = 0.02f;
                                float marginText = 0.1f;
                                float marginTextNum = 0.8f;
                                float marginBrush = 0.96f;
                                SolidBrush border = new SolidBrush(Color.DarkSlateGray);

                                brush = new SolidBrush(Color.LightGray);
                                var icon = DrawHelper.TextureMask(image_icon, DrawHelper.Mask_Circle(image_icon.Width, green), green);
                                
                                Rectangle rect = new Rectangle((int)(width * marginIcon), i * (size + offY) + top, (int)(width * marginBrush), size);
                                graphics.FillRectangle(texture_brush, rect);

                                graphics.DrawImage(icon, new PointF(width * marginIcon, i * (size + offY) + top));

                                if (list[i].index == 0)
                                {
                                    offset = 1;
                                }
                                for (int m = -2; m <= 2; m++)
                                {
                                    for (int n = -2; n <= 2; n++)
                                    {
                                        graphics.DrawString(header, new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold | FontStyle.Underline), border, new PointF(width * marginIcon + m, headerTop));
                                        graphics.DrawString($"{list[i].index + offset}. {list[i].data.username}", new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold), border, new PointF(width * marginText + m, i * (size + offY) + n + top));
                                        graphics.DrawString($"{output}", new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold), border, new PointF(width * marginTextNum + m, i * (size + offY) + n + top));
                                    }
                                }
                                graphics.DrawString(header, new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold | FontStyle.Underline), brush, new PointF(width * marginIcon, headerTop));
                                graphics.DrawString($"{list[i].index + offset}. {list[i].data.username}", new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold), brush, new PointF(width * marginText, i * (size + offY) + top));
                                graphics.DrawString($"{output}", new Font(FontFamily.Families.First(t => t.Name == "Tahoma"), fontSize, FontStyle.Bold), brush, new PointF(width * marginTextNum, i * (size + offY) + top));
                                break;
                        }
                    }
                }
                bitmap.MakeTransparent(green);
                bitmap.Save(file, ImageFormat.Png);
            }
            return file;
        }
    }
    public class DrawHelper
    {
        sealed class Error
        {
            internal static int[,] GetArray(int width, int height, int size = 16)
            {
                int i = width / size;
                int j = height / size;
                int[,] brush = new int[i, j];
                int num = -1;
                for (int n = 0; n < brush.GetLength(1); n++)
                {
                    for (int m = 0; m < brush.GetLength(0); m++)
                    {
                        if (n > 0 && m == 0)
                        {
                            num = brush[m, n - 1] * -1;
                            _write(ref brush, m, n, num);
                            continue;
                        }
                        _write(ref brush, m, n, num *= -1);
                    }
                }
                return brush;
            }
            static void _write(ref int[,] brush, int m, int n, int value)
            {
                brush[m, n] = value;
            }
        }
        public static Bitmap ErrorResult(int width, int height, int size = 16)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(result))
            {
                int[,] brush = Error.GetArray(width, height, size);
                for (int i = 0; i < brush.GetLength(0); i++)
                {
                    for (int j = 0; j < brush.GetLength(1); j++)
                    {
                        int x = i * size;
                        int y = j * size;
                        switch (brush[i, j])
                        {
                            case -1:
                                gfx.FillRectangle(Brushes.MediumPurple, new Rectangle(x, y, size, size));
                                gfx.DrawRectangle(Pens.Purple, new Rectangle(x, y, size - 1, size - 1));
                                break;
                            case 1:
                                gfx.FillRectangle(Brushes.Black, new Rectangle(x, y, size, size));
                                gfx.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(50, 50, 50))), new Rectangle(x, y, size - 1, size - 1));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return result;
        }
        public static Bitmap Mask_Circle(int size, Color mask)
        {
            float offset = 0.95f;
            Bitmap result = new Bitmap(size, size);
            using (Graphics gfx = Graphics.FromImage(result))
            {
                gfx.FillRectangle(new SolidBrush(mask), new RectangleF(0, 0, size, size));
                gfx.FillEllipse(Brushes.Black, new RectangleF(0, 0, size * offset, size * offset));
                result.MakeTransparent(Color.Black);
            }
            return result;
        }
        public static Image TextureMask(Bitmap image, Bitmap mask, Color transparency)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            using (Graphics _mask = Graphics.FromImage(mask))
            {
                using (Graphics _image = Graphics.FromImage(image))
                {
                    if (mask.Width < image.Width && mask.Height < image.Height)
                    {
                        _mask.ScaleTransform((float)image.Width / mask.Width, (float)image.Height / mask.Width);
                    }
                    _image.DrawImage(mask, Point.Empty);
                }
                image.MakeTransparent(transparency);
                Graphics gfx3 = Graphics.FromImage(result);
                gfx3.DrawImage(image, Point.Empty);
                gfx3.Dispose();
            }
            return result;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
