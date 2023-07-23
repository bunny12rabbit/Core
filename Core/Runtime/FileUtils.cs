using System.IO;

namespace Common.Core
{
    public static class FileUtils
    {
        public static void ClearDirectory(string path)
        {
            var di = new DirectoryInfo(path);

            if (!di.Exists)
                return;

            foreach (var file in di.GetFiles())
                file.Delete();

            foreach (var dir in di.GetDirectories())
                dir.Delete(true);
        }

        public static void DeleteFilesRecursively(string path, params string[] ext)
        {
            void DeleteFiles(DirectoryInfo directoryInfo)
            {
                if (!directoryInfo.Exists)
                    return;

                foreach (var file in directoryInfo.GetFiles())
                {
                    var deleteFile = false;

                    // Если указано хоть одно расширение - проверяем что файл ему соответствует.
                    if (ext?.Length > 0)
                    {
                        var extension = file.Extension;

                        foreach (var s in ext)
                        {
                            if (extension == s)
                            {
                                deleteFile = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        deleteFile = true;
                    }

                    if (deleteFile)
                        file.Delete();
                }

                foreach (var dir in directoryInfo.GetDirectories())
                    DeleteFiles(dir);
            }

            var di = new DirectoryInfo(path);
            DeleteFiles(di);
        }

        /// <summary>
        /// Создаёт в <param name="targetPath"/> всю иерархию директорий, которые есть в <param name="sourcePath"/>. Директории создаются пустые.
        /// </summary>
        public static void CreateDirectoriesHierarchy(string sourcePath, string targetPath)
        {
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }
    }
}