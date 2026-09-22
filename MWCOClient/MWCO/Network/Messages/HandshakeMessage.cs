using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class HandshakeMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 0;
			}
		}

		public HandshakeMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.modVersion);
				writer.Write(this.clock);
				writer.Write(this.skinCode);
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
				this.modVersion = reader.ReadString();
				this.clock = reader.ReadUInt64();
				this.skinCode = reader.ReadString();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string modVersion;

		public ulong clock;

		public string skinCode;
	}
}
