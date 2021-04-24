using Imas.UI;
using System;

namespace ImasArchiveApp
{
    public class UIAnimationModel : PaaElementModel
    {
        private readonly Animation animation;
        private UIControlAnimationsListModel ParentList { get; }
        public override object Element => animation;
        public string ShortDesc => animation switch
        {
            Animation0 a => $"Animation0 Time: {FormatTime(a.Time)}",
            VisibilityAnimation a => $"Visibility Time: {FormatTime(a.Time)}",
            Animation3 a => $"Animation3 Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            PositionAnimation a => $"Position Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            OpacityAnimation a => $"Opacity Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            ScaleAnimation a => $"Scale Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            AngleAnimation a => $"Angle Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            SpriteAnimation a => $"Sprite Time: {FormatTime(a.Time)}",
            ColorAnimation a => $"Color Time: {FormatTime(a.StartTime)}-{FormatTime(a.EndTime)}",
            _ => "Unknown Animation"
        };

        public UIAnimationModel(PaaModel paaModel, UIControlAnimationsListModel parentList, Animation animation) : base(paaModel)
        {
            this.animation = animation;
            ParentList = parentList;
        }

        private string FormatTime(float time)
        {
            return $"{Math.Floor(time)};{(int)Math.Round((time - Math.Floor(time)) * 60):D2}";
        }

        public override void Update()
        {
            ParentList.Update();
            OnPropertyChanged(nameof(ShortDesc));
        }
    }
}
