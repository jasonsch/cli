using System;
using System.IO;
using System.Linq;
using System.Text;

namespace flatten
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Invalid usage!");
            Console.WriteLine("Usage: flatten <dir>");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            if (args.Length != 1 || !Directory.Exists(args[0]))
            {
                PrintUsage();
            }

            FlattenDirectory(args[0]);
        }

        static void FlattenDirectory(string directory)
        {
            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                FlattenDirectoryWorker(directory, subDirectory);
            }
        }

        static void FlattenDirectoryWorker(string destination, string source)
        {
            foreach (var subDirectory in Directory.EnumerateDirectories(source))
            {
                FlattenDirectoryWorker(destination, subDirectory);
            }

            foreach (var file in Directory.EnumerateFiles(source))
            {
                MoveFile(destination, file);
            }

            Directory.Delete(source);
        }

        private static bool AreFilesEqual(string file1, string file2)
        {
            FileInfo fileInfo1 = new FileInfo(file1);
            FileInfo fileInfo2 = new FileInfo(file2);

            if (fileInfo1.Length == fileInfo2.Length)
            {
                return File.ReadAllBytes(file1).SequenceEqual(File.ReadAllBytes(file2));
            }

            return false;
        }

        private static void MoveFile(string destination, string file)
        {
            string fileName = Path.GetFileName(file);

            if (File.Exists(Path.Combine(destination, fileName)))
            {
                if (AreFilesEqual(Path.Combine(destination, fileName), file))
                {
                    File.Delete(file);
                    return;
                }

                // TODO -- Only decorate the file with as much path needed to make it distinct.
                fileName = RelativePath(destination, file);
            }

            File.Move(file, Path.Combine(destination, fileName));
        }

        //
        // TODO
        // Source will be something like "c:\a\b\c\d\e" and destination will be something like "c:\a\b" (i.e., destination is always an ancestor of source).
        //
        // TODO -- There's got to be a better way to do this.
        //
        static string RelativePath(string ancestorPath, string childPath)
        {
            string[] sourceComponents = childPath.Split(Path.DirectorySeparatorChar);
            string[] destComponents = ancestorPath.Split(Path.DirectorySeparatorChar);

            StringBuilder sb = new StringBuilder();

            for (int i = destComponents.Length; i < sourceComponents.Length; ++i)
            {
                sb.Append(sourceComponents[i]);
                sb.Append('-');
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
