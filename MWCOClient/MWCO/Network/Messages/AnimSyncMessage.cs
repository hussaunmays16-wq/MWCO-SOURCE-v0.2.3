using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class AnimSyncMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 18;
			}
		}

		public AnimSyncMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.isRunning);
				writer.Write(this.isLeaning);
				writer.Write(this.isGrounded);
				writer.Write(this.activeHandState);
				writer.Write(this.aimRot);
				writer.Write(this.crouchPosition);
				writer.Write(this.isDrunk);
				writer.Write(this.drinkId);
				writer.Write(this.swearId);
				writer.Write(this.pissRate);
				writer.Write(this.smoking);
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
				this.isRunning = reader.ReadBoolean();
				this.isLeaning = reader.ReadBoolean();
				this.isGrounded = reader.ReadBoolean();
				this.activeHandState = reader.ReadByte();
				this.aimRot = reader.ReadSingle();
				this.crouchPosition = reader.ReadSingle();
				this.isDrunk = reader.ReadBoolean();
				this.drinkId = reader.ReadByte();
				this.swearId = reader.ReadInt32();
				this.pissRate = reader.ReadSingle();
				this.smoking = reader.ReadInt32();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool isRunning;

		public bool isLeaning;

		public bool isGrounded;

		public byte activeHandState;

		public float aimRot;

		public float crouchPosition;

		public bool isDrunk;

		public byte drinkId;

		public int swearId;

		public float pissRate;

		public int smoking;
	}
}
