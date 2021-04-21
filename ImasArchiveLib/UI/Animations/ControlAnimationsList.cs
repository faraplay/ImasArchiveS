using System.Collections.Generic;
using System.Diagnostics;

namespace Imas.UI
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ControlAnimationsList
    {
        [SerialiseProperty(0, FixedCount = 16)]
        public byte[] ControlNameBuffer { get; set; }
        [Listed(0)]
        public string ControlName
        {
            get => ImasEncoding.Ascii.GetString(ControlNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, ControlNameBuffer);
        }

        [SerialiseProperty(1, IsCountOf = nameof(Animations))]
        public int AnimationCount { get; set; }
        [SerialiseProperty(2, CountProperty = nameof(AnimationCount))]
        public List<Animation> Animations { get; set; }

        private string GetDebuggerDisplay()
        {
            return $"{ControlName}, {AnimationCount} animations";
        }
    }
}
