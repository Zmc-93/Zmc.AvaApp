using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmc.Core.Tasks.Base
{
    public class TaskReturn<T>
    {
        public string Message { get; set; }
        public TaskStatus Status { get; set; }
        public T Result { get; set; }

        public TaskReturn(T result, TaskStatus status = TaskStatus.Success, string message = "")
        {
            Status = status;
            Message = message;
            Result = result;
        }
    }

    public enum TaskStatus
    {
        Success,
        Failure,
        Running,
        Exception,
        Canceled
    }
}
