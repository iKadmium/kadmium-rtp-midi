namespace KadmiumRtpMidi.Packets
{
	public enum ControlPacketCommand
	{
		IN, // invitation
		OK, // invitation acceptance
		NO, // invitation rejection
		BY, // session ending
		CK, // clock
		RS, // sync
	}
}
