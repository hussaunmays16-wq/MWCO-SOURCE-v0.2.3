using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class SaveMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 28;
			}
		}

		public SaveMessage()
		{
			this.saveBytes = new byte[0];
			this.itemsBytes = new byte[0];
			this.carPartsBytes = new byte[0];
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.saveBytes.Length);
				foreach (byte b in this.saveBytes)
				{
					writer.Write(b);
				}
				writer.Write(this.itemsBytes.Length);
				foreach (byte b2 in this.itemsBytes)
				{
					writer.Write(b2);
				}
				writer.Write(this.carPartsBytes.Length);
				foreach (byte b3 in this.carPartsBytes)
				{
					writer.Write(b3);
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
				int num = reader.ReadInt32();
				this.saveBytes = new byte[num];
				for (int i = 0; i < num; i++)
				{
					this.saveBytes[i] = reader.ReadByte();
				}
				int num2 = reader.ReadInt32();
				this.itemsBytes = new byte[num2];
				for (int j = 0; j < num2; j++)
				{
					this.itemsBytes[j] = reader.ReadByte();
				}
				int num3 = reader.ReadInt32();
				this.carPartsBytes = new byte[num3];
				for (int k = 0; k < num3; k++)
				{
					this.carPartsBytes[k] = reader.ReadByte();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public byte[] saveBytes;

		public byte[] itemsBytes;

		public byte[] carPartsBytes;
	}
}
