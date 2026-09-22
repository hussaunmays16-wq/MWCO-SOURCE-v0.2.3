using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PickupableDestroyMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 12;
			}
		}

		public PickupableDestroyMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.id);
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
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int id;
	}
}
