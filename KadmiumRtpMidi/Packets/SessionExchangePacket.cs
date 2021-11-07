using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class SessionExchangePacket : ControlPacket
	{
		const int MinHeaderLength = 16;

		public uint ProtocolVersion { get; set; } = 2;
		public uint InitiatorToken { get; set; }
		public uint Ssrc { get; set; }
		public string Name { get; set; }
		public int Length => MinHeaderLength + (Name != null ? Name.Length + 1 : 0);

		protected void ParseInternal(ReadOnlyMemory<byte> buffer)
		{
			var bytes = buffer.Span;
			Command = (ControlPacketCommand)BinaryPrimitives.ReadUInt16BigEndian(bytes[2..4]);
			ProtocolVersion = BinaryPrimitives.ReadUInt32BigEndian(bytes[4..8]);
			InitiatorToken = BinaryPrimitives.ReadUInt32BigEndian(bytes[8..12]);
			Ssrc = BinaryPrimitives.ReadUInt32BigEndian(bytes[12..16]);
			if (buffer.Length > MinHeaderLength)
			{
				//optional name is present
				Name = System.Text.Encoding.ASCII.GetString(bytes[16..^1]);
			}
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
			BinaryPrimitives.WriteUInt32BigEndian(span[4..8], ProtocolVersion);
			BinaryPrimitives.WriteUInt32BigEndian(span[8..12], InitiatorToken);
			BinaryPrimitives.WriteUInt32BigEndian(span[12..16], Ssrc);
			if (Name != null && Name.Length > 0)
			{
				System.Text.Encoding.ASCII.GetBytes(Name.ToString(), span[16..^1]);
			}
		}
	}
}