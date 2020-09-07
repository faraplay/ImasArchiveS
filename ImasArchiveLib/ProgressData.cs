using System;
using System.Collections.Generic;
using System.Text;

namespace Imas
{
    public class ProgressData
    {
        public int count;
        public int total;
        public string filename;

        public override string ToString()
        {
            return string.Format("{0} of {1}: {2}", count, total, filename);
        }
    }
}
