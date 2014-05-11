using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManifestGenerator
{
    public class Program
    {
        private static string _rootDirectory;
        private static string _manifestFile;
        private static FileSystemWatcher _fileSystemWatcher;
        static void Main(string[] args)
        {
            try
            {
                Console.BackgroundColor=ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.SetWindowSize(114,24);
                Console.Clear();

                Console.WriteLine("Web App Manifest Generator (c) Matt Stutely 2014");
                Console.WriteLine("========================================");
                Console.WriteLine();

                //how the exclude file works


                int argsSet = args.GetUpperBound(0);
                if (argsSet != 1)
                {
                    Console.WriteLine("You can set the arguments via the command line if you prefer - first argument is a string of the path to monitor, second is a string of the appcache file you want to generate");
                    Console.WriteLine();

                    //recapture, cos something wrong
                    Console.Write(@"Root of directory to monitor (e.g. C:\MyFolder) ? ");
                    _rootDirectory = Console.ReadLine();

                    Console.Write("Filename for manifest file to generate in that directory (e.g. mycache.appcache) ? ");
                    _manifestFile = Console.ReadLine();



                    Console.WriteLine();
                }
                else
                {
                    _rootDirectory = args[0];
                    _manifestFile = args[1];    
                }

                if (!File.Exists(_rootDirectory + @"\AppCacheExclude.txt"))
                {
                    throw new Exception("AppCacheExclude.txt file is not present in the root directory!");
                }

                _fileSystemWatcher = new FileSystemWatcher();
                _fileSystemWatcher.Path = _rootDirectory;
                _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite| NotifyFilters.FileName | NotifyFilters.DirectoryName;
                _fileSystemWatcher.Filter = "*.*";
                _fileSystemWatcher.Changed += new FileSystemEventHandler(FileChanged);
                _fileSystemWatcher.Created += new FileSystemEventHandler(FileChanged);
                _fileSystemWatcher.Deleted += new FileSystemEventHandler(FileChanged);
                _fileSystemWatcher.Renamed += new RenamedEventHandler(FileRenamed);
    
                _fileSystemWatcher.EnableRaisingEvents = true;
                Console.WriteLine(string.Format("Monitoring {0} for file changes, will save manifest to {1}",_rootDirectory, _manifestFile));
                Console.WriteLine();
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            Console.WriteLine("------------");
            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }


        private static void FileChanged(object source, FileSystemEventArgs e)
        {
            if (e.FullPath.ToLower().EndsWith("appcache")) return;
            Console.WriteLine(string.Format("{0} Change to {1} detected",DateTime.Now.ToString(),e.FullPath));
            RebuildManifest();
        }

        private static void FileRenamed(object source, RenamedEventArgs e)
        {
            if (e.FullPath.ToLower().EndsWith("appcache")) return;
            Console.WriteLine(string.Format("{0} File renamed {1}",DateTime.Now.ToString(), e.FullPath));
            RebuildManifest();
        }

        private static void RebuildManifest()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var generator = new Generator();
            generator.GenerateAppCache(_rootDirectory, _manifestFile);
            //Console.WriteLine(generator.GenerateAppCache(_rootDirectory, _manifestFile));
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}
