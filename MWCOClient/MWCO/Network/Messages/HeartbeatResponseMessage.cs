using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class HeartbeatResponseMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 2;
			}
		}

		public HeartbeatResponseMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.clientClock);
				writer.Write(this.clock);
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool Read(BinaryReader reader)
		{
			bool flag;
			try
			{
				this.clientClock = reader.ReadUInt64();
				this.clock = reader.ReadUInt64();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public ulong clientClock;

		public ulong clock;
	}
}
