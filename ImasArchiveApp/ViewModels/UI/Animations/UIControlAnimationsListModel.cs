﻿using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIControlAnimationsListModel : PaaElementModel
    {
        private UIAnimationGroupModel ParentGroup { get; }
        private ControlAnimationsList animationsList;
        public override object Element => animationsList;
        public ObservableCollection<UIAnimationModel> Animations { get; }
        public UIControlModel Control { get; }
        public string ShortDesc => $"{animationsList.ControlName}: {animationsList.Animations.Count} animations";
        public UIControlAnimationsListModel(PaaModel paaModel, UIAnimationGroupModel parent, ControlAnimationsList animationsList) : base(paaModel)
        {
            ParentGroup = parent;
            this.animationsList = animationsList;
            Animations = new ObservableCollection<UIAnimationModel>();
            Control = paaModel.subcomponentModel.PauModel.ControlDictionary.GetValueOrDefault(animationsList.ControlName);
            foreach (Animation animation in animationsList.Animations)
            {
                Animations.Add(new UIAnimationModel(paaModel, this, animation));
            }
        }

        public override void Update() => ParentGroup.Update();
        public Timeline GetTimeline()
        {
            ResetTimelines();
            ParallelTimeline timeline = new ParallelTimeline();
            foreach (Animation animation in animationsList.Animations)
            {
                switch (animation)
                {
                    case VisibilityAnimation visibilityAnimation:
                        AddVisibilityAnimation(visibilityAnimation);
                        break;
                    case PositionAnimation positionAnimation:
                        AddPositionAnimation(positionAnimation);
                        break;
                    case OpacityAnimation opacityTransformation:
                        AddOpacityAnimation(opacityTransformation);
                        break;
                    case ScaleAnimation scaleAnimation:
                        AddScaleAnimation(scaleAnimation);
                        break;
                    case AngleAnimation angleAnimation:
                        AddAngleAnimation(angleAnimation);
                        break;
                    case SpriteAnimation spriteAnimation:
                        AddSpriteAnimation(spriteAnimation);
                        break;
                }
            }

            if (VisibilityTimeline.KeyFrames.Count > 0)
            {
                double lastFrameTime = -1;
                double lastFrameValue = 0;
                foreach (var frame in VisibilityTimeline.KeyFrames)
                {
                    if (!(frame is DoubleKeyFrame doubleKeyFrame))
                        continue;
                    if (doubleKeyFrame.KeyTime.TimeSpan.TotalSeconds > lastFrameTime)
                    {
                        lastFrameTime = doubleKeyFrame.KeyTime.TimeSpan.TotalSeconds;
                        lastFrameValue = doubleKeyFrame.Value;
                    }
                }
                VisibilityEndValue = lastFrameValue;
            }

            timeline.Children.Add(VisibilityTimeline);
            timeline.Children.Add(PositionXTimeline);
            timeline.Children.Add(PositionYTimeline);
            timeline.Children.Add(OpacityTimeline);
            timeline.Children.Add(ScaleXTimeline);
            timeline.Children.Add(ScaleYTimeline);
            timeline.Children.Add(AngleTimeline);
            if (Control is UISpriteCollectionModel)
            {
                for (int i = 0; i < SpriteTimelinesCollection.Length; i++)
                {
                    SpriteTimeline.Children.Add(SpriteTimelinesCollection[i]);
                }
            }
            timeline.Children.Add(SpriteTimeline);

            return timeline;
        }

        private double? VisibilityEndValue { get; set; }

        private DoubleAnimationUsingKeyFrames VisibilityTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames PositionXTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames PositionYTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames OpacityTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames ScaleXTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames ScaleYTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames AngleTimeline = new DoubleAnimationUsingKeyFrames();
        private DoubleAnimationUsingKeyFrames[] SpriteTimelinesCollection;
        private ParallelTimeline SpriteTimeline = new ParallelTimeline();

        private void ResetTimelines()
        {
            VisibilityTimeline = new DoubleAnimationUsingKeyFrames();
            PositionXTimeline = new DoubleAnimationUsingKeyFrames();
            PositionYTimeline = new DoubleAnimationUsingKeyFrames();
            OpacityTimeline = new DoubleAnimationUsingKeyFrames();
            ScaleXTimeline = new DoubleAnimationUsingKeyFrames();
            ScaleYTimeline = new DoubleAnimationUsingKeyFrames();
            AngleTimeline = new DoubleAnimationUsingKeyFrames();
            if (Control?.UIElement is SpriteCollection spriteCollection)
            {
                SpriteTimelinesCollection = new DoubleAnimationUsingKeyFrames[spriteCollection.ChildSpriteGroups.Count];
                for (int i = 0; i < spriteCollection.ChildSpriteGroups.Count; i++)
                {
                    SpriteTimelinesCollection[i] = new DoubleAnimationUsingKeyFrames();
                }
            }
            SpriteTimeline = new ParallelTimeline();
        }

        private void AddVisibilityAnimation(VisibilityAnimation animation)
        {
            VisibilityTimeline.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(
                    animation.Visibility == 3 ? 1 : 0,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.Time)))
                );
        }
        private void AddPositionAnimation(PositionAnimation animation)
        {
            for (int i = 0; i < animation.PointCount; i++)
            {
                PositionXTimeline.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        animation.Points[i].X,
                        i == 0 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)) :
                        i == animation.PointCount - 1 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)) :
                        KeyTime.Uniform));
                PositionYTimeline.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        animation.Points[i].Y,
                        i == 0 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)) :
                        i == animation.PointCount - 1 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)) :
                        KeyTime.Uniform));
            }
        }
        private void AddOpacityAnimation(OpacityAnimation animation)
        {
            OpacityTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.StartTransparency,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)))
                );
            OpacityTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.EndTransparency,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)))
                );
        }
        private void AddScaleAnimation(ScaleAnimation animation)
        {
            ScaleXTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.StartXScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)))
                );
            ScaleYTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.StartYScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)))
                );
            ScaleXTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.EndXScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)))
                );
            ScaleYTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.EndYScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)))
                );
        }
        private void AddAngleAnimation(AngleAnimation animation)
        {
            AngleTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    -animation.StartAngle,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.StartTime)))
                );
            AngleTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    -animation.EndAngle,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.EndTime)))
                );
        }
        private void AddSpriteAnimation(SpriteAnimation animation)
        {
            if (Control is UISpriteCollectionModel)
            {
                for (int i = 0; i < SpriteTimelinesCollection.Length; i++)
                {
                    DoubleAnimationUsingKeyFrames spriteTimeline = SpriteTimelinesCollection[i];
                    spriteTimeline.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                        animation.SpriteIndex == i ? 1 : 0,
                        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.Time)))
                        );
                }
            }
        }

        public void ApplyAnimations(ClockGroup clockGroup)
        {
            if (Control == null)
                return;
            Control.VisibilityClock = (AnimationClock)clockGroup.Children[0];
            SetEndVisibility();
            Control.ApplyPositionAnimation((AnimationClock)clockGroup.Children[1], (AnimationClock)clockGroup.Children[2]);
            Control.OpacityClock = (AnimationClock)clockGroup.Children[3];
            Control.ApplyScaleAnimation((AnimationClock)clockGroup.Children[4], (AnimationClock)clockGroup.Children[5]);
            Control.ApplyAngleAnimation((AnimationClock)clockGroup.Children[6]);
            if (Control is UISpriteCollectionModel spriteCollectionModel)
            {
                spriteCollectionModel.ApplySpriteClocks((ClockGroup)clockGroup.Children[7]);
            }
        }

        public void RemoveAnimations()
        {
            if (Control == null)
                return;
            Control.VisibilityClock = null;
            Control.ApplyPositionAnimation(null, null);
            Control.OpacityClock = null;
            Control.ApplyScaleAnimation(null, null);
            Control.ApplyAngleAnimation(null);
            if (Control is UISpriteCollectionModel spriteCollectionModel)
            {
                spriteCollectionModel.ApplySpriteClocks(null);
            }
        }

        private void SetEndVisibility()
        {
            if (Control != null && VisibilityEndValue.HasValue) 
                Control.CurrentVisibility = VisibilityEndValue != 0;
        }
    }
}