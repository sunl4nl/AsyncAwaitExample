using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
//            EG_ThreadTask();

//            EG_GetAsyncResult();

//            EG_SynchronizationContext();

//            EG_TaskReturn();

//            EG_ConcurrentQueue();

            Console.Read();

            
        }

        #region 5.并行库

        static void EG_ConcurrentQueue()
        {
            // Construct a ConcurrentQueue.
            ConcurrentQueue<int> cq = new ConcurrentQueue<int>();

            // Populate the queue.
            for (int i = 0; i < 10000; i++) cq.Enqueue(i);

            // Peek at the first element.
            int result;
            if (!cq.TryPeek(out result))
            {
                Console.WriteLine("CQ: TryPeek failed when it should have succeeded");
            }
            else if (result != 0)
            {
                Console.WriteLine("CQ: Expected TryPeek result of 0, got {0}", result);
            }

            int outerSum = 0;
            // An action to consume the ConcurrentQueue.
            Action action = () =>
            {
                int localSum = 0;
                int localValue;
                while (cq.TryDequeue(out localValue)) localSum += localValue;
                Interlocked.Add(ref outerSum, localSum);
            };

            // Start 4 concurrent consuming actions.
            Parallel.Invoke(action, action, action, action);

            Console.WriteLine("outerSum = {0}, should be 49995000", outerSum);
        }


        #endregion

        #region 4.单线程异步
        private static int loopCount = 0;
        private static long time;
        private static TaskCompletionSource<bool> tcs;

        static void EG_TaskReturn()
        {
            Console.WriteLine($"主线程: {Thread.CurrentThread.ManagedThreadId}");
            Coroutine();
            while (true)
            {
                Thread.Sleep(1);
                CheckTimerOut();
                ++loopCount;
            }
        }

        private static void CheckTimerOut()
        {
            if (time == 0)
            {
                return;
            }
            long nowTicks = DateTime.Now.Ticks / 10000;
            if (time > nowTicks)
            {
                return;
            }

            time = 0;
            tcs.SetResult(true);
        }

        static async void Coroutine()
        {
            await WaitTimeAsync(1000);
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
            await WaitTimeAsync(2000);
            Console.WriteLine($"当前线程: {Thread.CurrentThread.ManagedThreadId}, WaitTimeAsync finsih loopCount的值是: {loopCount}");
        }

        static Task WaitTimeAsync(long waitTime)
        {
            TaskCompletionSource<bool> t = new TaskCompletionSource<bool>();
            tcs = t;
            time = DateTime.Now.Ticks / 10000 + waitTime;
            return t.Task;
        }


        #endregion

        #region 3.SynchronizationContext

        static void EG_SynchronizationContext()
        {
            SynchronizationContext _sync = new SynchronizationContext();
            Console.WriteLine("主线程执行");
            Thread.Sleep(2000);

            Thread td = new Thread(() =>
            {
                _sync.Send(EventMethod, "子线程Send");
                _sync.Post(EventMethod, "子线程Post");
            });
            td.Start();
            Console.WriteLine("主线程结束");
        }

        static void EventMethod(object arg)
        {   
            Console.WriteLine("CallBack::当前线程id：" + Thread.CurrentThread.ManagedThreadId + "     arg:" + (string)arg);
            Thread.Sleep(3000);
        }

        #endregion
        

        //1.EG
        static void EG_ThreadTask()
        {
            Console.WriteLine($"头部已执行，当前主线程Id为：{Thread.CurrentThread.ManagedThreadId}");
            var result = AsyncTaskName("John");
            Console.WriteLine($"尾部已执行，当前主线程Id为：{Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"B result：{result} 当前主线程Id为 {Thread.CurrentThread.ManagedThreadId}");
        }

        //2.EG
        static void EG_GetAsyncResult()
        {
            Console.WriteLine("执行前.....");
            GetResultAsync();
            Console.WriteLine("执行中.....");
        }

        async static void GetResultAsync()
        {
            Console.WriteLine("1111.....");
            Task<int> task1 = GetResult(10);
            Task<int> task2 = GetResult(20);
            await Task.WhenAll(task1, task2);
            Console.WriteLine($"结果分别为：{task1.Result}和{task2.Result}");
        }

        static Task<int> GetResult(int number)
        {
            return Task.Run<int>(() =>
            {
                Task.Delay(5000).Wait();
                return number + 10;
            });
        }


        async static Task<string> AsyncTaskName(string name)
        {
            Console.WriteLine($"异步调用头部执行，当前线程Id为：{Thread.CurrentThread.ManagedThreadId}");
            var result = await SayHiAsync(name);
            Console.WriteLine($"异步调用尾部执行，当前线程Id为：{Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"A result：{result} 当前线程Id为 {Thread.CurrentThread.ManagedThreadId}");
            return result;
        }

        static Task<string> SayHiAsync(string name)
        {
            return Task.Run<string>(() =>
            {   
                return SayHi(name);
            });
        }

        static string SayHi(string name)
        {
            Task.Delay(2000).Wait();//异步等待2s
            Console.WriteLine($"SayHi执行，当前线程Id为：{Thread.CurrentThread.ManagedThreadId}");
            return $"Hello,{name}";
        }
    }
}
