using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Objects;
using MWCO.Game.Places;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class GameWorld : IGameObjectCollector
	{
		public GamePlayer Player
		{
			get
			{
				return this.player;
			}
		}

		public float WorldTime
		{
			get
			{
				if (this.worldTimeFsm != null)
				{
					this.worldTimeCached = this.worldTimeFsm.Fsm.GetFsmFloat("Hours").Value;
				}
				return this.worldTimeCached;
			}
			set
			{
				while (value > 24f)
				{
					value -= 24f;
				}
				if (this.worldTimeCached != value)
				{
					PlayMakerFSM playMakerFSM = this.worldTimeFsm;
					if (playMakerFSM != null)
					{
						playMakerFSM.SendEvent(string.Format("MP_{0}", value));
					}
				}
				this.worldTimeCached = value;
			}
		}

		public int WorldDay
		{
			get
			{
				return PlayMakerGlobals.Instance.Variables.GetFsmInt("GlobalDay").Value;
			}
			set
			{
				PlayMakerGlobals.Instance.Variables.GetFsmInt("GlobalDay").Value = value;
			}
		}

		public string PlayerLastName
		{
			get
			{
				return this.lastnameTextMesh.text;
			}
			set
			{
				this.lastnameFSM.enabled = false;
				this.lastnameTextMesh.text = value;
			}
		}

		public void RefreshTime()
		{
			PlayMakerFSM playMakerFSM = this.worldTimeFsm;
			if (playMakerFSM == null)
			{
				return;
			}
			playMakerFSM.SendEvent(string.Format("MP_{0}", this.worldTimeCached));
		}

		public void SetupMailbox(GameObject mailboxGameObject)
		{
			this.lastnameTextMesh = mailboxGameObject.GetComponent<TextMesh>();
			this.lastnameFSM = mailboxGameObject.GetComponent<PlayMakerFSM>();
		}

		public GameWorld()
		{
			GameWorld.Instance = this;
			GameCallbacks.onPlayMakerObjectCreate = (GameCallbacks.OnPlayMakerObjectCreate)Delegate.Combine(GameCallbacks.onPlayMakerObjectCreate, new GameCallbacks.OnPlayMakerObjectCreate(this.HandleNewObject));
			GameCallbacks.onPlayMakerObjectDestroy = (GameCallbacks.OnPlayMakerObjectDestroy)Delegate.Combine(GameCallbacks.onPlayMakerObjectDestroy, new GameCallbacks.OnPlayMakerObjectDestroy(this.HandleObjectDestroy));
			this.gameObjectUsers.Add(this);
			this.gameObjectUsers.Add(this.doorsManager);
			this.gameObjectUsers.Add(this.gamePickupableDatabase);
			this.gameObjectUsers.Add(this.lightSwitchManager);
			this.gameObjectUsers.Add(this.gameWeatherManager);
			this.gameObjectUsers.Add(this.gameVehicleDatabase);
			this.gameObjectUsers.Add(this.gameHouseDatabase);
		}

		~GameWorld()
		{
			GameWorld.Instance = null;
		}

		public void Dispose()
		{
			GameCallbacks.onPlayMakerObjectCreate = (GameCallbacks.OnPlayMakerObjectCreate)Delegate.Remove(GameCallbacks.onPlayMakerObjectCreate, new GameCallbacks.OnPlayMakerObjectCreate(this.HandleNewObject));
			GameCallbacks.onPlayMakerObjectDestroy = (GameCallbacks.OnPlayMakerObjectDestroy)Delegate.Remove(GameCallbacks.onPlayMakerObjectDestroy, new GameCallbacks.OnPlayMakerObjectDestroy(this.HandleObjectDestroy));
			GameVehicleDatabase gameVehicleDatabase = this.gameVehicleDatabase;
			if (gameVehicleDatabase != null)
			{
				gameVehicleDatabase.Dispose();
			}
			ObjectSyncManager objectSyncManager = this.objectSyncManager;
			if (objectSyncManager != null)
			{
				objectSyncManager.Dispose();
			}
			this.gameObjectUsers.Clear();
			GameWorld.Instance = null;
		}

		public int WorldHash
		{
			get
			{
				return this.worldHash;
			}
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (gameObject.name == "SUN" && this.worldTimeFsm == null)
			{
				this.worldTimeFsm = Utils.GetPlaymakerScriptByName(gameObject, "Color");
				if (this.worldTimeFsm == null)
				{
					return;
				}
				if (!this.worldTimeFsm.Fsm.HasEvent("MP_REFRESH_WORLD_TIME"))
				{
					FsmEvent @event = this.worldTimeFsm.Fsm.GetEvent("MP_REFRESH_WORLD_TIME");
					PlayMakerUtils.AddNewGlobalTransition(this.worldTimeFsm, @event, "State 4");
					for (int i = 0; i < 24; i++)
					{
						string hour = (i + 1).ToString();
						EventHook.Add(this.worldTimeFsm, hour, delegate
						{
							if (NetManager.Instance.IsHost)
							{
								NetLocalPlayer.Instance.WriteFSMFloatMessage(77000, hour, (float)GameWorld.Instance.WorldDay);
							}
							return false;
						}, false, false);
					}
				}
				this.WorldTime = this.worldTimeCached;
				return;
			}
			else
			{
				if (Utils.IsGameObjectHierarchyMatching(gameObject, "mailbox_bottom_player/Name"))
				{
					this.SetupMailbox(gameObject);
					return;
				}
				if (gameObject.name == "TRAFFIC")
				{
					gameObject.AddComponent<TrafficManager>();
					return;
				}
				if (gameObject.name == "STORE_AREA")
				{
					new Shop(gameObject);
					return;
				}
				if (gameObject.name == "PERAPORTTI")
				{
					gameObject.AddComponent<Peraportti>();
					return;
				}
				if (gameObject.name == "FACTORY")
				{
					gameObject.AddComponent<Factory>();
					return;
				}
				if (gameObject.name.Contains("SORBET(190-200psi)"))
				{
					for (int j = 0; j < 4; j++)
					{
						gameObject.transform.GetChild(3).GetChild(j).gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoor, ObjectSyncManager.AUTOMATIC_ID);
					}
					gameObject.transform.FindChild("Hatch").GetChild(0).gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Boot, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name.Contains("GIFU(750/450psi)"))
				{
					gameObject.transform.FindChild("Cabin/DriverDoors/doorl").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoor, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Cabin/DriverDoors/doorr").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoor, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name.Contains("KEKMET(350-400psi)"))
				{
					gameObject.transform.FindChild("DriverDoors/doorl").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoor, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("DriverDoors/doorr").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoor, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name.Contains("BACHGLOTZ(1905kg)"))
				{
					gameObject.transform.FindChild("DriverDoors/door(leftx)/doors/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("DriverDoors/door(right)/doors/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Bootlid/Bootlid").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Boot, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name.Contains("MACHTWAGEN"))
				{
					MPController.Instance.machtwagen = gameObject;
					gameObject.transform.FindChild("Doors/DoorFront(leftx)/FrontL/PlayerColl/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Doors/DoorFront(right)/FrontR/PlayerColl/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Doors/DoorRear(leftx)/RearL/PlayerColl/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Doors/DoorRear(right)/RearR/PlayerColl/Handle").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.CarDoorF, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.FindChild("Hatch/Hatch").gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Boot, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name == "Fluid")
				{
					try
					{
						if (gameObject.GetComponent<EllipsoidParticleEmitter>() != null)
						{
							MPController.Instance.FluidObj = gameObject;
							if (NetManager.Instance.IsOnline)
							{
								foreach (NetPlayer netPlayer in NetManager.Instance.players)
								{
									if (netPlayer != null)
									{
										netPlayer.SetupPissParticles();
									}
								}
							}
							PlayMakerFSM component = gameObject.transform.parent.parent.GetComponent<PlayMakerFSM>();
							Logger.Debug("ADDING PISS " + component.FsmName);
							EventHook.Add(component, "Start urinate", delegate
							{
								NetLocalPlayer.Instance.pissRate = 0f;
								NetLocalPlayer.Instance.pissing = false;
								return false;
							}, false, false);
							EventHook.Add(component, "Full power", delegate
							{
								NetLocalPlayer.Instance.pissRate = 25f;
								NetLocalPlayer.Instance.pissing = true;
								return false;
							}, false, false);
							EventHook.Add(component, "Pump", delegate
							{
								NetLocalPlayer.Instance.pissRate = 10f;
								NetLocalPlayer.Instance.pissing = true;
								return false;
							}, false, false);
							EventHook.Add(component, "State 4", delegate
							{
								NetLocalPlayer.Instance.pissRate = 25f;
								NetLocalPlayer.Instance.pissing = true;
								return false;
							}, false, false);
						}
						return;
					}
					catch
					{
						Logger.Debug("Failed to initialize pissing");
						return;
					}
				}
				if (gameObject.name == "Cigarette")
				{
					try
					{
						if (MPController.Instance.CigaretteObj != null)
						{
							return;
						}
						MPController.Instance.CigaretteObj = gameObject;
						return;
					}
					catch
					{
						Logger.Debug("Failed to get cigarette obj");
						return;
					}
				}
				if (gameObject.name == "BreathSmoke" && gameObject.GetTopmostParent().name != "PLAYER" && gameObject.GetComponent<EllipsoidParticleEmitter>() != null)
				{
					try
					{
						if (MPController.Instance.BreathSmokeObj != null)
						{
							return;
						}
						MPController.Instance.BreathSmokeObj = gameObject;
						return;
					}
					catch
					{
						Logger.Debug("Failed to get breath smoke obj");
						return;
					}
				}
				if (gameObject.name == "Smoking" && gameObject.GetTopmostParent().name == "PLAYER")
				{
					try
					{
						if (MPController.Instance.SmokingObj != null)
						{
							return;
						}
						MPController.Instance.SmokingObj = gameObject;
						if (NetManager.Instance.IsOnline)
						{
							foreach (NetPlayer netPlayer2 in NetManager.Instance.players)
							{
								if (netPlayer2 != null)
								{
									netPlayer2.SetupCigarette();
								}
							}
						}
						PlayMakerFSM component2 = gameObject.GetComponent<PlayMakerFSM>();
						Logger.Debug("ADDING SMOKING " + component2.FsmName);
						EventHook.Add(component2, "Wait player", delegate
						{
							NetLocalPlayer.Instance.smoking = true;
							NetLocalPlayer.Instance.smokeMode = 1;
							return false;
						}, false, false);
						EventHook.Add(component2, "Inhale", delegate
						{
							NetLocalPlayer.Instance.smoking = true;
							NetLocalPlayer.Instance.smokeMode = 2;
							return false;
						}, false, false);
						EventHook.Add(component2, "Outhale", delegate
						{
							NetLocalPlayer.Instance.smoking = true;
							NetLocalPlayer.Instance.smokeMode = 1;
							return false;
						}, false, false);
						EventHook.Add(component2, "Put out", delegate
						{
							NetLocalPlayer.Instance.smoking = false;
							NetLocalPlayer.Instance.smokeMode = 0;
							return false;
						}, false, false);
						EventHook.Add(component2, "Put out 2", delegate
						{
							NetLocalPlayer.Instance.smoking = false;
							NetLocalPlayer.Instance.smokeMode = 0;
							return false;
						}, false, false);
						return;
					}
					catch
					{
						Logger.Debug("Failed to initialize smoking obj");
						return;
					}
				}
				if (gameObject.name == "GarageDoors")
				{
					gameObject.transform.GetChild(0).GetChild(1).GetChild(0)
						.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.GarageDoor, ObjectSyncManager.AUTOMATIC_ID);
					gameObject.transform.GetChild(1).GetChild(1).GetChild(0)
						.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.GarageDoor, ObjectSyncManager.AUTOMATIC_ID);
					return;
				}
				if (gameObject.name == "SleepTrigger")
				{
					PlayMakerFSM fsm = gameObject.GetComponent<PlayMakerFSM>();
					SleepRequest sr = gameObject.AddComponent<SleepRequest>();
					EventHook.Add(fsm, "Get positions", delegate
					{
						if (!sr.Allowed)
						{
							sr.GetPositions();
							return false;
						}
						fsm.enabled = true;
						sr.Allowed = false;
						return false;
					}, false, false);
					EventHook.Add(fsm, "State 1", () => false, false, false);
					EventHook.Add(fsm, "AnimateWake", delegate
					{
						if (!NetManager.Instance.IsHost)
						{
							NetLocalPlayer.Instance.WriteFSMBoolMessage(51171, "", true);
							return false;
						}
						return false;
					}, false, false);
				}
				return;
			}
		}

		private T[] AddFsmVariable<T>(T[] oldArray, T newVar)
		{
			return new List<T>(oldArray) { newVar }.ToArray();
		}

		public void DestroyObjects()
		{
			this.worldTimeFsm = null;
			this.lastnameTextMesh = null;
			this.lastnameFSM = null;
		}

		public void DestroyObject(GameObject gameObject)
		{
			if (this.worldTimeFsm != null && this.worldTimeFsm.gameObject == gameObject)
			{
				this.worldTimeFsm = null;
				return;
			}
			if (this.lastnameFSM != null && this.lastnameFSM.gameObject == gameObject)
			{
				this.lastnameFSM = null;
				this.lastnameTextMesh = null;
			}
		}

		public void HandleNewObject(GameObject instance, GameObject gameObject)
		{
			if (gameObject.GetTopmostParent().name == "Database")
			{
				Logger.Debug("Not syncing " + gameObject.name + " as it is in Database");
				return;
			}
			if (gameObject.name == "SAVEGAME")
			{
				MPController.Instance.DealWithSave(gameObject);
				return;
			}
			foreach (IGameObjectCollector gameObjectCollector in this.gameObjectUsers)
			{
				gameObjectCollector.CollectGameObject(gameObject, 0);
			}
		}

		public void HandleObjectDestroy(GameObject gameObject)
		{
			for (int i = this.gameObjectUsers.Count; i > 0; i--)
			{
				this.gameObjectUsers[i - 1].DestroyObject(gameObject);
			}
		}

		public void OnLoad()
		{
			foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (!this.worldHashGenerated)
				{
					Transform transform = gameObject.transform;
					while (transform != null)
					{
						this.worldHash ^= Utils.StringJenkinsHash(transform.name);
						transform = transform.parent;
					}
				}
				this.HandleNewObject(gameObject, gameObject);
			}
			Logger.Debug("World hash: " + this.worldHash.ToString());
			this.worldHashGenerated = true;
			if (GameCallbacks.onWorldLoad != null)
			{
				GameCallbacks.onWorldLoad();
			}
			GameVehicleDatabase.Instance.PopulateTriggers();
		}

		public void OnUnload()
		{
			for (int i = this.gameObjectUsers.Count; i > 0; i--)
			{
				this.gameObjectUsers[i - 1].DestroyObjects();
			}
			if (GameCallbacks.onWorldUnload != null)
			{
				GameCallbacks.onWorldUnload();
			}
			this.player = null;
		}

		public void SetPlayer(GameObject obj)
		{
			this.player = new GamePlayer(obj);
			if (GameCallbacks.onLocalPlayerCreated != null)
			{
				GameCallbacks.onLocalPlayerCreated();
			}
			if (obj != null)
			{
				Peraportti.Instance.LoadFuelNozzles(obj.transform);
			}
		}

		public GameObject SpawnPickupable(int prefabId, Vector3 position, Quaternion rotation, int objectID, string parentTriggerName)
		{
			GamePickupableDatabase.PrefabDesc pickupablePrefab = this.gamePickupableDatabase.GetPickupablePrefab(prefabId);
			if (pickupablePrefab == null)
			{
				Chat.Instance.AddMessage(string.Format("Avoided Major Crash: Unable to find pickupable prefab {0}", prefabId), MessageSeverity.Error);
				return null;
			}
			return pickupablePrefab.Spawn(position, rotation, parentTriggerName);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static GameWorld()
		{
		}

		public static GameWorld Instance = null;

		private GameDoorsManager doorsManager = new GameDoorsManager();

		public GamePickupableDatabase gamePickupableDatabase = new GamePickupableDatabase();

		private PlayMakerFSM worldTimeFsm;

		private LightSwitchManager lightSwitchManager = new LightSwitchManager();

		private GameWeatherManager gameWeatherManager = new GameWeatherManager();

		private GameVehicleDatabase gameVehicleDatabase = new GameVehicleDatabase();

		private GameHouseDatabase gameHouseDatabase = new GameHouseDatabase();

		private ObjectSyncManager objectSyncManager = new ObjectSyncManager();

		private EventHook eventHook = new EventHook();

		private GamePlayer player;

		private const string REFRESH_WORLD_TIME_EVENT = "MP_REFRESH_WORLD_TIME";

		private float worldTimeCached;

		private TextMesh lastnameTextMesh;

		private PlayMakerFSM lastnameFSM;

		private List<IGameObjectCollector> gameObjectUsers = new List<IGameObjectCollector>(7);

		private int worldHash;

		private bool worldHashGenerated;

		private static readonly string[] vehicleGoNames = new string[] { "JONNEZ ES(Clone)", "HAYOSIKO(1500kg, 250)", "SATSUMA(557kg, 248)", "RCO_RUSCKO12(270)", "KEKMET(350-400psi)", "FLATBED", "FERNDALE(1630kg)", "GIFU(750/450psi)" };

		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			// Note: this type is marked as 'beforefieldinit'.
			static <>c()
			{
			}

			public <>c()
			{
			}

			internal bool <CollectGameObject>b__36_1()
			{
				NetLocalPlayer.Instance.pissRate = 0f;
				NetLocalPlayer.Instance.pissing = false;
				return false;
			}

			internal bool <CollectGameObject>b__36_2()
			{
				NetLocalPlayer.Instance.pissRate = 25f;
				NetLocalPlayer.Instance.pissing = true;
				return false;
			}

			internal bool <CollectGameObject>b__36_3()
			{
				NetLocalPlayer.Instance.pissRate = 10f;
				NetLocalPlayer.Instance.pissing = true;
				return false;
			}

			internal bool <CollectGameObject>b__36_4()
			{
				NetLocalPlayer.Instance.pissRate = 25f;
				NetLocalPlayer.Instance.pissing = true;
				return false;
			}

			internal bool <CollectGameObject>b__36_5()
			{
				NetLocalPlayer.Instance.smoking = true;
				NetLocalPlayer.Instance.smokeMode = 1;
				return false;
			}

			internal bool <CollectGameObject>b__36_6()
			{
				NetLocalPlayer.Instance.smoking = true;
				NetLocalPlayer.Instance.smokeMode = 2;
				return false;
			}

			internal bool <CollectGameObject>b__36_7()
			{
				NetLocalPlayer.Instance.smoking = true;
				NetLocalPlayer.Instance.smokeMode = 1;
				return false;
			}

			internal bool <CollectGameObject>b__36_8()
			{
				NetLocalPlayer.Instance.smoking = false;
				NetLocalPlayer.Instance.smokeMode = 0;
				return false;
			}

			internal bool <CollectGameObject>b__36_9()
			{
				NetLocalPlayer.Instance.smoking = false;
				NetLocalPlayer.Instance.smokeMode = 0;
				return false;
			}

			internal bool <CollectGameObject>b__36_11()
			{
				return false;
			}

			internal bool <CollectGameObject>b__36_12()
			{
				if (!NetManager.Instance.IsHost)
				{
					NetLocalPlayer.Instance.WriteFSMBoolMessage(51171, "", true);
					return false;
				}
				return false;
			}

			public static readonly GameWorld.<>c <>9 = new GameWorld.<>c();

			public static Func<bool> <>9__36_1;

			public static Func<bool> <>9__36_2;

			public static Func<bool> <>9__36_3;

			public static Func<bool> <>9__36_4;

			public static Func<bool> <>9__36_5;

			public static Func<bool> <>9__36_6;

			public static Func<bool> <>9__36_7;

			public static Func<bool> <>9__36_8;

			public static Func<bool> <>9__36_9;

			public static Func<bool> <>9__36_11;

			public static Func<bool> <>9__36_12;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass36_0
		{
			public <>c__DisplayClass36_0()
			{
			}

			internal bool <CollectGameObject>b__0()
			{
				if (NetManager.Instance.IsHost)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(77000, this.hour, (float)GameWorld.Instance.WorldDay);
				}
				return false;
			}

			public string hour;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass36_1
		{
			public <>c__DisplayClass36_1()
			{
			}

			internal bool <CollectGameObject>b__10()
			{
				if (!this.sr.Allowed)
				{
					this.sr.GetPositions();
					return false;
				}
				this.fsm.enabled = true;
				this.sr.Allowed = false;
				return false;
			}

			public SleepRequest sr;

			public PlayMakerFSM fsm;
		}
	}
}
