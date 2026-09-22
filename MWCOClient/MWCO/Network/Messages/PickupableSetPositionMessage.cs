using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PickupableSetPositionMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 14;
			}
		}

		public PickupableSetPositionMessage()
		{
			this.position = new Vector3Message();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.id);
				if (!this.position.Write(writer))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
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
				if (!this.position.Read(reader))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int id;

		public Vector3Message position;
	}
}
