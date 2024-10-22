using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Telega_CidO_bot
{
    public class CidSticker
    {
        public CidSticker() { }

        public string id;
        public List<string> names;
        public string uniqueName;

        internal CidSticker(string id, List<string> names)
        {
            this.id = id;
            this.names = names;
        }

        internal CidSticker(string id, List<string> names, string uniqueName)
        {
            this.id = id;
            this.names = names;
            this.uniqueName = uniqueName;
        }

        internal CidSticker(string id)
        {
            this.id = id;
        }

        internal void AddNameToSticker(string name)
        {
            names.Add(name);
        }

        internal void RemoveName(string name)
        {
            names.Remove(name);
        }

        internal void ChangeUniqueName(string name)
        {
            uniqueName = name;
            names[0] = name;
        }

        /// <summary>
        /// Возвращает все имена в виде одной строки через запятую
        /// </summary>
        /// <returns></returns>
        internal string GetAllNames()
        {
            return String.Join(",", names);
        }
    }
}
