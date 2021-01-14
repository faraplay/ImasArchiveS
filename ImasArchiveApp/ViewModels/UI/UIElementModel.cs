using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    public class UIElementModel : INotifyPropertyChanged
    {
        public ObservableCollection<UIElementModel> Children { get; set; }

        public UIElementModel()
        {
            Children = new ObservableCollection<UIElementModel>();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
