using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AlgoRunner.Models
{
    public class AlgorithmNode
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public ObservableCollection<CppFile> Files { get; set; } = new();
    }

    public class CppFile : INotifyPropertyChanged
    {
        private string _displayName = "";

        public string FilePath { get; set; } = "";

        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(nameof(DisplayName)); }
        }

        public string Category { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DirectoryNode
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public ObservableCollection<AlgorithmNode> Algorithms { get; set; } = new();
    }
}
