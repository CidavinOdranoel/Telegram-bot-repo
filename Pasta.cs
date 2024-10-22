using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telega_CidO_bot
{
    public class Pasta
    {
        public Pasta() { }

        public string Name { get; set; }
        public string Content { get; set; }

        public Pasta(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
