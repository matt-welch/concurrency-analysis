using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ConcucrrencyTiming
{
    class SemaphoreTest
    {
        /* This class is trying to determine the cost of semaphores in a software system.
         * If they're not used, the costs include potential for deadlock and race conditions.
         * When they are used, the costs include slower operation and reduced performance, but 
         * these come with the benefit of CORRECT programs.  
         * 
         */
        static Thread[] threads;
        static int _numThreads;
        static Semaphore sem;
        static Semaphore exitDataMutex;
        int _initSemCount;
        int _maxSemCount;
        public static List<long> enterData;
        public static List<long> exitData;
        public static bool verboseOutput = false;

        public SemaphoreTest(int numThreads, int param_initialCount, int param_maximumCount)
        {
            enterData = new List<long>();
            exitData = new List<long>();

            _numThreads = Math.Min(numThreads, 100);
            _initSemCount = param_initialCount;
            _maxSemCount = param_maximumCount;
            threads = new Thread[_numThreads];
            sem = new Semaphore(_initSemCount, _maxSemCount);
            exitDataMutex = new Semaphore(1, 1);
        }

        public void run(int numThreads)
        {
            _numThreads = numThreads;
            run();
        }
        public void runReps(int numReps, int maxThreads)
        {
            string testName = "Semaphore Timing Test";
            Console.WriteLine("\nBeginning {0} ({1} reps, max {2} threads)",
               testName, numReps, maxThreads);
            Console.WriteLine("Type,#T,count,min,mean,median,std,max");
            for (int j = 1; j <= maxThreads; j++)
            {
                _numThreads = j;
                //Console.WriteLine("\nSemaphore test with {0} threads ({1} reps): ", j, numReps);
                
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
                    string filename = "SemEnter_T" + String.Format("{0:00}", _numThreads) + datafileExtension;
                    FileWriter writer = new FileWriter(filename, enterData);
                    filename = "SemExit_T" + String.Format("{0:00}", _numThreads) + datafileExtension;
                    writer = new FileWriter(filename, exitData);
                    Console.WriteLine("SemEnter_T{0:00}:\n\t{1}", _numThreads, enterStats.ToString());
                    Console.WriteLine("SemExit_T{0:00}:\n\t{1}", _numThreads, exitStats.ToString());
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
            
            //Console.WriteLine("{0} is waiting in line...", Thread.CurrentThread.Name);
            clock.Start();
            sem.WaitOne();
            clock.Stop();
            enterData.Add(clock.ElapsedTicks);
            //Console.WriteLine("{0} enters the semaphore test after {1} ticks", Thread.CurrentThread.Name, myClock.ElapsedTicks);
            //Console.WriteLine("{0} is leaving the semaphore test", Thread.CurrentThread.Name);
            clock.Restart();
            sem.Release();
            clock.Stop();
            exitDataMutex.WaitOne();
            exitData.Add(clock.ElapsedTicks);
            exitDataMutex.Release();
            
            //Console.WriteLine("{0} waited for {1} ticks", Thread.CurrentThread.Name, myClock.ElapsedTicks);
        }
    }
}
