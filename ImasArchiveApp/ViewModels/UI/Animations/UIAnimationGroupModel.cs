using Imas.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIAnimationGroupModel : PaaElementModel
    {
        private readonly PaaModel paaModel;
        private readonly AnimationGroup animationGroup;
        public override object Element => animationGroup;
        public override string ElementName => animationGroup.FileName[..^4];
        public ObservableCollection<UIControlAnimationsListModel> ListModels { get; }
        public ParallelTimeline Timeline { get; private set; }
        private ClockController controller;
        public UIAnimationGroupModel(PaaModel paaModel, AnimationGroup animationGroup) : base(paaModel)
        {
            this.paaModel = paaModel;
            this.animationGroup = animationGroup;
            ListModels = new ObservableCollection<UIControlAnimationsListModel>();
            foreach (ControlAnimationsList animationsList in animationGroup.ControlAnimations)
            {
                ListModels.Add(new UIControlAnimationsListModel(paaModel, this, animationsList));
            }
        }

        public override void Invalidate()
        {
            if (Timeline == null && controller == null)
                return;
            RemoveAnimations();
            Timeline = null;
            controller = null;
        }

        private void BuildTimeline()
        {
            Timeline = new ParallelTimeline();
            //Timeline.RepeatBehavior = RepeatBehavior.Forever;
            foreach (var animationsListModel in ListModels)
            {
                Timeline.Children.Add(animationsListModel.GetTimeline());
            }
            Timeline.Freeze();
        }

        public void ApplyAnimations()
        {
            ClockGroup clockGroup = Timeline.CreateClock();
            int index = 0;
            foreach (Clock clock in clockGroup.Children)
            {
                ListModels[index].ApplyAnimations((ClockGroup)clock);
                ++index;
            }
            controller = clockGroup.Controller;
            controller.Stop();
        }

        public void RemoveAnimations()
        {
            controller?.Stop();
            foreach (var listModel in ListModels)
            {
                listModel.RemoveAnimations();
            }
        }

        private void PlayAnimations()
        {
            if (Timeline == null)
            {
                BuildTimeline();
                ApplyAnimations();
                paaModel.subcomponentModel.PauModel.ForceRender();
            }
            controller?.Begin();
        }

        private void SelectAndPlay()
        {
            paaModel.SelectedAnimationGroupModel = this;
            PlayAnimations();
        }
        private RelayCommand _selectPlayCommand;
        public ICommand SelectPlayCommand
        {
            get
            {
                if (_selectPlayCommand == null)
                {
                    _selectPlayCommand = new RelayCommand(_ => SelectAndPlay());
                }
                return _selectPlayCommand;
            }
        }
    }
}
