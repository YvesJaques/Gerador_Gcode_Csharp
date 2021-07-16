using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gerador_Gcode
{
    public static class GCodeGenerator 
    {
        public static string GenerateGCode(System.Drawing.Image image)
        {
            Bitmap bm = new Bitmap(image);

            double startZ = 0;
            double minZ = 0;
            double maxZ = 0;
            double leftX = 0;
            double rightX = 100;
            double topY = 100;
            double bottomY = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("G21 (millimeters)");
            sb.AppendLine("G90 (absolute)");
            sb.AppendLine(string.Format("G92 X0 Y0 Z{0} (start at 0, 0, {0})", startZ));
            sb.AppendLine(string.Format("G00 X{0} Y{1} Z{2}", leftX, topY, startZ));

            BitmapData bmdata = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            const int bytesPerPixel = 4;
            double rangeZ = maxZ - minZ;
            double rangeX = rightX - leftX;
            double rangeY = bottomY - topY;
            unsafe
            {
                byte* scan0 = (byte*)bmdata.Scan0.ToPointer();
                for (int imagey = 0; imagey < bm.Height; ++imagey)
                {
                    byte* line = scan0 + imagey * bmdata.Stride;
                    for (int u = 0; u < bm.Width; ++u)
                    {
                        int imagex = u;
                        if (imagey % 2 == 1)
                        {
                            // every other line goes backwards.
                            imagex = bm.Width - 1 - u;
                        }
                        byte* pixel = line + bytesPerPixel * imagex;
                        byte val = pixel[1];
                        // scale z by green channel
                        double x = leftX + rangeX * imagex / (float)bm.Width;
                        double y = topY + rangeY * imagey / (float)bm.Height;
                        double z = minZ + rangeZ * (val / 255.0);
                        sb.AppendLine(string.Format("G01 X{0} Y{1} Z{2} ({3})", x, y, z, val));
                    }
                }
            }

            sb.AppendLine(string.Format("G00 Z{0} (up to start)", startZ));
            sb.AppendLine("G00 X0 Y0 (home)");
            return sb.ToString();
        }
    }
}
