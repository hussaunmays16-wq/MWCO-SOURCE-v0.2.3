using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ObjectSyncRequestMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 23;
			}
		}

		public ObjectSyncRequestMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.objectID);
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
				this.objectID = reader.ReadInt32();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int objectID;
	}
}
