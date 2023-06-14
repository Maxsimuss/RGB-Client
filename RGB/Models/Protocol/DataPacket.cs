using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO

namespace RGB.Models.Protocol
{
    internal class DataPacket : Packet
    {
        public DataPacket(int ledCount) : base(PacketType.Data)
        {
            
        }
    }
}
