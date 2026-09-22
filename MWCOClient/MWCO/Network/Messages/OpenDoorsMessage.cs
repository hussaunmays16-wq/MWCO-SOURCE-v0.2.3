using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class OpenDoorsMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 6;
			}
		}

		public OpenDoorsMessage()
		{
			this.position = new Vector3Message();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				if (!this.position.Write(writer))
				{
					flag = false;
				}
				else
				{
					writer.Write(this.open);
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
				if (!this.position.Read(reader))
				{
					flag = false;
				}
				else
				{
					this.open = reader.ReadBoolean();
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public Vector3Message position;

		public bool open;
	}
}
