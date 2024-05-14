using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets.MidiCommands
{
	public class PitchBend : MidiCommand
	{
		public const byte Length = 3;
		public byte Channel { get; set; }
		public ushort Value { get; set; }

		public override string ToString()
		{
			return ($"Pitch Bend: Channel {Channel} PitchBend Value {Value}");
		}

		public static PitchBend Parse(ReadOnlySpan<byte> buffer)
		{
			var bytes = buffer[0..Length];
			byte lsb = (byte)(bytes[1] & 0b_0111_1111);
			byte msb = (byte)(bytes[2] & 0b_0111_1111);

			var value = (ushort)((msb << 7) | (lsb));

			var command = new PitchBend
			{
				Channel = (byte)(bytes[0] & 0x0F),
				Value = value
			};
			return command;
		}
	}
}