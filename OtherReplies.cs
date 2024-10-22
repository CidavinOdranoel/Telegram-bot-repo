using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram_bot_WPF
{
    internal class OtherReplies
    {
        public string Name { get; set; }
        public List<string> Triggers { get; set; }
        public string Reply { get; set; }
        public int TriggerChance { get; set; }

        OtherReplies(string name, List<string> triggers, string reply, int triggerChance = 100)
        {
            Name = name;
            Triggers = triggers;
            Reply = reply;
            TriggerChance = triggerChance;
        }

        public void AddTrigger(string trigger)
        {
            Triggers.Add(trigger);
        }



    }
}
