using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// classes required by this program: 
using System.Diagnostics; // Stopwatch
using System.Threading;
using System.Threading.Tasks;

/* Objective:  Measure performance of various C# concurrency primitives and compare them to Java performance: 
 *      Semaphores 
 *      Mutex
 *      RWL
 *      Thread Creation & deletion
 *      Garbage Collection as a consequence of concurrency
 *      
 * Spawn threads and have them access shared data structures inside locks
 * 
 * Libraries required: 
 *      Threads, System.Diagnostics (stopwatch), 
 *      
 * TODO: Threading example in :\Users\Matt\Dropbox\classes\cse598_DistSoft\HW\LectureReview_Threads_C#\AnalyzeStrings_threaded\AnalyzeStrings_threaded\
 * TODO: timing example in C:\Users\Matt\Dropbox\classes\cse598_DistSoft\HW and A3_submit\windService
 */

namespace ConcucrrencyTiming
{
    
    public class ConcurrencyTester{
        private Stopwatch clock;
        private long [] resultsTicks;
        private long[] resultsMs;
        public long iterations;
        public ConcurrencyTester(long iterations){
            this.iterations = iterations;
            clock = new Stopwatch();
            resultsTicks = new long[iterations];
            resultsMs = new long[iterations];
        }
        public long[] getResults(){
            return resultsTicks;
        }

        public void calibrateClock()
        {
            int ms = 100;
            testClock("ticks", ms);
            Stats msStats = new Stats(resultsMs);
            Stats ticksStats = new Stats(resultsTicks);
            Console.WriteLine(msStats.ToString());
            Console.WriteLine(ticksStats.ToString());
            Console.WriteLine("Ticks/ms = {0:0.00}", ticksStats.sum / msStats.sum * 1000);
        }

        public Stats testClock(string type, int msSleep){
            resultsTicks = new long[iterations];
            resultsMs = new long[iterations];
            for (int i = 0; i < iterations; i++){
                clock.Start();
                {
                    Thread.Sleep(msSleep); 
                }
                clock.Stop();
                resultsTicks[i] = clock.ElapsedTicks;
                resultsMs[i] = clock.ElapsedMilliseconds;
                clock.Reset();
            }
            Stats msStats = new Stats(resultsMs);
            Stats tickStats = new Stats(resultsTicks);
            if (type == "ms")
                return (msStats);
            else
                return (tickStats);
        }


        public void testRWL(){

        }
        public void testThreadSpawn(){

        }
    }

    public class ControlVars
    {
        public static bool writeData = false;

    }
    
    class Program
    {
        public static Stopwatch sharedClock = new Stopwatch();
        public static string timerUnits = "ticks";
        public static List<long> resultsA;
        public static List<long> resultsB;
        public static List<long> resultsC;
       

        static void Main(string[] args)
        {
            // TODO: create shared data structures (objects, ints, computation)
            // TODO: Monitor
            // TODO: spawn threads that all attempt to access the shared data structure
            /* TODO: collect timing data of each of the shared objects
             *      lock time, unlock time, thread spawn time, thread delete time
            */
            // TODO: compare to single threaded performance (inside a VM with 1-6 cores for scaling?)
            Console.WriteLine("Concurrency utilities timing program:");
            
            int numIter = 20;
            Console.Write("Enter the timing type {ms,ticks}: ");
            timerUnits = Console.ReadLine();
            if (timerUnits.Length == 0)
                timerUnits = "ticks";
            
            Console.WriteLine();
            threadSpawnTest(1000);
            RWLTest rwlTest = new RWLTest(numIter);
            rwlTest.run();
            SemaphoreTest semTest = new SemaphoreTest(10,3,3);
            semTest.run();
            clockCalibrationTest(numIter);
            clockTest(numIter);

        }

        static void threadSpawnTest(int iter)
        {
            Console.WriteLine("Beginning Thread Timing Test (Create, start, join)");
            Thread thread1;
            resultsA = new List<long>();
            resultsB = new List<long>();
            resultsC = new List<long>();
            for (int i = 0; i < iter; i++)
            {
                sharedClock.Start();
                thread1 = new Thread(new ThreadStart(simpleTimerThread));
                sharedClock.Stop();
                resultsC.Add(sharedClock.ElapsedTicks);
                sharedClock.Restart();
                thread1.Start();
                thread1.Join();
                sharedClock.Stop();
                resultsB.Add(sharedClock.ElapsedTicks);
            }
            
            Stats spawnStats = new Stats(resultsA.ToArray());
            Stats joinStats = new Stats(resultsB.ToArray());
            Stats createStats = new Stats(resultsC.ToArray());

            Console.WriteLine("Thread Create(ticks):\n\t {0}", createStats.ToString());
            Console.WriteLine("Thread Spawn(ticks):\n\t {0}", spawnStats.ToString());
            Console.WriteLine("Thread Join (ticks):\n\t {0}", joinStats.ToString());
            Console.WriteLine();
        }
        static void simpleTimerThread()
        {
            sharedClock.Stop();
            resultsA.Add(sharedClock.ElapsedTicks);
            //Console.WriteLine("Thread spawn time = {0} ticks", readClock.ElapsedTicks);
            // store the spawn timing data here
            sharedClock.Restart();
        }

        static void clockCalibrationTest(int numIter)
        {
            Console.WriteLine("Beginning Clock Calibration..");
            ConcurrencyTester syscal = new ConcurrencyTester(numIter);
            syscal.calibrateClock();
            Console.WriteLine();
        }
 
        static void clockTest(int numIter)
        {
            string keepGoing;
            do
            {
                //string iter;
                //Console.Write("Enter the number of Iterations: ");
                //iter = Console.ReadLine();
                //if (iter.Length > 0)
                //    numIter = Convert.ToInt32(iter);
                //else
                //    return;
                Console.WriteLine("Beginning clock test...");
                Stopwatch mainClock = new Stopwatch();
                ConcurrencyTester tester = new ConcurrencyTester(numIter);
                mainClock.Start();
                Stats clockStats = tester.testClock(timerUnits, 10);
                Console.WriteLine("Clock test complete: ");
                Console.WriteLine(clockStats.ToString() + " " + timerUnits);

                mainClock.Stop();
                Console.WriteLine("Total time = {0:0.0000} s", mainClock.ElapsedMilliseconds / 1000);
                Console.Write("\nAgain? (any key continues, enter quits): ");
                keepGoing = Console.ReadLine();
            } while (keepGoing.Length > 0);
            Console.WriteLine();
        }
    }
}
//{
//             // Make and start all the workers, keeping them in a list. 
//             Thread[] workers = new Thread[numWorkers]; 
//            ArrayProcess[] process = new ArrayProcess[numWorkers]; 
//             Console.WriteLine(data.Length);
//             int lenOneWorker = data.Length / numWorkers; 
//             for (int i=0; i<numWorkers; i++) { 
//             int start = i * lenOneWorker; 
//             int end = (i+1) * lenOneWorker; 
//             // Special case: make the last worker take up all the excess. 
//             if (i==numWorkers-1) end = data.Length;
//             ArrayProcess p = new ArrayProcess(start, end, data);
//             process[i]=p;
//             Thread worker = new Thread( new ThreadStart(p.addArray));
//             workers[i] = worker; 
//             worker.Start(); 

//}
