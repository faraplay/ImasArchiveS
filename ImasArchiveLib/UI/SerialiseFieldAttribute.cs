using System;
using System.Collections.Generic;
using System.Text;

namespace Imas.UI
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class SerialiseFieldAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly int order;

        // This is a positional argument
        public SerialiseFieldAttribute(int order)
        {
            this.order = order;
        }

        public int Order
        {
            get { return order; }
        }

        // This is a named argument
        public int FixedCount { get; set; }
        public string ConditionProperty { get; set; }
        public string CountProperty { get; set; }
    }
}
