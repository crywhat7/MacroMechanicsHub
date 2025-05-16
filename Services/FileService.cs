using System.IO;

namespace MacroMechanicsHub.Services
{
    public class FileService
    {
        public void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
