using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KeyenceUdp
{
    internal static class KeyenceDriver
    {
        private const string MemoryAreaData = "DM";
 
        public static byte[] ReadDataCommand(ushort startAddress, ushort readCount)
            => ReadCommand(MemoryAreaData, startAddress, readCount);
        public static byte[] WriteDataCommand(ushort startAddress, ushort[] data)
            => WriteCommand(MemoryAreaData,  startAddress, data);
        
      

        public static byte[] ConnectCommand()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append("CR");                                // 连接命令
            cmd.Append("\r");                                //结束符
            byte[] _PLCCommand = Encoding.ASCII.GetBytes(cmd.ToString());
            return _PLCCommand;
        }

        public static byte[] DisConnecttCommand()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append("CQ");                                // 结束连接命令
            cmd.Append("\r");                                //结束符
            byte[] _PLCCommand = Encoding.ASCII.GetBytes(cmd.ToString());
            return _PLCCommand;
        }

        private static byte[] ReadCommand(string memoryArea,ushort startAddress, ushort readCount)
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append("RDS");                               // 批量读取
            cmd.Append(" ");                                 // 空格符
            cmd.Append(memoryArea);              // 软元件类型，如DM
            cmd.Append(startAddress);  // 软元件的地址，如1000
            cmd.Append(" ");                                 // 空格符
            cmd.Append(readCount);
            cmd.Append("\r");                                //结束符

            byte[] _PLCCommand = Encoding.ASCII.GetBytes(cmd.ToString());
            return _PLCCommand;
        }

        private static byte[] WriteCommand(string memoryArea, ushort startAddress, ushort[] data)
        {
            var value = ToBytes(data);
            StringBuilder cmd = new StringBuilder();
            cmd.Append("WRS");                         // 批量读取
            cmd.Append(" ");                           // 空格符
            cmd.Append(memoryArea);                    // 软元件类型，如DM
            cmd.Append(startAddress);                  // 地址
            cmd.Append(".U");                          // 数据格式
            cmd.Append(" ");                           // 空格符
            int length = value.Length / 2;
            cmd.Append(length.ToString());
            for (int i = 0; i < length; i++)
            {
                cmd.Append(" ");
                cmd.Append(BitConverter.ToUInt16(value, i * 2));
            }
            cmd.Append("\r");
            if (cmd.Length > 256) 
            {
                return new byte[0];
            }

            return (Encoding.ASCII.GetBytes(cmd.ToString()));
        }

        /// <summary>
		/// 获取当前的地址类型是字数据的倍数关系
		/// </summary>
		/// <param name="type">地址的类型</param>
		/// <returns>倍数关系</returns>
		public static int GetWordAddressMultiple(string type)
        {
            if (type == "CTH" || type == "CTC" || type == "C" || type == "T" || type == "TS" || type == "TC" || type == "CS" || type == "CC" || type == "AT")
                return 2;
            else if (type == "DM" || type == "CM" || type == "TM" || type == "EM" || type == "FM" || type == "Z" || type == "W" || type == "ZF" || type == "VM")
                return 1;
            return 1;
        }

        /// <summary>
		/// 校验读取返回数据状态，主要返回的第一个字节是不是E<br />
		/// Check the status of the data returned from reading, whether the first byte returned is E
		/// </summary>
		/// <param name="ack">反馈信息</param>
		/// <returns>是否成功的信息</returns>
		public static int CheckPlcReadResponse(byte[] ack)
        {
            if (ack.Length == 0)
            {
                return -2;
            }
            if (ack[0] == 0x45) 
            {
                string err = GetErrorText(Encoding.ASCII.GetString(ack));

                return -1;
            }
            //if ((ack[ack.Length - 1] != 0x0A) && (ack[ack.Length - 2] != 0x0D))
            //{
            //    //return OperateResult(StringResources.Language.MelsecFxAckWrong + " Actual: " + SoftBasic.ByteToHexString(ack, ' '));
            //    return -3;
            //} 
            return 0;
        }

        private static string GetErrorText(string err)
        {
            if (err.StartsWith("E0"))
                return  "1. 指定的软元件编号、存储体编号、单元编号、地址超出范围。\n" +
                        "2. 指定了程序不用的定时器、计数器、CTH 和 CTC 的编号。\n" +
                        "3. 未登录监控器，却要进行监控器读取。";
            if (err.StartsWith("E1"))
                return  "1. 发送了CPU单元不支持的指令。\n" +
                        "2. 指定指令的方法出错。\n" +
                        "3. 确立通讯前，发送了 CR 以外的指令。";
            if (err.StartsWith("E2"))
                return  "1. 在 CPU 单元没有存储程序的状态下， 发送了“M1（切换到 RUN 模式）”指令。\n" +
                        "2. 在 CPU 单元的 RUN/PROG 开关处于PROG 状态下，发送了“M1（切换到RUN 模式）”指令。";
            if (err.StartsWith("E4"))
                return "想要更改写入去能程序的定时器、计数器和 CTC 的设定值。";
            if (err.StartsWith("E5"))
                return "在尚未排除CPU单元错误的情况下， 发送了“M1( 切换到RUN模式)”指令。";
            if (err.StartsWith("E6"))
                return "读取“RDC”指令选定的软元件中。";
            return "";
        }

        public static ushort[] ToShorts(string[] data)
        {
            try 
            {
                ushort[] r = new ushort[data.Length];
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] = ushort.Parse(data[i]); ;
                }
                return r;
            }
            catch { return new ushort[1]; }

        }
      

        private static byte[] ToBytes(ushort[] shorts)
        {
            byte[] bytes = new byte[shorts.Length * 2]; // 每个 ushort 占 2 个字节
            Buffer.BlockCopy(shorts, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] RemoveLast2(byte[] value)
        {
            if (value == null || value.Length <= 2) return new byte[0];
            byte[] buffer = new byte[value.Length - 2];
            Array.Copy(value, buffer, buffer.Length);
            return buffer;
        }
    }

}