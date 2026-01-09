using CSharp14.Extensions;
using CSharp14.Models;

namespace CSharp14;

public static class Demo03
{
    public static void Run()
    {
        List<string> names = ["John", "Mary", "Peter", "Paul"];

        var first1 = names.MyFirst();
        var first2 = names.MyOtherFirst();
        var first3 = names.FirstAsAProperty;
        var range = int.RangeFromOne(5);
        var rangeWithScaler = int.RangeFromOne(10) * 7;
        var vector = rangeWithScaler.ToArray();
        vector *= 10;
    }
}