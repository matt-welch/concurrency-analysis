using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// classes required by this program: 
using System.Diagnostics; // Stopwatch
using System.Threading;

namespace ConcucrrencyTiming
{
    class ThreadTimingTest
    {
        public static Stopwatch sharedClock = new Stopwatch();
        public static Stopwatch[] clockArray = new Stopwatch[10];
        public static Thread[] threads = new Thread[10];
        public static string timerUnits = "ticks";
        public static List<long> sharedCreateResults;
        public static List<long> sharedStartResults;
        public static List<long> sharedStopResults;

        public void run(int iter)
        {
            Console.WriteLine("Beginning Thread Timing Test (Create, start, join)");
            Thread thread1;
            sharedCreateResults = new List<long>();
            sharedStartResults = new List<long>();
            sharedStopResults = new List<long>();
            for (int i = 0; i < iter; i++)
            {
                // inner loop on number of tws
                sharedClock.Start();
                thread1 = new Thread(new ThreadStart(simpleTimerThread));
                sharedClock.Stop();
                sharedStopResults.Add(sharedClock.ElapsedTicks);

                sharedClock.Restart();
                thread1.Start();
                thread1.Join();
                sharedClock.Stop();
                sharedStartResults.Add(sharedClock.ElapsedTicks);
            }

            Stats spawnStats = new Stats(sharedCreateResults.ToArray());
            string filename = "ThreadCreate.txt";
            FileWriter writer = new FileWriter(filename, sharedCreateResults);
            Stats joinStats = new Stats(sharedStartResults.ToArray());
            filename = "ThreadSpawn.txt";
            writer = new FileWriter(filename, sharedStartResults);
            Stats createStats = new Stats(sharedStopResults.ToArray());
            filename = "ThreadJoin.txt";
            writer = new FileWriter(filename, sharedStopResults);

            Console.WriteLine("Thread Create(ticks):\n\t {0}", createStats.ToString());
            Console.WriteLine("Thread Spawn(ticks):\n\t {0}", spawnStats.ToString());
            Console.WriteLine("Thread Join (ticks):\n\t {0}", joinStats.ToString());
            Console.WriteLine();
        }
        static void simpleTimerThread()
        {
            sharedClock.Stop();
            sharedCreateResults.Add(sharedClock.ElapsedTicks);
            //Console.WriteLine("Thread spawn time = {0} ticks", readClock.ElapsedTicks);
            // store the spawn timing data here
            sharedClock.Restart();
        }

        public void runMultiThread(int numIter, int maxThreads)
        {
            Console.WriteLine("Beginning MultiThread Timing Test (create, start, join)");

            for (int j = 0; j < numIter; j++)
            {
                sharedCreateResults = new List<long>();
                sharedStartResults = new List<long>();
                sharedStopResults = new List<long>();

                for (int i = 0; i < maxThreads; i++)
                {
                    // inner loop on number of tws
                    clockArray[i].Start();
                    threads[i] = new Thread(new ThreadStart(multiThreadTimer));
                    clockArray[i].Stop();
                    sharedStopResults.Add(sharedClock.ElapsedTicks);

                    clockArray[i].Restart();
                    threads[i].Start();
                }
                for (int i = 0; i < maxThreads; i++)
                {
                    threads[i].Join();
                    clockArray[i].Stop();
                    sharedStartResults.Add(clockArray[i].ElapsedTicks);
                }
                Stats spawnStats = new Stats(sharedCreateResults.ToArray());
                string filename = "ThreadCreate.txt";
                FileWriter writer = new FileWriter(filename, sharedCreateResults);
                Stats joinStats = new Stats(sharedStartResults.ToArray());
                filename = "ThreadSpawn.txt";
                writer = new FileWriter(filename, sharedStartResults);
                Stats createStats = new Stats(sharedStopResults.ToArray());
                filename = "ThreadJoin.txt";
                writer = new FileWriter(filename, sharedStopResults);

                Console.WriteLine("Thread Create(ticks):\n\t {0}", createStats.ToString());
                Console.WriteLine("Thread Spawn(ticks):\n\t {0}", spawnStats.ToString());
                Console.WriteLine("Thread Join (ticks):\n\t {0}", joinStats.ToString());
                Console.WriteLine();
            }
                
        }

