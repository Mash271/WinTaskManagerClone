using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TaskManagerClone.Models
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        private double _memoryUsage; // in MB
        public double MemoryUsage
        {
            get => _memoryUsage;
            set { if (_memoryUsage != value) { _memoryUsage = value; OnPropertyChanged(); } }
        }

        private string _status = "Running";
        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set { if (_fileName != value) { _fileName = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
