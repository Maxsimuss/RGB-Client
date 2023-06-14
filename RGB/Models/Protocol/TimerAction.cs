using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Protocol
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TimerAction
    {
        public byte id;
        public byte activated;
        public ulong nextExecution;
        public float r, g, b, w;
    }
}
