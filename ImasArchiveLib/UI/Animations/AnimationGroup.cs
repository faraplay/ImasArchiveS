using System.Collections.Generic;
using System.Diagnostics;

namespace Imas.UI
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AnimationGroup
    {
        public string FileName { get; set; }

        [SerialiseField(0)]
        public uint header;

        [SerialiseField(1)]
        public int a1;
        [SerialiseField(2)]
        public int a2;
        [SerialiseField(3)]
        public int a3;
        [SerialiseField(4)]
        public int a4;
        [SerialiseField(5)]
        public int a5;

        [SerialiseField(6, IsCountOf = nameof(controlAnimations))]
        public int controlAnimationCount;
        [SerialiseField(7, CountField = nameof(controlAnimationCount))]
        public List<ControlAnimationsList> controlAnimations;

        private string GetDebuggerDisplay()
        {
            return FileName;
        }
    }
}
