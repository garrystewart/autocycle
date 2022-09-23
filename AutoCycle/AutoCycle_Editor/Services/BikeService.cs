using AutoCycle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoCycle.Services
{
    internal static class BikeService
    {
        private readonly static UdpClient _udpClient = new UdpClient(5005);
        private readonly static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

        private static int _bikeLowerLimit = 1;
        private static int _bikeUpperLimit = 16;

        public static void SendResistance(int resistance)
        {
            Send(new Json
            {
                Count = 0,
                Result = resistance,
                Confidence = "100",
                OcrResultText = resistance.ToString()
            });
        }

        private static void Send(Json json)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(json));
            _udpClient.Send(bytes, bytes.Length, _ipEndPoint);
        }

        private static int IsWithinBikeUpperLimit(int resistance)
        {
            return (resistance <= _bikeUpperLimit) ? resistance : _bikeUpperLimit;
        }

        private static int IsWithinBikeLowerLimit(int resistance)
        {
            return (resistance >= _bikeLowerLimit) ? resistance : _bikeLowerLimit;
        }
    }
}
