using RGB.Util;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RGB.Models.Protocol;

namespace RGB.Models
{
    internal enum ConnectionState
    {
        Connected,
        Connecting,
        Disconnected,
    }

    public class RGBClient
    {
        private static readonly byte[] KEEPALIVE_PACKET = new byte[] { 0x03 };

        public readonly IPEndPoint Address;
        private readonly SimpleTcpClient client;
        private bool running = false;
        private bool hasWChannel = false;
        private bool hasDither = false;
        private bool is16Bit = false;
        private int LedCount = 0;

        private ConnectionState state = ConnectionState.Disconnected;

        private LedColor correction = new LedColor(1, 1, 1, 1);
        private LedColor[] colors, colorsPrev;
        private byte[] colorDataPacket;

        public event OnConnected Connected;
        public event OnConnectFailed ConnectFailed;

        public delegate void OnConnected();
        public delegate void OnConnectFailed();

        private ControllerModel controller;
        public AbstractEffectModel effect;
        private Stopwatch stopwatch = new Stopwatch();

        private Thread thread;
        private bool mustRefresh = false;

        public override bool Equals(object obj)
        {
            if (obj is not RGBClient) return false;

            return Address.Address.Equals(((RGBClient)obj).Address.Address);
        }

        public override int GetHashCode()
        {
            return Address.Address.GetHashCode();
        }

        public RGBClient(ControllerModel controller, AbstractEffectModel effectModel, IPEndPoint address)
        {
            this.controller = controller;
            effect = effectModel;
            Address = address;
            client = new SimpleTcpClient();
        }

