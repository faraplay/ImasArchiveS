using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIAnimationGroupModel : FileModel
    {
        private AnimationGroup animationGroup;
        public ObservableCollection<UIControlAnimationsListModel> ListModels { get; }
        public ParallelTimeline Timeline { get; }
        private ClockController controller;
        public UIAnimationGroupModel(UISubcomponentModel parent, AnimationGroup animationGroup) : base(parent, animationGroup.FileName)
        {
            this.animationGroup = animationGroup;
            ListModels = new ObservableCollection<UIControlAnimationsListModel>();
            Timeline = new ParallelTimeline();
            //Timeline.RepeatBehavior = RepeatBehavior.Forever;
            foreach (ControlAnimationsList animationsList in animationGroup.controlAnimations)
            {
                UIControlAnimationsListModel animationsListModel = new UIControlAnimationsListModel(parent, animationsList);
                ListModels.Add(animationsListModel);
                Timeline.Children.Add(animationsListModel.Timeline);
            }
            //ApplyAnimations();
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
            controller?.Begin();
        }

        private RelayCommand _playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (_playCommand == null)
                {
                    _playCommand = new RelayCommand(_ => PlayAnimations());
                }
                return _playCommand;
            }
        }
    }
}
