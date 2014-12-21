using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace ManifestGenerator
{
    public class Generator
    {
        StringBuilder _appCache = new StringBuilder();
        private List<string> _excludeList = new List<string>();
        private string _rootPath;
        public string GenerateAppCache(string rootPath, string fileToCreate)
        {
            _rootPath = rootPath + @"\";
            var fallback = new List<string>();
            _excludeList=new List<string>();

            //check if we have an exclude file and handle it
            if (File.Exists(_rootPath + @"AppCacheExclude.txt"))
            {
                var excludeFile = File.ReadAllLines(_rootPath + @"\AppCacheExclude.txt");
                for (var i = 0; i < excludeFile.Length; i++)
                {
                    if (excludeFile[i] == "FALLBACK:")
                    {
                        //get everything else after this and stop
                        for (var j = i+1; j < excludeFile.Length; j++)
                        {
                            fallback.Add(excludeFile[j]);
                        }
                        break;
                    }
                    else
                    {
                        _excludeList.Add(excludeFile[i]);
                    }
                }
            }

            _appCache.AppendLine("CACHE MANIFEST");
            _appCache.AppendLine(string.Format("# {0}:{1}", DateTime.Now.ToString("yyyy-MM-dd"), Guid.NewGuid()));


            //iterate every single file in the rootPath and add it to the string
            CacheFiles(_rootPath, "*.*"); //handles root
            DirectoryScan(_rootPath,"*.*"); //does recursion for everywhere else

            _appCache.AppendLine("NETWORK:");
            _appCache.AppendLine("*");

            //fallback stuff
            if (fallback.Count != 0)
            {
                _appCache.AppendLine("FALLBACK:");
                foreach (var item in fallback)
                {
                    _appCache.AppendLine(item);
                }
            }

            
            string fileName = _rootPath + fileToCreate;
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.Write(_appCache.ToString());    
            }
            
            return String.Format("New manifest file {0} created in {1}",fileToCreate,rootPath);
        }

        private void DirectoryScan(string root, string fileMatch)
        {
            foreach (string dir in Directory.GetDirectories(root))
            {
                string excludeCheck = dir.Replace(_rootPath, "") + @"\*";
                if (!_excludeList.Contains(excludeCheck))
                {
                    CacheFiles(dir, fileMatch);
                    //recurse as necessary
                    DirectoryScan(dir, fileMatch);    
                }
                
            }

        }

        private void CacheFiles(string fileLocation, string fileMatch)
        {
            string[] files;
            //match files in the path that meet our requirement at write them one by one to the db
            //if there is only one then that will work nicely too!
            files = Directory.GetFiles(fileLocation, fileMatch);
            foreach (string file in files)
            {
                string fileName = file.Replace(_rootPath, "");
                if (!_excludeList.Contains(fileName))
                {
                    _appCache.AppendLine(fileName.Replace(@"\", @"/"));    
                }
                
            }
        }
    }
}
