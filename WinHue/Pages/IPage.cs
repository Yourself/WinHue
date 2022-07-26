using System.ComponentModel;

namespace WinHue.Pages
{
    internal interface IPage : INotifyPropertyChanged
    {
        public string Title { get; }
    }
}
