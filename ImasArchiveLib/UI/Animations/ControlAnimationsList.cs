using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Imas.UI
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ControlAnimationsList
    {
        [SerialiseField(0, FixedCount = 16)]
        public byte[] controlNameBuffer;
        [Listed(0)]
        public string ControlName
        {
            get => ImasEncoding.Ascii.GetString(controlNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, controlNameBuffer);
        }

        [SerialiseField(1, IsCountOf = nameof(animations))]
        public int animationCount;
        [SerialiseField(2, CountField = nameof(animationCount))]
        public List<Animation> animations;

        private string GetDebuggerDisplay()
        {
            return $"{ControlName}, {animationCount} animations";
        }
    }
}
