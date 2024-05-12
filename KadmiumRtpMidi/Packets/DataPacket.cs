using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KadmiumRtpMidi.Packets.MidiCommands;

namespace KadmiumRtpMidi.Packets
{
	public class DataPacket : Packet
	{
		public ushort SequenceNumber { get; set; }
		public uint Timestamp { get; set; }
		public uint Ssrc { get; set; }
		public List<MidiCommand> Commands { get; set; }

		public DataPacket()
		{
			Commands = new List<MidiCommand>();
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"DataPacket {SequenceNumber} at {Timestamp} from {Ssrc}");
			foreach (var command in Commands)
			{
				builder.AppendLine($"\t{command.ToString()}");
			}
			return builder.ToString();
		}

		public static new DataPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var span = buffer.Span;

			var packet = new DataPacket();

			var version = (span[0] & 0b_1100_0000) >> 6;
			var padding = (span[0] & 0b_0010_0000) >> 5;
			var extension = (span[0] & 0b_0001_0000) >> 4;
			var csrc = (span[0] & 0b_0000_1111);
			var marker = (span[1] & 0b_1000_0000) >> 7;
			var payloadType = (span[1] & 0b_0111_1111);
			packet.SequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(span[2..4]);
			packet.Timestamp = BinaryPrimitives.ReadUInt32BigEndian(span[4..8]);
			packet.Ssrc = BinaryPrimitives.ReadUInt32BigEndian(span[8..12]);

			var bigHeader = ((span[12] & 0b_1000_0000) >> 7) == 1 ? true : false; // b
			var journal = ((span[12] & 0b_0100_0000) >> 6) == 1 ? true : false; // j
			var deltaTime = ((span[12] & 0b_0010_0000) >> 5) == 1 ? true : false; // z
			var phantom = ((span[12] & 0b_0001_0000) >> 4) == 1 ? true : false; // p
			ReadOnlySpan<byte> commands;
			if (bigHeader)
			{
				var len1 = span[12] & 0b_0000_1111;
				var len2 = span[13];
				var len = (len1 << 8) | len2;
				commands = span[14..(14 + len)];
			}
			else
			{
				var len = span[12] & 0b_0000_1111;
				commands = span[13..(13 + len)];
			}

			while (commands.Length > 0)
			{
				var type = commands[0] & 0xF0;
				switch (type)
				{
					case 0x90: // note on
						packet.Commands.Add(NoteOn.Parse(commands));
						commands = commands.Slice(NoteOn.Length);
						break;
					case 0xB0: // CC
						packet.Commands.Add(ControlChange.Parse(commands));
						commands = commands.Slice(ControlChange.Length);
						break;
					case 0xE0:
						packet.Commands.Add(PitchBend.Parse(commands));
						commands = commands.Slice(PitchBend.Length);
						break;
				}
			}

			return packet;
		}
	}
}