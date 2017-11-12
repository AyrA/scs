////////////////////////
// Text to Image Library
////////////////////////



//#mode complex
//#ref "System.Linq.dll"
//#ref "System.Drawing.dll"
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace text2img
{
    /// <summary>
    /// Provides Image generator features
    /// </summary>
    public static class ImageTools
    {
        /// <summary>
        /// Gets the font to render text
        /// </summary>
        /// <remarks>Only use non-proportional fonts</remarks>
        /// <returns>Font</returns>
        private static Font GetFont()
        {
            return new Font("Lucida Console", 12, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// Defines how text is rendered
        /// </summary>
        /// <param name="G">Graphics object</param>
        private static void SetRenderMode(Graphics G)
        {
            G.SmoothingMode = SmoothingMode.HighSpeed;
            G.InterpolationMode = InterpolationMode.NearestNeighbor;
            G.PixelOffsetMode = PixelOffsetMode.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        }

        /// <summary>
        /// Calculates the size of a single character in the font
        /// </summary>
        /// <remarks>Use GetExactSize for small enough text sizes</remarks>
        /// <returns>Character size</returns>
        public static SizeF GetCharSize()
        {
            //increase this if you render huge text files
            const int RECT = 1000;
            string LINE = string.Empty.PadRight(RECT, '#');
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < RECT; i++)
            {
                SB.AppendLine(LINE);
            }
            string TEMPLATE = SB.ToString().TrimEnd();

            using (Bitmap B = new Bitmap(50, 50))
            {
                using (Graphics G = Graphics.FromImage(B))
                {
                    using (Font F = GetFont())
                    {
                        var Measure = G.MeasureString(TEMPLATE, F);
                        Measure.Height /= RECT;
                        Measure.Width /= RECT;
                        return Measure;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size (in chars) of the text
        /// </summary>
        /// <param name="Text">Text</param>
        /// <remarks>This will trim trailing whitespace</remarks>
        /// <returns>Text site</returns>
        public static Size GetTextSize(string Text)
        {
            string[] Lines = Text.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            int H = Lines.Length;

            int W = 0;
            foreach (string L in Lines)
            {
                if (L.TrimEnd().Length > W)
                {
                    W = L.TrimEnd().Length;
                }
            }

            return new Size(W, H);
        }

        /// <summary>
        /// Get the exact size of the text in pixels
        /// </summary>
        /// <param name="Text">Text</param>
        /// <returns>Text size</returns>
        public static Size GetExactSize(string Text)
        {
            using (Bitmap B = new Bitmap(1, 1))
            {
                using (Graphics G = Graphics.FromImage(B))
                {
                    using (Font F = GetFont())
                    {
                        SetRenderMode(G);
                        var SF = G.MeasureString(Text, F);
                        return new Size((int)Math.Ceiling(SF.Width), (int)Math.Ceiling(SF.Height));
                    }
                }
            }
        }

        /// <summary>
        /// Renders a string into a bitmap (black on transparent)
        /// </summary>
        /// <param name="Text">Text to render</param>
        /// <returns>Bitmap containing text</returns>
        public static Bitmap RenderString(string Text)
        {
            return RenderString(Text, Color.Black, Color.Transparent);
        }

        /// <summary>
        /// Renders a string into a bitmap
        /// </summary>
        /// <param name="Text">Text to render</param>
        /// <param name="FG">Foreground color</param>
        /// <param name="BG">Background color</param>
        /// <returns>Bitmap containing text</returns>
        public static Bitmap RenderString(string Text, Color FG, Color BG)
        {
            var TS = GetTextSize(Text);
            Size IS;
            if (TS.Width > 2000 || TS.Height > 2000)
            {
                var CS = GetCharSize();
                IS = new Size((int)Math.Ceiling(TS.Width * CS.Width), (int)Math.Ceiling(TS.Height * CS.Height));
            }
            else
            {
                IS = GetExactSize(Text);
            }

            try
            {

                using (var B = new Bitmap(IS.Width, IS.Height, PixelFormat.Format32bppArgb))
                {
                    using (var G = Graphics.FromImage(B))
                    {
                        if (BG.A != 0)
                        {
                            using (var BB = new SolidBrush(BG))
                            {
                                G.FillRectangle(BB, new Rectangle(0, 0, B.Width, B.Height));
                            }
                        }
                        using (Font F = GetFont())
                        {
                            SetRenderMode(G);
                            using (var FB = new SolidBrush(FG))
                            {
                                G.DrawString(Text, F, FB, new PointF(0, 0));
                            }
                        }
                    }

                    return (Bitmap)B.Clone();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
