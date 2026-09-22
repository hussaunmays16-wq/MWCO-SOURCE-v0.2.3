using System;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class GamePlayer
	{
		public GameObject Object
		{
			get
			{
				return this.gameObject;
			}
		}

		public GameObject PickedUpObject
		{
			get
			{
				return this.pickedUpGameObject;
			}
		}

		public GamePlayer(GameObject gameObject)
		{
			this.gameObject = gameObject;
			GamePlayer.Instance = this;
			this.pickupFsm = Utils.GetPlaymakerScriptByName(gameObject, "PickUp");
			if (this.pickupFsm != null)
			{
				EventHook.Add(this.pickupFsm, "Part picked", delegate
				{
					this.PickupObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Item picked", delegate
				{
					this.PickupObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Tool picked", delegate
				{
					this.PickupObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Ragdoll picked", delegate
				{
					this.PickupObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Drop part 2", delegate
				{
					this.ThrowObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Drop part", delegate
				{
					this.DropObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Drop tool", delegate
				{
					this.DropObject();
					return false;
				}, false, false);
				EventHook.Add(this.pickupFsm, "Drop ragdoll", delegate
				{
					this.DropObject();
					return false;
				}, false, false);
				this.characterMotor = gameObject.GetComponent<CharacterMotor>();
				this.mouseLook = gameObject.GetComponent<MouseLook>();
			}
			GameObject gameObject2 = GameObject.CreatePrimitive(0);
			gameObject2.transform.localScale = new Vector3(100f, 100f, 100f);
			gameObject2.GetComponent<SphereCollider>().isTrigger = true;
			UnityEngine.Object.Destroy(gameObject2.GetComponent<MeshRenderer>());
			gameObject2.transform.position = gameObject.transform.position;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.AddComponent<ObjectSyncPlayerComponent>();
			this.FPSCamera = gameObject.transform.Find("Pivot/AnimPivot/Camera/FPSCamera").transform;
		}

		public void LoadRaycast()
		{
			MPController.Instance.productList = GameObject.Find("Spawner/BagContentsStore").GetComponent<PlayMakerFSM>();
			MPController.Instance.createItems = GameObject.Find("Spawner").transform.FindChild("CreateItems").gameObject;
			EventHook.SyncAllEvents(Utils.GetPlaymakerScriptByName(this.gameObject.transform.FindChild("Rain").gameObject, "Rain"), null);
			this.raycastGameObject = this.gameObject.transform.GetChild(0).GetChild(0).GetChild(0)
				.GetChild(0)
				.GetChild(2)
				.GetChild(2)
				.gameObject;
			this.raycastGameObject.transform.parent.gameObject.SetActive(true);
			foreach (PlayMakerFSM playMakerFSM in this.raycastGameObject.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.FsmName == "Check")
				{
					this.raycastFSM = playMakerFSM;
					break;
				}
			}
			EventHook.Add(this.raycastFSM, "Tighten", delegate
			{
				try
				{
					this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
				}
				catch (Exception ex)
				{
					string text = "Error sending bolt sync.";
					Exception ex2 = ex;
					Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
				}
				return false;
			}, false, false);
			EventHook.Add(this.raycastFSM, "Untighten", delegate
			{
				try
				{
					this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
				}
				catch (Exception ex3)
				{
					string text2 = "Error sending bolt sync.";
					Exception ex4 = ex3;
					Logger.Debug(text2 + ((ex4 != null) ? ex4.ToString() : null));
				}
				return false;
			}, false, false);
			EventHook.Add(this.raycastFSM, "Tighten 2", delegate
			{
				try
				{
					this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
				}
				catch (Exception ex5)
				{
					string text3 = "Error sending bolt sync. ";
					Exception ex6 = ex5;
					Logger.Debug(text3 + ((ex6 != null) ? ex6.ToString() : null));
				}
				return false;
			}, false, false);
			EventHook.Add(this.raycastFSM, "Untighten 2", delegate
			{
				try
				{
					this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
				}
				catch (Exception ex7)
				{
					string text4 = "Error sending bolt sync.";
					Exception ex8 = ex7;
					Logger.Debug(text4 + ((ex8 != null) ? ex8.ToString() : null));
				}
				return false;
			}, false, false);
			this.raycastGameObject.transform.parent.gameObject.SetActive(false);
		}

		public void PaintObject(GameObject go, Color color)
		{
			this.receiver = true;
			this.sprayGameObject.SetActive(true);
			this.spraycanFSM.Fsm.GetFsmColor("SprayColor").Value = color;
			this.spraycanFSM.Fsm.GetFsmGameObject("PaintedPart").Value = go;
			this.spraycanFSM.SendEvent("MP_Regular paint");
			this.sprayGameObject.SetActive(false);
			this.receiver = false;
		}

		private void PickupObject()
		{
			try
			{
				NetLocalPlayer.Instance.pickUp = true;
				this.pickedUpGameObject = this.pickupFsm.Fsm.GetFsmGameObject("PickedObject").Value;
				ObjectSyncComponent component = this.pickedUpGameObject.GetComponent<ObjectSyncComponent>();
				component.TakeSyncControl();
				component.SendConstantSync(true, true);
				this.picked = true;
				this.dropped = false;
				string text = "Picked up object: ";
				GameObject gameObject = this.pickedUpGameObject;
				Logger.Info(text + ((gameObject != null) ? gameObject.ToString() : null));
			}
			catch (Exception ex)
			{
				Logger.Debug("Exception picking up object. " + ex.Message);
			}
		}

		private void ThrowObject()
		{
			try
			{
				NetLocalPlayer.Instance.pickUp = false;
				string text = "Threw object: ";
				GameObject gameObject = this.pickedUpGameObject;
				Logger.Info(text + ((gameObject != null) ? gameObject.ToString() : null));
				this.picked = false;
				ObjectSyncComponent component = this.pickedUpGameObject.GetComponent<ObjectSyncComponent>();
				component.SendConstantSync(false, true);
				NetLocalPlayer.Instance.WritePickupMessage(component.ObjectID, false);
				this.pickedUpGameObject = null;
			}
			catch (Exception ex)
			{
				Logger.Debug("Exception dropping object. " + ex.Message);
			}
		}

		public void DropObject()
		{
			try
			{
				NetLocalPlayer.Instance.pickUp = false;
				string text = "Dropped object: ";
				GameObject gameObject = this.pickedUpGameObject;
				Logger.Info(text + ((gameObject != null) ? gameObject.ToString() : null));
				this.dropped = true;
				this.picked = false;
				ObjectSyncComponent component = this.pickedUpGameObject.GetComponent<ObjectSyncComponent>();
				component.SendConstantSync(false, true);
				NetLocalPlayer.Instance.WritePickupMessage(component.ObjectID, false);
				this.pickedUpGameObject = null;
			}
			catch (Exception ex)
			{
				Logger.Debug("Exception dropping object. " + ex.Message);
			}
		}

		public void ResetAttributes()
		{
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerAllergy").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerDirtiness").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerBurns").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerDrunk").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerFatigue").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerHunger").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerThirst").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerStress").Value = 0f;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerUrine").Value = 0f;
			DevTools.blindnessFsm.SendEvent("MP_Reset 2");
		}

		public void DropStolenObject()
		{
			this.pickupFsm.SendEvent("MP_Drop part");
		}

		public bool isPicked()
		{
			return this.pickupFsm.Fsm.GetState("Part picked").Active && !this.picked;
		}

		public bool isDropped()
		{
			return this.dropped;
		}

		public void LookForObject()
		{
			this.pickupFsm.SendEvent("MP_State 2");
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_0()
		{
			this.PickupObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_1()
		{
			this.PickupObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_2()
		{
			this.PickupObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_3()
		{
			this.PickupObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_4()
		{
			this.ThrowObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_5()
		{
			this.DropObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_6()
		{
			this.DropObject();
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__17_7()
		{
			this.DropObject();
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_0()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_1()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_2()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync. ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_3()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_4()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_5()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_6()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendTighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync. ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_7()
		{
			try
			{
				this.raycastFSM.Fsm.GetFsmGameObject("Bolt").Value.GetComponent<Bolt>().SendUntighten();
			}
			catch (Exception ex)
			{
				string text = "Error sending bolt sync.";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LoadRaycast>b__18_8()
		{
			if (this.receiver)
			{
				return false;
			}
			try
			{
				Color value = this.spraycanFSM.Fsm.GetFsmColor("SprayColor").Value;
				GameObject value2 = this.spraycanFSM.Fsm.GetFsmGameObject("PaintedPart").Value;
				NetLocalPlayer.Instance.WritePaintMessage(value2, value);
			}
			catch (Exception)
			{
				Logger.Debug("Error syncing paint");
			}
			return false;
		}

		private bool dropped;

		private bool picked;

		private PlayMakerFSM pickupFsm;

		private GameObject gameObject;

		private GameObject pickedUpGameObject;

		public CharacterMotor characterMotor;

		public MouseLook mouseLook;

		private GameObject raycastGameObject;

		public PlayMakerFSM raycastFSM;

		private GameObject sprayGameObject;

		private PlayMakerFSM spraycanFSM;

		public Transform FPSCamera;

		public static GamePlayer Instance;

		private bool receiver;
	}
}
