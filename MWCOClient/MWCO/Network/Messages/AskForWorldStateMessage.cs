using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class AskForWorldStateMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 8;
			}
		}

		public AskForWorldStateMessage()
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
