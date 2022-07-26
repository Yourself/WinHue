using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WinHue.Pages;

namespace WinHue
{
    internal sealed class Main : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Main() {
            mPages = Enumerable.Empty<IPage>();
        }

        public IEnumerable<IPage> Pages
        {
            get => mPages;
            private set
            {
                if (mPages == value) return;
                mPages = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private IEnumerable<IPage> mPages;
    }
}
