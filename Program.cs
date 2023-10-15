namespace TestingConsoleDotNet8;

internal class Program
{
    static void Main()
    {
        long totalTicks = 0;
        const int limit = 1000000;
        const int repetitions = 100;
        var lockObject = new object();
        var stopwatch = new Stopwatch();

        Console.WriteLine("Press [M] For multi-threaded or Press [S] for single-threaded Prime Counting");
        var input = Console.ReadKey().Key;
        Console.WriteLine();

        if (input == ConsoleKey.M)
        {
            MultiThreaded();
        }
        else if (input == ConsoleKey.S)
        {
            SingleThreaded();
        }
        else
        {
            Console.WriteLine("No valid option selected. Press any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        void SingleThreaded()
        {
            for (int i = 0; i < repetitions; i++)
            {
                stopwatch.Restart();
                int primeCount = CountPrimes(limit);
                stopwatch.Stop();
                totalTicks += stopwatch.ElapsedTicks;

                Console.Clear();
                Console.WriteLine($"Iteration {i + 1}: Found {primeCount} prime numbers in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        void MultiThreaded()
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.For(0, repetitions, options, i =>
            {
                stopwatch.Restart();
                int primeCount = CountPrimesMultiThreaded(limit);
                stopwatch.Stop();
                totalTicks += stopwatch.ElapsedTicks;

                lock (lockObject)
                {
                    Console.Clear();
                    Console.WriteLine($"Iteration {i + 1}: Found {primeCount} prime numbers in {stopwatch.ElapsedMilliseconds} ms");
                }
            });
        }

        double averageTimeMs = (double)totalTicks / Stopwatch.Frequency / repetitions * 1000;
        double averageTimeSeconds = (double)totalTicks / Stopwatch.Frequency / repetitions;

        Console.WriteLine($"\nAverage time for {repetitions} iterations: \n\n{averageTimeMs} milliseconds \n{averageTimeSeconds} seconds");
        Console.WriteLine("\nPress Any Key To Exit");

        Console.ReadKey();
    }

    static int CountPrimes(int n)
    {
        if (n < 2) return 0;
        int count = 0;
        int sieveSize = 10000;

        for (int k = 0; k < n; k += sieveSize)
        {
            bool[] sieve = new bool[Math.Min(sieveSize, n - k)];
            int start = k == 0 ? 2 : k;

            int sqrt = (int)Math.Sqrt(k + sieveSize) + 1;

            for (int i = 2; i < sqrt; i++)
            {
                if (i < start) continue;
                if (!sieve[i - k])
                {
                    for (int j = Math.Max(i * i, (k + i - 1) / i * i); j < k + sieveSize; j += i)
                    {
                        if (j - k < sieve.Length)
                            sieve[j - k] = true;
                    }
                }
            }

            for (int i = 0; i < sieve.Length; i++)
            {
                if (k + i < n && !sieve[i])
                {
                    count++;
                }
            }
        }

        return count;
    }

    static int CountPrimesMultiThreaded(int n)
    {
        int processorCount = Environment.ProcessorCount;
        int count = 0;

        Task<int>[] tasks = new Task<int>[processorCount];
        int partitionSize = n / processorCount;

        for (int p = 0; p < processorCount; p++)
        {
            int start = p * partitionSize;
            int end = p == processorCount - 1 ? n : (p + 1) * partitionSize;
            tasks[p] = Task.Run(() => CountPrimesInRange(start, end));
        }

        Task.WaitAll(tasks);

        foreach (var task in tasks)
        {
            count += task.Result;
        }

        return count;
    }

    static int CountPrimesInRange(int start, int end)
    {
        int count = 0;
        bool[] sieve = new bool[end - start];

        int sqrt = (int)Math.Sqrt(end) + 1;
        for (int i = 2; i < sqrt; i++)
        {
            if (i >= start && !sieve[i - start])
            {
                for (int j = Math.Max(i * i, (start + i - 1) / i * i); j < end; j += i)
                {
                    if (j >= start)
                    {
                        sieve[j - start] = true;
                    }
                }
            }
        }

        for (int i = 0; i < end - start; i++)
        {
            if (i + start < end && i >= 2 && !sieve[i])
            {
                count++;
            }
        }

        return count;
    }
}
