using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class VehicleInitMessage
	{
		public VehicleInitMessage()
		{
			this.transform = new TransformMessage();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.id);
				if (!this.transform.Write(writer))
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
				this.id = reader.ReadByte();
				if (!this.transform.Read(reader))
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

		public byte id;

		public TransformMessage transform;
	}
}
