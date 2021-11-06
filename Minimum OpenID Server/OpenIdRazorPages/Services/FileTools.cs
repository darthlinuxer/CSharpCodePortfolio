using System.IO;
using Newtonsoft.Json;

namespace OpenIDAppRazor.Services
{
    public class FileTools
    {
        public static T ReadJsonFromFile<T>(string addr) where T : class
        {
            if(!File.Exists(addr)) return null;            
            string jsonText = File.ReadAllText(addr);
            T obj = JsonConvert.DeserializeObject<T>(jsonText);
            return obj;
        }

        public static void WriteJsonToFile<T>(T obj, string addr) where T : class
        {
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(@addr, JsonConvert.SerializeObject(obj));                
        }
    }
}