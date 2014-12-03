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
    class RWLStatefulTest
    {
        public static List<long> sharedResultsReadAcquire;
        public static List<long> sharedResultsReadRelease;
        public static List<long> sharedResultsWriteAcquire;
        public static List<long> sharedResultsWriteRelease;
        //public static List<long> sharedStopResults;
        public static int readInt = 0;
        public static int writeInt = 0;
        static int numReaderTimeouts = 0; // timeout count
        static int numWriterTimeouts = 0; // timeout count
        public static bool running = true;
        public static bool goFlag = false;
        public static ReaderWriterLock rwl = new ReaderWriterLock();

        // For this example, the shared resource protected by the 
        // ReaderWriterLock is just an integer. 
        static int resource = 0;

        public static int readTimeoutMS = 100; // timeout in ms
        public static int writeTimeoutMS = 100; // timeout in ms
        public static bool verboseOutput = false;

        public class NoDataCollectedException : ApplicationException
        {
                // Use the default ApplicationException constructors
            public NoDataCollectedException() : base() {}
            public NoDataCollectedException(string s) : base(s) {}
            public NoDataCollectedException(string s, Exception ex) : base(s, ex) { }
        }
        public class statefulRWLThread
        {
            public int TID;
            public Stopwatch myClock;
            public List<long> resultsReadAcquire;
            //public List<long> resultsReadRelease;
            public List<long> resultsWriteAcquire;
            //public List<long> resultsWriteRelease;
            private Random rnd = new Random();
            private double randomThresh = 0.8;
            public int readerTimeouts = 0;
            public int writerTimeouts = 0;
            public int rAcquires = 0;
            public int rReleases = 0;
            public int wAcquires = 0;
            public int wReleases = 0;

            public statefulRWLThread(int paramTID)
            {
                TID = paramTID;
                myClock = new Stopwatch();
                resultsReadAcquire = new List<long>();
                //resultsReadRelease = new List<long>();
                resultsWriteAcquire = new List<long>();
                //resultsWriteRelease = new List<long>();
            }

            public  void ThreadProc()
            {
                // As long as a thread runs, it randomly selects 
                // to read and write from the shared resource. 
                // delay all threads from starting until the the flag trips
                do
                {
                    Thread.Sleep(1);
                } while (!goFlag);

                while (running)
                {
                    double action = rnd.NextDouble();
                    if (action < randomThresh)
                        ReadFromResource(readTimeoutMS);
                    else
                        WriteToResource(writeTimeoutMS);
                }
            }

            public void ReadFromResource(int timeOut)
            {
                myClock.Start();
                try
                {
                    rwl.AcquireReaderLock(timeOut);
                    try
                    {
                        myClock.Stop();
                        // It is safe for this thread to read from 
                        // the shared resource.
                        //Console.WriteLine("Thread {0} writes resource value {1}: {2} ticks.", 
                        //    Thread.CurrentThread.Name, resource, readClock.ElapsedTicks);
                        resultsReadAcquire.Add(myClock.ElapsedTicks);
                        myClock.Reset();
                        this.rAcquires++;
                        Interlocked.Increment(ref readInt);
                        myClock.Start();
                    }
                    finally
                    {
                        // Ensure that the lock is released.
                        //TODO timing
                        rwl.ReleaseReaderLock();
                        myClock.Stop();
                        //resultsReadRelease.Add(myClock.ElapsedTicks);
                        myClock.Reset();
                        this.rReleases++;

                    }
                }
                catch (ApplicationException)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref this.readerTimeouts);
                }
                myClock.Reset();
            }

            public void WriteToResource(int timeOut)
            {
                myClock.Start();
                try
                {
                    rwl.AcquireWriterLock(timeOut);
                    try
                    {
                        myClock.Stop();
                        // It is safe for this thread to read or write 
                        // from the shared resource.
                        resource = rnd.Next(500);
                        //Console.WriteLine("Thread {0} reads resource value {1}: {2} ticks.", Thread.CurrentThread.Name, resource, writeClock.ElapsedTicks);
                        resultsWriteAcquire.Add(myClock.ElapsedTicks);
                        myClock.Reset();
                        this.wAcquires++;
                        Interlocked.Increment(ref writeInt);
                        myClock.Start();
                    }
                    finally
                    {
                        // Ensure that the lock is released.
                        rwl.ReleaseWriterLock();

                        myClock.Stop();
                        //resultsWriteRelease.Add(myClock.ElapsedTicks);
                        myClock.Reset();
                        this.wReleases++;
                    }
                }
                catch (ApplicationException)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref this.writerTimeouts);
                }
                myClock.Reset();
            }
        }

        public void runRWLStatefulTest(int maxThreads, int numReps)
        {
            string testName = "RWL MultiThread Timing Test";
            Console.WriteLine("\nBeginning {0} ({1} reps, max {2} threads)",
               testName, numReps, maxThreads);
            Console.WriteLine("Type,#T,count,min,mean,median,std,max");

            int initNumThreads = 0;
            for (int i = initNumThreads; i < maxThreads; i++)
            {
                int numThreads = i + 1;
                // initialize shared vars: 
                readInt = 0;
                writeInt = 0;
                numReaderTimeouts = 0;
                numWriterTimeouts = 0;
                running = true;
                goFlag = false;
                // initialize results lists
                sharedResultsReadAcquire = new List<long>();// acquire read lock
                sharedResultsReadRelease = new List<long>();// release read lock
                sharedResultsWriteAcquire = new List<long>();// acquire write lock
                sharedResultsWriteRelease = new List<long>();// release write lock

               
                
                statefulRWLThread[] tws = new statefulRWLThread[numThreads];
                Thread[] threads = new Thread[numThreads];

                for (int k = 0; k < numThreads; k++)
                {// initialize the statefulThreads to their correct TID
                    tws[k] = new statefulRWLThread(k + 1);
                }

                for (int j = 0; j < numReps; j++)
                {// repeat the measurements for the correct number of reps 
                    for (int k = 0; k < numThreads; k++)
                    {
                        // initialize the threads 
                        threads[k] = new Thread(new ThreadStart(tws[k].ThreadProc));
                        threads[k].Name = new String(Convert.ToChar(i + 65), 1);
                        threads[k].Start();
                    }
                    goFlag = true;
                    Thread.Sleep(1000);

                    // Tell the tws to shut down, then wait until they all 
                    // finish.
                    running = false; // terminate threads
                    
                    for (int k = 0; k < numThreads; k++)
                    {// join threads
                        threads[k].Join();
                    }
                    goFlag = true;
                }
                for (int k = 0; k < numThreads; k++)
                {
                    if (tws[k].resultsReadAcquire.Count == 0)
                    {
                        throw new NoDataCollectedException("No results available for ReadAcquire");
                    }
                    else if (tws[k].resultsWriteAcquire.Count  == 0)
                    {
                        throw new NoDataCollectedException("No results available for WriteAcquire");
                    }
                    sharedResultsReadAcquire.AddRange(tws[k].resultsReadAcquire);
                    //sharedResultsReadRelease.AddRange(tws[k].resultsReadRelease);
                    sharedResultsWriteAcquire.AddRange(tws[k].resultsWriteAcquire);
                    //sharedResultsWriteRelease.AddRange(tws[k].resultsWriteRelease);
                    numReaderTimeouts += tws[k].readerTimeouts;
                    numWriterTimeouts += tws[k].writerTimeouts;
                }
                Stats rAcquireStats = new Stats(sharedResultsReadAcquire.ToArray());
                //Stats rReleaseStats = new Stats(sharedResultsReadRelease.ToArray());
                Stats wAcquireStats = new Stats(sharedResultsWriteAcquire.ToArray());
                //Stats wReleaseStats = new Stats(sharedResultsWriteRelease.ToArray());

                if (verboseOutput)
                {
                    Console.WriteLine("RWL Read Acquire (ticks) N={1}:\n\t {0}", rAcquireStats.ToString(), numThreads);
                    //Console.WriteLine("RWL Read Release (ticks) N={1}:\n\t {0}", rReleaseStats.ToString(), numThreads);
                    Console.WriteLine("RWL Write Acquire (ticks) N={1}:\n\t {0}", wAcquireStats.ToString(), numThreads);
                    //Console.WriteLine("RWL Write Release (ticks) N={1}:\n\t {0}", wReleaseStats.ToString(), numThreads);
                    Console.WriteLine("{0} reads, {1} writes, {2} reader time-outs, {3} writer time-outs.",
                        readInt, writeInt, numReaderTimeouts, numWriterTimeouts);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("ReadAcquire,{1},{0}", rAcquireStats.ToCSV(), numThreads);
                    //Console.WriteLine("ReadRelease,{1},{0}", rReleaseStats.ToCSV(), numThreads);
                    Console.WriteLine("WriteAcquire,{1},{0}", wAcquireStats.ToCSV(), numThreads);
                    //Console.WriteLine("WriteAcquire,{1},{0}", wReleaseStats.ToCSV(), numThreads);
                    if (numReaderTimeouts > 0)
                    {
                        Console.WriteLine("ReadTimeouts,{1},{0}", numReaderTimeouts, numThreads);
                    }
                    if (numWriterTimeouts > 0)
                    {
                        Console.WriteLine("WriteTimeouts,{1},{0}", numWriterTimeouts, numThreads);
                    }
                }
            }
        }
    }
}
