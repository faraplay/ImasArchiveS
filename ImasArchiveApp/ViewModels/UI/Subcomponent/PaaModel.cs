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
                SelectedModel = value;
                if (_selectedAnimationGroupModel == value)
                    return;
                _selectedAnimationGroupModel?.Invalidate();
                _selectedAnimationGroupModel = value;
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
    }
}
