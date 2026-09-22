using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class LightSwitchMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 16;
			}
		}

		public LightSwitchMessage()
		{
			this.pos = new Vector3Message();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				if (!this.pos.Write(writer))
				{
					flag = false;
				}
				else
				{
					writer.Write(this.toggle);
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
				if (!this.pos.Read(reader))
				{
					flag = false;
				}
				else
				{
					this.toggle = reader.ReadBoolean();
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public Vector3Message pos;

		public bool toggle;
	}
}
