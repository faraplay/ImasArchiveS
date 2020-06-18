using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ImasArchiveApp
{
    class BrowserTree
    {
        private readonly BrowserTree _parent;
        private readonly List<BrowserTree> _children = new List<BrowserTree>();
        private readonly string _entryName;
        private readonly BrowserEntryType _type;

        private BrowserTree(BrowserTree parent, string entryName, BrowserEntryType type)
        {
            _parent = parent;
            _entryName = entryName;
            _type = type;
        }

        public BrowserTree(string dirName, List<string> entries)
        {
            _entryName = dirName;
            foreach (string entry in entries)
            {
                AddEntry(entry);
            }
        }

        private void AddEntry(string entry)
        {
            if (!entry.Contains('/'))
            {
                _children.Add(new BrowserTree(this, entry, BrowserEntryType.RegularFile));
            }
            else
            {
                if (entry[0] == '/')
                {
                    entry = entry.Substring(1);
                }
                int index = entry.IndexOf('/');
                string dir = entry.Substring(0, index);
                BrowserTree child = _children.Find(tree => tree._entryName == dir);
                if (child == null)
                {
                    child = new BrowserTree(this, dir, BrowserEntryType.Directory);
                    _children.Add(child);
                }
                child.AddEntry(entry.Substring(index + 1));
            }
        }

        public override string ToString()
        {
            if (_parent == null)
            {
                return _entryName;
            }
            else
            {
                return _parent.ToString() + "/" + _entryName;
            }
        }

        public ReadOnlyCollection<BrowserTree> Entries { get => new ReadOnlyCollection<BrowserTree>(_children); }
        public BrowserEntryType Type { get => _type; }
        public string Name { get => _entryName; }

        public Uri IconUri => _type switch
        {
            BrowserEntryType.Directory => new Uri("/Icons/FolderClosed_16x.png", UriKind.Relative),
            BrowserEntryType.RegularFile => new Uri("/Icons/TextFile_16x.png", UriKind.Relative),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    enum BrowserEntryType
    {
        Directory = 0,
        RegularFile = 1
    }
}
