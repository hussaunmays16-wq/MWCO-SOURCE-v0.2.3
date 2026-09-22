using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class SatsumaSwitchMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 26;
			}
		}

		public SatsumaSwitchMessage()
		{
			this.color = new ColorMessage();
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
					writer.Write(this.assemble);
				}
				if ((this.optionalsMask & 2) != 0)
				{
					writer.Write(this.tighten);
				}
				if ((this.optionalsMask & 4) != 0)
				{
					writer.Write(this.boltMesh);
				}
				if ((this.optionalsMask & 8) != 0)
				{
					writer.Write(this.floatValue);
				}
				if ((this.optionalsMask & 16) != 0)
				{
					writer.Write(this.triggerID);
				}
				if ((this.optionalsMask & 32) != 0)
				{
					writer.Write(this.partObjectID);
				}
				if ((this.optionalsMask & 64) != 0 && !this.color.Write(writer))
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
				this.optionalsMask = reader.ReadByte();
				this.objectID = reader.ReadInt32();
				if ((this.optionalsMask & 1) != 0)
				{
					this.assemble = reader.ReadBoolean();
				}
				if ((this.optionalsMask & 2) != 0)
				{
					this.tighten = reader.ReadBoolean();
				}
				if ((this.optionalsMask & 4) != 0)
				{
					this.boltMesh = reader.ReadString();
				}
				if ((this.optionalsMask & 8) != 0)
				{
					this.floatValue = reader.ReadSingle();
				}
				if ((this.optionalsMask & 16) != 0)
				{
					this.triggerID = reader.ReadInt32();
				}
				if ((this.optionalsMask & 32) != 0)
				{
					this.partObjectID = reader.ReadInt32();
				}
				if ((this.optionalsMask & 64) != 0 && !this.color.Read(reader))
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

		public bool Assemble
		{
			get
			{
				return this.assemble;
			}
			set
			{
				this.assemble = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasAssemble
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public bool Tighten
		{
			get
			{
				return this.tighten;
			}
			set
			{
				this.tighten = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasTighten
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		public string BoltMesh
		{
			get
			{
				return this.boltMesh;
			}
			set
			{
				this.boltMesh = value;
				this.optionalsMask |= 4;
			}
		}

		public bool HasBoltMesh
		{
			get
			{
				return (this.optionalsMask & 4) > 0;
			}
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
				this.optionalsMask |= 8;
			}
		}

		public bool HasFloatValue
		{
			get
			{
				return (this.optionalsMask & 8) > 0;
			}
		}

		public int TriggerID
		{
			get
			{
				return this.triggerID;
			}
			set
			{
				this.triggerID = value;
				this.optionalsMask |= 16;
			}
		}

		public bool HasTriggerID
		{
			get
			{
				return (this.optionalsMask & 16) > 0;
			}
		}

		public int PartObjectID
		{
			get
			{
				return this.partObjectID;
			}
			set
			{
				this.partObjectID = value;
				this.optionalsMask |= 32;
			}
		}

		public bool HasPartObjectID
		{
			get
			{
				return (this.optionalsMask & 32) > 0;
			}
		}

		public ColorMessage Color
		{
			get
			{
				return this.color;
			}
			set
			{
				this.color = value;
				this.optionalsMask |= 64;
			}
		}

		public bool HasColor
		{
			get
			{
				return (this.optionalsMask & 64) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		private bool assemble;

		private bool tighten;

		private string boltMesh;

		private float floatValue;

		private int triggerID;

		private int partObjectID;

		private ColorMessage color;
	}
}
