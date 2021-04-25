using System.ComponentModel;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public abstract class PaaElementModel : NotifyPropertyChanged, IElementModel
    {
        public abstract object Element { get; }
        public abstract string ElementName { get; }
        public PaaModel PaaModel { get; }
        protected PaaElementModel(PaaModel paaModel)
        {
            PaaModel = paaModel;
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
                            PaaModel.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        public abstract void Update();

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ElementName));
            Update();
        }
    }
}
