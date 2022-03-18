using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets.MidiCommands
{
	public class ControlChange : MidiCommand
	{
		public const byte Length = 3;
		public byte Channel { get; set; }
		public byte CcNumber { get; set; }
		public byte Value { get; set; }

		public override string ToString()
		{
			return ($"Channel {Channel} CC {CcNumber} Value {Value}");
		}

		public static ControlChange Parse(ReadOnlySpan<byte> buffer)
		{
			var bytes = buffer[0..Length];
			var command = new ControlChange
			{
				Channel = (byte)(bytes[0] & 0x0F),
				CcNumber = bytes[1],
				Value = bytes[2]
			};
			return command;
		}
	}
}