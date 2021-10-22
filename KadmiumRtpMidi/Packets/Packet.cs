using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class Packet
	{
		public static Packet Parse(ReadOnlyMemory<byte> buffer)
		{
			var bytes = buffer.Span;
			if (bytes.Length < 4)
			{
				return null;
			}

			var header = bytes[0..5];
			if (header[0] == 0xff && header[1] == 0xff)
			{
				return ControlPacket.Parse(buffer);
			}
			else
			{
				return DataPacket.Parse(buffer);
			}
		}
	}
}