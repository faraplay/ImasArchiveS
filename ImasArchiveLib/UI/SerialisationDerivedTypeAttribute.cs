using System;

namespace Imas.UI
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SerialisationDerivedTypeAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly int derivedTypeID;

        // This is a positional argument
        public SerialisationDerivedTypeAttribute(int derivedTypeID)
        {
            this.derivedTypeID = derivedTypeID;
        }

        public int DerivedTypeID
        {
            get { return derivedTypeID; }
        }

        // This is a named argument
        //public int NamedInt { get; set; }
    }
}
