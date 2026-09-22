using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class VehicleEnterMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 9;
			}
		}

		public VehicleEnterMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.objectID);
				writer.Write(this.passenger);
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
				this.passenger = reader.ReadInt32();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public int objectID;

		public int passenger;
	}
}
