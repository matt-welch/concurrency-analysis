using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;

namespace ConcucrrencyTiming
{
    class SemaphoreTest
    {
        /* This class is trying to determine the cost of semaphores in a software system.
         * If they're not used, the costs include potential for deadlock and race conditions.
         * When they are used, the costs include slower operation and reduced performance, but 
         * these come with the benefit of CORRECT programs.  
         */
        static Thread[] threads;
        static int _numThreads;
        static Semaphore sem;
        int _initSemCount;
        int _maxSemCount; 
        public SemaphoreTest(int numThreads, int param_initialCount, int param_maximumCount)
        {
            _numThreads = Math.Min(numThreads, 100);
            _initSemCount = param_initialCount;
            _maxSemCount = param_maximumCount;
            threads = new Thread[_numThreads];
            sem = new Semaphore(_initSemCount, _maxSemCount);
        }

        public void run()
        {
            for (int i = 0; i < 10; i++)
            {
                threads[i] = new Thread(threadFcn);
                threads[i].Name = "thread_" + i;
                threads[i].Start();
            }
            Console.Read();
        }

        static void threadFcn()
        {
            Console.WriteLine("{0} is waiting in line...", Thread.CurrentThread.Name);
            sem.WaitOne();
            Console.WriteLine("{0} enters the semaphore test", Thread.CurrentThread.Name);
            Thread.Sleep(300);
            Console.WriteLine("{0} is leaving the semaphore test", Thread.CurrentThread.Name);
            sem.Release();
        }
    }
}
