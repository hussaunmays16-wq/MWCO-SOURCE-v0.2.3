using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class DisconnectMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 3;
			}
		}

		public DisconnectMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
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
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}
	}
}
