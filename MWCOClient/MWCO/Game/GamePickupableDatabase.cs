using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class GamePickupableDatabase : IGameObjectCollector
	{
		public static GamePickupableDatabase Instance
		{
			get
			{
				return GamePickupableDatabase.instance;
			}
		}

		public Dictionary<int, GameObject> Pickupables
		{
			get
			{
				return this.pickupables;
			}
		}

		public GamePickupableDatabase()
		{
			GamePickupableDatabase.instance = this;
			GameCallbacks.onPlayMakerObjectCreate = (GameCallbacks.OnPlayMakerObjectCreate)Delegate.Combine(GameCallbacks.onPlayMakerObjectCreate, new GameCallbacks.OnPlayMakerObjectCreate(delegate(GameObject instance, GameObject prefab)
			{
				GamePickupableDatabase.PrefabDesc prefabDesc = this.GetPrefabDesc(prefab);
				if (prefabDesc != null)
				{
					PickupableMetaDataComponent pickupableMetaDataComponent = instance.GetComponent<PickupableMetaDataComponent>();
					if (pickupableMetaDataComponent == null)
					{
						Logger.Debug(string.Format("Didnt have metadata creating... {0} ({1})", instance.name, instance.GetInstanceID()));
						pickupableMetaDataComponent = instance.AddComponent<PickupableMetaDataComponent>();
					}
					pickupableMetaDataComponent.prefabId = prefabDesc.id;
				}
			}));
		}

		~GamePickupableDatabase()
		{
			GamePickupableDatabase.instance = null;
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (gameObject.name == "ThrowBody" && gameObject.transform.parent.name == "Pivot")
			{
				if (MPController.Instance.KnockOutObj == null)
				{
					GameObject clone = Object.Instantiate<GameObject>(gameObject);
					MPController.Instance.KnockOutObj = clone;
					MPController.Instance.KnockOutObj.SetActive(false);
					EventHook.Add(clone.transform.GetChild(0).GetComponent<PlayMakerFSM>(), "State 3", delegate
					{
						clone.SetActive(false);
						return false;
					}, false, false);
				}
			}
			else if (gameObject.name == "Walkers")
			{
				for (int i = 0; i < gameObject.transform.childCount; i++)
				{
					gameObject.transform.GetChild(i).gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Transform, ObjectSyncManager.AUTOMATIC_ID);
					foreach (PlayMakerFSM playMakerFSM in gameObject.transform.GetChild(i).GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM.gameObject.name == "HumanTriggerCrime")
						{
							EventHook.AddWithSync(playMakerFSM, "Accident", null, false);
							break;
						}
					}
				}
			}
			else if (gameObject.name == "RagDoll" && gameObject.GetComponent<PlayMakerFSM>() != null)
			{
				EventHook.SyncAllEvents(gameObject.GetComponent<PlayMakerFSM>(), null);
			}
			if (!GamePickupableDatabase.IsPickupable(gameObject) && !gameObject.name.StartsWith("log") && !gameObject.name.StartsWith("Hose") && !gameObject.name.StartsWith("firewood") && !gameObject.name.StartsWith("warning triangle") && !gameObject.name.StartsWith("haybale"))
			{
				return;
			}
			if (gameObject.name.StartsWith("Hose"))
			{
				if (!(gameObject.name == "Hose(xxxxx)") || gameObject.transform.childCount <= 0)
				{
					return;
				}
				if (!gameObject.transform.GetChild(0).name.StartsWith("Suction"))
				{
					return;
				}
			}
			else if (gameObject.name.StartsWith("haybale"))
			{
				Logger.Debug("Registering " + gameObject.name + " as a pickupable");
			}
			int count = this.prefabs.Count;
			PickupableMetaDataComponent pickupableMetaDataComponent = gameObject.GetComponent<PickupableMetaDataComponent>();
			if (pickupableMetaDataComponent == null)
			{
				pickupableMetaDataComponent = gameObject.AddComponent<PickupableMetaDataComponent>();
			}
			pickupableMetaDataComponent.prefabId = count;
			GamePickupableDatabase.PrefabDesc prefabDesc = new GamePickupableDatabase.PrefabDesc();
			prefabDesc.gameObject = gameObject;
			prefabDesc.id = count;
			bool activeSelf = prefabDesc.gameObject.activeSelf;
			if (!activeSelf)
			{
				prefabDesc.gameObject.SetActive(true);
			}
			if (prefabDesc.gameObject.GetComponent<ObjectSyncComponent>() == null)
			{
				prefabDesc.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
			}
			else
			{
				Object.Destroy(prefabDesc.gameObject.GetComponent<ObjectSyncComponent>());
				prefabDesc.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
			}
			if (!activeSelf)
			{
				prefabDesc.gameObject.SetActive(false);
			}
			this.prefabs.Add(prefabDesc);
		}

		public void DestroyObjects()
		{
			this.prefabs.Clear();
		}

		public void DestroyObject(GameObject gameObject)
		{
			if (!GamePickupableDatabase.IsPickupable(gameObject))
			{
				return;
			}
			GamePickupableDatabase.PrefabDesc prefabDesc = this.GetPrefabDesc(gameObject);
			if (prefabDesc != null)
			{
				Logger.Debug("Deleting prefab descriptor - " + gameObject.name + ".");
				this.prefabs[prefabDesc.id] = null;
			}
		}

		public void RemoveObject(GameObject go)
		{
			GamePickupableDatabase.PrefabDesc prefabDesc = this.GetPrefabDesc(go);
			if (prefabDesc != null)
			{
				Logger.Debug("Deleting prefab descriptor - " + go.name + ".");
				this.prefabs[prefabDesc.id] = null;
			}
		}

		public GamePickupableDatabase.PrefabDesc GetPickupablePrefab(int prefabId)
		{
			if (prefabId < this.prefabs.Count)
			{
				return this.prefabs[prefabId];
			}
			return null;
		}

		public GamePickupableDatabase.PrefabDesc GetPrefabDesc(GameObject prefab)
		{
			foreach (GamePickupableDatabase.PrefabDesc prefabDesc in this.prefabs)
			{
				if (prefabDesc != null && prefabDesc.gameObject == prefab)
				{
					return prefabDesc;
				}
			}
			return null;
		}

		public static bool IsPickupable(GameObject gameObject)
		{
			if (gameObject.name.StartsWith("well cover"))
			{
				return true;
			}
			if (!gameObject.CompareTag("PART") && !gameObject.name.Contains("log") && !gameObject.name.Contains("firewood") && !gameObject.CompareTag("ITEM") && !gameObject.name.Contains("xxx") && !gameObject.name.StartsWith("rearlight") && !gameObject.name.StartsWith("floor jack") && !gameObject.name.StartsWith("motor hoist") && !gameObject.name.StartsWith("warning triangle") && !gameObject.name.StartsWith("register plate"))
			{
				return false;
			}
			if (GameVehicleDatabase.Instance.IsRivett(gameObject) || gameObject.name.StartsWith("warning triangle") || gameObject.name.StartsWith("register plate"))
			{
				if (!(gameObject.GetComponent<CarPart>() == null) || !(gameObject.GetComponent<PlayMakerFSM>() != null))
				{
					return true;
				}
				gameObject.AddComponent<CarPart>().Initialize();
				if (!GameVehicleDatabase.Instance.carParts.Contains(gameObject))
				{
					GameVehicleDatabase.Instance.carParts.Add(gameObject);
				}
			}
			return (!gameObject.name.Contains("xxx") || gameObject.name.Contains("envelope") || gameObject.name.Contains("windshield") || gameObject.name.StartsWith("warning triangle")) && gameObject.GetComponent<Rigidbody>();
		}

		[CompilerGenerated]
		private void <.ctor>b__6_0(GameObject instance, GameObject prefab)
		{
			GamePickupableDatabase.PrefabDesc prefabDesc = this.GetPrefabDesc(prefab);
			if (prefabDesc != null)
			{
				PickupableMetaDataComponent pickupableMetaDataComponent = instance.GetComponent<PickupableMetaDataComponent>();
				if (pickupableMetaDataComponent == null)
				{
					Logger.Debug(string.Format("Didnt have metadata creating... {0} ({1})", instance.name, instance.GetInstanceID()));
					pickupableMetaDataComponent = instance.AddComponent<PickupableMetaDataComponent>();
				}
				pickupableMetaDataComponent.prefabId = prefabDesc.id;
			}
		}

		private static GamePickupableDatabase instance;

		private Dictionary<int, GameObject> pickupables = new Dictionary<int, GameObject>();

		public List<GamePickupableDatabase.PrefabDesc> prefabs = new List<GamePickupableDatabase.PrefabDesc>();

		public class PrefabDesc
		{
			public GameObject Spawn(Vector3 position, Quaternion rotation, string triggerParentName)
			{
				try
				{
					if (this.gameObject.name.StartsWith("JONNEZ ES"))
					{
						return null;
					}
				}
				catch (Exception)
				{
				}
				GameObject gameObject = (GameObject)Object.Instantiate(this.gameObject, position, rotation);
				gameObject.SetActive(true);
				gameObject.transform.SetParent(null);
				PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(gameObject, "Use");
				if (playmakerScriptByName != null)
				{
					FsmState state = playmakerScriptByName.Fsm.GetState("Load");
					if (state != null)
					{
						PlayMakerUtils.AddNewAction(state, new SendEvent
						{
							eventTarget = new FsmEventTarget(),
							eventTarget = 
							{
								excludeSelf = false,
								target = 0
							},
							sendEvent = playmakerScriptByName.Fsm.GetEvent("FINISHED")
						}, false);
						Logger.Debug("Installed skip load hack for prefab " + gameObject.name);
					}
					else
					{
						Logger.Debug("Failed to find state on " + gameObject.name);
					}
				}
				return gameObject;
			}

			public PrefabDesc()
			{
			}

			public int id;

			public GameObject gameObject;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public <>c__DisplayClass10_0()
			{
			}

			internal bool <CollectGameObject>b__0()
			{
				this.clone.SetActive(false);
				return false;
			}

			public GameObject clone;
		}
	}
}
