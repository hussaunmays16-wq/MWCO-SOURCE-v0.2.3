using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ModChangeMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 30;
			}
		}

		public ModChangeMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.objectID);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.booleanChanged);
				}
				if ((this.optionalsMask & 2) != 0)
				{
					writer.Write(this.intChanged);
				}
				if ((this.optionalsMask & 4) != 0)
				{
					writer.Write(this.stringChange);
				}
				if ((this.optionalsMask & 8) != 0)
				{
					writer.Write(this.floatChanged);
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
				if ((this.optionalsMask & 1) != 0)
				{
					this.booleanChanged = reader.ReadBoolean();
				}
				if ((this.optionalsMask & 2) != 0)
				{
					this.intChanged = reader.ReadInt32();
				}
				if ((this.optionalsMask & 4) != 0)
				{
					this.stringChange = reader.ReadString();
				}
				if ((this.optionalsMask & 8) != 0)
				{
					this.floatChanged = reader.ReadSingle();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool BooleanChanged
		{
			get
			{
				return this.booleanChanged;
			}
			set
			{
				this.booleanChanged = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasBooleanChanged
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public int IntChanged
		{
			get
			{
				return this.intChanged;
			}
			set
			{
				this.intChanged = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasIntChanged
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		public string StringChange
		{
			get
			{
				return this.stringChange;
			}
			set
			{
				this.stringChange = value;
				this.optionalsMask |= 4;
			}
		}

		public bool HasStringChange
		{
			get
			{
				return (this.optionalsMask & 4) > 0;
			}
		}

		public float FloatChanged
		{
			get
			{
				return this.floatChanged;
			}
			set
			{
				this.floatChanged = value;
				this.optionalsMask |= 8;
			}
		}

		public bool HasFloatChanged
		{
			get
			{
				return (this.optionalsMask & 8) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		private bool booleanChanged;

		private int intChanged;

		private string stringChange;

		private float floatChanged;
	}
}
