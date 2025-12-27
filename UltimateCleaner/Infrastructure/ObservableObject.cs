using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryCleaner.Infrastructure;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string? prop = null)
    => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));


    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? prop = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        RaisePropertyChanged(prop);
        return true;
    }
}
