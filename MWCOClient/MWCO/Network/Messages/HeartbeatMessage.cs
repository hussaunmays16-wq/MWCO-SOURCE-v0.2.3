using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class HeartbeatMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 1;
			}
		}

		public HeartbeatMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.clientClock);
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
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public ulong clientClock;
	}
}
