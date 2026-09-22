using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class WeatherUpdateMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 17;
			}
		}

		public WeatherUpdateMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.weatherPos);
				writer.Write(this.cloudsX);
				writer.Write(this.cloudsZ);
				writer.Write(this.cloudsRot);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.stateName);
				}
				if ((this.optionalsMask & 2) != 0)
				{
					writer.Write(this.cloudsEnabled);
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
				this.weatherPos = reader.ReadSingle();
				this.cloudsX = reader.ReadSingle();
				this.cloudsZ = reader.ReadSingle();
				this.cloudsRot = reader.ReadSingle();
				if ((this.optionalsMask & 1) != 0)
				{
					this.stateName = reader.ReadString();
				}
				if ((this.optionalsMask & 2) != 0)
				{
					this.cloudsEnabled = reader.ReadBoolean();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string StateName
		{
			get
			{
				return this.stateName;
			}
			set
			{
				this.stateName = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasStateName
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		public bool CloudsEnabled
		{
			get
			{
				return this.cloudsEnabled;
			}
			set
			{
				this.cloudsEnabled = value;
				this.optionalsMask |= 2;
			}
		}

		public bool HasCloudsEnabled
		{
			get
			{
				return (this.optionalsMask & 2) > 0;
			}
		}

		private byte optionalsMask;

		public float weatherPos;

		public float cloudsX;

		public float cloudsZ;

		public float cloudsRot;

		private string stateName;

		private bool cloudsEnabled;
	}
}
