using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Imas.UI
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ControlAnimationsList
    {
        [SerialiseProperty(0)]
        public byte[] ControlNameBuffer { get; set; } = new byte[16];
        [Listed(0)]
        public string ControlName
        {
            get => ImasEncoding.Ascii.GetString(ControlNameBuffer);
            set => ImasEncoding.Ascii.GetBytes(value, ControlNameBuffer);
        }

        [SerialiseProperty(1)]
        public int AnimationCount
        {
            get => Animations.Count;
            set
            {
                if (value == Animations.Count)
                    return;
                if (value < Animations.Count && value >= 0)
                {
                    Animations.RemoveRange(value, Animations.Count - value);
                    return;
                }
                if (value > Animations.Count)
                {
                    Animations.AddRange(Enumerable.Repeat<Animation>(null, value - Animations.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(2)]
        public List<Animation> Animations { get; set; } = new List<Animation>();

        private string GetDebuggerDisplay()
        {
            return $"{ControlName}, {AnimationCount} animations";
        }
    }
}
