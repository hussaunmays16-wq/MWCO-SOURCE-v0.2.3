using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class HouseSwitchMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 24;
			}
		}

		public HouseSwitchMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.objectID);
				writer.Write(this.switchID);
				writer.Write(this.switchValue);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.switchValueFloat);
				}
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
				this.optionalsMask = reader.ReadByte();
				this.objectID = reader.ReadInt32();
				this.switchID = reader.ReadInt32();
				this.switchValue = reader.ReadBoolean();
				if ((this.optionalsMask & 1) != 0)
				{
					this.switchValueFloat = reader.ReadSingle();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public float SwitchValueFloat
		{
			get
			{
				return this.switchValueFloat;
			}
			set
			{
				this.switchValueFloat = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasSwitchValueFloat
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		public int switchID;

		public bool switchValue;

		private float switchValueFloat;
	}
}
