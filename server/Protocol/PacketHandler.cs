﻿using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Leeward.Protocol.Packets;

namespace Leeward.Protocol
{
    internal static class PacketHandler
    {
        public static List<Packet> Handle(MemoryStream data, uint skipSize = 0)
        {
            List<Packet> packets = new List<Packet>();
            
            try
            {
                using (BinaryReader dataReader = new BinaryReader(data))
                {
                    uint expectedSize = dataReader.ReadUInt32();

                    if (expectedSize == 542393671) // TODO: Add other HTTP common headers
                    {
                        data.Position = 0;
                        packets.Add(new HttpRequest(data));
                    }
                    else
                    {
                        expectedSize += 4; // First int with packet size
                        uint code = dataReader.ReadByte();

                        switch (code)
                        {
                            case (uint) PacketType.RequestID:
                                packets.Add(HandleRequestId(dataReader));
                                break;
                            case (uint) PacketType.RequestSetAlias:
                                packets.Add(HandleRequestSetAlias(dataReader));
                                break;
                            case (uint) PacketType.RequestJoinZone:
                                packets.Add(HandleRequestJoinChannel(dataReader));
                                break;
                            default: 
                                Console.WriteLine("BIN   ==> " + BitConverter.ToString(data.ToArray().Skip((int)skipSize).Take((int)expectedSize).ToArray()));
                                Console.WriteLine("ASCII ==> " + Encoding.ASCII.GetString(data.ToArray().Skip((int)skipSize).Take((int)expectedSize).ToArray()));
                                Console.WriteLine($"Data length: {data.Length - skipSize}");
                                Console.WriteLine($"Expected size: {expectedSize}");
                                throw new UnrecognizedPacketException(code, data.Length - skipSize);
                        }
                        
                        if ((data.Length - skipSize) > expectedSize)
                        {
                            packets.AddRange(Handle(data, expectedSize + skipSize));
                        }
                    }
                }
            }
            catch (ArgumentException er)
            {
                Console.WriteLine("The packet can't be readed: " + data.CanRead); // TODO: DEBUG ONLY, DELETE
            }

            return packets;
        }

        public static RequestIdPacket HandleRequestId(BinaryReader dr)
        {
            if (dr.ReadInt32() != Constants.ProtocolVersion)
                throw new VersionNotFoundException("Protocol version not compatible.");
            
            return new RequestIdPacket(
                dr.ReadString()
            );
        }

        public static RequestSetAliasPacket HandleRequestSetAlias(BinaryReader dr)
        {
            return new RequestSetAliasPacket(
                dr.ReadString()
            );
        }

        public static RequestJoinZonePacket HandleRequestJoinChannel(BinaryReader dr)
        {
            return new RequestJoinZonePacket(
                id:         dr.ReadInt32(),
                password:   dr.ReadString(),
                name:       dr.ReadString(),
                persistent: dr.ReadBoolean(),
                maxPlayers: dr.ReadUInt16()
            );
        }
    }
}