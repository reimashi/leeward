﻿using System.IO;

namespace Leeward.Protocol.Packets
{
    internal class HttpRequest : Packet
    {
        public HttpRequest(MemoryStream data) : base(PacketType.HttpRequest)
        {
        }

        public override string ToString()
        {
            return "Generic HTTP Request"; 
        }
    }
}