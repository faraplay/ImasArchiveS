using System.Windows.Input;

namespace ImasArchiveApp
{
    public abstract class PtaElementModel : UIModel, IElementModel
    {
        public abstract object Element { get; }
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
    }
}
