using System;
using System.Threading.Tasks;
using KadmiumRtpMidi;

namespace KadmiumRtpMidi.ClientApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var session = new Session(5023);
			while (true)
			{
				await Task.Delay(1000);
			}
		}
	}
}
