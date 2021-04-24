using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class PaaModel : SubcompPart<PaaElementModel>
    {
        protected override int VisibilityIndex => 2;
        public ObservableCollection<UIAnimationGroupModel> AnimationGroups { get; }

        private UIAnimationGroupModel _selectedAnimationGroupModel;
        public UIAnimationGroupModel SelectedAnimationGroupModel
        {
            get => _selectedAnimationGroupModel;
            set
            {
                _selectedAnimationGroupModel?.RemoveAnimations();
                _selectedAnimationGroupModel = value;
                _selectedAnimationGroupModel?.ApplyAnimations();
                subcomponentModel.PauModel.ForceRender();
                OnPropertyChanged();
            }
        }

        public PaaModel(UISubcomponentModel subcomponentModel) : base(subcomponentModel)
        {
            AnimationGroups = new ObservableCollection<UIAnimationGroupModel>();
            foreach (var animationGroup in subcomponentModel.Subcomponent.animationGroups)
            {
                AnimationGroups.Add(new UIAnimationGroupModel(this, animationGroup));
            }
            PropertyChangedEventHandler = UpdateTimelines;
        }

        public void Reset()
        {
            SelectedAnimationGroupModel = null;
            subcomponentModel.PauModel.ResetDefaultValues();
        }

        private RelayCommand _resetCommand;
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new RelayCommand(
                        _ => Reset());
                return _resetCommand;
            }
        }

        public void UpdateTimelines(object sender, PropertyChangedEventArgs e)
        {
            SelectedModel?.Update();
            subcomponentModel.PauModel.ForceRender();
        }
    }
}
