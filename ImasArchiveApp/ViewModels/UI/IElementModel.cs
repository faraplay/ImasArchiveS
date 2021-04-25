using System.ComponentModel;

namespace ImasArchiveApp
{
    public interface IElementModel
    {
        public object Element { get; }
        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e);
    }
}
