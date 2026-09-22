using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ObjectSyncResponseMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 21;
			}
		}

		public ObjectSyncResponseMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.objectID);
				writer.Write(this.accepted);
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
				this.accepted = reader.ReadBoolean();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int objectID;

		public bool accepted;
	}
}
