﻿using Newtonsoft.Json;
using Obsidian.Logging;
using Obsidian.Net;
using Obsidian.Net.Packets;
using Obsidian.Net.Packets.Play;
using Obsidian.Serializer;
using Obsidian.Util.DataTypes;
using Obsidian.Util.Extensions;
using SharpCompress.Compressors.Deflate;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Obsidian.Util
{
    public class PacketHandler
    {
        private static readonly AsyncLogger Logger = new AsyncLogger("Packets", LogLevel.Debug, "packets.log");

        public static ProtocolVersion Protocol = ProtocolVersion.v1_15_2;

        public const float MaxDiggingRadius = 6;

        public static async Task<Packet> ReadPacketAsync(MinecraftStream stream)
        {
            int length = await stream.ReadVarIntAsync();
            byte[] receivedData = new byte[length];

            await stream.ReadAsync(receivedData, 0, length);

            int packetId = 0;
            byte[] packetData = Array.Empty<byte>();

            using (var packetStream = new MinecraftStream(receivedData))
            {
                try
                {
                    packetId = await packetStream.ReadVarIntAsync();
                    int arlen = 0;

                    if (length - packetId.GetVarIntLength() > -1)
                        arlen = length - packetId.GetVarIntLength();

                    packetData = new byte[arlen];
                    await packetStream.ReadAsync(packetData, 0, packetData.Length);
                }
                catch
                {
                    throw;
                }
            }

            return new Packet(packetId, packetData);
        }

        public static async Task<Packet> ReadCompressedPacketAsync(MinecraftStream stream)
        {
            var packetLength = await stream.ReadVarIntAsync();
            var dataLength = await stream.ReadVarIntAsync();

            using var deStream = new MinecraftStream(new ZlibStream(stream, SharpCompress.Compressors.CompressionMode.Decompress, CompressionLevel.BestSpeed));

            var packetId = await deStream.ReadVarIntAsync();
            var packetData = await deStream.ReadUInt8ArrayAsync(dataLength - packetId.GetVarIntLength());

            return new Packet(packetId, packetData);
        }


        public static async Task HandlePlayPackets(Packet packet, Client client)
        {
            Server server = client.Server;
            switch (packet.id)
            {
                case 0x00:
                    // Teleport Confirm
                    // GET X Y Z FROM PACKET TODO
                    //this.Player.Position = new Position((int)x, (int)y, (int)z);
                    //await Logger.LogDebugAsync("Received teleport confirm");
                    break;

                case 0x01:
                    // Query Block NBT
                    await Logger.LogDebugAsync("Received query block nbt");
                    break;

                case 0x02://Set difficulty

                    break;

                case 0x03:
                    // Incoming chat message
                    await Logger.LogDebugAsync("Received chat message");
                    var message = await PacketSerializer.FastDeserializeAsync<IncomingChatMessage>(packet.data);

                    await server.ParseMessage(message.Message, client);
                    break;

                case 0x04:
                    // Client status
                    break;
                case 0x05:
                    // Client Settings
                    client.ClientSettings = await PacketSerializer.FastDeserializeAsync<ClientSettings>(packet.data);
                    await Logger.LogDebugAsync("Received client settings");
                    break;

                case 0x06:
                    // Tab-Complete
                    await Logger.LogDebugAsync("Received tab-complete");
                    break;

                case 0x07:
                    // Window Confirmation (serverbound)
                    await Logger.LogDebugAsync("Received confirm transaction");
                    break;

                case 0x08:
                    // Click Window Button

                    break;

                case 0x09:// Click Window
                    await Logger.LogDebugAsync("Received click window");
                    var window = PacketSerializer.FastDeserialize<ClickWindow>(packet.data);

                    await Logger.LogDebugAsync("Received click window");
                    break;

                case 0x0A:
                    // Close Window (serverbound)
                    await Logger.LogDebugAsync("Received close window");
                    break;

                case 0x0B:
                    // Plugin Message (serverbound)
                    var msg = await PacketSerializer.DeserializeAsync<PluginMessage>(packet.data);

                    await Logger.LogDebugAsync($"Received plugin message: {msg.Channel}");
                    break;

                case 0x0C:
                    // Edit Book
                    await Logger.LogDebugAsync("Received edit book");
                    break;

                case 0x0E:
                    //Interact Entity
                    await Logger.LogDebugAsync("Interact entity");
                    break;

                case 0x0F:
                    // Keep Alive (serverbound)
                    var keepalive = PacketSerializer.FastDeserialize<KeepAlive>(packet.data);
                    await Logger.LogDebugAsync($"Successfully kept alive player {client.Player.Username} with ka id " +
                        $"{keepalive.KeepAliveId} previously missed {client.missedKeepalives - 1} ka's"); // missed is 1 more bc we just handled one

                    // Server is alive, reset missed keepalives.
                    client.missedKeepalives = 0;
                    break;

                case 0x10:
                    //Lock difficulty
                    break;

                case 0x11:// Player Position
                    var pos = PacketSerializer.FastDeserialize<PlayerPosition>(new MinecraftStream(packet.data));

                    await client.Player.UpdateAsync(pos.Position, pos.OnGround);
                    break;

                case 0x12:
                    //Player Position And rotation (serverbound)
                    var ppos = PacketSerializer.FastDeserialize<ServerPlayerPositionLook>(new MinecraftStream(packet.data));

                    await client.Player.UpdateAsync(ppos.Position, ppos.Yaw, ppos.Pitch, ppos.OnGround);
                    break;

                case 0x13:
                    // Player rotation
                    var look = PacketSerializer.FastDeserialize<PlayerRotation>(packet.data);

                    await client.Player.UpdateAsync(look.Yaw, look.Pitch, look.OnGround);
                    break;

                case 0x14://Player movement
                    break;

                case 0x15:
                    // Vehicle Move (serverbound)
                    await Logger.LogDebugAsync("Received vehicle move");
                    break;

                case 0x16:
                    // Steer Boat
                    await Logger.LogDebugAsync("Received steer boat");
                    break;

                case 0x17:
                    // Pick Item
                    await Logger.LogDebugAsync("Received pick item");
                    break;

                case 0x18:
                    // Craft Recipe Request
                    await Logger.LogDebugAsync("Received craft recipe request");
                    break;

                case 0x19:
                    // Player Abilities (serverbound)
                    await Logger.LogDebugAsync("Received player abilities");
                    break;

                case 0x1A:
                    // Player Digging
                    await Logger.LogDebugAsync("Received player digging");

                    var digging = await PacketSerializer.FastDeserializeAsync<PlayerDigging>(packet.data);

                    server.EnqueueDigging(digging);
                    break;

                case 0x1B:
                    // Entity Action
                    await Logger.LogDebugAsync("Received entity action");
                    break;

                case 0x1C:
                    // Steer Vehicle
                    await Logger.LogDebugAsync("Received steer vehicle");
                    break;

                case 0x1D:
                    // Recipe Book Data
                    await Logger.LogDebugAsync("Received recipe book data");
                    break;

                case 0x1E:
                    // Name Item
                    await Logger.LogDebugAsync("Received name item");
                    break;

                case 0x1F:
                    // Resource Pack Status
                    await Logger.LogDebugAsync("Received resource pack status");
                    break;

                case 0x20:
                    // Advancement Tab
                    await Logger.LogDebugAsync("Received advancement tab");
                    break;

                case 0x21:
                    // Select Trade
                    await Logger.LogDebugAsync("Received select trade");
                    break;

                case 0x22:
                    // Set Beacon Effect
                    await Logger.LogDebugAsync("Received set beacon effect");
                    break;

                case 0x23:
                    // Held Item Change (serverbound)//TODO fix this
                    //var hic = await CreateAsync(new HeldItemChange(packet.PacketData));
                    //client.Player.HeldItemSlot = hic.Slot;


                    //await Logger.LogDebugAsync($"Received held item change: {hic.Slot}");

                    break;

                case 0x24:
                    // Update Command Block
                    await Logger.LogDebugAsync("Received update command block");
                    break;

                case 0x25:
                    // Update Command Block Minecart
                    await Logger.LogDebugAsync("Received update command block minecart");
                    break;

                case 0x26:
                    // Creative Inventory Action
                    await Logger.LogDebugAsync("Received creative inventory action");
                    var ca = await PacketSerializer.DeserializeAsync<CreativeInventoryAction>(packet.data);

                    var json = JsonConvert.SerializeObject(ca.ClickedItem);

                    client.Player.HeldItemSlot = ca.ClickedSlot;

                    var dir = Path.Combine(Path.GetTempPath(), "obsidian", "slots");
                    Directory.CreateDirectory(dir);

                    var file = Path.Combine(dir, $"{Path.GetRandomFileName()}-slotData.json");

                    File.WriteAllText(file, json);
                    break;

                case 0x27:
                    // Update jigsaw Block
                    await Logger.LogDebugAsync("Received update jigsaw block");
                    break;

                case 0x28:
                    // Update Structure Block
                    await Logger.LogDebugAsync("Received update structure block");
                    break;

                case 0x29:
                    // Update sign
                    break;

                case 0x2A:
                    // Animation (serverbound)
                    var serverAnim = await PacketSerializer.FastDeserializeAsync<AnimationServerPacket>(packet.data);

                    await Logger.LogDebugAsync("Received animation (serverbound)");
                    break;

                case 0x2B:
                    // Spectate
                    await Logger.LogDebugAsync("Received spectate");
                    break;

                case 0x2C:
                    // Player Block Placement
                    var pbp = await PacketSerializer.FastDeserializeAsync<PlayerBlockPlacement>(packet.data);

                    await server.BroadcastBlockPlacementAsync(client.Player.Uuid, pbp);
                    await Logger.LogDebugAsync("Received player block placement");

                    break;

                case 0x2D:
                    // Use Item
                    await Logger.LogDebugAsync("Received use item");
                    break;
            }
        }
    }
}