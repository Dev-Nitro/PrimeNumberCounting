
namespace PrimeNumberCounting;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Press any key to start\n");
        Console.ReadKey(true);
        RunProgram();
    }

    static void RunProgram()
    {
        // Times might be faster or slower depending on your system hardware keep in mind
        int limit = 1000000;
        int iterations = 100;
        double totalTime = 0;

        // Warm-up Iterations
        // When using NativeAOT Runtime initial threaded can be slower so we run a 100 warm up iterations to get the runtime up to speed before recording the actual measurments
        const int warmUpIterations = 100;
        Console.WriteLine("Warming Up Runtime");
        double warmUpTime = CountPrimesWithWarmUp(limit, warmUpIterations);
        Console.Clear();
        Console.WriteLine($"Warm-up completed in {warmUpTime} ms\n");

        // Recorded Iterations
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

        Console.WriteLine("\nPress [Space] To run again or press any other key to exit.");
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Spacebar)
        {
            Console.Clear();
            RunProgram();
        }
    }

    public static int CountPrimes(int n)
    {
        // Seperates n into different segments based on segmentSize, then proceeds to run each segment multi-threaded, each thread will calcualte how many prime numbers are within its segment then add back to the global count of Primes counted then returns when each thread completes its segment
        const int segmentSize = 10000; // 1000 2500 5000 10000 are all square roots of 1000000 that you can use. I have the fastest times with 10000
        int sqrt = (int)Math.Sqrt(n);
        int globalCount = -1;
        object lockObject = new();

        // pre-calculate the primes up to the square root of n before running the multi-threaded segments. This way, you avoid redundant calculations within each segment.
        List<int> primes = new();
        for (int i = 2; i <= sqrt; i++)
        {
            bool isPrime = true;
            foreach (int prime in primes)
            {
                if (i % prime == 0)
                {
                    isPrime = false;
                    break;
                }
            }
            if (isPrime) primes.Add(i);
        }

        // Multi-threaded segment logic
        Parallel.For(0, (n / segmentSize) + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, segment =>
        {
            int start = segment * segmentSize;
            int end = Math.Min(start + segmentSize, n);
            int localCount = 0;

            bool[] isPrime = new bool[segmentSize];
            Array.Fill(isPrime, true);

            // Sieve of Eratosthenes logic
            foreach (int prime in primes)
            {
                if (prime * prime > start + segmentSize) break;

                int offset = start % prime == 0 ? 0 : prime - (start % prime);

                if (start % prime == 0)
                {
                    for (int i = offset; i < segmentSize; i += prime)
                    {
                        if (i + start != prime) isPrime[i] = false;
                    }
                }
                else
                {
                    int j = (start / prime + 1) * prime;

                    if (j < start) j += prime;

                    for (int i = j - start; i < segmentSize; i += prime)
                    {
                        isPrime[i] = false;
                    }
                }
            }

            for (int i = 0; i < segmentSize && start + i <= n; i++)
            {
                if (isPrime[i]) localCount++;
            }

            lock (lockObject) globalCount += localCount;
        });

        return globalCount;
    }
    public static double CountPrimesWithWarmUp(int n, int warmUpIterations)
    {
        // Warm-Up Iterations Logic
        double totalTime = 0;
        for (int i = 0; i < warmUpIterations; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CountPrimes(n);
            sw.Stop();
            totalTime += sw.Elapsed.TotalMilliseconds;
        }

        return totalTime / warmUpIterations;
    }
}
