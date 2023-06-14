using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Protocol
{
    internal class Packet
    {
        PacketType packetType;

        public Packet(PacketType packetType)
        {
            this.packetType = packetType;
        }
    }
}
