using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace OgeApp.TaskProcessing
{
    public static class PictureAdder
    {
        private static IEnumerable<string> NGetFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.AllDirectories)
        {
            Regex reSearchPattern = new(searchPatternExpression);
            return Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(Path.GetFileName(file)));
        }

        public static void AddPicture(string? dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                var dirFrom = Path.Combine(dirPath, "Images");
                var dirTo = Path.Combine(Directory.GetCurrentDirectory(), "Images");

                if (!Directory.Exists(dirTo)) { Directory.CreateDirectory(dirTo); }

                if (Directory.Exists(dirFrom))
                {
                    //Искомые расширения:
                    string LookForExt = ".png";
                    //Пути папок источника и приёмника:
                    string SourcePath = dirFrom;
                    string TargetPath = dirTo;
                    //Получаем файлы и копируем их:
                    IEnumerable<string> files = NGetFiles(SourcePath, LookForExt);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Copy(file, Path.Combine(TargetPath, Path.GetFileName(file)), true);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }
        }
    }
}
