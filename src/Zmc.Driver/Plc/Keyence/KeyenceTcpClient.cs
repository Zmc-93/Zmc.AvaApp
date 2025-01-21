using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyenceUdp
{
    public class KeyenceTcpClient : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private TcpClient _udpClient;
        private KeyenceResponse _responses = new KeyenceResponse(null);
        private Thread _readerThread;
        private object _lockObject = new object();
        private bool isDisConnected = true;

        public KeyenceTcpClient(IPEndPoint remoteIpEndPoint)
        {
            _udpClient = new TcpClient();
            _udpClient.Connect(remoteIpEndPoint);
            Timeout = TimeSpan.FromSeconds(2);
        }

        public TimeSpan Timeout { get; set; }


        public bool Connected()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _readerThread = new Thread(ReadWorker);
            _readerThread.Start();
            var cmd = KeyenceDriver.ConnectCommand();
            isDisConnected = false;
            return CmdBool(cmd);
        }

        public bool Close()
        {
            isDisConnected = true;
            bool ret = false;
                ret = DisConnected();
                _cancellationTokenSource?.Cancel();
                _readerThread?.Join();
            return ret;
        }

        public void Dispose()
        {
            Close();
            _udpClient?.Dispose();
        }

        public int ReadData(ushort startAddress, ushort count, out ushort[] ushorts)
        {

            var cmd = KeyenceDriver.ReadDataCommand(startAddress, count);
            return Read(cmd, (ushort)count, out ushorts);
        }

        public int WriteData(ushort startAddress, ushort[] data)
        {
            if (data.Length > 39)//基础命令20 一个short占用6  最大长度256, 得出最大数据长度为39
            {
                var dataList = SplitArray(data, 39);
                int ret = 0;
                for (int i = 0; i < dataList.Count; i++) 
                {
                    var cmd = KeyenceDriver.WriteDataCommand((ushort)(startAddress+i*39), dataList[i]);
                    int retD = Write(cmd);
                    if(retD != 0)
                        ret = retD;
                }
                return ret;
            }
            else 
            {
                var cmd = KeyenceDriver.WriteDataCommand(startAddress, data);
                return Write(cmd);
            }
        }

        static List<ushort[]> SplitArray(ushort[] array, int chunkSize)
        {
            List<ushort[]> result = new List<ushort[]>();

            for (int i = 0; i < array.Length; i += chunkSize)
            {
                // 计算当前块的剩余长度
                int remainingLength = Math.Min(chunkSize, array.Length - i);
                ushort[] chunk = new ushort[remainingLength];

                // 复制数据到当前块
                Array.Copy(array, i, chunk, 0, remainingLength);
                result.Add(chunk);
            }

            return result;
        }
        private ManualResetEvent WaitEvent = new ManualResetEvent(false);

        private bool Send(byte[] cmd)
        {
            _udpClient.GetStream().Write(cmd, 0, cmd.Length);
            return false;
            //return _udpClient.Send(cmd, cmd.Length) != cmd.Length;
        }
        private bool CmdBool(byte[] cmd)
        {
            lock (_lockObject)
            {
                _responses.Reset();
                WaitEvent.Reset();
                if (Send(cmd))
                    throw new Exception();
                if (!WaitEvent.WaitOne(Timeout))
                    throw new TimeoutException();
                return _responses.ErrCode == 0;
            }
        }


        private bool Cmd(byte[] cmd)
        {
            lock (_lockObject)
            {
                _responses.Reset();
                WaitEvent.Reset();
                if (Send(cmd))
                    throw new Exception();
                return _responses.ErrCode == 0;
            }
        }
        private int Read(byte[] cmd, int length, out ushort[] data)
        {
            lock (_lockObject)
            {
                if (isDisConnected)
                {
                    data = null;
                    return 0;
                }
                _responses.Reset();
                WaitEvent.Reset();
                if (Send(cmd))
                    throw new Exception();
                if (!WaitEvent.WaitOne(Timeout))
                    throw new TimeoutException();
                data = _responses.Data;
                if (data.Length != length) 
                {
                    return -9;
                } 
                return _responses.ErrCode;
            }
        }

        private int Write(byte[] cmd)
        {
            lock (_lockObject)
            {
                if (isDisConnected) 
                {
                    return 0;
                }
                _responses.Reset();
                WaitEvent.Reset();
                if (Send(cmd))
                    throw new Exception();
                if (!WaitEvent.WaitOne(Timeout))
                    throw new TimeoutException();
                if (cmd.Length == 0) return -1;
                return _responses.ErrCode;
            }
        }


        private void ReadWorker()
        {
            try
            {
                while (true)
                {
                    //var task = _udpClient.ReceiveAsync();
                    byte[] data = new byte[256];
                    String responseData = String.Empty;
                    var task = _udpClient.GetStream().ReadAsync(data,0,256);

                    task.Wait(_cancellationToken);
                    if (task.IsFaulted)
                        throw new AggregateException(task.Exception);
                    //ProcessResponse(task.Result);
                    ProcessResponse(data);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
            }
        }

        private bool DisConnected()
        {
                var cmd = KeyenceDriver.DisConnecttCommand();
                return Cmd(cmd);
        }

        private void ProcessResponse(byte[] data)
        {
            int errCode = KeyenceDriver.CheckPlcReadResponse(data);
            if (errCode == 0)
            {
                string strResponse = Encoding.ASCII.GetString(data);
                if (HasLetter(strResponse))
                {
                    _responses.PutValue(null, errCode);
                }
                else 
                {
                    string[] splits = strResponse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _responses.PutValue(KeyenceDriver.ToShorts(splits), errCode);
                }
              
            }
            else
            {
                _responses.PutValue(null, errCode);
            }
            WaitEvent?.Set();
        }

        static bool HasLetter(string input)
        {
            foreach (char c in input)
            {
                if (char.IsLetter(c))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
