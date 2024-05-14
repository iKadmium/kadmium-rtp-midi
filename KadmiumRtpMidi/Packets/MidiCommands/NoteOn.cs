using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets.MidiCommands
{
	public class NoteOn : MidiCommand
	{
		public const byte Length = 3;
		public byte Channel { get; set; }
		public byte Note { get; set; }
		public byte Velocity { get; set; }

		private static string[] NoteNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

		public string NoteName
		{
			get
			{
				var octave = (Note / 12) - 1;
				var note = (Note % 12);
				return $"{NoteNames[note]}{octave}";
			}
		}

		public override string ToString()
		{
			return ($"Note On: Channel {Channel} Note {NoteName} Velocity {Velocity}");
		}

		public static NoteOn Parse(ReadOnlySpan<byte> buffer)
		{
			var bytes = buffer[0..Length];
			var command = new NoteOn
			{
				Channel = (byte)(bytes[0] & 0x0F),
				Note = bytes[1],
				Velocity = bytes[2]
			};
			return command;
		}

	}
}