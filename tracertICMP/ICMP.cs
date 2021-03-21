using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracertICMP
{
    public class ICMP
    {
        public byte Type { get; protected set; } = 0x08;
        public byte Code { get; protected set; } = 0x00;
        public int MessageSize { get; protected set; }
        public int PacketSize { get; protected set; }

        public byte[] Message = new byte[1024];
        private UInt16 SequenceNumber = 107;
        private byte[] Data;

        public UInt16 Checksum = 0;

        public ICMP(byte[] data)
        {
            this.Data = data;
            MessageSize = data.Length + 4;
            PacketSize = MessageSize + 4;
            Message = createPacket();
        }

        public UInt16 UpdateSequence()
        {
            SequenceNumber++;
            Message = createPacket();

            return SequenceNumber;
        }

        private byte[] createPacket()
        {
            byte[] Packet = new byte[MessageSize + 9];

            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, Packet, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, Packet, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, Packet, 4, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(SequenceNumber), 0, Packet, 7, 2);
            Buffer.BlockCopy(Data, 0, Packet, 8, Data.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(getChecksum(Packet)), 0, Packet, 2, 2);

             return Packet;
        }

        private UInt16 getChecksum(byte[] bytes)
        {
            UInt32 chcksm = 0;
            int packetsize = MessageSize + 8;

            for (int index = 0; index < packetsize; index += 2)
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(bytes, index));

            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);

            return (UInt16)(~chcksm);
        }
    }
}
