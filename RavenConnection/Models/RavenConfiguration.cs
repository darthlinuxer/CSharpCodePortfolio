using System.Collections.Generic;

namespace RavenConnection.Models
{
     public class RavenConfiguration
    {
        public List<string> UrlsFromWindows { get; set; } = new List<string>();
        public List<string> UrlsFromContainer { get; set; } = new List<string>();
        public string Database { get; set; }
    }
}