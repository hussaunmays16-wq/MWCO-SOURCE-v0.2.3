using System;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class Trigger : MonoBehaviour
	{
		private void Start()
		{
			if (this.IsSetup)
			{
				return;
			}
			this.GetFSM();
			this.HookEvents();
			this.IsSetup = true;
		}

		private void Update()
		{
			if (this.HasSyncableFluid && this.fsm.Fsm.GetFsmFloat(this.FluidName).Value != this.FluidValue)
			{
				if (this.fsm.Fsm.GetFsmFloat(this.FluidName).Value > this.FluidValue)
				{
					this.SendFluidSync();
					return;
				}
				if (NetManager.Instance.IsHost)
				{
					if (this.timer <= 0f)
					{
						this.SendFluidSync();
						this.timer = 10f;
						return;
					}
					this.timer -= Time.deltaTime;
				}
			}
		}

		private void GetFSM()
		{
			if (base.name.ToLower().StartsWith("vinp") || base.name.StartsWith("Fuse"))
			{
				this.fsm = base.GetComponent<PlayMakerFSM>();
			}
			if (base.name.Contains("Oilpan"))
			{
				this.HasSyncableFluid = true;
				this.FluidName = "Oil";
				return;
			}
			if (base.name.Contains("Fuel tank"))
			{
				this.HasSyncableFluid = true;
				this.FluidName = "FuelLevel";
				return;
			}
			if (base.name.Contains("Radiator"))
			{
				this.HasSyncableFluid = true;
				this.FluidName = "Coolant";
				return;
			}
			if (base.name.Contains("BrakeMaster"))
			{
				this.HasSyncableFluid = true;
				this.FluidName = "BrakeFluidF";
			}
		}

		private void HookEvents()
		{
			if (this.fsm == null)
			{
				Logger.Debug("[Trigger Error] " + base.name + " doesn't have an FSM!");
			}
			FsmState state = this.fsm.Fsm.GetState("Install 1");
			this.UsesAssembly = state != null && state.Actions.Length > 4;
			EventHook.Add(this.fsm, "Install 1", delegate
			{
				ObjectSyncComponent objectSyncComponent = (this.UsesAssembly ? GameVehicleDatabase.Instance.RivettAssembly.Fsm.GetFsmGameObject("ActivePart").Value.GetComponent<ObjectSyncComponent>() : this.fsm.Fsm.GetFsmGameObject("ActivePart").Value.GetComponent<ObjectSyncComponent>());
				objectSyncComponent.enabled = false;
				if (this.receiver)
				{
					this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteTriggerMessage(this.TriggerID, objectSyncComponent.ObjectID, this.UsesAssembly ? this.fsm.Fsm.GetFsmInt("AssemblyID").Value : 0);
				}
				if (base.name == "VINP_Block" && GamePlayer.Instance.PickedUpObject != null && GamePlayer.Instance.PickedUpObject == base.gameObject)
				{
					GamePlayer.Instance.DropStolenObject();
				}
				return false;
			}, false, false);
		}

		private void SendFluidSync()
		{
			this.FluidValue = this.fsm.Fsm.GetFsmFloat(this.FluidName).Value;
			NetLocalPlayer.Instance.WriteFSMFloatMessage(this.TriggerID, "Trigger", this.FluidValue);
		}

		public void ReadFluidSyncMessage(float newValue)
		{
			this.FluidValue = newValue;
			this.fsm.Fsm.GetFsmFloat(this.FluidName).Value = newValue;
		}

		public void ReadTriggerMessage(int partId, int assembId)
		{
			GameObject gameObject = ObjectSyncManager.Instance.ObjectIDs[partId].gameObject;
			if (gameObject == null)
			{
				Logger.Debug(string.Format("[Trigger] Error reading trigger message on {0}, partId {1}", base.name, partId));
				return;
			}
			if (this.UsesAssembly)
			{
				GameVehicleDatabase.Instance.RivettAssembly.Fsm.GetFsmGameObject("ActivePart").Value = gameObject;
				this.fsm.Fsm.GetFsmInt("AssemblyID").Value = assembId;
			}
			else
			{
				this.fsm.Fsm.GetFsmGameObject("ActivePart").Value = gameObject;
			}
			ObjectSyncManager.Instance.ObjectIDs[partId].enabled = false;
			this.receiver = true;
			this.fsm.SendEvent("MP_Install 1");
			if (this.UsesAssembly)
			{
				GameVehicleDatabase.Instance.RivettAssembly.SendEvent("MP_State 1");
			}
		}

		public Trigger()
		{
		}

		[CompilerGenerated]
		private bool <HookEvents>b__12_0()
		{
			ObjectSyncComponent objectSyncComponent = (this.UsesAssembly ? GameVehicleDatabase.Instance.RivettAssembly.Fsm.GetFsmGameObject("ActivePart").Value.GetComponent<ObjectSyncComponent>() : this.fsm.Fsm.GetFsmGameObject("ActivePart").Value.GetComponent<ObjectSyncComponent>());
			objectSyncComponent.enabled = false;
			if (this.receiver)
			{
				this.receiver = false;
			}
			else
			{
				NetLocalPlayer.Instance.WriteTriggerMessage(this.TriggerID, objectSyncComponent.ObjectID, this.UsesAssembly ? this.fsm.Fsm.GetFsmInt("AssemblyID").Value : 0);
			}
			if (base.name == "VINP_Block" && GamePlayer.Instance.PickedUpObject != null && GamePlayer.Instance.PickedUpObject == base.gameObject)
			{
				GamePlayer.Instance.DropStolenObject();
			}
			return false;
		}

		public int TriggerID;

		public PlayMakerFSM fsm;

		private bool UsesAssembly;

		private bool IsSetup;

		private bool receiver;

		private bool HasSyncableFluid;

		private string FluidName = "";

		private float FluidValue;

		private float timer = 0.5f;
	}
}
