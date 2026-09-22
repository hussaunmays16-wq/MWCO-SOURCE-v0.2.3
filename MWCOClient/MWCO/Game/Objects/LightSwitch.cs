using System;
using HutongGames.PlayMaker;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class LightSwitch
	{
		public GameObject GameObject
		{
			get
			{
				return this.go;
			}
		}

		public bool SwitchStatus
		{
			get
			{
				return this.fsm.FsmVariables.FindFsmBool("Switch").Value;
			}
		}

		public Vector3 Position
		{
			get
			{
				return this.go.transform.position;
			}
		}

		public LightSwitch(GameObject gameObject)
		{
			this.go = gameObject;
			this.fsm = Utils.GetPlaymakerScriptByName(this.go, "Use");
			if (this.fsm.Fsm.HasEvent("MPSWITCH"))
			{
				Logger.Info("Light switch " + this.go.name + " is already hooked!");
				return;
			}
			FsmEvent @event = this.fsm.Fsm.GetEvent("MPSWITCH");
			PlayMakerUtils.AddNewGlobalTransition(this.fsm, @event, "Switch");
			PlayMakerUtils.AddNewAction(this.fsm.Fsm.GetState("Switch"), new LightSwitch.OnLightSwitchUseAction(this), false);
		}

		public void TurnOn(bool on)
		{
			Logger.Debug(string.Format("Toggled light switch, on: {0}", on));
			if (this.SwitchStatus != on)
			{
				this.fsm.SendEvent("MPSWITCH");
			}
		}

		private GameObject go;

		private PlayMakerFSM fsm;

		public LightSwitch.OnLightSwitchUse onLightSwitchUse;

		private const string EVENT_NAME = "MPSWITCH";

		public delegate void OnLightSwitchUse(GameObject lswitch, bool turnOn);

		private class OnLightSwitchUseAction : FsmStateAction
		{
			public OnLightSwitchUseAction(LightSwitch theLightSwitch)
			{
				this.lightSwitch = theLightSwitch;
			}

			public override void OnEnter()
			{
				base.Finish();
				Logger.Debug(string.Format("Light switch set to: {0}", !this.lightSwitch.SwitchStatus));
				if (base.State.Fsm.LastTransition.EventName == "MPSWITCH")
				{
					return;
				}
				this.lightSwitch.onLightSwitchUse(this.lightSwitch.go, !this.lightSwitch.SwitchStatus);
			}

			private LightSwitch lightSwitch;
		}
	}
}
