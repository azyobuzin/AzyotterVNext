using System.Runtime.CompilerServices;
using Livet;

namespace Azyotter.ViewModels
{
    public abstract class ViewModelBase : ViewModel
    {
        protected void Set<T>(T value, [CallerMemberName] string propName = null)
        {
            PropertyHelper.Set(this, propName, value, this.RaisePropertyChanged);
        }
    }
}
