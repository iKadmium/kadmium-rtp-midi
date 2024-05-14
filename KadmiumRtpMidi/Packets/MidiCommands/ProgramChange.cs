using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets.MidiCommands
{
	public class ProgramChange : MidiCommand
	{
		public const byte Length = 2;
		public byte Channel { get; set; }
		public byte Value { get; set; }

		public override string ToString()
		{
			return ($"Program Change: Channel {Channel} Value {Value}");
		}

		public static ProgramChange Parse(ReadOnlySpan<byte> buffer)
		{
			var bytes = buffer[0..Length];
			var command = new ProgramChange
			{
				Channel = (byte)(bytes[0] & 0x0F),
				Value = bytes[1]
			};
			return command;
		}
	}
}