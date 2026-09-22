using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class GameVehicleDatabase : IGameObjectCollector
	{
		public GameVehicleDatabase()
		{
			GameVehicleDatabase.Instance = this;
		}

		~GameVehicleDatabase()
		{
			GameVehicleDatabase.Instance = null;
		}

		public void DestroyObject(GameObject gameObject)
		{
			this.vehiclesAI.Clear();
		}

		public void DestroyObjects()
		{
			this.vehiclesAI.Clear();
		}

		public void PopulateTriggers()
		{
			foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				foreach (PlayMakerFSM playMakerFSM in gameObject.GetComponents<PlayMakerFSM>())
				{
					if (playMakerFSM.FsmName == "Data")
					{
						this.dataGameObject.Add(playMakerFSM.gameObject);
						if (playMakerFSM.gameObject.name.StartsWith("Wiring"))
						{
							this.wiringDataFsms.Add(playMakerFSM);
						}
						if (playMakerFSM.gameObject.name == "AlternatorBelt")
						{
							MPController.Instance.AlternatorBeltData = playMakerFSM;
						}
					}
					else if (playMakerFSM.FsmName == "Scale")
					{
						this.fluidFsms.Add(playMakerFSM);
					}
				}
				if (gameObject.name.StartsWith("bolts_shock_f"))
				{
					this.extraParts.Add(gameObject.gameObject);
				}
			}
		}

		public void Dispose()
		{
			this.vehiclesAI.Clear();
			this.vehiclesPlayer.Clear();
			this.satsumaSyncables.Clear();
			this.wiringDataFsms.Clear();
			this.fluidFsms.Clear();
			this.dataGameObject.Clear();
			this.triggerGameObjects.Clear();
			this.carParts.Clear();
			this.extraParts.Clear();
			GameVehicleDatabase.Instance = null;
		}

		public bool IsRivett(GameObject go)
		{
			GameObject topmostParent = go.GetTopmostParent();
			string text = ((topmostParent != null) ? topmostParent.gameObject.name : null);
			if (go.transform.parent == null)
			{
				text = go.name;
			}
			bool flag = go.name.Contains("VINX") || (!go.name.StartsWith("Order") && (go.name == "CARPARTS" || go.name.StartsWith("CARPARTS") || go.name.Contains("CARPARTS") || text == "CARPARTS" || (MPController.Instance.boat == null && (text.StartsWith("SPRKPLUG") || text.ToLower().StartsWith("sparkplug") || text.StartsWith("bolt") || text.ToLower().Contains("light bulb") || text.ToLower().StartsWith("lightbulb") || text.ToLower().StartsWith("alternatorbelt") || text.ToLower().Contains("alternator belt") || (text.ToLower().StartsWith("sparkplug") && !text.ToLower().Contains("box")) || (text.ToLower().Contains("spark plug") && !text.ToLower().Contains("box")) || text.ToLower().StartsWith("oilfilter") || text.ToLower().Contains("oil filter") || text.ToLower().StartsWith("battery") || text.ToLower().Contains("coolant") || text.ToLower().StartsWith("motoroil") || text.ToLower().StartsWith("brakefluid") || text.ToLower().StartsWith("macaron box") || text.ToLower().StartsWith("pizza")) && !text.StartsWith("trigger"))));
			if (flag)
			{
				foreach (PlayMakerFSM playMakerFSM in go.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if ((playMakerFSM.FsmName == "Use" || playMakerFSM.FsmName == "Knob") && !playMakerFSM.gameObject.name.StartsWith("wheel_") && !this.satsumaSyncables.Contains(playMakerFSM))
					{
						this.satsumaSyncables.Add(playMakerFSM);
						this.HookSyncable(playMakerFSM);
					}
				}
			}
			return flag;
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (!gameObject.name.StartsWith("VINP") && !gameObject.name.StartsWith("TriggerWheel") && (!gameObject.name.StartsWith("Fuse") || gameObject.name.Length != 5) && !gameObject.name.StartsWith("FuseTrigger"))
			{
				if ((gameObject.name == "Colliders" && gameObject.transform.FindChild("CarCollider") != null) || (gameObject.name == "Colliders" && gameObject.transform.FindChild("Coll") != null))
				{
					if (this.vehiclesPlayer.ContainsValue(gameObject))
					{
						Logger.Debug("Duplicate Player vehicle prefab '" + gameObject.name + "' rejected");
					}
					else
					{
						this.vehiclesPlayer.Add(this.vehiclesPlayer.Count + 1, gameObject);
						Logger.Debug(string.Format("Registered Player vehicle prefab '{0}' (Player Vehicle ID: {1})", gameObject.transform.parent.name, this.vehiclesPlayer.Count));
						if (gameObject.transform.FindChild("CarCollider") == null)
						{
							GameObject gameObject2 = gameObject.transform.FindChild("Coll").gameObject;
							gameObject2.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerVehicle, ObjectSyncManager.AUTOMATIC_ID);
						}
						else if (gameObject.transform.parent.name == "FLATBED")
						{
							gameObject.transform.parent.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerVehicle, ObjectSyncManager.AUTOMATIC_ID);
						}
						else
						{
							GameObject gameObject2;
							if (gameObject.GetTopmostParent().name == "JOBS")
							{
								gameObject2 = gameObject.transform.parent.gameObject;
							}
							else
							{
								gameObject2 = gameObject.GetTopmostParent();
							}
							gameObject2.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerVehicle, (assignedID == 0) ? ObjectSyncManager.AUTOMATIC_ID : assignedID);
						}
					}
				}
				else if (gameObject.name == "Colliders" && gameObject.transform.FindChild("coll11") != null)
				{
					if (this.vehiclesPlayer.ContainsValue(gameObject))
					{
						Logger.Debug("Duplicate Player vehicle prefab '" + gameObject.name + "' rejected");
					}
					else
					{
						this.vehiclesPlayer.Add(this.vehiclesPlayer.Count + 1, gameObject);
						Logger.Debug(string.Format("Registered Player vehicle prefab '{0}' (Player Vehicle ID: {1})", gameObject.transform.parent.name, this.vehiclesPlayer.Count));
						gameObject.transform.FindChild("coll11").gameObject.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerVehicle, ObjectSyncManager.AUTOMATIC_ID);
					}
				}
				else if (gameObject.transform.FindChild("CarColliderAI") != null)
				{
					if (this.vehiclesAI.ContainsValue(gameObject) || gameObject.name == "CARAVAN")
					{
						Logger.Debug("Duplicate AI vehicle prefab '" + gameObject.name + "' rejected");
					}
					else
					{
						if (this.vehiclesHighway == null && gameObject.transform.parent != null && gameObject.transform.parent.name == "VehiclesHighway")
						{
							this.vehiclesHighway = gameObject.transform.parent.gameObject;
						}
						this.vehiclesAI.Add(this.vehiclesAI.Count + 1, gameObject);
						Logger.Debug(string.Format("Registered AI vehicle prefab '{0}' (AI Vehicle ID: {1})", gameObject.name, this.vehiclesAI.Count));
						gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.AIVehicle, ObjectSyncManager.AUTOMATIC_ID);
					}
				}
				else
				{
					if (!(gameObject.name == "TRAIN") || !(gameObject.GetComponent<PlayMakerFSM>() != null))
					{
						return;
					}
					if (gameObject.GetComponent<ObjectSyncComponent>() != null)
					{
						Logger.Debug("Duplicate TRAIN prefab '" + gameObject.name + "' rejected");
					}
					else
					{
						Logger.Debug("Registered TRAIN prefab '" + gameObject.name + "'");
						gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Train, ObjectSyncManager.AUTOMATIC_ID);
					}
				}
				string[] array = new string[6];
				array[0] = "Handling new Vehicle Object ";
				array[1] = gameObject.name;
				array[2] = ", Parent = ";
				int num = 3;
				Transform parent = gameObject.transform.parent;
				array[num] = ((parent != null) ? parent.name : null);
				array[4] = ", Topmost = ";
				array[5] = gameObject.GetTopmostParent().name;
				Logger.Debug(string.Concat(array));
				return;
			}
			if (gameObject == null)
			{
				return;
			}
			if (gameObject.GetComponent<Trigger>() != null)
			{
				return;
			}
			Trigger trigger = gameObject.gameObject.AddComponent<Trigger>();
			trigger.TriggerID = this.triggerGameObjects.Count;
			this.triggerGameObjects.Add(trigger);
		}

		public void HookSyncable(PlayMakerFSM fsm)
		{
			List<PlayMakerFSM> list = this.satsumaSyncables;
			if (fsm.FsmName == "Use")
			{
				if (fsm.name.StartsWith("Door("))
				{
					EventHook.AddWithSync(fsm, "Open door", null, false);
					EventHook.AddWithSync(fsm, "Open door 2", null, false);
					EventHook.AddWithSync(fsm, "Open door 3", null, false);
					return;
				}
				if (fsm.name.StartsWith("Hood("))
				{
					EventHook.AddWithSync(fsm, "Mouse over", null, false);
					EventHook.AddWithSync(fsm, "Mouse over 2", null, false);
					EventHook.AddWithSync(fsm, "State 2", null, false);
					EventHook.AddWithSync(fsm, "Open hood", null, false);
					return;
				}
				if (fsm.name.StartsWith("Bootlid("))
				{
					EventHook.AddWithSync(fsm, "Open hood", null, false);
					EventHook.AddWithSync(fsm, "Drop", null, false);
					EventHook.AddWithSync(fsm, "Mouse over 2", null, false);
					return;
				}
				if (fsm.name == "coll")
				{
					Transform parent = fsm.transform.parent;
					string text;
					if (parent == null)
					{
						text = null;
					}
					else
					{
						Transform parent2 = parent.parent;
						text = ((parent2 != null) ? parent2.name : null);
					}
					if (text == "Glovebox")
					{
						EventHook.AddWithSync(fsm, "Open door", null, false);
						EventHook.AddWithSync(fsm, "Close door", null, false);
					}
				}
			}
		}

		public static GameVehicleDatabase Instance;

		public PlayMakerFSM satsumaFuelTankFsm;

		public Dictionary<int, GameObject> vehiclesAI = new Dictionary<int, GameObject>();

		public List<PlayMakerFSM> satsumaSyncables = new List<PlayMakerFSM>();

		private List<PlayMakerFSM> wiringDataFsms = new List<PlayMakerFSM>();

		public List<PlayMakerFSM> fluidFsms = new List<PlayMakerFSM>();

		public List<GameObject> dataGameObject = new List<GameObject>();

		public List<Trigger> triggerGameObjects = new List<Trigger>();

		public List<Bolt> bolts = new List<Bolt>();

		public List<GameObject> carParts = new List<GameObject>();

		public List<GameObject> extraParts = new List<GameObject>();

		public GameObject motorBolts;

		public GameObject vehiclesHighway;

		public PlayMakerFSM RivettAssembly;

		public Dictionary<int, GameObject> vehiclesPlayer = new Dictionary<int, GameObject>();

		public PlayMakerFSM chokeFsm;

		public PlayMakerFSM chokeFsm2;

		public PlayMakerFSM radioV;

		public PlayMakerFSM radioT;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass27_0
		{
			public <>c__DisplayClass27_0()
			{
			}

			internal bool <HookSyncable>b__0()
			{
				FixedJoint[] components = this.fsm.gameObject.GetComponents<FixedJoint>();
				for (int i = 0; i < components.Length; i++)
				{
					Object.Destroy(components[i]);
				}
				return false;
			}

			internal bool <HookSyncable>b__1()
			{
				FixedJoint[] components = this.fsm.gameObject.GetComponents<FixedJoint>();
				for (int i = 0; i < components.Length; i++)
				{
					Object.Destroy(components[i]);
				}
				return false;
			}

			public PlayMakerFSM fsm;
		}
	}
}
