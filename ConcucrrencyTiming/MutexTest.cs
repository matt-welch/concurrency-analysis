using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;


namespace ConcucrrencyTiming
{
    class MutexTest
    {
        /* This class is trying to determine the cost of mutex in a software system. */
        static Thread[] threads;
        static int _numThreads;
        static Mutex mutex;
        static Mutex exitDataMutex;
        public static List<long> enterData;
        public static List<long> exitData;
        public static bool verboseOutput = false;

        public MutexTest(int numThreads)
        {
            enterData = new List<long>();
            exitData = new List<long>();

            _numThreads = Math.Min(numThreads, 100);
            threads = new Thread[_numThreads];
            mutex = new Mutex();
            exitDataMutex = new Mutex();
        }

        public void run(int numThreads)
        {
            _numThreads = numThreads;
            run();
        }
        public void runReps(int numReps, int maxThreads)
        {
            string testName = "Mutex Timing Test";
            Console.WriteLine("\nBeginning {0} ({1} reps, max {2} threads)",
               testName, numReps, maxThreads);
            Console.WriteLine("Type,#T,count,min,mean,median,std,max");
            for (int j = 1; j <= maxThreads; j++)
            {
                _numThreads = j;
                //Console.WriteLine("\nMutex test with {0} threads ({1} reps): ", j, numReps);
                
                enterData = new List<long>();
                exitData = new List<long>();
                for (int i = 0; i < numReps; i++)
                {
                    run();
                }
                Stats enterStats = new Stats(enterData.ToArray());
                Stats exitStats = new Stats(exitData.ToArray());

                if (verboseOutput)
                {
                    string datafileExtension = ".txt";
                    string filename = "MutexEnter_T" + String.Format("{0:00}", _numThreads) + datafileExtension;
                    FileWriter writer = new FileWriter(filename, enterData);
                    filename = "MutexExit_T" + String.Format("{0:00}", _numThreads) + datafileExtension;
                    writer = new FileWriter(filename, exitData);
                    Console.WriteLine("MutexEnter_T{0:00}:\n\t{1}", _numThreads, enterStats.ToString());
                    Console.WriteLine("MutexExit_T{0:00}:\n\t{1}", _numThreads, exitStats.ToString());
                }
                else
                {
                    Console.WriteLine("Enter,{0},{1}", _numThreads, enterStats.ToCSV());
                    Console.WriteLine("Exit,{0},{1}", _numThreads, exitStats.ToCSV());
                }
            }           
        }

        public void run()
        {
            for (int i = 0; i < _numThreads; i++)
            {
                threads[i] = new Thread(threadFcn);
                threads[i].Name = "thread_" + i;
                threads[i].Start();
            }
            for (int i = 0; i < _numThreads; i++)
            {
                threads[i].Join();
            }

        }

        static void threadFcn()
        {
            Stopwatch clock = new Stopwatch();
            
            clock.Start();
            mutex.WaitOne();
            clock.Stop();
            enterData.Add(clock.ElapsedTicks);
            clock.Restart();
            mutex.ReleaseMutex();
            clock.Stop();
            exitDataMutex.WaitOne();
            exitData.Add(clock.ElapsedTicks);
            exitDataMutex.ReleaseMutex();
        }
    }
}
