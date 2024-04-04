using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalon
{
    public class Files
    {
        public string Name { get; set; } = string.Empty;
        public string UserTag { get; set; } = string.Empty;
        public string Descr1 { get; set; } = string.Empty;
        public string Descr2 { get; set; } = string.Empty;
        public string Descr3 { get; set; } = string.Empty;
        public string Descr4 { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;

        public Files(string name, string userTag, string descr1, string descr2, string descr3, string descr4, string color, string project, string type, string path)
        {
            Name = name;
            UserTag = userTag;
            Descr1 = descr1;
            Descr2 = descr2;
            Descr3 = descr3;
            Descr4 = descr4;
            Color = color;
            Project = project;
            Type = type;
            Path = path;
        }

        
    }  
}
