using System;
using System.Collections.Generic;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;

namespace MWCO.Game
{
	public class ObjectSyncManager
	{
		public ObjectSyncManager()
		{
			ObjectSyncManager.Instance = this;
		}

		public int AddNewObject(ObjectSyncComponent osc, int objectID)
		{
			if (objectID == ObjectSyncManager.AUTOMATIC_ID)
			{
				if (this.steamID.m_SteamID == 0UL)
				{
					this.steamID = SteamUser.GetSteamID();
				}
				this.ObjectIDs.Add(this.ObjectIDs.Count + 1, osc);
				return this.ObjectIDs.Count;
			}
			Logger.Debug(string.Format("Force adding new ObjectID at: {0}", objectID));
			if (this.ObjectIDs.ContainsKey(objectID))
			{
				this.ObjectIDs[objectID] = osc;
			}
			else
			{
				this.ObjectIDs.Add(objectID, osc);
			}
			return objectID;
		}

		public void Dispose()
		{
			this.ObjectIDs.Clear();
			ObjectSyncManager.Instance = null;
		}

		public void listParts()
		{
			for (int i = 0; i < this.ObjectIDs.Count - 1; i++)
			{
				try
				{
					if (this.ObjectIDs[i].gameObject.GetTopmostParent().name == "SATSUMA(557kg, 248)")
					{
						Logger.Debug(string.Format("RETARDATION: {0} is a Satsuma child... Object type {1}", this.ObjectIDs[i].transform.name, this.ObjectIDs[i].ObjectType));
					}
				}
				catch
				{
				}
			}
		}

		public bool ShouldPeriodicSync(ulong owner, bool syncEnabled)
		{
			return NetManager.Instance.TicksSinceConnectionStarted % 500UL == 0UL && (syncEnabled || (owner == ObjectSyncManager.NO_OWNER && NetManager.Instance.IsHost));
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ObjectSyncManager()
		{
		}

		public static ObjectSyncManager Instance = null;

		public Dictionary<int, ObjectSyncComponent> ObjectIDs = new Dictionary<int, ObjectSyncComponent>();

		public static int AUTOMATIC_ID = -1;

		public static ulong NO_OWNER = 0UL;

		public CSteamID steamID;

		public enum ObjectTypes
		{
			Pickupable,
			Item,
			PlayerVehicle,
			AIVehicle,
			GarageDoor,
			CarDoor,
			CarDoorF,
			Boot,
			BootF,
			PlayerHouse,
			Satsuma,
			PassengerSeat,
			Train,
			Transform,
			Mod
		}

		public enum SyncTypes
		{
			GenericSync,
			SetOwner,
			RemoveOwner,
			ForceSetOwner,
			PeriodicSync
		}
	}
}
