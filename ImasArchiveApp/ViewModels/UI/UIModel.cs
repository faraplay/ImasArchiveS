using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class UIModel : RenderableModel
    {
        protected readonly UISubcomponentModel subcomponent;

        protected UIModel(UISubcomponentModel subcomponent, string name) : base(subcomponent, name, subcomponent.GetFileName)
        {
            this.subcomponent = subcomponent;
            //ms = new MemoryStream();
        }

        private RelayCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ => {
                            subcomponent.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }
    }
}
