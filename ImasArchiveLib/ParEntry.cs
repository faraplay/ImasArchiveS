using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    public class ParEntry
    {
        public string Name { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public ParFile Parent { get; set; }
        public int Property { get; set; }

        internal ParEntry(ParFile parent, string name, int offset, int length, int property)
        {
            Parent = parent;
            Name = name;
            Offset = offset;
            Length = length;
            Property = property;
        }

        public Stream Open()
        {
            return Parent.GetSubstream(Offset, Length);
        }
    }
}
