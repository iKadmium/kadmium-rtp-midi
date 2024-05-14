using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class ClockSyncPacket : ControlPacket
	{
		private const byte TotalTimestampCount = 3;

		public uint Ssrc { get; set; }
		public byte TimestampCount { get; set; }
		public UInt64[] Timestamps { get; } = new UInt64[3];
		public int Length => 36;

		public ClockSyncPacket()
		{
			Command = ControlPacketCommand.CK;
		}

		public static new ClockSyncPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var bytes = buffer.Span;
			var count = bytes[8];
			var packet = new ClockSyncPacket
			{
				Ssrc = BinaryPrimitives.ReadUInt32BigEndian(bytes[4..8]),
				TimestampCount = count
			};
			var timestampBytes = bytes[12..];
			for (int i = 0; i < TotalTimestampCount; i++)
			{
				packet.Timestamps[i] = BinaryPrimitives.ReadUInt64BigEndian(timestampBytes[0..8]);
				timestampBytes = timestampBytes[8..];
			}
			var timestampTotal = BinaryPrimitives.ReadUInt64BigEndian(bytes[12..]);
			return packet;
		}

		public void WriteTo(Memory<byte> bytes)
		{
			if (bytes.Length < Length)
			{
				throw new OutOfMemoryException($"Memory was not large enough - needed {Length}, received {bytes.Length}");
			}

			var span = bytes.Span[0..Length];
			span[0] = 0xff;
			span[1] = 0xff;
			System.Text.Encoding.ASCII.GetBytes(Command.ToString(), span[2..4]);
			BinaryPrimitives.WriteUInt32BigEndian(span[4..8], Ssrc);
			span[8] = TimestampCount;
			var timestampBytes = span[12..];
			for (int i = 0; i < TotalTimestampCount; i++)
			{
				BinaryPrimitives.WriteUInt64BigEndian(timestampBytes[0..8], Timestamps[i]);
				timestampBytes = timestampBytes[8..];
			}
		}
	}
}