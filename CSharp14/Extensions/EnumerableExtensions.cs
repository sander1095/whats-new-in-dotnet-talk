using System.Numerics;

namespace CSharp14.Extensions;

public static class EnumerableExtensions
{
    extension<TSource>(IEnumerable<TSource> source)
    {
        public TSource MyOtherFirst()
        {
            return source.First();
        }

        public TSource FirstAsAProperty
        {
            get => source.First();
        }
    }

    extension<T>(T number) where T : INumber<T>
    {
        public static IEnumerable<T> RangeFromOne(int count)
        {
            var start = T.One;
            for (int i = 0; i < count; i++) yield return start++;
        }

        public static IEnumerable<T> operator *(IEnumerable<T> vector, T scalar)
        {
            return vector.Select(v => v * scalar);
        }
    }

    extension<T>(T[] array) where T : INumber<T>
    {
        public void operator *=(T scalar)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] *= scalar;
            }
        }
    }

    public static TSource MyFirst<TSource>(this IEnumerable<TSource> source) => source.First();


}