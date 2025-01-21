using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zmc.Core.Tasks.Base
{
    /// <summary>
    /// 任务基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TasksBase<T> : IDisposable
    {
        #region 私有变量
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task<TaskReturn<T>> _task { get;set; }
        #endregion

        #region 公共属性
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; } = -1;
        /// <summary>
        /// 任务状态
        /// </summary>
        public bool IsRunning { get; set; } = false;
        /// <summary>
        /// 是否显示耗时
        /// </summary>
        public bool ShowTimeConsuming { get; set; } = false;
        #endregion

        #region 回调方法
        /// <summary>
        /// 任务工作
        /// </summary>
        public Func<object, TaskReturn<T>> WorkAction { get; set; }
        /// <summary>
        /// 开始前回调
        /// </summary>
        public Action BeforStartAction { get; set; } = null;
        /// <summary>
        /// 结束回调
        /// </summary>
        public Action<T> EndAction { get; set; } = null;
        /// <summary>
        /// 失败回调
        /// </summary>
        public Action FailureAction { get; set; } = null;
        /// <summary>
        /// 异常回调
        /// </summary>
        public Action<Exception> ErrorAction { get; set; } = null;
        /// <summary>
        /// 取消回调
        /// </summary>
        public Action CancelAction { get; set; } = null;
        #endregion

        #region 共有方法
        public void Dispose()
        {
            Cancell();
        }

        public Task<TaskReturn<T>> Run(object obj = null)
        {
            _task?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            return AsyncMethod(obj, _cancellationToken);
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public void Cancell()
        {
            _cancellationTokenSource?.Cancel();
        }
 
        /// <summary>
        /// 等待任务完成
        /// </summary>
        public void Wait()
        {
            _task.Wait();
        }

        /// <summary>
        /// 等待指定时间任务完成为True，超时为False
        /// </summary>
        /// <param name="ms">毫秒</param>
        /// <returns></returns>
        public bool Wait(int ms)
        {
            return _task.Wait(ms);
        }
        #endregion

        #region 私有方法
        private async Task<TaskReturn<T>> AsyncMethod(object obj, CancellationToken token)
        {
            if (ShowTimeConsuming)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //LogHelper.Info($"[任务名称:{Name}][任务ID:{Id}]:任务开始------------------");
            }
            BeforStartAction?.Invoke();
            _task = Task.Factory.StartNew(() => Work(obj), token);
            TaskReturn<T> ret = null;
            Id = _task.Id;
            try
            {
                //等待任务完成
                await _task;
            }
            catch (OperationCanceledException)
            {
                //被取消
                CancelAction?.Invoke();
                string msg = $"[任务名称:{Name}][任务ID:{Id}]:任务被取消";
                ret = new TaskReturn<T>(default, TaskStatus.Canceled, msg);
                //LogHelper.Info(msg);
            }
            catch (Exception ex)
            {
                //异常
                ErrorAction?.Invoke(ex);
                string msg = $"[任务名称:{Name}][任务ID:{Id}]:任务异常:{ex.Message}";
                ret = new TaskReturn<T>(default, TaskStatus.Exception, msg);
                //LogHelper.Error(msg);
            }
            finally 
            {
                if (_task.Result.Status == TaskStatus.Failure) 
                {
                    FailureAction?.Invoke();
                }
                if (ShowTimeConsuming)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    sw.Stop();
                    //LogHelper.Info($"[任务名称:{Name}][任务ID:{Id}]:任务耗时:{sw.ElapsedMilliseconds}ms-------------------");
                }
            }
            return _task.Result;
        }

        private TaskReturn<T> Work(object obj)
        {
            var ret = WorkAction?.Invoke(obj);
            if (ret != null)
            {
                //结束
                EndAction?.Invoke(ret.Result);
            }
            return ret;
        }
        #endregion
    }
}
