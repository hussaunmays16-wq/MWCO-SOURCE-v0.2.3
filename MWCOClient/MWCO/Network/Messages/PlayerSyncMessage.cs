using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PlayerSyncMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 4;
			}
		}

		public PlayerSyncMessage()
		{
			this.position = new Vector3Message();
			this.rotation = new QuaternionMessage();
			this.pickedUpData = new PickedUpSync();
			this.playerAttributes = new float[0];
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				if (!this.position.Write(writer))
				{
					flag = false;
				}
				else if (!this.rotation.Write(writer))
				{
					flag = false;
				}
				else
				{
					writer.Write(this.aimYaw);
					writer.Write(this.aimPitch);
					if ((this.optionalsMask & 1) != 0 && !this.pickedUpData.Write(writer))
					{
						flag = false;
					}
					else
					{
						if ((this.optionalsMask & 2) != 0)
						{
							writer.Write(this.playerAttributes.Length);
							foreach (float num in this.playerAttributes)
							{
								writer.Write(num);
							}
						}
						if ((this.optionalsMask & 4) != 0)
						{
							writer.Write(this.pissRate);
						}
						if ((this.optionalsMask & 8) != 0)
						{
							writer.Write(this.smoking);
						}
						flag = true;
					}
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
				if (!this.position.Read(reader))
				{
					flag = false;
				}
				else if (!this.rotation.Read(reader))
				{
					flag = false;
				}
				else
				{
					this.aimYaw = reader.ReadSingle();
					this.aimPitch = reader.ReadSingle();
					if ((this.optionalsMask & 1) != 0 && !this.pickedUpData.Read(reader))
					{
						flag = false;
					}
					else
					{
						if ((this.optionalsMask & 2) != 0)
						{
							int num = reader.ReadInt32();
							this.playerAttributes = new float[num];
							for (int i = 0; i < num; i++)
							{
								this.playerAttributes[i] = reader.ReadSingle();
							}
						}
						if ((this.optionalsMask & 4) != 0)
						{
							this.pissRate = reader.ReadSingle();
						}
						if ((this.optionalsMask & 8) != 0)
						{
							this.smoking = reader.ReadInt32();
						}
						flag = true;
					}
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public PickedUpSync PickedUpData
		{
			get
			{
				return this.pickedUpData;
			}
			set
			{
				this.pickedUpData = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasPickedUpData
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public float[] PlayerAttributes
		{
			get
			{
				return this.playerAttributes;
			}
			set
			{
				this.playerAttributes = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasPlayerAttributes
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		public float PissRate
		{
			get
			{
				return this.pissRate;
			}
			set
			{
				this.pissRate = value;
				this.optionalsMask |= 4;
			}
		}

		public bool HasPissRate
		{
			get
			{
				return (this.optionalsMask & 4) > 0;
			}
		}

		public int Smoking
		{
			get
			{
				return this.smoking;
			}
			set
			{
				this.smoking = value;
				this.optionalsMask |= 8;
			}
		}

		public bool HasSmoking
		{
			get
			{
				return (this.optionalsMask & 8) > 0;
			}
		}

		private byte optionalsMask;

		public Vector3Message position;

		public QuaternionMessage rotation;

		public float aimYaw;

		public float aimPitch;

		private PickedUpSync pickedUpData;

		private float[] playerAttributes;

		private float pissRate;

		private int smoking;
	}
}
