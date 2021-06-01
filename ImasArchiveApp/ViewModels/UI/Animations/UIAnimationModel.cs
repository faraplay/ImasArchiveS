using Imas.UI;
using System;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class UIAnimationModel : PaaElementModel
    {
        private readonly Animation animation;
        private UIControlAnimationsListModel ParentList { get; }
        public override object Element => animation;
        public override string ElementName => animation switch
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

        public override void Invalidate() => ParentList.Invalidate();

        public void InsertNewAnimation<T>() where T : Animation, new()
        {
            ParentList.InsertNewAnimation<T>(ParentList.IndexOf(this));
        }

        public void Delete()
        {
            ParentList.RemoveAnimation(this);
        }

        private RelayCommand _insertVisibilityAnimationCommand;
        public ICommand InsertVisibilityAnimationCommand
        {
            get
            {
                if (_insertVisibilityAnimationCommand == null)
                    _insertVisibilityAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<VisibilityAnimation>());
                return _insertVisibilityAnimationCommand;
            }
        }
        private RelayCommand _insertPositionAnimationCommand;
        public ICommand InsertPositionAnimationCommand
        {
            get
            {
                if (_insertPositionAnimationCommand == null)
                    _insertPositionAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<PositionAnimation>());
                return _insertPositionAnimationCommand;
            }
        }
        private RelayCommand _insertOpacityAnimationCommand;
        public ICommand InsertOpacityAnimationCommand
        {
            get
            {
                if (_insertOpacityAnimationCommand == null)
                    _insertOpacityAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<OpacityAnimation>());
                return _insertOpacityAnimationCommand;
            }
        }
        private RelayCommand _insertScaleAnimationCommand;
        public ICommand InsertScaleAnimationCommand
        {
            get
            {
                if (_insertScaleAnimationCommand == null)
                    _insertScaleAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<ScaleAnimation>());
                return _insertScaleAnimationCommand;
            }
        }
        private RelayCommand _insertAngleAnimationCommand;
        public ICommand InsertAngleAnimationCommand
        {
            get
            {
                if (_insertAngleAnimationCommand == null)
                    _insertAngleAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<AngleAnimation>());
                return _insertAngleAnimationCommand;
            }
        }
        private RelayCommand _insertSpriteAnimationCommand;
        public ICommand InsertSpriteAnimationCommand
        {
            get
            {
                if (_insertSpriteAnimationCommand == null)
                    _insertSpriteAnimationCommand = new RelayCommand(
                        _ => InsertNewAnimation<SpriteAnimation>());
                return _insertSpriteAnimationCommand;
            }
        }

        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(
                        _ => Delete());
                return _deleteCommand;
            }
        }
    }
}
