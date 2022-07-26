using System.ComponentModel;

namespace WinHue.Pages
{
    internal class Home : IPage
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title => "Home";

    }
}
