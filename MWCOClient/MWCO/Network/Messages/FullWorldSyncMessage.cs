using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class FullWorldSyncMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 7;
			}
		}

		public FullWorldSyncMessage()
		{
			this.doors = new DoorsInitMessage[0];
			this.mods = new ModOSCMessage[0];
			this.pickupables = new PickupableSpawnMessage[0];
			this.lights = new LightSwitchMessage[0];
			this.currentWeather = new WeatherUpdateMessage();
			this.spawnPosition = new Vector3Message();
			this.spawnRotation = new QuaternionMessage();
			this.spawnPosition2 = new Vector3Message();
			this.spawnRotation2 = new QuaternionMessage();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.mailboxName);
				writer.Write(this.day);
				writer.Write(this.hostModel);
				writer.Write(this.dayTime);
				writer.Write(this.doors.Length);
				DoorsInitMessage[] array = this.doors;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].Write(writer))
					{
						return false;
					}
				}
				writer.Write(this.mods.Length);
				ModOSCMessage[] array2 = this.mods;
				for (int i = 0; i < array2.Length; i++)
				{
					if (!array2[i].Write(writer))
					{
						return false;
					}
				}
				writer.Write(this.pickupables.Length);
				PickupableSpawnMessage[] array3 = this.pickupables;
				for (int i = 0; i < array3.Length; i++)
				{
					if (!array3[i].Write(writer))
					{
						return false;
					}
				}
				writer.Write(this.lights.Length);
				LightSwitchMessage[] array4 = this.lights;
				for (int i = 0; i < array4.Length; i++)
				{
					if (!array4[i].Write(writer))
					{
						return false;
					}
				}
				if (!this.currentWeather.Write(writer))
				{
					flag = false;
				}
				else if (!this.spawnPosition.Write(writer))
				{
					flag = false;
				}
				else if (!this.spawnRotation.Write(writer))
				{
					flag = false;
				}
				else if (!this.spawnPosition2.Write(writer))
				{
					flag = false;
				}
				else if (!this.spawnRotation2.Write(writer))
				{
					flag = false;
				}
				else
				{
					writer.Write(this.occupiedVehicleId);
					writer.Write(this.passenger);
					writer.Write(this.pickedUpObject);
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
				this.mailboxName = reader.ReadString();
				this.day = reader.ReadInt32();
				this.hostModel = reader.ReadString();
				this.dayTime = reader.ReadSingle();
				int num = reader.ReadInt32();
				this.doors = new DoorsInitMessage[num];
				for (int i = 0; i < num; i++)
				{
					this.doors[i] = new DoorsInitMessage();
					if (!this.doors[i].Read(reader))
					{
						return false;
					}
				}
				int num2 = reader.ReadInt32();
				this.mods = new ModOSCMessage[num2];
				for (int j = 0; j < num2; j++)
				{
					this.mods[j] = new ModOSCMessage();
					if (!this.mods[j].Read(reader))
					{
						return false;
					}
				}
				int num3 = reader.ReadInt32();
				this.pickupables = new PickupableSpawnMessage[num3];
				for (int k = 0; k < num3; k++)
				{
					this.pickupables[k] = new PickupableSpawnMessage();
					if (!this.pickupables[k].Read(reader))
					{
						return false;
					}
				}
				int num4 = reader.ReadInt32();
				this.lights = new LightSwitchMessage[num4];
				for (int l = 0; l < num4; l++)
				{
					this.lights[l] = new LightSwitchMessage();
					if (!this.lights[l].Read(reader))
					{
						return false;
					}
				}
				if (!this.currentWeather.Read(reader))
				{
					flag = false;
				}
				else if (!this.spawnPosition.Read(reader))
				{
					flag = false;
				}
				else if (!this.spawnRotation.Read(reader))
				{
					flag = false;
				}
				else if (!this.spawnPosition2.Read(reader))
				{
					flag = false;
				}
				else if (!this.spawnRotation2.Read(reader))
				{
					flag = false;
				}
				else
				{
					this.occupiedVehicleId = reader.ReadByte();
					this.passenger = reader.ReadBoolean();
					this.pickedUpObject = reader.ReadUInt16();
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string mailboxName;

		public int day;

		public string hostModel;

		public float dayTime;

		public DoorsInitMessage[] doors;

		public ModOSCMessage[] mods;

		public PickupableSpawnMessage[] pickupables;

		public LightSwitchMessage[] lights;

		public WeatherUpdateMessage currentWeather;

		public Vector3Message spawnPosition;

		public QuaternionMessage spawnRotation;

		public Vector3Message spawnPosition2;

		public QuaternionMessage spawnRotation2;

		public byte occupiedVehicleId;

		public bool passenger;

		public ushort pickedUpObject;
	}
}
