using System;

namespace Imas.UI
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SerialisationBaseTypeAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public SerialisationBaseTypeAttribute()
        {
        }

    }
}
