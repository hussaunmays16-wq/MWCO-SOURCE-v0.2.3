using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PickupableSpawnMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 11;
			}
		}

		public PickupableSpawnMessage()
		{
			this.transform = new TransformMessage();
			this.data = new float[0];
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.id);
				writer.Write(this.prefabId);
				if (!this.transform.Write(writer))
				{
					flag = false;
				}
				else
				{
					if ((this.optionalsMask & 1) != 0)
					{
						writer.Write(this.parentTriggerName);
					}
					writer.Write(this.active);
					if ((this.optionalsMask & 2) != 0)
					{
						writer.Write(this.data.Length);
						foreach (float num in this.data)
						{
							writer.Write(num);
						}
					}
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
				this.id = reader.ReadInt32();
				this.prefabId = reader.ReadInt32();
				if (!this.transform.Read(reader))
				{
					flag = false;
				}
				else
				{
					if ((this.optionalsMask & 1) != 0)
					{
						this.parentTriggerName = reader.ReadString();
					}
					this.active = reader.ReadBoolean();
					if ((this.optionalsMask & 2) != 0)
					{
						int num = reader.ReadInt32();
						this.data = new float[num];
						for (int i = 0; i < num; i++)
						{
							this.data[i] = reader.ReadSingle();
						}
					}
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string ParentTriggerName
		{
			get
			{
				return this.parentTriggerName;
			}
			set
			{
				this.parentTriggerName = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasParentTriggerName
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public float[] Data
		{
			get
			{
				return this.data;
			}
			set
			{
				this.data = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasData
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		private byte optionalsMask;

		public int id;

		public int prefabId;

		public TransformMessage transform;

		private string parentTriggerName;

		public bool active;

		private float[] data;
	}
}
