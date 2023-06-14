using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Protocol
{
    internal enum PacketType : byte
    {
        Info = 0x1,
        Data = 0x2,
        Keepalive = 0x3,
        Timestamp = 0x4,
        AddAction = 0x5,
        RemoveAction = 0x6
    }
}
