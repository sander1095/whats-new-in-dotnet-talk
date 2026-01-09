using System.Runtime.CompilerServices;

namespace TaskProgressDemo.RandomNumbers;

internal static class RandomNumberGenerator
{
    public static async IAsyncEnumerable<int> GenerateRandomNumbers([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var random = new Random();
    
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return random.Next(1, 1000);
            await Task.Delay(1000, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
        }
    }
}