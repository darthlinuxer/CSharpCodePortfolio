using System;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using App.Models;
using Newtonsoft.Json;

namespace App.Services
{
    public class FileTools
    {
        public static T ReadJsonFromFile<T>(string addr) where T : class
        {
            string jsonText = File.ReadAllText(addr);
            T obj = JsonConvert.DeserializeObject<T>(jsonText);
            return obj;
        }
    }
}