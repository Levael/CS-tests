using System;
using Timings;


class MainProgram
{
    static void Main()
    {
        Thread RunTimingTestInParallel = new Thread(TimingTest.Run);

        RunTimingTestInParallel.Start();

        RunTimingTestInParallel.Join();

        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();
    }
}
