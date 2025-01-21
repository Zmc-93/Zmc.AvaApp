using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zmc.Core.Threads.Base
{
    interface IThread: IDisposable
    {
        void Start();
        void Stop();
        bool IsRunning { get;  set; }
        bool IsIdle { get; }
        bool IsError { get; }
        Exception LastException { get; }
        void BusyWait(int ms);
    }
}
