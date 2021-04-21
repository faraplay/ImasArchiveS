using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    public class UIAnimationModel : PaaElementModel
    {
        private Animation animation;
        public override object Element => animation;
        public string ShortDesc => animation switch
        {
            Animation0 a => $"Animation0 Time: {FormatTime(a.time)}",
            VisibilityAnimation a => $"Visibility Time: {FormatTime(a.time)}",
            Animation3 a => $"Animation3 Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            PositionAnimation a => $"Position Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            OpacityAnimation a => $"Position Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            ScaleAnimation a => $"Position Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            AngleAnimation a => $"Position Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            SpriteAnimation a => $"Visibility Time: {FormatTime(a.time)}",
            ColorAnimation a => $"Position Time: {FormatTime(a.startTime)}-{FormatTime(a.endTime)}",
            _ => "Unknown Animation"
        };

        public UIAnimationModel(PaaModel paaModel, Animation animation) : base(paaModel)
        {
            this.animation = animation;
        }

        private string FormatTime(float time)
        {
            return $"{Math.Floor(time)};{(int)Math.Round((time - Math.Floor(time)) * 60):D2}";
        }
    }
}
