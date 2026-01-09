using System;

namespace CSharp14.Models;

public class PersonWithFieldBackedProperty
{
    public string Name
    {
        get;
        set
        {
            if (value == field) return;
            var old = field;
            field = value;
            OnNameChanged(new NameChangedEventArgs(old, value));
        }
    }

    // Event other classes can subscribe to
    public event EventHandler<NameChangedEventArgs>? NameChanged;

    // Protected virtual invoker so subclasses can override behavior
    protected virtual void OnNameChanged(NameChangedEventArgs e) => NameChanged?.Invoke(this, e);

    // Event args carrying old and new name
    public class NameChangedEventArgs : EventArgs
    {
        public string OldName { get; }
        public string NewName { get; }

        public NameChangedEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }


    public static PersonWithFieldBackedProperty? GetIfAvailable()
    {
        return null;
    }
}