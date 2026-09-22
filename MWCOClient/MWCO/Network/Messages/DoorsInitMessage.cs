using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class DoorsInitMessage
	{
		public DoorsInitMessage()
		{
			this.position = new Vector3Message();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.open);
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
				this.open = reader.ReadBoolean();
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

		public bool open;

		public Vector3Message position;
	}
}
