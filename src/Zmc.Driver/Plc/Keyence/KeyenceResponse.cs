using System;
using System.Threading;

namespace KeyenceUdp
{
    internal struct KeyenceResponse
    {
        public KeyenceResponse(ushort[] data)
        {
            Data = data;
            ErrCode = 0;
        }

        public ushort[] Data { get; private set; }
        public int ErrCode { get; private set; }
        
        public void Reset()
        {
            Data = null;
            ErrCode = 0;
        }
        
        public void PutValue( ushort[] data,int errCode)
        {
            Data = data;
            ErrCode = errCode;
        }
    }
}