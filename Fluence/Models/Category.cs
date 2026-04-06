using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fluence.Models
{
    class Category : INotifyPropertyChanged
    {
        private string _name;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string Name 
        { 
            get { return _name; }
            set 
            { 
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsSystem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
