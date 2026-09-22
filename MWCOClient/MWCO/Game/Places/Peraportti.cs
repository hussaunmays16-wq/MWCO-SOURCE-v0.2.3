using System;
using System.Runtime.CompilerServices;
using Boo.Lang;
using HutongGames.PlayMaker;
using MWCO.Game.Objects;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Places
{
	public class Peraportti : MonoBehaviour
	{
		private void Start()
		{
			Peraportti.Instance = this;
			Logger.Debug("Setting up 2nd cam on " + base.name);
			Component component = base.transform.FindChild("ATMs").GetChild(0).GetComponent<Camera>();
			GameObject gameObject = base.transform.FindChild("ATMs").GetChild(2).gameObject;
			GameObject gameObject2 = base.transform.FindChild("ATMs").GetChild(1).gameObject;
			RenderTexture renderTexture = base.transform.FindChild("Building/ATM/screen").GetComponent<MeshRenderer>().material.mainTexture as RenderTexture;
			gameObject.SetActive(true);
			gameObject2.SetActive(true);
			GameObject gameObject3 = Object.Instantiate<GameObject>(component.gameObject);
			gameObject3.transform.SetParent(base.transform.FindChild("ATMs"));
			Camera component2 = gameObject3.GetComponent<Camera>();
			component2.enabled = true;
			RenderTexture renderTexture2 = new RenderTexture(renderTexture.width, renderTexture.height, renderTexture.depth);
			renderTexture2.Create();
			component2.targetTexture = renderTexture2;
			gameObject2.transform.localPosition = new Vector3(32f, 0f, 0f);
			component2.transform.localPosition = new Vector3(32f, 0f, -10f);
			component2.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			this.fuelPumpAtmCam = component2;
			base.transform.FindChild("Building/FuelPumps_1-2/ScreenMesh").GetComponent<MeshRenderer>().material.mainTexture = renderTexture2;
			PlayMakerFSM[] componentsInChildren = base.GetComponentsInChildren<PlayMakerFSM>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				PlayMakerFSM fsm = componentsInChildren[i];
				if (fsm.name == "CreditCardTrigger")
				{
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Wait player 2", null, false);
					EventHook.AddWithSync(fsm, "Insert card", null, false);
					EventHook.AddWithSync(fsm, "Take bill", null, false);
				}
				else if (fsm.name == "Automat")
				{
					fsm.gameObject.SetActive(true);
					FsmStateAction[] actions = fsm.Fsm.GetState("PIN check").Actions;
					for (int j = 0; j < actions.Length; j++)
					{
						actions[j].Enabled = false;
					}
					EventHook.Add(fsm, "PIN check", delegate
					{
						fsm.SendEvent("GOOD");
						fsm.Fsm.GetFsmGameObject("ScreenPIN").Value.SetActive(false);
						return false;
					}, false, false);
					EventHook.SyncAllEvents(fsm, null);
					Transform parent = fsm.transform.parent;
					if (((parent != null) ? parent.name : null) == "ATM")
					{
						fsm.Fsm.GetState("Check money").Actions[0].Enabled = false;
						fsm.Fsm.GetState("CARD").Actions[0].Enabled = false;
					}
					else
					{
						this.automat2Fsm = fsm;
						EventHook.Add(fsm, "State 5", delegate
						{
							if (this.receiver2)
							{
								this.receiver2 = false;
							}
							else
							{
								NetLocalPlayer.Instance.WriteFSMFloatMessage(50150, fsm.name, fsm.Fsm.GetFsmFloat("Reserve").Value);
							}
							return false;
						}, false, false);
					}
					fsm.gameObject.SetActive(false);
				}
				else if (fsm.name == "FuelTriggerGasoline")
				{
					if (fsm.transform.parent.parent.parent.name == "Pump1")
					{
						this.trigger98FsmP1 = fsm;
					}
					else
					{
						this.trigger98FsmP2 = fsm;
					}
					EventHook.AddWithSync(fsm, "State 2", null, false);
					EventHook.Add(fsm, "Activate hand", delegate
					{
						if (this.receiver)
						{
							this.receiver = false;
						}
						else
						{
							NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, fsm.transform.parent.parent.parent.name + "," + fsm.name, 0f);
						}
						return false;
					}, false, false);
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Anim drop", delegate
					{
						fsm.GetComponent<CapsuleCollider>().enabled = true;
						this.ReceivedDropSync();
						this.isPouring = false;
						fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
						return false;
					}, false);
				}
				else if (fsm.name == "FuelTriggerDiesel")
				{
					if (fsm.transform.parent.parent.parent.name == "Pump1")
					{
						this.triggerDFsmP1 = fsm;
					}
					else
					{
						this.triggerDFsmP2 = fsm;
					}
					EventHook.AddWithSync(fsm, "State 2", null, false);
					EventHook.Add(fsm, "Activate hand", delegate
					{
						if (this.receiver)
						{
							this.receiver = false;
						}
						else
						{
							NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, fsm.transform.parent.parent.parent.name + "," + fsm.name, 0f);
						}
						return false;
					}, false, false);
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Anim drop", delegate
					{
						fsm.GetComponent<CapsuleCollider>().enabled = true;
						this.ReceivedDropSync();
						this.isPouring = false;
						fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
						return false;
					}, false);
				}
				else if (fsm.name.StartsWith("FuelTriggerMP"))
				{
					if (fsm.transform.parent.parent.parent.name == "Pump1")
					{
						this.triggerPoFsmP1 = fsm;
					}
					else
					{
						this.triggerPoFsmP2 = fsm;
					}
					EventHook.AddWithSync(fsm, "State 2", null, false);
					EventHook.Add(fsm, "Activate hand", delegate
					{
						if (this.receiver)
						{
							this.receiver = false;
						}
						else
						{
							NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, fsm.transform.parent.parent.parent.name + "," + fsm.name, 0f);
						}
						return false;
					}, false, false);
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Anim drop", delegate
					{
						fsm.GetComponent<CapsuleCollider>().enabled = true;
						this.ReceivedDropSync();
						this.isPouring = false;
						fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
						return false;
					}, false);
				}
				else if (fsm.FsmName == "Buy")
				{
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Wait", null, false);
					if (fsm.Fsm.GetFsmString("ID") != null)
					{
						if (fsm.Fsm.GetFsmString("ID").Value.StartsWith("FOOD"))
						{
							EventHook.AddWithSync(fsm, "Cashier", null, false);
							EventHook.AddWithSync(fsm, "State 1", null, false);
						}
						else
						{
							EventHook.AddWithSync(fsm, "Check inventory", null, false);
							EventHook.AddWithSync(fsm, "Check if 0", null, false);
						}
					}
					else
					{
						EventHook.AddWithSync(fsm, "Check inventory", null, false);
						EventHook.AddWithSync(fsm, "Check if 0", null, false);
					}
				}
				else if (fsm.name == "CashRegisterLogic")
				{
					int index = this.cashRegisterFsms.Count;
					this.cashRegisterFsms.Add(fsm);
					EventHook.Add(fsm, "Check money", delegate
					{
						if (this.receiverCR)
						{
							fsm.Fsm.GetFsmFloat("PriceTotal").Value = 0f;
							this.receiverCR = false;
						}
						else
						{
							NetLocalPlayer.Instance.WriteFSMBoolMessage(51000 + index, "Purchase", true);
						}
						return false;
					}, false, false);
				}
				else if (fsm.name == "CashTrigger")
				{
					EventHook.AddWithSync(fsm, "Take bill", null, false);
				}
				else if (((!(fsm.name == "Keijo") && !(fsm.name == "Jouni") && !(fsm.name == "Virpi")) || !(fsm.FsmName == "Work")) && fsm.FsmName == "LOD")
				{
					if (fsm.gameObject == base.gameObject)
					{
						fsm.gameObject.AddComponent<LODManager>().Initialize(fsm, 250f, false, "LOD", "Object");
					}
					else
					{
						fsm.gameObject.AddComponent<LODManager>().Initialize(fsm, 10f, false, "MinDistance", "Object");
					}
					if (fsm.name == "FuelPumps_1-2")
					{
						fsm.Fsm.GetFsmObject("Cam").Value = this.fuelPumpAtmCam;
					}
				}
			}
		}

		public void ReadCashRegisterMessage(int index)
		{
			this.receiverCR = true;
			this.cashRegisterFsms[index].SendEvent("MP_Check money");
		}

		public void LoadFuelNozzles(Transform plr)
		{
			PlayMakerFSM fsm98 = plr.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Fuel/Pivot/hand/Armature/Bone/Bone_001/Pistol/Pistol 98/FuelNozzle").GetComponent<PlayMakerFSM>();
			PlayMakerFSM fsmD = plr.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Fuel/Pivot/hand/Armature/Bone/Bone_001/Pistol/Pistol D/FuelNozzle").GetComponent<PlayMakerFSM>();
			PlayMakerFSM fsmPo = plr.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Fuel/Pivot/hand/Armature/Bone/Bone_001/Pistol/Pistol PÖ/FuelNozzle").GetComponent<PlayMakerFSM>();
			this.trigger98mesh = fsm98.transform.parent.GetComponent<MeshRenderer>();
			this.triggerDmesh = fsmD.transform.parent.GetComponent<MeshRenderer>();
			this.triggerPomesh = fsmPo.transform.parent.GetComponent<MeshRenderer>();
			this.nozzle98Fsm = fsm98;
			this.nozzleDFsm = fsmD;
			this.nozzlePoFsm = fsmPo;
			this.FuelPivotParent = plr.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Fuel/Pivot").gameObject;
			EventHook.Add(fsm98, "Pour", delegate
			{
				this.isPouring = true;
				this.currentNozzle = fsm98.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + fsm98.Fsm.GetFsmGameObject("Pump").Value.name;
				this.activeNozzle = fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}, false, false);
			EventHook.Add(fsm98, "Wait", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
			EventHook.Add(fsm98, "Wait for cap", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
			EventHook.Add(fsmD, "Pour", delegate
			{
				this.isPouring = true;
				this.currentNozzle = fsmD.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + fsmD.Fsm.GetFsmGameObject("Pump").Value.name;
				this.activeNozzle = fsmD.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}, false, false);
			EventHook.Add(fsmD, "Wait", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsmD.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
			EventHook.Add(fsmD, "Wait for cap", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
			EventHook.Add(fsmPo, "Pour", delegate
			{
				this.isPouring = true;
				this.currentNozzle = fsmPo.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + fsmPo.Fsm.GetFsmGameObject("Pump").Value.name;
				this.activeNozzle = fsmPo.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}, false, false);
			EventHook.Add(fsmPo, "Wait", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsmPo.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
			EventHook.Add(fsmPo, "Wait for cap", delegate
			{
				if (this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.currentNozzle, fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.isPouring = false;
				}
				return false;
			}, false, false);
		}

		private void Update()
		{
			if (this.isPouring)
			{
				if (this.timeToSendSync <= 0f)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50420, this.currentNozzle, this.activeNozzle.Fsm.GetFsmFloat("Fuel").Value);
					if (this.activeVehicle != null)
					{
						NetLocalPlayer.Instance.WriteFSMFloatMessage(this.activeVehicle.syncComponent.ObjectID, "FuelTankLevel", this.activeVehicle.Fuel);
					}
					this.timeToSendSync = 0.1f;
					return;
				}
				this.timeToSendSync -= Time.deltaTime;
			}
		}

		public void ReceivedPourSync(string nozzle, float newValue, bool stopped = false)
		{
			if (stopped)
			{
				MasterAudio.StopAllOfSound("Store");
			}
			else
			{
				MasterAudio.PlaySound3DAtTransform("Store", this.trigger98FsmP1.transform, 1f, new float?(1f), 0f, "fueling_2", false, false);
			}
			string[] array = nozzle.Split(new char[] { ',' });
			if (!(array[0] == "Pump1"))
			{
				if (array[0] == "Pump2")
				{
					if (array[1] == "FuelTriggerGasoline")
					{
						this.trigger98FsmP2.Fsm.GetFsmFloat("Fuel").Value = newValue;
						this.trigger98FsmP2.SendEvent("MP_Calculate");
						return;
					}
					if (array[1] == "FuelTriggerDiesel")
					{
						this.triggerDFsmP2.Fsm.GetFsmFloat("Fuel").Value = newValue;
						this.triggerDFsmP2.SendEvent("MP_Calculate");
						return;
					}
					this.triggerPoFsmP2.Fsm.GetFsmFloat("Fuel").Value = newValue;
					this.triggerPoFsmP2.SendEvent("MP_Calculate");
				}
				return;
			}
			if (array[1] == "FuelTriggerGasoline")
			{
				this.trigger98FsmP1.Fsm.GetFsmFloat("Fuel").Value = newValue;
				this.trigger98FsmP1.SendEvent("MP_Calculate");
				return;
			}
			if (array[1] == "FuelTriggerDiesel")
			{
				this.triggerDFsmP1.Fsm.GetFsmFloat("Fuel").Value = newValue;
				this.triggerDFsmP1.SendEvent("MP_Calculate");
				return;
			}
			this.triggerPoFsmP1.Fsm.GetFsmFloat("Fuel").Value = newValue;
			this.triggerPoFsmP1.SendEvent("MP_Calculate");
		}

		public void ReceivedTakeSync(string nozzle)
		{
			this.receiver = true;
			string[] array = nozzle.Split(new char[] { ',' });
			if (array[0] == "Pump1")
			{
				if (array[1] == "FuelTriggerGasoline")
				{
					this.trigger98FsmP1.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.trigger98FsmP1.SendEvent("MP_Activate hand");
					this.trigger98mesh.enabled = false;
					this.nozzle98Fsm.enabled = false;
					this.trigger98FsmP1.GetComponent<CapsuleCollider>().enabled = false;
				}
				else if (array[1] == "FuelTriggerDiesel")
				{
					this.triggerDFsmP1.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.triggerDFsmP1.SendEvent("MP_Activate hand");
					this.triggerDmesh.enabled = false;
					this.nozzleDFsm.enabled = false;
					this.triggerDFsmP1.GetComponent<CapsuleCollider>().enabled = false;
				}
				else
				{
					this.triggerPoFsmP1.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.triggerPoFsmP1.SendEvent("MP_Activate hand");
					this.triggerPomesh.enabled = false;
					this.nozzlePoFsm.enabled = false;
					this.triggerPoFsmP1.GetComponent<CapsuleCollider>().enabled = false;
				}
			}
			else if (array[0] == "Pump2")
			{
				if (array[1] == "FuelTriggerGasoline")
				{
					this.trigger98FsmP2.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.trigger98FsmP2.SendEvent("MP_Activate hand");
					this.trigger98mesh.enabled = false;
					this.nozzle98Fsm.enabled = false;
					this.trigger98FsmP2.GetComponent<CapsuleCollider>().enabled = false;
				}
				else if (array[1] == "FuelTriggerDiesel")
				{
					this.triggerDFsmP2.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.triggerDFsmP2.SendEvent("MP_Activate hand");
					this.triggerDmesh.enabled = false;
					this.nozzleDFsm.enabled = false;
					this.triggerDFsmP2.GetComponent<CapsuleCollider>().enabled = false;
				}
				else
				{
					this.triggerPoFsmP2.Fsm.GetState("Calculate").Actions[14].Enabled = false;
					this.triggerPoFsmP2.SendEvent("MP_Activate hand");
					this.triggerPomesh.enabled = false;
					this.nozzlePoFsm.enabled = false;
					this.triggerPoFsmP2.GetComponent<CapsuleCollider>().enabled = false;
				}
			}
			this.FuelPivotParent.SetActive(false);
		}

		public void ReceivedDropSync()
		{
			this.trigger98mesh.enabled = true;
			this.triggerDmesh.enabled = true;
			this.triggerPomesh.enabled = true;
			this.nozzle98Fsm.enabled = true;
			this.nozzleDFsm.enabled = true;
			this.nozzlePoFsm.enabled = true;
			this.FuelPivotParent.SetActive(true);
			foreach (NetPlayer netPlayer in NetManager.Instance.players)
			{
				netPlayer.DropNozzle();
			}
		}

		public void ReceivedReserveSync(float newValue)
		{
			this.receiver2 = true;
			this.automat2Fsm.Fsm.GetFsmFloat("Reserve").Value = newValue;
		}

		public Peraportti()
		{
		}

		public PlayMakerFSM storeRegisterFSM;

		public PlayMakerFSM storePostOrderFSM;

		public PlayMakerFSM pubRegisterFSM;

		public PlayMakerFSM pump1Fsm;

		public PlayMakerFSM pump2Fsm;

		public PlayMakerFSM trigger98FsmP1;

		public PlayMakerFSM triggerDFsmP1;

		public PlayMakerFSM triggerPoFsmP1;

		public PlayMakerFSM trigger98FsmP2;

		public PlayMakerFSM triggerDFsmP2;

		public PlayMakerFSM triggerPoFsmP2;

		public MeshRenderer trigger98mesh;

		public MeshRenderer triggerDmesh;

		public MeshRenderer triggerPomesh;

		public PlayMakerFSM nozzle98Fsm;

		public PlayMakerFSM nozzleDFsm;

		public PlayMakerFSM nozzlePoFsm;

		public GameObject FuelPivotParent;

		public PlayMakerFSM automatFsm;

		public PlayMakerFSM automat2Fsm;

		public Camera fuelPumpAtmCam;

		public static Peraportti Instance;

		private List<PlayMakerFSM> cashRegisterFsms = new List<PlayMakerFSM>();

		private bool receiver;

		private bool receiver2;

		private bool receiverReserve;

		private bool receiverCR;

		private bool isPouring;

		private string currentNozzle;

		private PlayMakerFSM activeNozzle;

		public PlayerVehicle activeVehicle;

		private const float SYNC_INTERVAL = 0.1f;

		private float timeToSendSync;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass22_0
		{
			public <>c__DisplayClass22_0()
			{
			}

			internal bool <Start>b__0()
			{
				this.fsm.SendEvent("GOOD");
				this.fsm.Fsm.GetFsmGameObject("ScreenPIN").Value.SetActive(false);
				return false;
			}

			internal bool <Start>b__1()
			{
				if (this.<>4__this.receiver2)
				{
					this.<>4__this.receiver2 = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50150, this.fsm.name, this.fsm.Fsm.GetFsmFloat("Reserve").Value);
				}
				return false;
			}

			internal bool <Start>b__2()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, this.fsm.transform.parent.parent.parent.name + "," + this.fsm.name, 0f);
				}
				return false;
			}

			internal bool <Start>b__3()
			{
				this.fsm.GetComponent<CapsuleCollider>().enabled = true;
				this.<>4__this.ReceivedDropSync();
				this.<>4__this.isPouring = false;
				this.fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
				return false;
			}

			internal bool <Start>b__4()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, this.fsm.transform.parent.parent.parent.name + "," + this.fsm.name, 0f);
				}
				return false;
			}

			internal bool <Start>b__5()
			{
				this.fsm.GetComponent<CapsuleCollider>().enabled = true;
				this.<>4__this.ReceivedDropSync();
				this.<>4__this.isPouring = false;
				this.fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
				return false;
			}

			internal bool <Start>b__6()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50100, this.fsm.transform.parent.parent.parent.name + "," + this.fsm.name, 0f);
				}
				return false;
			}

			internal bool <Start>b__7()
			{
				this.fsm.GetComponent<CapsuleCollider>().enabled = true;
				this.<>4__this.ReceivedDropSync();
				this.<>4__this.isPouring = false;
				this.fsm.Fsm.GetState("Calculate").Actions[14].Enabled = true;
				return false;
			}

			public PlayMakerFSM fsm;

			public Peraportti <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass22_1
		{
			public <>c__DisplayClass22_1()
			{
			}

			internal bool <Start>b__8()
			{
				if (this.CS$<>8__locals1.<>4__this.receiverCR)
				{
					this.CS$<>8__locals1.fsm.Fsm.GetFsmFloat("PriceTotal").Value = 0f;
					this.CS$<>8__locals1.<>4__this.receiverCR = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMBoolMessage(51000 + this.index, "Purchase", true);
				}
				return false;
			}

			public int index;

			public Peraportti.<>c__DisplayClass22_0 CS$<>8__locals1;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass25_0
		{
			public <>c__DisplayClass25_0()
			{
			}

			internal bool <LoadFuelNozzles>b__0()
			{
				this.<>4__this.isPouring = true;
				this.<>4__this.currentNozzle = this.fsm98.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + this.fsm98.Fsm.GetFsmGameObject("Pump").Value.name;
				this.<>4__this.activeNozzle = this.fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}

			internal bool <LoadFuelNozzles>b__1()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			internal bool <LoadFuelNozzles>b__2()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			internal bool <LoadFuelNozzles>b__3()
			{
				this.<>4__this.isPouring = true;
				this.<>4__this.currentNozzle = this.fsmD.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + this.fsmD.Fsm.GetFsmGameObject("Pump").Value.name;
				this.<>4__this.activeNozzle = this.fsmD.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}

			internal bool <LoadFuelNozzles>b__4()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsmD.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			internal bool <LoadFuelNozzles>b__5()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			internal bool <LoadFuelNozzles>b__6()
			{
				this.<>4__this.isPouring = true;
				this.<>4__this.currentNozzle = this.fsmPo.Fsm.GetFsmGameObject("Pump").Value.transform.parent.parent.parent.name + "," + this.fsmPo.Fsm.GetFsmGameObject("Pump").Value.name;
				this.<>4__this.activeNozzle = this.fsmPo.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>();
				return false;
			}

			internal bool <LoadFuelNozzles>b__7()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsmPo.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			internal bool <LoadFuelNozzles>b__8()
			{
				if (this.<>4__this.isPouring)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(50421, this.<>4__this.currentNozzle, this.fsm98.Fsm.GetFsmGameObject("Pump").Value.GetComponent<PlayMakerFSM>().Fsm.GetFsmFloat("Fuel").Value);
					this.<>4__this.isPouring = false;
				}
				return false;
			}

			public Peraportti <>4__this;

			public PlayMakerFSM fsm98;

			public PlayMakerFSM fsmD;

			public PlayMakerFSM fsmPo;
		}
	}
}
