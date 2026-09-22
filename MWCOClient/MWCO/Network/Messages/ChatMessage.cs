using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ChatMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 27;
			}
		}

		public ChatMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.message);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.floatValue);
				}
				writer.Write(this.clock);
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
				this.message = reader.ReadString();
				if ((this.optionalsMask & 1) != 0)
				{
					this.floatValue = reader.ReadSingle();
				}
				this.clock = reader.ReadUInt64();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public float FloatValue
		{
			get
			{
				return this.floatValue;
			}
			set
			{
				this.floatValue = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasFloatValue
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		private byte optionalsMask;

		public string message;

		private float floatValue;

		public ulong clock;
	}
}
