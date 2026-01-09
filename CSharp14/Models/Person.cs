using System;

namespace CSharp14.Models;

public class Person
{
    // use a backing field so we can detect changes
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            var old = _name;
            _name = value;
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


    public static Person? GetIfAvailable()
    {
        return null;
    }
}