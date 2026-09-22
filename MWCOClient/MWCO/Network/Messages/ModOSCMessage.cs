using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ModOSCMessage
	{
		public ModOSCMessage()
		{
			this.transform = new TransformMessage();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.ObjectName);
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
				this.ObjectName = reader.ReadString();
				this.id = reader.ReadInt32();
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

		public string ObjectName;

		public int id;

		public TransformMessage transform;
	}
}
