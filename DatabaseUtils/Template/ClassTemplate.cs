using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseUtils.Template
{
    public class ClassTemplate
    {
        public string Name { get; set; }
        public bool HasContent { get; set; }
        public string Template { get; set; }
        public string SavePath { get; set; }
        public string Content { get; set; }
        public string FileNameTemplate { get; set; }
    }
}
