using System;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace PackagesList.CustomStorage
{
    public static class CustomLocalStorage
    {
        #region HandleJson

        const string FolderName = "OrbitarPackages";
        const string FileName = "orbitar_packages_config";
        const string FileExtension = "json";

        static string FilePath => $"{FolderName}/{FileName}.{FileExtension}";
        static string FolderPath => $"{FolderName}";


        //to load from System.IO
        static string SystemFolderPath => $"{Application.dataPath}/{FolderPath}".Replace("////", "//");
        static string SystemFilePath => $"{Application.dataPath}/{FilePath}".Replace("////", "//");
 
        public static string StorageLocation => SystemFilePath;

        static JObject cache;

        static JObject GetJson()
        {
            if (cache != null)
            {
                return cache;
            }

            // if dont exists folder => create folder
            if (!Directory.Exists(SystemFolderPath))
            {
                Directory.CreateDirectory(SystemFolderPath);
                return cache = new JObject();
            }


            // if dont exists file => create file and add an empty json to it
            if (!File.Exists(SystemFilePath))
            {
                File.Create(SystemFilePath).Close();
                WriteToFile("{}");
                return cache = new JObject();
            }

            // if exists file => read file
            var fileContent = ReadFromFile();

            // if file is empty => create json
            if (string.IsNullOrEmpty(fileContent))
            {
                return cache = new JObject();
            }

            // if file is not empty => parse json
            return cache = JObject.Parse(fileContent);
        }
//
        #endregion

        static void WriteToFile(string content) => File.WriteAllText(SystemFilePath, content);

        static string ReadFromFile() => File.ReadAllText(SystemFilePath);


        public static void Set(string key, string value)
        {
            var json = GetJson();
            json[key] = value;
            WriteToFile(json.ToString());
        }

        public static string Get(string key)
        {
            var json = GetJson();
            return json[key]?.ToString();
        }
    }
}