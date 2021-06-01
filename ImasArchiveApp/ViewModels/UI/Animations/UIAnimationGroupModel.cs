using Imas.UI;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
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
                listModel.UnapplyAnimations();
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


        public int IndexOf(UIControlAnimationsListModel listModel) => ListModels.IndexOf(listModel);
        public void InsertAnimationList(int index, ControlAnimationsList animationList)
        {
            animationGroup.ControlAnimations.Insert(index, animationList);
            ListModels.Insert(index, new UIControlAnimationsListModel(PaaModel, this, animationList));
            Invalidate();
        }

        public void RemoveAnimation(UIControlAnimationsListModel listModel)
        {
            int index = IndexOf(listModel);
            if (index == -1)
                return;
            ListModels.RemoveAt(index);
            Invalidate();
        }

        public void InsertNewAnimationList(int index) => InsertAnimationList(index, new ControlAnimationsList());
        public void AddNewAnimationList() => InsertNewAnimationList(ListModels.Count);


        private RelayCommand _addAnimationListCommand;
        public ICommand AddAnimationListCommand
        {
            get
            {
                if (_addAnimationListCommand == null)
                    _addAnimationListCommand = new RelayCommand(
                        _ => AddNewAnimationList());
                return _addAnimationListCommand;
            }
        }
        public void Paste()
        {
            try
            {
                ControlAnimationsList animationsList = (ControlAnimationsList)Base64.FromBase64(Clipboard.GetText(), typeof(ControlAnimationsList));
                InsertAnimationList(ListModels.Count, animationsList);
            }
            catch (System.FormatException)
            {
            }
            catch (System.IO.EndOfStreamException)
            { }
        }
        private RelayCommand _pasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                    _pasteCommand = new RelayCommand(
                        _ => Paste());
                return _pasteCommand;
            }
        }

        public void SavePaa(string fileName)
        {
            using FileStream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            animationGroup.SaveToStream(outStream);
        }
    }
}
