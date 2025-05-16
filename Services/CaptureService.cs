using System.Drawing;
using MacroMechanicsHub.Models;

namespace MacroMechanicsHub.Services
{
    public class CaptureService
    {
        public void CaptureRegion(CaptureRegion region, string outputPath)
        {
            if (region == null || !region.IsValid()) return;
            using (var bmp = new Bitmap((int)region.Width, (int)region.Height))
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen((int)region.X, (int)region.Y, 0, 0, bmp.Size);
                bmp.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
