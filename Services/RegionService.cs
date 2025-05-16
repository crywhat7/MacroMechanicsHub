using System.IO;
using MacroMechanicsHub.Models;

namespace MacroMechanicsHub.Services
{
    public class RegionService
    {
        public CaptureRegion LoadRegion(string filePath)
        {
            if (!File.Exists(filePath))
                return new CaptureRegion();

            var line = File.ReadAllText(filePath).Trim();
            var parts = line.Split(',');

            if (parts.Length != 4 ||
                !double.TryParse(parts[0], out double x) ||
                !double.TryParse(parts[1], out double y) ||
                !double.TryParse(parts[2], out double width) ||
                !double.TryParse(parts[3], out double height))
            {
                return new CaptureRegion();
            }

            return new CaptureRegion { X = x, Y = y, Width = width, Height = height };
        }

        public void SaveRegion(string filePath, CaptureRegion region)
        {
            File.WriteAllText(filePath, $"{region.X},{region.Y},{region.Width},{region.Height}");
        }
    }
}
