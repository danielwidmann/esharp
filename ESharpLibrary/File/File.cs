using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcsTarget
{
    public class EcsFile
    {

        public string Name { get; set; }
        public string Content { get; set; }

        public bool Override { get; set; }

        public EcsFile()
        {
            Override = true;
        }

        public EcsFile(string name, string content, bool @override = true)
        {
            // TODO: Complete member initialization
            this.Name = name;
            this.Content = content;
            this.Override = @override;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Name, String.Concat(Content.Take(40)));
            
        }
    }
}
