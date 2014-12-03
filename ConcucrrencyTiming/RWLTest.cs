using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 
using System.Diagnostics; // Stopwatch
using System.Threading;


namespace ConcucrrencyTiming
{
    class RWLTest
    {
        public static List<long> resultsRead;
        public static List<long> resultsWrite;
        //public static List<long> sharedStopResults;
        public static int readInt = 0;
        public static int writeInt = 0;
        static int readerTimeouts = 0;
        static int writerTimeouts = 0; 
        public static bool running = true;
        public static ReaderWriterLock rwl = new ReaderWriterLock();
        public static Stopwatch readClock = new Stopwatch();
        public static Stopwatch writeClock = new Stopwatch();
        // For this example, the shared resource protected by the 
        // ReaderWriterLock is just an integer. 
        static int resource = 0;
        static Random rnd = new Random();
        int _numThreads = 10;

        public static int readTimeout = 100;
        public static int writeTimeout = 100;
        public static bool verboseOutput = false;

        public RWLTest(int iter)
        {
            resultsRead = new List<long>();
            resultsWrite = new List<long>();


        }
        public void run()
        {
            string testName = "ReaderWriterLock Timing Test";
            Console.WriteLine("\nBeginning {0} ({1} reps, max {2} threads)",
                testName, 1000, _numThreads);
            Thread[] t = new Thread[_numThreads];
            for (int i = 0; i < _numThreads; i++)
            {
                t[i] = new Thread(new ThreadStart(ThreadProc));
                t[i].Name = new String(Convert.ToChar(i + 65), 1);
                t[i].Start();

            }
            Thread.Sleep(1000);

            // Tell the tws to shut down, then wait until they all 
            // finish.
            running = false;
            for (int i = 0; i < _numThreads; i++)
            {
                t[i].Join();
            }

            // Display statistics.
            Stats readStats = new Stats(resultsRead.ToArray());
            Stats writeStats = new Stats(resultsWrite.ToArray()); 
            if (verboseOutput)
            {
                Console.WriteLine("{0} reads, {1} writes, {2} reader time-outs, {3} writer time-outs.",
                    readInt, writeInt, readerTimeouts, writerTimeouts);
                Console.WriteLine("Read timing:\n\t {0}", readStats.ToString());
                Console.WriteLine("Write timing:\n\t {0}", writeStats.ToString());
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Read,{0},{1}", _numThreads, readStats.ToCSV());
                Console.WriteLine("Write,{0},{1}", _numThreads, writeStats.ToCSV());
            }
        }
        public void runMultipleThread()
        {
            string testName = "ReaderWriterLock Timing Test";
            Console.WriteLine("\nBeginning {0} ({1} reps, max {2} threads)",
                testName, 1000, _numThreads);
            Thread[] t = new Thread[_numThreads];
            for (int i = 0; i < _numThreads; i++)
            {
                t[i] = new Thread(new ThreadStart(ThreadProc));
                t[i].Name = new String(Convert.ToChar(i + 65), 1);
                t[i].Start();

            }
            Thread.Sleep(1000);

            // Tell the tws to shut down, then wait until they all 
            // finish.
            running = false;
            for (int i = 0; i < _numThreads; i++)
            {
                t[i].Join();
            }

            // Display statistics.
            Stats readStats = new Stats(resultsRead.ToArray());
            Stats writeStats = new Stats(resultsWrite.ToArray());
            if (verboseOutput)
            {
                Console.WriteLine("{0} reads, {1} writes, {2} reader time-outs, {3} writer time-outs.",
                    readInt, writeInt, readerTimeouts, writerTimeouts);
                Console.WriteLine("Read timing:\n\t {0}", readStats.ToString());
                Console.WriteLine("Write timing:\n\t {0}", writeStats.ToString());
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Read,{0},{1}", _numThreads, readStats.ToCSV());
                Console.WriteLine("Write,{0},{1}", _numThreads, writeStats.ToCSV());
            }
        }
        static void ThreadProc()
        {
            // As long as a thread runs, it randomly selects 
            // various ways to read and write from the shared  
            // resource. Each of the methods demonstrates one  
            // or more features of ReaderWriterLock. 
            while (running)
            {
                double action = rnd.NextDouble();
                if (action < .8)
                    ReadFromResource(readTimeout);
                else
                    WriteToResource(writeTimeout);
            }
        }

        // Shows how to request and release a reader lock, and 
        // how to handle time-outs. 
        static void ReadFromResource(int timeOut)
        {
            Stopwatch readClock = new Stopwatch();
            readClock.Start();
            try
            {
                rwl.AcquireReaderLock(timeOut);
                try
                {
                    readClock.Stop();
                    // It is safe for this thread to read from 
                    // the shared resource.
                    //Console.WriteLine("Thread {0} writes resource value {1}: {2} ticks.", 
                    //    Thread.CurrentThread.Name, resource, readClock.ElapsedTicks);
                    resultsRead.Add(readClock.ElapsedTicks);
                    readClock.Reset();
                    Interlocked.Increment(ref readInt);
                }
                finally
                {
                    // Ensure that the lock is released.
                    //TODO timing
                    rwl.ReleaseReaderLock();
                }
            }
            catch (ApplicationException)
            {
                // The reader lock request timed out.
                Interlocked.Increment(ref readerTimeouts);
            }
        }

        // Shows how to request and release the writer lock, and 
        // how to handle time-outs. 
        static void WriteToResource(int timeOut)
        {
            Stopwatch writeClock = new Stopwatch();
            writeClock.Start();
            try
            {
                rwl.AcquireWriterLock(timeOut);
                try
                {
                    writeClock.Stop();
                    // It is safe for this thread to read or write 
                    // from the shared resource.
                    resource = rnd.Next(500);
                    //Console.WriteLine("Thread {0} reads resource value {1}: {2} ticks.", Thread.CurrentThread.Name, resource, writeClock.ElapsedTicks);
                    resultsWrite.Add(writeClock.ElapsedTicks);
                    writeClock.Reset();
                    Interlocked.Increment(ref writeInt);
                }
                finally
                {
                    // Ensure that the lock is released.
                    rwl.ReleaseWriterLock();
                }
            }
            catch (ApplicationException)
            {
                // The writer lock request timed out.
                Interlocked.Increment(ref writerTimeouts);
            }
        }
        static void Display(string msg)
        {
            Console.Write("Thread {0} {1}.       \r", Thread.CurrentThread.Name, msg);
        }
    }
}
