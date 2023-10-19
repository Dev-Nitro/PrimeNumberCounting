
namespace PrimeNumberCounting;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Press any key to start the prime number search.\n");
        Console.ReadKey();

        int limit = 1000000;
        int iterations = 100;
        double totalTime = 0;

        for (int i = 0; i < iterations; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int count = CountPrimes(limit);
            sw.Stop();
            Console.WriteLine($"Iteration {i + 1}: Found {count} prime numbers in {sw.Elapsed.TotalMilliseconds} ms");
            totalTime += sw.Elapsed.TotalMilliseconds;
        }

        double averageTime = totalTime / iterations;
        Console.WriteLine($"\nAverage execution time over {iterations} iterations: {averageTime} ms");
        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

    static int CountPrimes(int n)
    {
        const int segmentSize = 10000;
        int sqrt = (int)Math.Sqrt(n);
        int globalCount = 0;

        Parallel.For(0, (n / segmentSize) + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, segment =>
        {
            int start = segment * segmentSize;
            int end = Math.Min(start + segmentSize, n);
            int localCount = 0;

            bool[] isPrime = new bool[segmentSize];
            for (int i = 0; i < segmentSize; i++)
            {
                isPrime[i] = true;
            }

            for (int p = 2; p <= sqrt; p++)
            {
                if (p * p > start + segmentSize)
                {
                    break;
                }

                if (start % p == 0)
                {
                    for (int i = start; i < end; i += p)
                    {
                        if (i != p)
                        {
                            isPrime[i - start] = false;
                        }
                    }
                }
                else
                {
                    int j = (start / p + 1) * p;
                    if (j < start)
                    {
                        j += p;
                    }
                    for (int i = j; i < end; i += p)
                    {
                        isPrime[i - start] = false;
                    }
                }
            }

            for (int i = 0; i < segmentSize && start + i <= n; i++)
            {
                if (isPrime[i])
                {
                    localCount++;
                }
            }

            Interlocked.Add(ref globalCount, localCount);
        });

        return globalCount - 2;
    }
}
