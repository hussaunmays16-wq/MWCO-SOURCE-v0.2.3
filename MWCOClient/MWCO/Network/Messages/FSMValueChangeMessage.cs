using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class FSMValueChangeMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 25;
			}
		}

		public FSMValueChangeMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.objectID);
				writer.Write(this.fsmValueName);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.newBool);
				}
				if ((this.optionalsMask & 2) != 0)
				{
					writer.Write(this.newFloat);
				}
				if ((this.optionalsMask & 4) != 0)
				{
					writer.Write(this.newString);
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
				this.fsmValueName = reader.ReadString();
				if ((this.optionalsMask & 1) != 0)
				{
					this.newBool = reader.ReadBoolean();
				}
				if ((this.optionalsMask & 2) != 0)
				{
					this.newFloat = reader.ReadSingle();
				}
				if ((this.optionalsMask & 4) != 0)
				{
					this.newString = reader.ReadString();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool NewBool
		{
			get
			{
				return this.newBool;
			}
			set
			{
				this.newBool = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasNewBool
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public float NewFloat
		{
			get
			{
				return this.newFloat;
			}
			set
			{
				this.newFloat = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasNewFloat
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		public string NewString
		{
			get
			{
				return this.newString;
			}
			set
			{
				this.newString = value;
				this.optionalsMask |= 4;
			}
		}

		public bool HasNewString
		{
			get
			{
				return (this.optionalsMask & 4) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		public string fsmValueName;

		private bool newBool;

		private float newFloat;

		private string newString;
	}
}
