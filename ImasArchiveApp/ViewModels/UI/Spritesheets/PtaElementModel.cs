using System.ComponentModel;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public abstract class PtaElementModel : UIModel, IElementModel
    {
        public abstract object Element { get; }
        public abstract string ElementName { get; }
        public PtaElementModel(UISubcomponentModel subcomponent, string name) : base(subcomponent, name)
        {
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
                            subcomponent.PtaModel.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ElementName));
        }
    }
}
