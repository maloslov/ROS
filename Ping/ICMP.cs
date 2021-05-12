using System;

namespace Ping
{
    class ICMP
    {
        //ДЕКАРАЦИЯ ПЕРЕМЕННЫХ
        public byte Type;
        public byte Code;
        public UInt16 Checksum;
        public int MessageSize;
        public byte[] Message;

        public ICMP()
        {
            //ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
            Type = 8;
            Code = 0;
            Message = new byte[32];
            for (int i = 0; i < Message.Length; i++)
                Message[i] = 1;
            Checksum = getChecksum();
        }

        public ICMP(byte[] data, int size)
        {
            //ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
            Type = data[20];
            Code = data[21];
            Checksum = BitConverter.ToUInt16(data, 22);
            MessageSize = size - 24;
            Message = new byte[MessageSize];
            Buffer.BlockCopy(data, 24, Message, 0, MessageSize);
        }

        public byte[] getBytes()
        {
            //декларация переменных
            byte[] data;
            //инициализация переменных
            data = new byte[MessageSize + 9];
            //ТЕЛО ПРОЦЕДУРЫ
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Checksum), 0, data, 2, 2);
            Buffer.BlockCopy(Message, 0, data, 4, MessageSize);
            return data;
        }

        public UInt16 getChecksum()
        {
            //ДЕКЛАРАЦИЯ ПЕРЕМЕННЫХ
            UInt32 chcksm;
            byte[] data;
            int packetsize;
            int index;
            //ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
            chcksm = 0;
            data = getBytes();
            packetsize = MessageSize + 8;
            index = 0;
            //ТЕЛО ПРОЦЕДУРЫ
            while (index < packetsize)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            return (UInt16)(~chcksm);
        }
    }
}
