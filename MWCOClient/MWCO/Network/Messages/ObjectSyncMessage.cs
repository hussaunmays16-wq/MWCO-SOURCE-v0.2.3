using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ObjectSyncMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 20;
			}
		}

		public ObjectSyncMessage()
		{
			this.position = new Vector3Message();
			this.rotation = new QuaternionMessage();
			this.syncedVariables = new float[0];
			this.velocity = new Vector3Message();
			this.angularVelocity = new Vector3Message();
			this.wheelRpms = new float[0];
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.objectID);
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
					if ((this.optionalsMask & 1) != 0)
					{
						writer.Write(this.syncType);
					}
					if ((this.optionalsMask & 2) != 0)
					{
						writer.Write(this.syncedVariables.Length);
						foreach (float num in this.syncedVariables)
						{
							writer.Write(num);
						}
					}
					if ((this.optionalsMask & 4) != 0)
					{
						writer.Write(this.pickUp);
					}
					if ((this.optionalsMask & 8) != 0 && !this.velocity.Write(writer))
					{
						return false;
					}
					if ((this.optionalsMask & 16) != 0 && !this.angularVelocity.Write(writer))
					{
						return false;
					}
					if ((this.optionalsMask & 32) != 0)
					{
						writer.Write(this.wheelRpms.Length);
						for (int j = 0; j < this.wheelRpms.Length; j++)
						{
							writer.Write(this.wheelRpms[j]);
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
				this.objectID = reader.ReadInt32();
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
					if ((this.optionalsMask & 1) != 0)
					{
						this.syncType = reader.ReadInt32();
					}
					if ((this.optionalsMask & 2) != 0)
					{
						int num = reader.ReadInt32();
						this.syncedVariables = new float[num];
						for (int i = 0; i < num; i++)
						{
							this.syncedVariables[i] = reader.ReadSingle();
						}
					}
					if ((this.optionalsMask & 4) != 0)
					{
						this.pickUp = reader.ReadBoolean();
					}
					if ((this.optionalsMask & 8) != 0 && !this.velocity.Read(reader))
					{
						return false;
					}
					if ((this.optionalsMask & 16) != 0 && !this.angularVelocity.Read(reader))
					{
						return false;
					}
					if ((this.optionalsMask & 32) != 0)
					{
						int num2 = reader.ReadInt32();
						this.wheelRpms = new float[num2];
						for (int k = 0; k < num2; k++)
						{
							this.wheelRpms[k] = reader.ReadSingle();
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

		public int SyncType
		{
			get
			{
				return this.syncType;
			}
			set
			{
				this.syncType = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasSyncType
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public float[] SyncedVariables
		{
			get
			{
				return this.syncedVariables;
			}
			set
			{
				this.syncedVariables = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasSyncedVariables
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		public bool PickUp
		{
			get
			{
				return this.pickUp;
			}
			set
			{
				this.pickUp = value;
				this.optionalsMask |= 4;
			}
		}

		public bool HasPickUp
		{
			get
			{
				return (this.optionalsMask & 4) > 0;
			}
		}

		public Vector3Message Velocity
		{
			get
			{
				return this.velocity;
			}
			set
			{
				this.velocity = value;
				this.optionalsMask |= 8;
			}
		}

		public bool HasVelocity
		{
			get
			{
				return (this.optionalsMask & 8) > 0;
			}
		}

		public Vector3Message AngularVelocity
		{
			get
			{
				return this.angularVelocity;
			}
			set
			{
				this.angularVelocity = value;
				this.optionalsMask |= 16;
			}
		}

		public bool HasAngularVelocity
		{
			get
			{
				return (this.optionalsMask & 16) > 0;
			}
		}

		public float[] WheelRpms
		{
			get
			{
				return this.wheelRpms;
			}
			set
			{
				this.wheelRpms = value;
				this.optionalsMask |= 32;
			}
		}

		public bool HasWheelRpms
		{
			get
			{
				return (this.optionalsMask & 32) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		public Vector3Message position;

		public QuaternionMessage rotation;

		private int syncType;

		private float[] syncedVariables;

		private bool pickUp;

		private Vector3Message velocity;

		private Vector3Message angularVelocity;

		private float[] wheelRpms;
	}
}
