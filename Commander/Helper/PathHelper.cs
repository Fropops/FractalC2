using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Helper
{
    public static class PathHelper
    {
        public static string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                // Le chemin est déjà absolu
                return path;
            }

            // On récupère le dossier où se trouve l'application (exécutable)
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // On combine avec le chemin relatif
            string fullPath = Path.Combine(baseDir, path);

            // On normalise le chemin (résout les .. etc.)
            return Path.GetFullPath(fullPath);
        }
    }
}
