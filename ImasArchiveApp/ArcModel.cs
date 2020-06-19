using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    class ArcModel : INotifyPropertyChanged
    {
        private string _current_file;
        private ArcFile _arc_file;
        private BrowserTree _root;
        private List<string> _browser_entries = new List<string>();

        public string CurrentFile
        {
            get => _current_file;
            set
            {
                _current_file = value;
                OnPropertyChanged(nameof(CurrentFile));
            }
        }

        public ArcFile ArcFile
        {
            get => _arc_file;
            set
            {
                _arc_file = value;
                OnPropertyChanged(nameof(ArcFile));
            }
        }

        public BrowserTree Root 
        { 
            get => _root;
            set
            {
                _root = value;
                OnPropertyChanged(nameof(Root));
            }
        }

        public List<string> BrowserEntries
        {
            get => _browser_entries;
            set
            {
                _browser_entries = value;
                OnPropertyChanged(nameof(BrowserEntries));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OpenArc(string arcPath)
        {

            string truncFilename = arcPath;
            string extension;
            if (truncFilename.EndsWith(".arc"))
            {
                truncFilename = truncFilename.Remove(truncFilename.Length - 4);
                extension = "";
            }
            else if (truncFilename.EndsWith(".arc.dat"))
            {
                truncFilename = truncFilename.Remove(truncFilename.Length - 8);
                extension = ".dat";
            }
            else
            {
                throw new Exception();
            }
            ArcFile = new ArcFile(truncFilename, extension);
            BrowserEntries.Clear();
            foreach (ArcEntry entry in ArcFile.Entries)
            {
                BrowserEntries.Add(entry.Filepath);
            }
            Root = new BrowserTree("", BrowserEntries);
        }
    }
}
