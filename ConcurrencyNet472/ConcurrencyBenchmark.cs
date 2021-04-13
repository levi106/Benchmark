using BenchmarkDotNet.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyNet472
{
    public class ConcurrencyBenchmark
    {
        private readonly object _obj = new object();
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);
        private int _c = 0;

        [Benchmark]
        public void LockBench() => Run(1000, Lock);
        [Benchmark]
        public void SemaphoreSlimBench() => Run(1000, SemSlim);
        [Benchmark]
        public void SemaphoreSlimAsyncBench() => Run(1000, SemSlimAsync);

        public void Run(int numTasks, Func<int> f)
        {
            var ts = new Task[numTasks];
            for (int i = 0; i < numTasks; i++)
            {
                ts[i] = Task.Run(() => f());
            }
            Task.WaitAll(ts);
        }

        public void Run(int numTasks, Func<Task<int>> f)
        {
            var ts = new Task[numTasks];
            for (int i = 0; i < numTasks; i++)
            {
                ts[i] = Task.Run(async () => await f());
            }
            Task.WaitAll(ts);
        }

        public int Lock()
        {
            int c;
            lock (_obj)
            {
                _c += 1;
                c = _c;
            }
            return c;
        }

        public int SemSlim()
        {
            int c;
            _sem.Wait();
            try
            {
                _c += 1;
                c = _c;
            }
            finally
            {
                _sem.Release();
            }
            return c;
        }

        public async Task<int> SemSlimAsync()
        {
            int c;
            await _sem.WaitAsync();
            try
            {
                _c += 1;
                c = _c;
            }
            finally
            {
                _sem.Release();
            }
            return c;
        }
    }
}
