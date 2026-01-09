using CSharp14.Models;

namespace CSharp14;

public static class Demo01
{
    public static void Run()
    {
        Person? person = Person.GetIfAvailable();

        // null-conditional assignment
        person?.Name = "John";
        person?.NameChanged += (sender, args) => Console.WriteLine($"Name changed from {args.OldName} to {args.NewName}");
    }
}