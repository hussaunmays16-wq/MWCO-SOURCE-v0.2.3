using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class OrderListMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 29;
			}
		}

		public OrderListMessage()
		{
			this.orderList = new string[0];
			this.fleetariOrderList = new bool[0];
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag2;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.fleetari);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.orderList.Length);
					foreach (string text in this.orderList)
					{
						writer.Write(text);
					}
				}
				if ((this.optionalsMask & 2) != 0)
				{
					writer.Write(this.fleetariOrderList.Length);
					foreach (bool flag in this.fleetariOrderList)
					{
						writer.Write(flag);
					}
				}
				flag2 = true;
			}
			catch (Exception)
			{
				flag2 = false;
			}
			return flag2;
		}

		public bool Read(BinaryReader reader)
		{
			bool flag;
			try
			{
				this.optionalsMask = reader.ReadByte();
				this.fleetari = reader.ReadBoolean();
				if ((this.optionalsMask & 1) != 0)
				{
					int num = reader.ReadInt32();
					this.orderList = new string[num];
					for (int i = 0; i < num; i++)
					{
						this.orderList[i] = reader.ReadString();
					}
				}
				if ((this.optionalsMask & 2) != 0)
				{
					int num2 = reader.ReadInt32();
					this.fleetariOrderList = new bool[num2];
					for (int j = 0; j < num2; j++)
					{
						this.fleetariOrderList[j] = reader.ReadBoolean();
					}
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string[] OrderList
		{
			get
			{
				return this.orderList;
			}
			set
			{
				this.orderList = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasOrderList
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public bool[] FleetariOrderList
		{
			get
			{
				return this.fleetariOrderList;
			}
			set
			{
				this.fleetariOrderList = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasFleetariOrderList
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		private byte optionalsMask;

		public bool fleetari;

		private string[] orderList;

		private bool[] fleetariOrderList;
	}
}
