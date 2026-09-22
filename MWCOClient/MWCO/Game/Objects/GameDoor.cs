using System;
using HutongGames.PlayMaker;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class GameDoor
	{
		public GameObject GameObject
		{
			get
			{
				return this.gameObject;
			}
		}

		public Vector3 Position
		{
			get
			{
				Vector3 vector;
				try
				{
					vector = this.gameObject.transform.position;
				}
				catch (Exception)
				{
					Chat.Instance.AddMessage("Can Not Fetch GameDoor Position(51)! Crash Avoided But It May Become Buggy", MessageSeverity.Error);
					vector = Vector3.zero;
				}
				return vector;
			}
		}

		public GameDoor(GameDoorsManager manager, GameObject gameObject)
		{
			this.manager = manager;
			this.gameObject = gameObject;
			this.fsm = Utils.GetPlaymakerScriptByName(gameObject, "Use");
			if (this.fsm.Fsm.HasEvent("MPOPEN"))
			{
				Logger.Debug("Failed to hook game door " + gameObject.name + ". It is already hooked.");
				return;
			}
			FsmEvent @event = this.fsm.Fsm.GetEvent("MPOPEN");
			FsmEvent event2 = this.fsm.Fsm.GetEvent("MPCLOSE");
			PlayMakerUtils.AddNewGlobalTransition(this.fsm, @event, "Open door");
			PlayMakerUtils.AddNewGlobalTransition(this.fsm, event2, "Close door");
			PlayMakerUtils.AddNewAction(this.fsm.Fsm.GetState("Open door"), new GameDoor.OnOpenDoorsAction(this), false);
			PlayMakerUtils.AddNewAction(this.fsm.Fsm.GetState("Close door"), new GameDoor.OnCloseDoorsAction(this), false);
		}

		public void Open(bool open)
		{
			if (open)
			{
				this.fsm.SendEvent("MPOPEN");
				return;
			}
			this.fsm.SendEvent("MPCLOSE");
		}

		private GameObject gameObject;

		private GameDoorsManager manager;

		private PlayMakerFSM fsm;

		private const string OPEN_EVENT_NAME = "OPEN";

		private const string CLOSE_EVENT_NAME = "CLOSE";

		private const string MP_OPEN_EVENT_NAME = "MPOPEN";

		private const string MP_CLOSE_EVENT_NAME = "MPCLOSE";

		private class OnOpenDoorsAction : FsmStateAction
		{
			public OnOpenDoorsAction(GameDoor door)
			{
				this.gameDoor = door;
			}

			public override void OnEnter()
			{
				base.Finish();
				if (base.State.Fsm.LastTransition.EventName != "OPEN")
				{
					return;
				}
				this.gameDoor.manager.HandleDoorsAction(this.gameDoor, true);
			}

			private GameDoor gameDoor;
		}

		private class OnCloseDoorsAction : FsmStateAction
		{
			public OnCloseDoorsAction(GameDoor door)
			{
				this.gameDoor = door;
			}

			public override void OnEnter()
			{
				base.Finish();
				if (base.State.Fsm.LastTransition.EventName != "CLOSE")
				{
					return;
				}
				this.gameDoor.manager.HandleDoorsAction(this.gameDoor, false);
			}

			private GameDoor gameDoor;
		}
	}
}
