using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class WorldPeriodicalUpdateMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 15;
			}
		}

		public WorldPeriodicalUpdateMessage()
		{
			this.currentWeather = new WeatherUpdateMessage();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.sunClock);
				writer.Write(this.worldDay);
				if (!this.currentWeather.Write(writer))
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
				this.sunClock = reader.ReadByte();
				this.worldDay = reader.ReadByte();
				if (!this.currentWeather.Read(reader))
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

		public byte sunClock;

		public byte worldDay;

		public WeatherUpdateMessage currentWeather;
	}
}
