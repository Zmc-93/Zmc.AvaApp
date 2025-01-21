namespace Zmc.Core.Threads.Base
{
    public abstract class ThreadBase : IThread
    {
        private Thread _thread{ get;set; }
        protected ThreadBase()
        {

        }
        public bool IsRunning { get; set; } = false;

        public bool IsIdle { get; set; } = false;

        public bool IsError { get; set; } = false;

        public Exception LastException { get; set; } = null;

        public void BusyWait(int ms)
        {
            
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(Run))
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public void Stop()
        {
            _thread?.Join();
        }


        public void Run() 
        {
            
        }
    }
}
