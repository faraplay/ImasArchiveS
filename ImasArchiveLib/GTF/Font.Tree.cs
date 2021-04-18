using System;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private void BuildTree()
        {
            Array.Sort(chars);
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].index = i;
            }
            root = BuildSubTree(chars);
        }

        private int BuildSubTree(Span<CharData> span)
        {
            if (span.Length == 0)
                return -1;

            int midpoint = span.Length / 2;
            span[midpoint].left = BuildSubTree(span.Slice(0, midpoint));
            span[midpoint].right = BuildSubTree(span.Slice(midpoint + 1));
            return span[midpoint].index;
        }

        internal bool CheckTree()
        {
            if (chars[0].index != 0)
                return false;
            for (int i = 0; i < chars.Length - 1; i++)
            {
                if (chars[i + 1].index != i + 1 || chars[i].CompareTo(chars[i + 1]) >= 0)
                    return false;
            }
            return CheckSubTree(chars, root);
        }

        private bool CheckSubTree(Span<CharData> span, int root)
        {
            if (span.Length == 0)
                return (root == -1);

            int midpoint = span.Length / 2;
            if (span[midpoint].index != root)
                return false;

            return CheckSubTree(span.Slice(0, midpoint), span[midpoint].left) &&
                CheckSubTree(span.Slice(midpoint + 1), span[midpoint].right);
        }

        private CharData Find(ushort charID)
        {
            int index = root;
            while (true)
            {
                if (index == -1)
                    return null;
                ushort ikey = chars[index].key;
                if (charID < ikey)
                    index = chars[index].left;
                else if (charID > ikey)
                    index = chars[index].right;
                else
                    return chars[index];
            }
        }
    }
}