using System.Threading;

namespace SoftLock
{
    public class SoftLock
    {
        private volatile int _qtdThreadInSoftLock;
        private volatile int _qtdThreadInHardLock;

        private bool _inBlockCode;
        public bool InBlockCode => _inBlockCode;

        private readonly static object _lockObjHard = new object();
        private readonly static object _lockObjSoft = new object();
        private readonly static object _lockCondition = new object();

        public void EnterSoftLock()
        {
            lock (_lockCondition)
            {
                // Wait Hard Block
                if (Monitor.IsEntered(_lockObjHard))
                {
                    Monitor.Wait(_lockObjHard);
                }
                if (!Monitor.IsEntered(_lockObjSoft))
                {
                    Monitor.Enter(_lockObjSoft);
                }
            }
            Interlocked.Increment(ref _qtdThreadInSoftLock);
        }

        public void EnterHardLock()
        {
            lock (_lockCondition)
            {
                Monitor.Enter(_lockObjHard);
                Monitor.Enter(_lockObjSoft);
                Interlocked.Increment(ref _qtdThreadInHardLock);
                Volatile.Write(ref _inBlockCode, true);
            }
        }

        public void ExitSoftLock()
        {
            Interlocked.Decrement(ref _qtdThreadInSoftLock);
            Monitor.Exit(_lockObjSoft);
        }

        public void ExitHardLock()
        {
            Interlocked.Decrement(ref _qtdThreadInHardLock);
            if (_qtdThreadInHardLock == 0)
            {
                Volatile.Write(ref _inBlockCode, false);
            }
            Monitor.Exit(_lockObjHard);
            Monitor.Exit(_lockObjSoft);
        }
    }
}
