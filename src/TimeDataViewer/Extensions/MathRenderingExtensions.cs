using System;
using TimeDataViewer.Spatial;
using Avalonia;
using Avalonia.Controls;

namespace TimeDataViewer
{
    public static class MathRenderingExtensions
    {
        static MathRenderingExtensions()
        {
            SubAlignment = 0.6;
            SuperAlignment = 0;
            SubSize = 0.62;
            SuperSize = 0.62;
        }

        /// <summary>
        /// Gets or sets the subscript alignment.
        /// </summary>
        private static double SubAlignment { get; set; }

        /// <summary>
        /// Gets or sets the subscript size.
        /// </summary>
        private static double SubSize { get; set; }

        /// <summary>
        /// Gets or sets the superscript alignment.
        /// </summary>
        private static double SuperAlignment { get; set; }

        /// <summary>
        /// Gets or sets the superscript size.
        /// </summary>
        private static double SuperSize { get; set; }

        /// <summary>
        /// Draws or measures text containing sub- and superscript.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="pt">The point.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="ha">The horizontal alignment.</param>
        /// <param name="va">The vertical alignment.</param>
        /// <param name="maxSize">The maximum size of the text.</param>
        /// <param name="measure">Measure the size of the text if set to <c>true</c>.</param>
        /// <returns>The size of the text.</returns>
        /// <example>Subscript: H_{2}O
        /// Superscript: E=mc^{2}
        /// Both: A^{2}_{i,j}</example>
        public static OxySize DrawMathText(this CanvasRenderContext rc, ScreenPoint pt, TextBlock textBlock,    
            Core.HorizontalAlignment ha, Core.VerticalAlignment va, OxySize? maxSize, bool measure)
        {
            var text = textBlock.Text;

            if (string.IsNullOrEmpty(text))
            {
                return OxySize.Empty;
            }

            if (text.Contains("^{") || text.Contains("_{"))
            {
                var x = pt.X;
                var y = pt.Y;

                // Measure
                var size = InternalDrawMathText(rc, textBlock, x, y, 0, 0, true);

                var dx = 0d;
                var dy = 0d;

                switch (ha)
                {
                    case Core.HorizontalAlignment.Right:
                        dx = -size.Width;
                        break;
                    case Core.HorizontalAlignment.Center:
                        dx = -size.Width * 0.5;
                        break;
                }

                switch (va)
                {
                    case Core.VerticalAlignment.Bottom:
                        dy = -size.Height;
                        break;
                    case Core.VerticalAlignment.Middle:
                        dy = -size.Height * 0.5;
                        break;
                }

                InternalDrawMathText(rc, textBlock, x, y, dx, dy, false);
                return measure ? size : OxySize.Empty;
            }

            rc.DrawText(pt, textBlock, ha, va, maxSize);
            if (measure)
            {
                return rc.MeasureText(textBlock);
            }

            return OxySize.Empty;
        }

        /// <summary>
        /// Draws text containing sub- and superscript.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="pt">The point.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="ha">The horizontal alignment.</param>
        /// <param name="va">The vertical alignment.</param>
        /// <param name="maxSize">The maximum size of the text.</param>
        /// <example>Subscript: H_{2}O
        /// Superscript: E=mc^{2}
        /// Both: A^{2}_{i,j}</example>
        public static void DrawMathText(this CanvasRenderContext rc, ScreenPoint pt, TextBlock textBlock, 
            Core.HorizontalAlignment ha, Core.VerticalAlignment va, OxySize? maxSize = null)
        {
            DrawMathText(rc, pt, textBlock, ha, va, maxSize, false);
        }

        // Draws text with sub- and superscript items.
        private static OxySize InternalDrawMathText(CanvasRenderContext rc, TextBlock textBlock, double x, double y, double dx, double dy, bool measureOnly)
        {
            var i = 0;
            var cosAngle = Math.Round(Math.Cos(0.0), 5);
            var sinAngle = Math.Round(Math.Sin(0.0), 5);

            var currentX = x;
            var maximumX = x;
            var minimumX = x;
            var currentY = y;
            var maximumY = y;
            var minimumY = y;

            var fontSize = textBlock.FontSize;
            var s = textBlock.Text;

            // http://en.wikipedia.org/wiki/Subscript_and_superscript
            var superScriptYDisplacement = fontSize * SuperAlignment;

            var subscriptYDisplacement = fontSize * SubAlignment;

            var superscriptFontSize = fontSize * SuperSize;
            var subscriptFontSize = fontSize * SubSize;

            Func<double, double, string/*, double*/, OxySize> drawText = (xb, yb, text/*, fSize*/) =>
            {
                if (!measureOnly)
                {
                    var xr = x + ((xb - x + dx) * cosAngle) - ((yb - y + dy) * sinAngle);
                    var yr = y + ((xb - x + dx) * sinAngle) + ((yb - y + dy) * cosAngle);
                    rc.DrawText(new ScreenPoint(xr, yr), textBlock, Core.HorizontalAlignment.Left, Core.VerticalAlignment.Top, null);
                }

                var flatSize = rc.MeasureText(textBlock);
                return new OxySize(flatSize.Width, flatSize.Height);
            };

            while (i < s.Length)
            {
                // Superscript
                if (i + 1 < s.Length && s[i] == '^' && s[i + 1] == '{')
                {
                    var i1 = s.IndexOf('}', i);
                    if (i1 != -1)
                    {
                        var supString = s.Substring(i + 2, i1 - i - 2);
                        i = i1 + 1;
                        var sx = currentX;
                        var sy = currentY + superScriptYDisplacement;
                        var size = drawText(sx, sy, supString/*, superscriptFontSize*/);
                        maximumX = Math.Max(sx + size.Width, maximumX);
                        maximumY = Math.Max(sy + size.Height, maximumY);
                        minimumX = Math.Min(sx, minimumX);
                        minimumY = Math.Min(sy, minimumY);

                        continue;
                    }
                }

                // Subscript
                if (i + 1 < s.Length && s[i] == '_' && s[i + 1] == '{')
                {
                    var i1 = s.IndexOf('}', i);
                    if (i1 != -1)
                    {
                        var subString = s.Substring(i + 2, i1 - i - 2);
                        i = i1 + 1;
                        var sx = currentX;
                        var sy = currentY + subscriptYDisplacement;
                        var size = drawText(sx, sy, subString/*, subscriptFontSize*/);
                        maximumX = Math.Max(sx + size.Width, maximumX);
                        maximumY = Math.Max(sy + size.Height, maximumY);
                        minimumX = Math.Min(sx, minimumX);
                        minimumY = Math.Min(sy, minimumY);

                        continue;
                    }
                }

                // Regular text
                var i2 = s.IndexOfAny("^_".ToCharArray(), i + 1);
                string regularString;
                if (i2 == -1)
                {
                    regularString = s.Substring(i);
                    i = s.Length;
                }
                else
                {
                    regularString = s.Substring(i, i2 - i);
                    i = i2;
                }

                currentX = maximumX + 2;
                var size2 = drawText(currentX, currentY, regularString/*, fontSize*/);

                maximumX = Math.Max(currentX + size2.Width, maximumX);
                maximumY = Math.Max(currentY + size2.Height, maximumY);
                minimumX = Math.Min(currentX, minimumX);
                minimumY = Math.Min(currentY, minimumY);

                currentX = maximumX;
            }

            return new OxySize(maximumX - minimumX, maximumY - minimumY);
        }
    }
}
