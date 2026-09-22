using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Places;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class GameHouseDatabase : IGameObjectCollector
	{
		public GameHouseDatabase()
		{
			GameHouseDatabase.Instance = this;
		}

		~GameHouseDatabase()
		{
			GameHouseDatabase.Instance = null;
		}

		public void DestroyObject(GameObject gameObject)
		{
		}

		public void DestroyObjects()
		{
		}

		public void SetChannel1Song(int index)
		{
			AudioClip audioClip = this.channel1ArrayList.arrayList[index] as AudioClip;
			this.channel1Fsm.Fsm.GetFsmObject("Audio").Value = audioClip;
			this.channel1Fsm.SendEvent("MP_Play radio 2");
		}

		public void ReadPhoneKey(int phoneIndex, string inputKey)
		{
			this.pReceiver = true;
			this.keypadPhones[phoneIndex].Fsm.GetFsmString("Key").Value = inputKey;
			this.keypadPhones[phoneIndex].SendEvent("MP_Send key");
		}

		public void ReadRequiredRings(int phoneIndex, int rings)
		{
			this.keypadPhonesCalling[phoneIndex].Fsm.GetFsmInt("RequiredRings").Value = rings;
		}

		public void ReadPhoneFlip(int phoneIndex, bool flip)
		{
			this.pReceiver2 = true;
			this.useHandles[phoneIndex].SendEvent("MP_Flip");
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (gameObject.name == "WaterWell")
			{
				PlayMakerFSM component = gameObject.transform.GetChild(2).GetChild(1).GetComponent<PlayMakerFSM>();
				EventHook.AddWithSync(component, "Wait player", null, false);
				EventHook.AddWithSync(component, "Move lever", null, false);
				return;
			}
			if (gameObject.name == "RadioChannels")
			{
				PlayMakerFSM fsm2 = gameObject.transform.FindChild("Channel1").GetComponent<PlayMakerFSM>();
				this.channel1Fsm = fsm2;
				EventHook.Add(fsm2, "Play radio 2", delegate
				{
					if (NetManager.Instance.IsHost)
					{
						int num = this.channel1ArrayList.arrayList.IndexOf(fsm2.Fsm.GetFsmObject("Audio").Value);
						NetLocalPlayer.Instance.WriteFSMFloatMessage(53001, "Channel1", (float)num);
					}
					else if (fsm2.Fsm.States[1].Actions[0].Enabled)
					{
						Logger.Debug("IMNOTAHOST");
						for (int k = 1; k < fsm2.Fsm.States.Length; k++)
						{
							FsmStateAction[] actions = fsm2.Fsm.States[k].Actions;
							for (int l = 0; l < actions.Length; l++)
							{
								actions[l].Enabled = false;
							}
						}
					}
					return false;
				}, false, false);
			}
			else if (gameObject.name == "PaikallisradioALL")
			{
				Component component2 = gameObject.GetComponent<PlayMakerFSM>();
				gameObject.SetActive(true);
				foreach (PlayMakerArrayListProxy playMakerArrayListProxy in component2.GetComponents<PlayMakerArrayListProxy>())
				{
					if (playMakerArrayListProxy.referenceName == "Music")
					{
						this.channel1ArrayList = playMakerArrayListProxy;
						break;
					}
				}
			}
			else if (gameObject.name.StartsWith("KeypadPhone"))
			{
				PlayMakerFSM[] array = gameObject.GetComponents<PlayMakerFSM>();
				for (int i = 0; i < array.Length; i++)
				{
					PlayMakerFSM fsm3 = array[i];
					if (fsm3.FsmName == "Input")
					{
						int index2 = this.keypadPhones.Count;
						this.keypadPhones.Add(fsm3);
						EventHook.Add(fsm3, "Send key", delegate
						{
							if (this.pReceiver)
							{
								this.pReceiver = false;
								return false;
							}
							int num2;
							if (int.TryParse(fsm3.Fsm.GetFsmString("Key").Value, out num2))
							{
								NetLocalPlayer.Instance.WriteFSMFloatMessage(54000, index2.ToString(), (float)num2);
							}
							return false;
						}, false, false);
						PlayMakerFSM[] components2 = fsm3.gameObject.GetComponents<PlayMakerFSM>();
						for (int j = 0; j < components2.Length; j++)
						{
							PlayMakerFSM sideFsm = components2[j];
							if (sideFsm.FsmName == "Calling")
							{
								this.keypadPhonesCalling.Add(sideFsm);
								EventHook.Add(sideFsm, "Ring", delegate
								{
									if (NetManager.Instance.IsHost)
									{
										NetLocalPlayer.Instance.WriteFSMFloatMessage(54100, index2.ToString(), (float)sideFsm.Fsm.GetFsmInt("RequiredRings").Value);
									}
									return false;
								}, false, false);
								break;
							}
						}
					}
				}
			}
			else if (gameObject.name == "UseHandle")
			{
				PlayMakerFSM fsm = gameObject.GetComponent<PlayMakerFSM>();
				if (fsm.name == "UseHandle")
				{
					int index = this.useHandles.Count;
					this.useHandles.Add(fsm);
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.Add(fsm, "Flip", delegate
					{
						if (this.pReceiver2)
						{
							this.pReceiver2 = false;
							if (!fsm.Fsm.GetFsmBool("Use").Value)
							{
								fsm.Fsm.GetState("Pick phone").Actions[5].Enabled = false;
								fsm.Fsm.GetState("Close phone").Actions[5].Enabled = false;
								fsm.GetComponent<CapsuleCollider>().enabled = false;
								fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = false;
							}
							else
							{
								fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = true;
								fsm.GetComponent<CapsuleCollider>().enabled = true;
							}
							return false;
						}
						fsm.Fsm.GetState("Pick phone").Actions[5].Enabled = true;
						fsm.Fsm.GetState("Close phone").Actions[5].Enabled = true;
						fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = true;
						fsm.GetComponent<CapsuleCollider>().enabled = true;
						NetLocalPlayer.Instance.WriteFSMBoolMessage(54500, index.ToString(), !fsm.Fsm.GetFsmBool("Use").Value);
						return false;
					}, false, false);
				}
			}
			else if (gameObject.name == "WoodCaller1")
			{
				foreach (PlayMakerFSM playMakerFSM in gameObject.GetComponents<PlayMakerFSM>())
				{
					if (playMakerFSM.FsmName == "Animations")
					{
						EventHook.AddWithSync(playMakerFSM, "Explain car", null, false);
						EventHook.AddWithSync(playMakerFSM, "Give keys", null, false);
						EventHook.AddWithSync(playMakerFSM, "Money", null, false);
						EventHook.AddWithSync(playMakerFSM, "Wait", null, false);
					}
				}
			}
			else if (gameObject.name == "PhoneNumbers")
			{
				foreach (PlayMakerFSM playMakerFSM2 in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM2.FsmName == "Data")
					{
						EventHook.AddWithSync(playMakerFSM2, "Idle", null, false);
						EventHook.AddWithSync(playMakerFSM2, "State 1", null, false);
						if (playMakerFSM2.name == "08712112")
						{
							EventHook.AddWithSync(playMakerFSM2, "State 2", null, false);
							EventHook.AddWithSync(playMakerFSM2, "State 3", null, false);
						}
					}
				}
			}
			else if (gameObject.name == "Mesh")
			{
				PlayMakerFSM component3 = gameObject.GetComponent<PlayMakerFSM>();
				if (component3 != null)
				{
					EventHook.AddWithSync(component3, "Wait player", null, false);
					EventHook.AddWithSync(component3, "Check position", null, false);
				}
			}
			else if (gameObject.name.StartsWith("HouseWood"))
			{
				gameObject.AddComponent<LODManager>().Initialize(gameObject.GetComponent<PlayMakerFSM>(), -1f, false, "LOD", "Object");
			}
			else if (gameObject.name == "Tutorial")
			{
				PlayMakerFSM component4 = gameObject.GetComponent<PlayMakerFSM>();
				if (component4 != null)
				{
					EventHook.AddWithSync(component4, "Get out", null, false);
				}
			}
			else if (gameObject.name == "TAXIJOB")
			{
				PlayMakerFSM component5 = gameObject.GetComponent<PlayMakerFSM>();
				if (component5 != null)
				{
					EventHook.SyncAllEvents(component5, null);
				}
			}
			else if (gameObject.name == "GetACup")
			{
				PlayMakerFSM component6 = gameObject.GetComponent<PlayMakerFSM>();
				if (component6 != null)
				{
					EventHook.AddWithSync(component6, "Wait player", null, false);
					EventHook.AddWithSync(component6, "State 1", null, false);
					EventHook.AddWithSync(component6, "State 2", null, false);
				}
			}
			else if (gameObject.name == "CoffeeButton")
			{
				PlayMakerFSM component7 = gameObject.GetComponent<PlayMakerFSM>();
				if (component7 != null)
				{
					EventHook.AddWithSync(component7, "Wait player", null, false);
					EventHook.AddWithSync(component7, "Purchase", null, false);
				}
			}
			if (gameObject.name == "HOMENEW")
			{
				if (this.houseComp.ContainsValue(gameObject))
				{
					Logger.Debug("Duplicate Player house comp prefab '" + gameObject.name + "' rejected");
					return;
				}
				this.houseComp.Add(this.houseComp.Count + 1, gameObject);
				Logger.Debug(string.Format("Registered Player house comp prefab '{0}' (Player house comp ID: {1})", gameObject.name, this.houseComp.Count));
				gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerHouse, ObjectSyncManager.AUTOMATIC_ID);
				return;
			}
			else
			{
				if (gameObject.name == "Building")
				{
					Transform parent = gameObject.transform.parent;
					if (((parent != null) ? parent.name : null) == "YARD")
					{
						Logger.Debug("Found Building " + gameObject.name);
						if (this.houseComp.ContainsValue(gameObject))
						{
							Logger.Debug("Duplicate Player house comp prefab '" + gameObject.name + "' rejected");
							return;
						}
						this.houseComp.Add(this.houseComp.Count + 1, gameObject);
						Logger.Debug(string.Format("Registered Player house comp prefab '{0}' (Player house comp ID: {1})", gameObject.name, this.houseComp.Count));
						gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerHouse, ObjectSyncManager.AUTOMATIC_ID);
						return;
					}
				}
				if (!(gameObject.name == "COTTAGE"))
				{
					if (gameObject.name == "EQUIPMENTS")
					{
						if (this.houseComp.ContainsValue(gameObject))
						{
							Logger.Debug("Duplicate Player house comp prefab '" + gameObject.name + "' rejected");
							return;
						}
						this.houseComp.Add(this.houseComp.Count + 1, gameObject);
						Logger.Debug(string.Format("Registered Player house comp prefab '{0}' (Player house comp ID: {1})", gameObject.name, this.houseComp.Count));
						gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerHouse, ObjectSyncManager.AUTOMATIC_ID);
					}
					return;
				}
				if (this.houseComp.ContainsValue(gameObject))
				{
					Logger.Debug("Duplicate Player house comp prefab '" + gameObject.name + "' rejected");
					return;
				}
				this.houseComp.Add(this.houseComp.Count + 1, gameObject);
				Logger.Debug(string.Format("Registered Player house comp prefab '{0}' (Player house comp ID: {1})", gameObject.name, this.houseComp.Count));
				gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.PlayerHouse, ObjectSyncManager.AUTOMATIC_ID);
				return;
			}
		}

		public static GameHouseDatabase Instance;

		private Dictionary<int, GameObject> houseComp = new Dictionary<int, GameObject>();

		public PlayMakerFSM channel1Fsm;

		public PlayMakerArrayListProxy channel1ArrayList;

		public PlayMakerFSM channel2Fsm;

		public PlayMakerArrayListProxy channel2ArrayList;

		public List<PlayMakerFSM> keypadPhones = new List<PlayMakerFSM>();

		public List<PlayMakerFSM> keypadPhonesCalling = new List<PlayMakerFSM>();

		public List<PlayMakerFSM> useHandles = new List<PlayMakerFSM>();

		public PlayMakerFSM[] alarmClocks = new PlayMakerFSM[2];

		private bool pReceiver;

		private bool pReceiver2;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass20_0
		{
			public <>c__DisplayClass20_0()
			{
			}

			internal bool <CollectGameObject>b__0()
			{
				if (NetManager.Instance.IsHost)
				{
					int num = this.<>4__this.channel1ArrayList.arrayList.IndexOf(this.fsm.Fsm.GetFsmObject("Audio").Value);
					NetLocalPlayer.Instance.WriteFSMFloatMessage(53001, "Channel1", (float)num);
				}
				else if (this.fsm.Fsm.States[1].Actions[0].Enabled)
				{
					Logger.Debug("IMNOTAHOST");
					for (int i = 1; i < this.fsm.Fsm.States.Length; i++)
					{
						FsmStateAction[] actions = this.fsm.Fsm.States[i].Actions;
						for (int j = 0; j < actions.Length; j++)
						{
							actions[j].Enabled = false;
						}
					}
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public GameHouseDatabase <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass20_1
		{
			public <>c__DisplayClass20_1()
			{
			}

			internal bool <CollectGameObject>b__1()
			{
				if (this.<>4__this.pReceiver)
				{
					this.<>4__this.pReceiver = false;
					return false;
				}
				int num;
				if (int.TryParse(this.fsm.Fsm.GetFsmString("Key").Value, out num))
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(54000, this.index.ToString(), (float)num);
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public int index;

			public GameHouseDatabase <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass20_2
		{
			public <>c__DisplayClass20_2()
			{
			}

			internal bool <CollectGameObject>b__2()
			{
				if (NetManager.Instance.IsHost)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(54100, this.CS$<>8__locals1.index.ToString(), (float)this.sideFsm.Fsm.GetFsmInt("RequiredRings").Value);
				}
				return false;
			}

			public PlayMakerFSM sideFsm;

			public GameHouseDatabase.<>c__DisplayClass20_1 CS$<>8__locals1;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass20_3
		{
			public <>c__DisplayClass20_3()
			{
			}

			internal bool <CollectGameObject>b__3()
			{
				if (this.<>4__this.pReceiver2)
				{
					this.<>4__this.pReceiver2 = false;
					if (!this.fsm.Fsm.GetFsmBool("Use").Value)
					{
						this.fsm.Fsm.GetState("Pick phone").Actions[5].Enabled = false;
						this.fsm.Fsm.GetState("Close phone").Actions[5].Enabled = false;
						this.fsm.GetComponent<CapsuleCollider>().enabled = false;
						this.fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = false;
					}
					else
					{
						this.fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = true;
						this.fsm.GetComponent<CapsuleCollider>().enabled = true;
					}
					return false;
				}
				this.fsm.Fsm.GetState("Pick phone").Actions[5].Enabled = true;
				this.fsm.Fsm.GetState("Close phone").Actions[5].Enabled = true;
				this.fsm.Fsm.GetFsmGameObject("Keypad").Value.GetComponent<PlayMakerFSM>().Fsm.States[0].Actions[0].Enabled = true;
				this.fsm.GetComponent<CapsuleCollider>().enabled = true;
				NetLocalPlayer.Instance.WriteFSMBoolMessage(54500, this.index.ToString(), !this.fsm.Fsm.GetFsmBool("Use").Value);
				return false;
			}

			public PlayMakerFSM fsm;

			public int index;

			public GameHouseDatabase <>4__this;
		}
	}
}
