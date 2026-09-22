using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PickupableActivateMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 13;
			}
		}

		public PickupableActivateMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.id);
				writer.Write(this.activate);
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
				this.id = reader.ReadInt32();
				this.activate = reader.ReadBoolean();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int id;

		public bool activate;
	}
}
