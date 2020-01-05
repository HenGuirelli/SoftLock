using System;
using System.Threading;

namespace SoftLock
{
    public class SoftLock
    {
        private volatile int _qtdThreadInSoftLock;
        private volatile int _qtdThreadInHardLock;

        private bool _inBlockCode;
        public bool InBlockCode => _inBlockCode;

        private readonly static object _lockObj = new object();

        public void EnterSoftLock()
        {
            lock (_lockObj)
            {
                if (!_inBlockCode)
                {
                    return;
                }
            }
            SpinWait.SpinUntil(() => _inBlockCode);
            Interlocked.Increment(ref _qtdThreadInSoftLock);
        }

        public void ExitSoftLock()
        {
            Interlocked.Decrement(ref _qtdThreadInSoftLock);
        }

        public void EnterHardLock()
        {
            SpinWait.SpinUntil(() =>
            {
                try
                {
                    Monitor.Enter(_lockObj);
                    return _qtdThreadInSoftLock > 0 || _qtdThreadInHardLock > 0;
                }
                finally
                {
                    if (_qtdThreadInSoftLock > 0 || _qtdThreadInHardLock > 0)
                    {
                        Monitor.Exit(_lockObj);
                    }
                }
            });


            if (_qtdThreadInSoftLock <= 0 && _qtdThreadInHardLock <= 0)
            {
                Interlocked.Increment(ref _qtdThreadInHardLock);
                Volatile.Write(ref _inBlockCode, true);
                return;
            }
        }

        public void ExitHardLock()
        {
            lock (_lockObj)
            {
                Interlocked.Decrement(ref _qtdThreadInHardLock);
                if (_qtdThreadInHardLock == 0)
                {
                    Volatile.Write(ref _inBlockCode, false);
                }
            }
        }
    }
}
