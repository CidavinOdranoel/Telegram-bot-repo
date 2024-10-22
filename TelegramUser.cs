using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Telegram_bot_WPF
{
    class TelegramUser : INotifyPropertyChanged, IEquatable<TelegramUser>
    {
        public TelegramUser(string Name, long Id)
        {
            this.name = Name;
            this.id = Id;
            Messages = new ObservableCollection<string>();
        }

        private string name;
        private long id;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.name)));
            }
        }

        public long Id
        {
            get { return this.id; }
            set
            {
                this.id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.id)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(TelegramUser other) => other.Id == this.id;

        public ObservableCollection<string> Messages { get; set; }

        public void AddMessage(string text) => Messages.Add(text);

    }
}
