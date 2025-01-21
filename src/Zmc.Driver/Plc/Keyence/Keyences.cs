
/*----------------------------------------------------------------
* 项目名称 ：CmxsPDPrint
* 
* 项目描述 ：
* 
* 类 名 称 ：KeyenceHelper
* 
* 类 描 述 ：基恩士PLC工具类
* 
* 命名空间 ：CmxsInkJetBll
* 
* 作    者 ：yangshuai
* 
* 创建时间 ：2024/10/8 11:52:43
* 
* 更新时间 ：2024/10/8 11:52:43
* 
* 版 本 号 ：v1.0.0.0
* 
*******************************************************************
* Copyright @ CMXS 2024. All rights reserved.
*******************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHL = KvHostLink.KvHostLink;
using KHST = KvStruct.KvStruct;


namespace KeyenceTool
{
    public class Keyences : IDisposable
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public int sock = 0;
        public int open(string ip, int port, int timeout, out int err, KvHostLink.KHLSockType stype = KvHostLink.KHLSockType.SOCK_TCP)
        {
            err = 0;
            err = KHL.KHLInit();
            if (err != 0)
            {
                var str = Enum.ToObject(typeof(KHL), err);
                Console.WriteLine("错误提示：" + str);
                return sock;
            }

            err = KHL.KHLConnect(ip, (ushort)port, (uint)timeout, KvHostLink.KHLSockType.SOCK_TCP, ref sock);
            if (err != 0)
            {
                var str = Enum.ToObject(typeof(KHL), err);
                Console.WriteLine(str);
                return sock;
            }
            return sock;
        }


        public bool close(int sock)
        {
            KHL.KHLDisconnect(sock);
            sock = 0;
            return true;
        }


        public byte[] ReadBit(uint devTopNum, uint bitNum)
        {
            byte[] readBuf = new byte[bitNum];
            bool[] rdBool = new bool[8];
            int err = 0; //KHL.KHLReadDevicesAsBits(sock, KvHostLink.KHLDevType.DEV_DM, devTopNum, 0, bitNum, readBuf);
            if (err != 0)
            {
                Console.WriteLine(err);
                return null;
            }
            return readBuf;
        }

        public byte[] ReadWords(uint devTopNum, uint wordNum)
        {
            byte[] readBuf = new byte[wordNum];
            bool[] rdBool = new bool[8];
            int err = 0;//KHL.KHLReadDevicesAsWords(sock, KvHostLink.KHLDevType.DEV_DM, devTopNum, wordNum, readBuf);
            if (err != 0)
            {
                Console.WriteLine(err);
                return null;
            }
            return readBuf;
        }


        public bool WriteBits(byte[] writeBuf, uint devNum, uint length)
        {
            int err = 0;// KHL.KHLWriteDevicesAsBits(sock, KvHostLink.KHLDevType.DEV_DM, devNum, 0, length, writeBuf);
            if (err != 0)
            {
                Console.WriteLine("写入值错误：" + err);
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (sock != 0)
                close(sock);
        }

        //public bool WriteWords(byte[] writeBuf,uint devNum,uint length)
        //{
        //    int err = KHL.KHLWriteDevicesAsWords(sock, KvHostLink.KHLDevType.DEV_DM, devNum, length, writeBuf);
        //    if (err != 0)
        //    {
        //        Console.WriteLine("写入值错误：" + err);
        //        return false;
        //    }
        //    return true;
        //}
    }
}
