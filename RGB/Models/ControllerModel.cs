using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using SimpleTCP;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using CSCore.DSP;
using RGB.Util;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using RGB.Models.Protocol;

namespace RGB.Models
{
    [INotifyPropertyChanged]
    public partial class ControllerModel : IDisposable
    {
        private bool running = false;

        private HashSet<RGBClient> clients = new HashSet<RGBClient>();

        public int ClientCount { get { return clients.Count; } }

        public void RemoveClient(RGBClient client)
        {
            clients.Remove(client);
            OnPropertyChanged(nameof(clients));
        }

        public void AnnounceTimer(bool active, ulong nextExec, float r, float g, float b, float w)
        {
            int sizeofTimer = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TimerAction));
            byte[] timerPacket = new byte[1 + sizeofTimer];
            timerPacket[0] = (byte)PacketType.AddAction;

            TimerAction action = new TimerAction();
            action.id = 0;
            action.activated = (byte)(active ? 1 : 0);
            action.r = r;
            action.g = g;
            action.b = b;
            action.w = w;
            action.nextExecution = (ulong)nextExec;
            //action.nextExecution = (ulong)(((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds() + 10000);

            Array.Copy(Utils.GetBytes(action), 0, timerPacket, 1, sizeofTimer);
            //client.Write(new byte[] { (byte)PacketType.RemoveAction, 0 });
            
            foreach (var item in clients)
            {
                item.SendPacket(timerPacket);
            }
        }

        private AbstractEffectModel effect;
        public AbstractEffectModel CurrentEffect
        {
            get => effect;
            set
            {
                effect?.End();
                effect = value;
                effect.Begin();

                foreach (var item in clients)
                {
                    item.effect = value;
                }
            }
        }

        public ControllerModel(AbstractEffectModel CurrentEffect)
        {
            this.CurrentEffect = CurrentEffect;
        }

        public void RefreshAll()
        {
            foreach (var item in clients)
            {
                item.Refresh();
            }
        }

        private List<IPAddress> GetEndpoints()
        {
            List<IPAddress> AddressList = new List<IPAddress>();
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface I in Interfaces)
            {
                if ((I.NetworkInterfaceType == NetworkInterfaceType.Ethernet || I.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && I.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var Unicast in I.GetIPProperties().UnicastAddresses)
                    {
                        if (Unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            AddressList.Add(Unicast.Address);
                        }
                    }
                }
            }
            return AddressList;
        }

        public void Connect()
        {
            RefreshAll();

            if (running) return;
            running = true;
            Task receiveTask = Task.Run(() =>
            {
                UdpClient udpReceive = new UdpClient(8082);
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < 5000)
                {
                    IPEndPoint addr = new IPEndPoint(IPAddress.Broadcast, 8082);
                    while (Encoding.ASCII.GetString(udpReceive.Receive(ref addr)) != "RGB STRIP RESPONCE") { }

                    RGBClient client = new RGBClient(this, CurrentEffect, addr);
                    if (!clients.Contains(client))
                    {
                        clients.Add(client);
                        client.Start();
                        OnPropertyChanged(nameof(clients));
                    }
                }

                udpReceive.Close();
            });

            bool announce = true;
            Task.Run(() =>
            {
                while (announce)
                {
                    UdpClient udpSend = new UdpClient(8081);
                    udpSend.EnableBroadcast = true;
                    udpSend.Send(Encoding.ASCII.GetBytes("RGB STRIP BROADCAST"), new IPEndPoint(IPAddress.Broadcast, 8081));
                    udpSend.Close();
                    foreach (var item in GetEndpoints())
                    {
                        udpSend = new UdpClient(new IPEndPoint(item, 8081));
                        udpSend.EnableBroadcast = true;
                        udpSend.Send(Encoding.ASCII.GetBytes("RGB STRIP BROADCAST"), new IPEndPoint(IPAddress.Broadcast, 8081));
                        udpSend.Close();
                    }

                    Task.Delay(1000).Wait();
                }
            });

            receiveTask.Wait();
            announce = false;
            running = false;
        }

        public void Dispose()
        {
            foreach (var item in clients)
            {
                item.Stop();
            }

            clients.Clear();
            effect?.End();
        }
    }
}
