using Imas.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : UIModel, IElementModel
    {
        protected abstract UIElementModel Parent { get; }

        protected bool currentVisibility = true;
        public bool CurrentVisibility
        {
            get => currentVisibility;
            set
            {
                currentVisibility = value;
                OnPropertyChanged();
                subcomponent.PauModel.ForceRender();
            }
        }

        public ObservableCollection<UIElementModel> Children { get; set; }
        public abstract UIElement UIElement { get; }
        public object Element => UIElement;
        public abstract string ModelName { get; }

        public string GetUniqueString() => Parent == null ? "0" : $"{Parent.GetUniqueString()},{Parent.Children.IndexOf(this)}";
        public string ParentUniqueID => Parent.GetUniqueString();

        protected UIElementModel(UISubcomponentModel subcomponent, string name) : base(subcomponent, name)
        {
            Children = new ObservableCollection<UIElementModel>();
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool forceVisible)
        {
            if (!CurrentVisibility && !forceVisible)
                return;
            foreach (UIElementModel child in Children)
            {
                child.RenderElement(drawingContext, multiplier, false);
            }
        }

        public virtual void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ModelName));
            subcomponent.PauModel.ForceRender();
        }

        public override int BoundingPixelWidth => 1280;
        public override int BoundingPixelHeight => 720;

        private RelayCommand _displayCommand;

        public ICommand DisplayCommand
        {
            get
            {
                if (_displayCommand == null)
                {
                    _displayCommand = new RelayCommand(
                        _ => {
                            subcomponent.PauModel.DisplayedModel = this;
                        });
                }
                return _displayCommand;
            }
        }

        private RelayCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ =>
                        {
                            subcomponent.PauModel.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }
    }
}
