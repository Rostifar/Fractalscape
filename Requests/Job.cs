using System.Collections;
using System.Threading;

namespace Fractalscape
{
    //Convenient wrapper for c# threads, as Unity is not thread safe.
    public class Job
    {
        private Thread _thread;
        private bool _done;

        public IEnumerator Start()
        {
            _thread.Start();
            while (_thread.IsAlive)
            {
                yield return null;
            }
        }

        public virtual void Abort()
        {
            _thread.Abort();
        }

        public Job(Thread thread)
        {
            _thread = thread;
        }

        public bool Alive()
        {
            return _thread.IsAlive;
        }
    }
}