        public void runMultiStatefulThreads(int maxThreads, int numReps)
        {
            Console.WriteLine("Beginning Stateful MultiThread Timing Test (create, start, join)");
            for (int i = 0; i < maxThreads; i++)
            {
                // initialize results lists
                sharedCreateResults = new List<long>();// thread.create
                sharedStartResults = new List<long>();// thread.start
                sharedStopResults = new List<long>();// thread.join
                int maxThreadsCurrent = i + 1;
                Console.WriteLine("Number of Threads = {0}", maxThreadsCurrent);
                statefulThread[] tws = new statefulThread[maxThreadsCurrent];
                Thread[] threads = new Thread[maxThreadsCurrent];

                for (int k = 0; k < maxThreadsCurrent; k++)
                {// initialize the statefulThreads to their correct TID
                    tws[k] = new statefulThread(k + 1);
                }

                for (int j = 0; j < numReps; j++)
                {// repeat the measurements for the correct number of reps 
                    for (int k = 0; k < maxThreadsCurrent; k++)
                    {
                        // initialize the threads 
                        tws[k].clock.Start();
                        threads[k] = new Thread(new ThreadStart(tws[k].threadFcn));
                        tws[k].clock.Stop();
                        tws[k].createResults.Add(tws[k].clock.ElapsedTicks);
                        
                        tws[k].clock.Restart();
                        threads[k].Start();
                    }

                    for (int k = 0; k < maxThreadsCurrent; k++)
                    {// join threads
                        tws[k].clock.Start();
                        threads[k].Join();
                        tws[k].clock.Stop();
                        tws[k].joinResults.Add(tws[k].clock.ElapsedTicks);
                    }
                }
                for (int k = 0; k < maxThreadsCurrent; k++)
                {
                    sharedCreateResults.AddRange(tws[k].createResults);
                    sharedStartResults.AddRange(tws[k].startResults);
                    sharedStopResults.AddRange(tws[k].joinResults);
                }
                Stats createStats = new Stats(sharedCreateResults.ToArray());
                Stats startStats = new Stats(sharedStartResults.ToArray());
                Stats joinStats = new Stats(sharedStopResults.ToArray());

                Console.WriteLine("Thread Create(ticks) N={1}:\n\t {0}", createStats.ToString(), maxThreadsCurrent);
                Console.WriteLine("Thread Spawn(ticks) N={1}:\n\t {0}", startStats.ToString(), maxThreadsCurrent);
                Console.WriteLine("Thread Join (ticks) N={1}:\n\t {0}", joinStats.ToString(), maxThreadsCurrent);
                Console.WriteLine();
            }
        }

        static void multiThreadTimer()
        {

            sharedClock.Stop();
            sharedCreateResults.Add(sharedClock.ElapsedTicks);
            //Console.WriteLine("Thread spawn time = {0} ticks", readClock.ElapsedTicks);
            // store the spawn timing data here
            sharedClock.Restart();
        }
        public class statefulThread
        {
            public int TID;
            public Stopwatch clock;
            public List<long> createResults;
            public List<long> startResults;
            public List<long> joinResults;
            public statefulThread(int paramTID)
            {
                TID = paramTID;
                clock = new Stopwatch();
                createResults = new List<long>();
                startResults= new List<long>();
                joinResults= new List<long>();
            }

            public void threadFcn()
            {
                clock.Stop();
                startResults.Add(clock.ElapsedTicks);
                Thread.Sleep(10);
                clock.Restart();
            }

        }
    }
}
