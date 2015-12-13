using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Helpers
{
    public class ImageHelper
    {
        public static void CreateThumbnailImage(string ImagePath, string ThumbnailPath, int Width, int Height, bool Cut = false)
        {
            Bitmap bmp1;//, bmp2;
            if (!File.Exists(ImagePath))
            {
                throw new FileNotFoundException(ImagePath);
            }
            bmp1 = new Bitmap(ImagePath);
            //int width = Width, height = Height;
            RotateFlip(bmp1);

            Bitmap bmp2 = CreateThumbnailImage(bmp1, Width, Height, Cut);
            bmp2.Save(ThumbnailPath, GetImageEncoder(ThumbnailPath), GetQualityParameters(100));
            bmp1.Dispose();
            bmp2.Dispose();

            return;

            //if (bmp1.Width / (decimal)Width > bmp1.Height / (decimal)Height)
            //{
            //    width = bmp1.Width > Width ? Width : bmp1.Width;
            //    height = width * bmp1.Height / bmp1.Width;
            //}
            //else if (bmp1.Width / (decimal)Width < bmp1.Height / (decimal)Height)
            //{
            //    height = bmp1.Height > Height ? Height : bmp1.Height;
            //    width = height * bmp1.Width / bmp1.Height;
            //}

            //if (bmp1.Width > width || bmp1.Height > height)
            //{
            //    Bitmap bmp2 = new Bitmap(width, height);
            //    Graphics graphics = Graphics.FromImage(bmp2);
            //    Matrix transform = new Matrix();
            //    float scale = (float)width / bmp1.Width;

            //    transform.Scale(scale, scale, MatrixOrder.Append);
            //    graphics.SetClip(new Rectangle(0, 0, width, height));
            //    graphics.Transform = transform;

            //    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //    graphics.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            //    bmp2.Save(ThumbnailPath, GetImageEncoder(ThumbnailPath), GetQualityParameters(ImageQuality));

            //    bmp2.Dispose();
            //    graphics.Dispose();
            //    transform.Dispose();
            //}
            //else
            //{
            //    bmp1.Save(ThumbnailPath);
            //}
        }

        public static void CreateThumbnailImage(string ImagePath, string ThumbnailPath, int Width, int Height, int RateX, int RateY, long ImageQuality)
        {
            Bitmap bmp1, bmp2;
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageQuality);
            encoderParams.Param = new System.Drawing.Imaging.EncoderParameter[] { encoderParam };

            if (!File.Exists(ThumbnailPath))
            {
                if (File.Exists(ImagePath))
                {
                    bmp1 = new Bitmap(ImagePath);
                    bmp2 = new Bitmap(bmp1.GetThumbnailImage((int)((decimal)bmp1.Width / ((decimal)bmp1.Height / (decimal)Height)), Height, null, IntPtr.Zero));
                    bmp1.Dispose();
                    bmp1 = Justify(bmp2, RateX, RateY);
                    bmp2.Dispose();
                    try
                    {
                        if (ImageQuality > 0)
                        {
                            bmp1.Save(ThumbnailPath, GetImageEncoder(ThumbnailPath), encoderParams);
                        }
                        else
                        {
                            bmp1.Save(ThumbnailPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        bmp1.Dispose();
                    }
                }
            }
        }

        public static void CreateThumbnailImage(Bitmap SourceImage, string DestinationPath, int Width, int RateX, int RateY, long ImageQuality)
        {
            int h = (int)((decimal)SourceImage.Height / ((decimal)SourceImage.Width / (decimal)Width));

            CreateThumbnailImage(SourceImage, DestinationPath, Width, h, RateX, RateY, ImageQuality);
        }

        public static void CreateThumbnailImage(Bitmap SourceImage, string DestinationPath, int Width, int Height, int RateX, int RateY, long ImageQuality)
        {
            Bitmap bmp1, bmp2;
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageQuality);
            encoderParams.Param = new System.Drawing.Imaging.EncoderParameter[] { encoderParam };

            if (RateX > 0)
            {
                bmp1 = Justify(SourceImage, RateX, RateY);
            }
            else
            {
                bmp1 = new Bitmap(SourceImage);
            }

            bmp2 = new Bitmap(bmp1.GetThumbnailImage((int)((decimal)bmp1.Width / ((decimal)bmp1.Height / (decimal)Height)), Height, null, IntPtr.Zero));
            bmp1.Dispose();
            try
            {
                if (ImageQuality > 0)
                {
                    bmp2.Save(DestinationPath, GetImageEncoder(DestinationPath), encoderParams);
                }
                else
                {
                    bmp2.Save(DestinationPath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                bmp2.Dispose();
            }
        }

        public static Bitmap CreateThumbnailImage(Bitmap SourceImage, int Width, int Height, int RateX, int RateY, bool Cut = true, Brush BackColor = null)
        {
            Bitmap bmp1, bmp2;

            if (Cut && BackColor != null)
            {
                bmp1 = Justify(SourceImage, RateX, RateY, BackColor);
            }
            else if (Cut)
            {
                bmp1 = Justify(SourceImage, RateX, RateY);
            }
            else
            {
                bmp1 = new Bitmap(SourceImage);

                float xr, yr;
                xr = (float)bmp1.Width / RateX;
                yr = (float)bmp1.Height / RateY;

                if (xr > yr)
                {
                    Height = (int)((float)bmp1.Height / ((float)bmp1.Width / (float)Width));
                }
                else if (yr > xr)
                {
                    Width = (int)((float)bmp1.Width / ((float)bmp1.Height / (float)Height));
                }
            }

            if (bmp1.Width <= Width && bmp1.Height <= Height)
                return new Bitmap(bmp1);

            //bmp2 = new Bitmap(bmp1.GetThumbnailImage(Width, Height, null, IntPtr.Zero));
            bmp2 = new Bitmap(Width, Height);
            Graphics graphics = Graphics.FromImage(bmp2);
            Matrix transform = new Matrix();
            float scale = (float)Width / bmp1.Width;

            transform.Scale(scale, scale, MatrixOrder.Append);
            graphics.SetClip(new Rectangle(0, 0, Width, Height));
            graphics.Transform = transform;

            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            bmp1.Dispose();
            graphics.Dispose();
            transform.Dispose();

            return bmp2;
        }

        public static Bitmap CreateThumbnailImage(Bitmap SourceImage, int Width)
        {
            int w, h;
            if (SourceImage.Width <= Width)
            {
                return new Bitmap(SourceImage);
            }
            w = Width;
            h = (int)((decimal)SourceImage.Height / ((decimal)SourceImage.Width / (decimal)Width));
            return CreateThumbnailImage(SourceImage, w, h);
        }

        public static Bitmap CreateThumbnailImage(Bitmap SourceImage, int Width, int Height, bool Cut = false)
        {
            if (SourceImage.Width <= Width && SourceImage.Height <= Height)
            {
                return new Bitmap(SourceImage);
            }
            return CreateThumbnailImage(SourceImage, Width, Height, Width, Height, Cut);
            //Bitmap bmp1, bmp2;
            //bmp1 = new Bitmap(SourceImage);
            //bmp2 = new Bitmap(bmp1.GetThumbnailImage(Width, Height, null, IntPtr.Zero));
            //bmp1.Dispose();
            //return bmp2;
        }

        public static string ImageToBase64(string ImagePath)
        {
            System.Drawing.Imaging.ImageFormat format;
            Bitmap bmp = null;
            string result = null;
            try
            {
                bmp = new Bitmap(ImagePath);
                result = string.Format("data:{0};base64,", GetImageFormat(ImagePath, out format));
                result += ImageToBase64(bmp, format);
            }
            catch (Exception ex)
            { }
            finally
            {
                if (bmp != null)
                    bmp.Dispose();
            }
            return result;
        }

        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            string base64String = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(imageBytes);
            }
            return base64String;
        }

        public static void AddRibbon(Stream SourceImage, string RibbonText, MemoryStream OutputStream)
        {
            Bitmap img = new Bitmap(SourceImage);
            Graphics gr = Graphics.FromImage(img);
            Font font = new Font("Tahoma", (float)12);
            Color color = Color.Red;
            double angle = -45;

            int h, x, y, textY;
            Point point;
            List<Point> points = new List<Point>();

            h = img.Height / 3;
            textY = h;

            point = new Point(h, 0);
            points.Add(point);
            point = new Point(h * 2, 0);
            points.Add(point);

            x = h * 2 - 1;
            while (x > 0)
            {
                point = new Point(x, h * 2 - x);
                points.Add(point);
                x--;
            }

            point = new Point(0, h * 2);
            points.Add(point);
            point = new Point(0, h);
            points.Add(point);

            y = h - 1;
            while (y > 0)
            {
                point = new Point(h - y, y);
                points.Add(point);
                y--;
            }

            int l = (int)Math.Sqrt((h * 2) * (h * 2) * 2);

            for (int i = 500; i > 0; i--)
            {
                font = new Font("Tahoma", i, FontStyle.Bold);
                SizeF sizef = gr.MeasureString(RibbonText, font, int.MaxValue);

                if (sizef.Height <= h / 2 && sizef.Width <= l)
                {
                    textY = (int)(h + (h - sizef.Height) / 4);
                    break;
                }
            }

            Brush brush = new SolidBrush(Color.FromArgb(200, 0xFF, 0, 0));
            StringFormat stringFormat = new StringFormat();

            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            gr.FillClosedCurve(brush, points.ToArray(), FillMode.Winding);

            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.RotateTransform((float)angle);
            gr.DrawString(RibbonText, font, Brushes.White, 0, textY, stringFormat);

            img.Save(OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            img.Dispose();
        }

        public static void AddWaterMark(Stream SourceImage, string WatermarkText, MemoryStream OutputStream)
        {
            Color color = Color.FromArgb(50, 241, 235, 105);
            AddWaterMark(SourceImage, WatermarkText, OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg, color);
        }

        public static void AddWaterMark(Stream SourceImage, string WatermarkText, MemoryStream OutputStream, Color Color)
        {
            AddWaterMark(SourceImage, WatermarkText, OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg, Color);
        }

        public static void AddWaterMark(Stream SourceImage, string WatermarkText, MemoryStream OutputStream, System.Drawing.Imaging.ImageFormat Format)
        {
            Color color = Color.FromArgb(50, 241, 235, 105);
            AddWaterMark(SourceImage, WatermarkText, OutputStream, Format, color);
        }

        public static void AddWaterMark(Stream SourceImage, string WatermarkText, MemoryStream OutputStream, System.Drawing.Imaging.ImageFormat Format, Color Color)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(SourceImage);
            Graphics gr = Graphics.FromImage(img);
            Font font = new Font("Tahoma", (float)40);
            double tangent = (double)img.Height / (double)img.Width;
            double angle = Math.Atan(tangent) * (180 / Math.PI);
            double halfHypotenuse = Math.Sqrt((img.Height * img.Height) + (img.Width * img.Width)) / 2;
            double sin, cos, opp1, adj1, opp2, adj2;

            for (int i = 500; i > 0; i--)
            {
                font = new Font("Tahoma", i, FontStyle.Bold);
                SizeF sizef = gr.MeasureString(WatermarkText, font, int.MaxValue);

                sin = Math.Sin(angle * (Math.PI / 180));
                cos = Math.Cos(angle * (Math.PI / 180));
                opp1 = sin * sizef.Width;
                adj1 = cos * sizef.Height;
                opp2 = sin * sizef.Height;
                adj2 = cos * sizef.Width;

                if (opp1 + adj1 < img.Height && opp2 + adj2 < img.Width)
                {
                    break;
                }
            }

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.RotateTransform((float)angle);
            gr.DrawString(WatermarkText, font, new SolidBrush(Color), new Point((int)halfHypotenuse, 0), stringFormat);

            img.Save(OutputStream, Format);
            img.Dispose();
        }

        public static void AddWaterMark(Stream SourceImage, Bitmap WatermarkImage, MemoryStream OutputStream, int Padding = 10)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(SourceImage);
            Graphics gr = Graphics.FromImage(img);

            gr.SmoothingMode = SmoothingMode.AntiAlias;

            int x = img.Width - WatermarkImage.Width - Padding;
            int y = img.Height - WatermarkImage.Height - Padding;

            //if (img.Width < WatermarkImage.Width)
            //{
            //    WatermarkImage = CreateThumbnailImage(WatermarkImage, img.Width);
            //    x = 0;
            //}
            //else
            //{
            //    x = (img.Width - WatermarkImage.Width) / 2;
            //}

            gr.DrawImage(WatermarkImage, new Rectangle(x, y, WatermarkImage.Width, WatermarkImage.Height));

            img.Save(OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            img.Dispose();
            gr.Dispose();
        }

        public static void AddWaterMarkCenter(Stream SourceImage, Bitmap WatermarkImage, MemoryStream OutputStream)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(SourceImage);
            Graphics gr = Graphics.FromImage(img);

            gr.SmoothingMode = SmoothingMode.AntiAlias;

            int x = 0;
            int y = 0;

            if (img.Width < WatermarkImage.Width)
            {
                WatermarkImage = CreateThumbnailImage(WatermarkImage, img.Width);
                x = 0;
            }
            else
            {
                x = (img.Width - WatermarkImage.Width) / 2;
            }
            if (img.Height < WatermarkImage.Height)
            {
                y = 0;
            }
            else
            {
                y = (img.Height - WatermarkImage.Height) / 2;
            }

            gr.DrawImage(WatermarkImage, new Rectangle(x, y, WatermarkImage.Width, WatermarkImage.Height));

            img.Save(OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            img.Dispose();
            gr.Dispose();
        }

        public static System.Drawing.Imaging.ImageFormat GetImageFormat(string ImagePath)
        {
            System.Drawing.Imaging.ImageFormat Format;
            GetImageFormat(ImagePath, out Format);
            return Format;
        }

        public static string GetImageFormat(string ImagePath, out System.Drawing.Imaging.ImageFormat Format)
        {
            string result;
            if (ImagePath.Contains(".jpg") || ImagePath.Contains(".jpeg"))
            {
                result = "image/jpeg";
                Format = System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            else if (ImagePath.Contains(".if"))
            {
                result = "image/gif";
                Format = System.Drawing.Imaging.ImageFormat.Gif;
            }
            else if (ImagePath.Contains(".png"))
            {
                result = "image/png";
                Format = System.Drawing.Imaging.ImageFormat.Png;
            }
            else if (ImagePath.Contains(".bmp"))
            {
                result = "image/bmp";
                Format = System.Drawing.Imaging.ImageFormat.Bmp;
            }
            else
            {
                result = "image/jpeg";
                Format = System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            return result;
        }

        public static System.Drawing.Imaging.ImageCodecInfo GetImageEncoder(string ImagePath)
        {
            System.Drawing.Imaging.ImageCodecInfo Encoder;
            System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            ImagePath = ImagePath.ToLower();
            if (ImagePath.Contains(".jpg") || ImagePath.Contains(".jpeg"))
            {
                Encoder = arrayICI.FirstOrDefault(val => val.FormatDescription.Contains("JPEG"));
            }
            else if (ImagePath.Contains(".gif"))
            {
                Encoder = arrayICI.FirstOrDefault(val => val.FormatDescription.Contains("GIF"));
            }
            else if (ImagePath.Contains(".png"))
            {
                Encoder = arrayICI.FirstOrDefault(val => val.FormatDescription.Contains("PNG"));
            }
            else if (ImagePath.Contains(".bmp"))
            {
                Encoder = arrayICI.FirstOrDefault(val => val.FormatDescription.Contains("BMP"));
            }
            else
            {
                Encoder = arrayICI.FirstOrDefault(val => val.FormatDescription.Contains("JPEG"));
            }
            return Encoder;
        }

        public static System.Drawing.Imaging.EncoderParameters GetQualityParameters(long ImageQuality)
        {
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageQuality);
            encoderParams.Param = new System.Drawing.Imaging.EncoderParameter[] { encoderParam };
            return encoderParams;
        }

        public static Bitmap Justify(Bitmap Image, int XRate, int YRate, Brush BackColor)
        {
            int x, y;
            int xn, yn;
            int sx, sy;
            decimal xc, yc;
            decimal rate;
            Bitmap result;

            y = Image.Height;
            x = Image.Width;
            rate = (decimal)x / (decimal)y;
            rate = rate - ((decimal)XRate / (decimal)YRate);
            if (Math.Abs(rate) < (decimal)0.01)
            {
                return new Bitmap(Image);
            }

            xc = (decimal)x / (decimal)XRate;
            yc = (decimal)y / (decimal)YRate;
            if (xc > yc)
            {
                xn = x;
                yn = (int)((decimal)x / (decimal)XRate * (decimal)YRate);
                sx = 0;
                sy = (yn - y) / 2;
            }
            else
            {
                xn = (int)((decimal)y / (decimal)YRate * (decimal)XRate);
                yn = y;
                sx = (xn - x) / 2;
                sy = 0;
            }

            result = new Bitmap(xn, yn);

            Rectangle rectangle = new Rectangle(0, 0, xn, yn);
            Graphics graphics = System.Drawing.Graphics.FromImage(result);

            graphics.FillRectangle(BackColor, rectangle);
            rectangle = new Rectangle(sx, sy, x, y);
            graphics.DrawImage(Image, rectangle);
            graphics.Dispose();

            return result;
        }

        public static Bitmap Justify(Bitmap Image, int XRate, int YRate)
        {
            int x, y;
            int sx, sy;
            decimal rate;
            decimal xc, yc;
            Bitmap result = new Bitmap(Image);

            y = Image.Height;
            x = Image.Width;
            rate = (decimal)x / (decimal)y;
            rate = rate - ((decimal)XRate / (decimal)YRate);
            if (Math.Abs(rate) < (decimal)0.01)
            {
                return result;
            }

            xc = (decimal)x / (decimal)XRate;
            yc = (decimal)y / (decimal)YRate;
            if (xc > yc)
            {
                x = (int)((decimal)x / (decimal)xc * (decimal)yc);
                if (x < 1 || y < 1)
                {
                    return result;
                }
                sx = (Image.Width - x) / 2;
                sy = 0;
            }
            else
            {
                y = (int)((decimal)y / (decimal)yc * (decimal)xc);
                if (x < 1 || y < 1)
                {
                    return result;
                }
                sy = (Image.Height - y) / 2;
                sx = 0;
            }

            result.Dispose();
            result = new Bitmap(x, y, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            result.SetResolution(Image.HorizontalResolution, Image.VerticalResolution);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(Image, new Rectangle(0, 0, x, y), new Rectangle(sx, sy, x, y), GraphicsUnit.Pixel);
            }
            return result;
        }

        public static string GetContentType(string path)
        {
            string ext = Path.GetExtension(path);

            switch (ext)
            {
                case ".swf":
                    return "application/x-shockwave-flash";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                default:
                    return "image/jpeg";
            }
        }

        public static void RotateFlip(Image img)
        {
            int id = 274;
            if (Array.IndexOf(img.PropertyIdList, id) == -1)
                return;

            var p = img.GetPropertyItem(id);
            if (p == null)
                return;

            var orientation = (int)p.Value.FirstOrDefault();
            switch (orientation)
            {
                case 1: // No rotation required.
                    break;
                case 2:
                    img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    img.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 5:
                    img.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:
                    img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    img.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            // This EXIF data is now invalid and should be removed.
            img.RemovePropertyItem(274);
        }

        public static void ResaveToOriginQuality(string OriginPath, string Path)
        {
            FileInfo mfi = new FileInfo(OriginPath);
            ResaveToSize(Path, mfi.Length);
        }

        public static void ResaveToSize(string Path, long Size)
        {
            FileInfo fi = new FileInfo(Path);
            if (fi.Length > Size)
            {
                long q = 100;
                var encoder = Helpers.ImageHelper.GetImageEncoder(Path);
                string temp = Path + ".tmp";
                fi.CopyTo(temp, true);
                Image img = Image.FromFile(temp);
                while (fi.Length > Size && q >= 50)
                {
                    q -= 10;
                    img.Save(Path, encoder, Helpers.ImageHelper.GetQualityParameters(q));
                    fi = new FileInfo(Path);
                }
                img.Dispose();
                System.IO.File.Delete(temp);
            }
        }
    }
}
