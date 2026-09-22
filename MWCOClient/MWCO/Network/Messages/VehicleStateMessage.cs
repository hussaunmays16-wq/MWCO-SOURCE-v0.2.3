using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class VehicleStateMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 5;
			}
		}

		public VehicleStateMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.objectID);
				writer.Write(this.state);
				writer.Write(this.ignition);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.startTime);
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
				this.state = reader.ReadString();
				this.ignition = reader.ReadBoolean();
				if ((this.optionalsMask & 1) != 0)
				{
					this.startTime = reader.ReadSingle();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public float StartTime
		{
			get
			{
				return this.startTime;
			}
			set
			{
				this.startTime = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasStartTime
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		private byte optionalsMask;

		public int objectID;

		public string state;

		public bool ignition;

		private float startTime;
	}
}
