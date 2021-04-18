using System;

namespace Imas.UI
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ListedAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly int order;

        // This is a positional argument
        public ListedAttribute(int order)
        {
            this.order = order;
        }

        public int Order
        {
            get { return order; }
        }

        // This is a named argument
        public bool StringMultiline { get; set; }
    }
}
