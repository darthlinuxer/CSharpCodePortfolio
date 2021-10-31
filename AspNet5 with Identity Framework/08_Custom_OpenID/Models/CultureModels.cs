using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace App.Models
{
     public class Language
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public string Culture { get; set; }
        public virtual ICollection<StringResource> StringResources {get; set;}
    }

    public class StringResource
    {
        public int Id { get; set; }
        public int? LanguageId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public virtual Language Language {get; set;}
    }
}