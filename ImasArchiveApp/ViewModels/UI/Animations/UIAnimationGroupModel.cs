using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIAnimationGroupModel
    {
        private PaaModel paaModel;
        private AnimationGroup animationGroup;
        public string Name => animationGroup.FileName[..^4];
        public ObservableCollection<UIControlAnimationsListModel> ListModels { get; }
        public ParallelTimeline Timeline { get; private set; }
        private ClockController controller;
        public UIAnimationGroupModel(PaaModel paaModel, AnimationGroup animationGroup)
        {
            this.paaModel = paaModel;
            this.animationGroup = animationGroup;
            ListModels = new ObservableCollection<UIControlAnimationsListModel>();
            foreach (ControlAnimationsList animationsList in animationGroup.ControlAnimations)
            {
                ListModels.Add(new UIControlAnimationsListModel(paaModel, this, animationsList));
            }
            BuildTimeline();
        }

        public void Update()
        {
            if (paaModel.SelectedAnimationGroupModel == this)
            {
                RemoveAnimations();
                BuildTimeline();
                ApplyAnimations();
            }
            else
            {
                BuildTimeline();
            }
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