        public void Start()
        {
            client.DataReceived += (s, e) =>
            {
                switch ((PacketType)e.Data[0])
                {
                    case PacketType.Info:
                        {
                            LedCount = e.Data[1];
                            is16Bit = (e.Data[2] & 0x1) != 0;
                            hasWChannel = (e.Data[2] & 0x2) != 0;
                            hasDither = (e.Data[2] & 0x4) != 0;

                            correction.R = BitConverter.ToSingle(e.Data, 3);
                            correction.G = BitConverter.ToSingle(e.Data, 7);
                            correction.B = BitConverter.ToSingle(e.Data, 11);
                            if (hasWChannel) correction.W = BitConverter.ToSingle(e.Data, 15);

                            colorDataPacket = new byte[(LedCount * (hasWChannel ? 4 : 3) + (hasDither ? 1 : 0)) * (is16Bit ? 2 : 1) + 1];

                            colorDataPacket[0] = (byte)PacketType.Data;
                            client.TcpClient.SendBufferSize = colorDataPacket.Length;
                            colors = new LedColor[LedCount];
                            colorsPrev = new LedColor[LedCount];
                            state = ConnectionState.Connected;
                            mustRefresh = true;

                            byte[] timestampPacket = new byte[9];
                            timestampPacket[0] = (byte)PacketType.Timestamp;
                            Array.Copy(BitConverter.GetBytes(((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()), 0, timestampPacket, 1, 8);

                            client.Write(timestampPacket);

                            break;
                        }
                }
                Connected?.Invoke();

            };

            running = true;

            CreateThread();

            thread.Start();
        }

        private void CreateThread()
        {
            thread = new Thread(() =>
            {
                if (state == ConnectionState.Disconnected)
                {
                    Connect();
                }

                stopwatch.Start();

                while (running)
                {
                    if (state == ConnectionState.Disconnected)
                    {
                        if (!Connect()) Dispose();
                    }

                    if (state != ConnectionState.Connected)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (stopwatch.ElapsedMilliseconds > 2000)
                    {
                        SendPacket(KEEPALIVE_PACKET);
                        stopwatch.Restart();
                    }

                    if (stopwatch.ElapsedMilliseconds > 10)
                    {
                        int channelCount = (hasWChannel ? 4 : 3);

                        ushort[] ushorts = new ushort[LedCount * channelCount];

                        effect.GetColors(colors, hasWChannel);

                        if (!colors.SequenceEqual(colorsPrev) || mustRefresh)
                        {
                            mustRefresh = false;
                            Array.Copy(colors, colorsPrev, colors.Length);
                            for (int i = 0; i < LedCount; i++)
                            {
                                //colors[i].R = (float)Math.Pow(colors[i].R, 1.5);
                                //colors[i].G = (float)Math.Pow(colors[i].G, 1.5);
                                //colors[i].B = (float)Math.Pow(colors[i].B, 1.5);
                                //colors[i].W = (float)Math.Pow(colors[i].W, 1.5);

                                colors[i] *= correction;
                            }

                            if (is16Bit)
                            {
                                if (hasWChannel)
                                {
                                    for (int i = 0; i < LedCount; i++)
                                    {
                                        ushorts[i * 4] = (ushort)(Math.Clamp(colors[i].R, 0, 1) * ushort.MaxValue);
                                        ushorts[i * 4 + 1] = (ushort)(Math.Clamp(colors[i].G, 0, 1) * ushort.MaxValue);
                                        ushorts[i * 4 + 2] = (ushort)(Math.Clamp(colors[i].B, 0, 1) * ushort.MaxValue);
                                        ushorts[i * 4 + 3] = (ushort)(Math.Clamp(colors[i].W, 0, 1) * ushort.MaxValue);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < LedCount; i++)
                                    {
                                        ushorts[i * 3] = (ushort)(Math.Clamp(colors[i].R, 0, 1) * ushort.MaxValue);
                                        ushorts[i * 3 + 1] = (ushort)(Math.Clamp(colors[i].G, 0, 1) * ushort.MaxValue);
                                        ushorts[i * 3 + 2] = (ushort)(Math.Clamp(colors[i].B, 0, 1) * ushort.MaxValue);
                                    }
                                }
                                Buffer.BlockCopy(ushorts, 0, colorDataPacket, 1, ushorts.Length * 2);
                            }
                            else
                            {
                                float max = float.Epsilon;
                                for (int i = 0; i < LedCount; i++)
                                {
                                    max = Math.Max(colors[i].R, max);
                                    max = Math.Max(colors[i].G, max);
                                    max = Math.Max(colors[i].B, max);
                                }
                                max *= 4;
                                max = Math.Clamp(max, 0, 1);
                                max = 1;
                                for (int i = 0; i < LedCount; i++)
                                {
                                    colorDataPacket[i * 3 + 1] = (byte)Math.Round(colors[i].R / max * 255);
                                    colorDataPacket[i * 3 + 2] = (byte)Math.Round(colors[i].G / max * 255);
                                    colorDataPacket[i * 3 + 3] = (byte)Math.Round(colors[i].B / max * 255);
                                }
                                max = 255;
                                colorDataPacket[LedCount * 3 + 1] = (byte)Math.Clamp(max * 255, 0, 255);
                            }

                            SendPacket(colorDataPacket);
                        }


                        stopwatch.Restart();
                    }
                    Thread.Sleep(1);
                }

                client.Disconnect();
                client.Dispose();
            });
        }

        public void SendPacket(byte[] packet)
        {
            try
            {
                client.Write(packet);
            }
            catch
            {
                if (!Connect()) Dispose();
            }
        }

        public bool Connect()
        {
            state = ConnectionState.Connecting;
            bool success = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    client.Connect(Address.Address.ToString(), 8080);
                    success = true;
                    break;
                }
                catch (SocketException) { }
            }

            if (!success)
            {
                state = ConnectionState.Disconnected;
                return false;
            }

            client.TcpClient.NoDelay = true;
            client.Write(new byte[] { 0x01 });

            return true;
        }

        public void Stop()
        {
            running = false;
        }

        public void Dispose()
        {
            running = false;
            controller.RemoveClient(this);
        }

        public void Refresh()
        {
            if (thread != null && !thread.IsAlive)
            {
                CreateThread();
                thread.Start();
            }
        }
    }
}
