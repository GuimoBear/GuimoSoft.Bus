using DotNetEnv;
using System;
using System.IO;
using System.Linq;

namespace GuimoSoft.Bus.Examples.Utils
{
    public static class EnvFile
    {
        public static void CarregarVariaveis(LoadOptions loadOptions = default)
        {
            ConfigurarEnv();

            const string fileName = ".env";
            var assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var file = Path.Combine(assemblyDirectory, fileName);

            if (File.Exists(file))
            {
                Env.Load(file, loadOptions);
            }
        }

        private static void ConfigurarEnv()
        {
            const string fileName = ".env";

            var sourcePath = ObterDiretorio(fileName)?.FullName ?? string.Empty;
            var sourceFile = Path.Combine(sourcePath, fileName);

            var destPath = AppDomain.CurrentDomain.BaseDirectory;
            var destFile = Path.Combine(destPath, fileName);

            if (!File.Exists(sourceFile)) return;

            var sr = File.OpenText(sourceFile);
            var fileTxt = sr.ReadToEnd();
            sr.Close();


            var fileInfo = new FileInfo(destFile);
            var sw = fileInfo.CreateText();
            sw.Write(fileTxt);
            sw.Close();
        }

        public static DirectoryInfo ObterDiretorio(string filename)
        {
            var directory = Directory.GetParent(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles(filename).Any())
            {
                directory = directory.Parent;
            }

            return directory;
        }
    }
}
