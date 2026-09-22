using System;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game;
using UnityEngine;

namespace MWCO.UI
{
	public class CustomButton : MonoBehaviour
	{
		public void Setup(int type, bool saveFsm = false)
		{
			this.collider = base.GetComponent<BoxCollider>();
			this.type = type;
			string text = "";
			switch (type)
			{
			case 0:
				text = "Multiplayer";
				this.collider.center = new Vector3(-0.26f, 0.01f, 0f);
				this.collider.size = new Vector3(0.5f, 0.06f, 0.1f);
				break;
			case 1:
				if (MPController.Instance.AccessLevel > 2)
				{
					text = "Watermark: ON";
					this.collider.center = new Vector3(-0.45f, 0.01f, 0f);
					this.collider.size = new Vector3(0.9f, 0.06f, 0.1f);
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
				break;
			case 2:
				text = "Quit";
				break;
			case 3:
				text = "Begin";
				saveFsm = true;
				break;
			case 4:
				text = "Continue";
				saveFsm = true;
				break;
			}
			if (!saveFsm)
			{
				Object.Destroy(base.GetComponent<PlayMakerFSM>());
			}
			else
			{
				this.SetupFSM();
			}
			base.transform.GetChild(0).GetComponent<TextMesh>().text = text;
			base.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>()
				.text = text;
		}

		private void SetupFSM()
		{
			this.buttonFsm = base.GetComponent<PlayMakerFSM>();
			this.buttonFsm.enabled = false;
			if (base.name != "ButtonBegin")
			{
				FsmState fsmState = this.buttonFsm.Fsm.GetState("Reset globals 2");
				if (fsmState != null)
				{
					Func<bool> func = delegate
					{
						this.buttonFsm.SendEvent("MP_Reset globals");
						return false;
					};
					PlayMakerUtils.AddNewAction(fsmState, new EventHook.CustomAction(func, "Reset globals 2", false), false);
					FsmEvent @event = this.buttonFsm.Fsm.GetEvent("MP_Reset globals 2");
					PlayMakerUtils.AddNewGlobalTransition(this.buttonFsm, @event, "Reset globals 2");
				}
				fsmState = this.buttonFsm.Fsm.GetState("Reset globals");
				if (fsmState != null)
				{
					Func<bool> func2 = delegate
					{
						this.buttonFsm.SendEvent("MP_Init Steam");
						return false;
					};
					PlayMakerUtils.AddNewAction(fsmState, new EventHook.CustomAction(func2, "Reset globals", false), false);
					FsmEvent event2 = this.buttonFsm.Fsm.GetEvent("MP_Reset globals");
					PlayMakerUtils.AddNewGlobalTransition(this.buttonFsm, event2, "Reset globals");
				}
				fsmState = this.buttonFsm.Fsm.GetState("Init Steam");
				if (fsmState != null)
				{
					Func<bool> func3 = delegate
					{
						this.buttonFsm.SendEvent("MP_State 1");
						return false;
					};
					PlayMakerUtils.AddNewAction(fsmState, new EventHook.CustomAction(func3, "Init Steam", false), false);
					FsmEvent event3 = this.buttonFsm.Fsm.GetEvent("MP_Init Steam");
					PlayMakerUtils.AddNewGlobalTransition(this.buttonFsm, event3, "Init Steam");
				}
				fsmState = this.buttonFsm.Fsm.GetState("State 1");
				if (fsmState != null)
				{
					FsmEvent event4 = this.buttonFsm.Fsm.GetEvent("MP_State 1");
					PlayMakerUtils.AddNewGlobalTransition(this.buttonFsm, event4, "State 1");
					return;
				}
			}
			else if (this.buttonFsm.Fsm.GetState("State 2") != null)
			{
				FsmEvent event5 = this.buttonFsm.Fsm.GetEvent("MP_State 2");
				PlayMakerUtils.AddNewGlobalTransition(this.buttonFsm, event5, "State 2");
			}
		}

		private void OnMouseEnter()
		{
			base.transform.GetChild(0).localScale = new Vector3(0.9f, 0.9f, 0.9f);
		}

		private void OnMouseExit()
		{
			base.transform.GetChild(0).localScale = Vector3.one;
		}

		private void OnMouseDown()
		{
			MPController.Instance.ButtonPressed(this.type, this);
		}

		public void ChangeText(string text)
		{
			base.transform.GetChild(0).GetComponent<TextMesh>().text = text;
			base.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>()
				.text = text;
		}

		public CustomButton()
		{
		}

		[CompilerGenerated]
		private bool <SetupFSM>b__4_0()
		{
			this.buttonFsm.SendEvent("MP_Reset globals");
			return false;
		}

		[CompilerGenerated]
		private bool <SetupFSM>b__4_1()
		{
			this.buttonFsm.SendEvent("MP_Init Steam");
			return false;
		}

		[CompilerGenerated]
		private bool <SetupFSM>b__4_2()
		{
			this.buttonFsm.SendEvent("MP_State 1");
			return false;
		}

		public BoxCollider collider;

		public int type;

		public PlayMakerFSM buttonFsm;
	}
}
