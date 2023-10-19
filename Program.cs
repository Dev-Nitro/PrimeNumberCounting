
using System.Collections.Generic;

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
        int globalCount = -3;
        int segmentSize = IdealSegmentSize(limit);
        int sqrtLimit = (int)Math.Sqrt(limit);

        // Warm-up Iterations
        // When using NativeAOT Runtime initial threaded can be slower so we run a 100 warm up iterations to get the runtime up to speed before recording the actual measurments
        const int warmUpIterations = 100;
        Console.WriteLine("Warming Up Runtime");
        double warmUpTime = CountPrimesWarmUp(limit, warmUpIterations);
        Console.WriteLine($"\nWarm-up completed in {warmUpTime} ms");

        // Recorded Iterations
        for (int i = 0; i < iterations; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int count = CountPrimes(limit, segmentSize, globalCount, sqrtLimit);
            sw.Stop();
            Console.Clear();
            Console.WriteLine($"Iteration {i + 1}: Found {count} prime numbers using segments of {IdealSegmentSize(limit)} in {sw.Elapsed.TotalMilliseconds} ms");
            totalTime += sw.Elapsed.TotalMilliseconds;
        }

        double averageTime = totalTime / iterations;
        Console.WriteLine($"\nAverage execution time over {iterations} iterations: {averageTime} ms");

        Console.WriteLine("\nPress [Space] To run again or press any other key to exit.");
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Spacebar)
        {
            RunProgram();
        }
    }

    public static int CountPrimes(int n, int s, int gl, int sq)
    {
        object lockObject = new();

        // List to store prime numbers
        List<int> primes = new List<int>();
        bool[] isComposite = new bool[s];

        // Pre-calculate primes up to the square root of n
        Parallel.For(2, sq + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
        {
            if (!isComposite[i])
            {
                lock (lockObject) primes.Add(i);
                for (int j = i * i; j <= sq; j += i)
                {
                    isComposite[j] = true;
                }
            }
        });

        // Multi-threaded segment logic
        Parallel.For(0, (n / s) + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, segment =>
        {
            int start = segment * s;
            int end = Math.Min(start + s, n);
            int localCount = 0;
            bool[] segmentIsComposite = new bool[s];

            // Sieve of Eratosthenes logic for each segment
            foreach (int prime in primes)
            {
                int startMultiple = start / prime * prime;
                if (startMultiple < start) startMultiple += prime;

                for (int j = Math.Max(startMultiple, prime * prime); j < end; j += prime)
                {
                    segmentIsComposite[j - start] = true;
                }
            }

            // Counting local primes for each segment
            for (int i = 0; i < s && start + i <= n; i++)
            {
                if (!segmentIsComposite[i]) localCount++;
            }

            gl += localCount;
        });

        return gl;
    }
    private static int IdealSegmentSize(int n)
    {
        // Method to calculate the ideal segment size for Multi-threading based on your processor
        int processorCount = Environment.ProcessorCount;
        int idealSegmentSize = n / (2 * processorCount);
        return idealSegmentSize;
    }
    private static double CountPrimesWarmUp(int n, int warmUpIterations)
    {
        // Warm-Up Iterations Logic
        double totalTime = 0;
        for (int i = 0; i < warmUpIterations; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CountPrimes(n, IdealSegmentSize(n), -3, (int)Math.Sqrt(n));
            sw.Stop();
            totalTime += sw.Elapsed.TotalMilliseconds;
        }

        return totalTime / warmUpIterations;
    }
}
