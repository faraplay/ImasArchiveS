using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    public class ParEntry
    {
        public string _name;
        public int _offset;
        public int _length;
        public ParFile _parent;
        public int _property;

        internal ParEntry(ParFile parent, string name, int offset, int length, int property)
        {
            _parent = parent;
            _name = name;
            _offset = offset;
            _length = length;
            _property = property;
        }

        public Stream Open()
        {
            return _parent.GetSubstream(_offset, _length);
        }
    }
}
