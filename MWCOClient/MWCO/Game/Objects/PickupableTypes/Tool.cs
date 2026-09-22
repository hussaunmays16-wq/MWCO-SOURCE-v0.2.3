using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	public class Tool : MonoBehaviour
	{
		public Tool()
		{
			Tool.Instance = this;
		}

		public void SetHoistAngle(float angle, bool up)
		{
			this.hoistFsm.Fsm.GetFsmFloat("Angle").Value = angle;
			base.transform.GetChild(2).transform.localEulerAngles = new Vector3(angle, 0f, 0f);
			if (up)
			{
				this.receiver = true;
				this.hoistFsm.SendEvent("MP_Up");
				this.receiver = false;
			}
		}

		private IEnumerator Timer()
		{
			Tool.<Timer>d__5 <Timer>d__ = new Tool.<Timer>d__5(0);
			<Timer>d__.<>4__this = this;
			return <Timer>d__;
		}

		public void LateHookEvents()
		{
			this.hoistFsm = Utils.GetPlaymakerScriptByName(base.gameObject, "Usage");
			EventHook.Add(this.hoistFsm, "Up", delegate
			{
				if (!this.receiver)
				{
					base.StartCoroutine(this.Timer());
				}
				return false;
			}, false, false);
			EventHook.Add(this.hoistFsm, "Down", delegate
			{
				if (!this.receiver)
				{
					NetLocalPlayer.Instance.WriteHouseSwitchMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), PlayerHouse.SwitchIDs.Hoist, false, -1f);
				}
				return false;
			}, false, false);
			EventHook.Add(this.hoistFsm, "State 1", delegate
			{
				if (!this.receiver)
				{
					NetLocalPlayer.Instance.WriteHouseSwitchMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), PlayerHouse.SwitchIDs.Hoist, false, this.hoistFsm.Fsm.GetFsmFloat("Angle").Value);
				}
				else
				{
					this.receiver = false;
				}
				return false;
			}, false, false);
			foreach (PlayMakerFSM playMakerFSM in base.gameObject.transform.GetChild(1).GetChild(2).GetChild(0)
				.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				EventHook.AddWithSync(playMakerFSM, "Screw 2", null, false);
				EventHook.AddWithSync(playMakerFSM, "Unscrew 2", null, false);
			}
		}

		[CompilerGenerated]
		private bool <LateHookEvents>b__6_0()
		{
			if (!this.receiver)
			{
				base.StartCoroutine(this.Timer());
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LateHookEvents>b__6_1()
		{
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteHouseSwitchMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), PlayerHouse.SwitchIDs.Hoist, false, -1f);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <LateHookEvents>b__6_2()
		{
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteHouseSwitchMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), PlayerHouse.SwitchIDs.Hoist, false, this.hoistFsm.Fsm.GetFsmFloat("Angle").Value);
			}
			else
			{
				this.receiver = false;
			}
			return false;
		}

		public static Tool Instance;

		public PlayMakerFSM hoistFsm;

		public bool receiver;

		[CompilerGenerated]
		private sealed class <Timer>d__5 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <Timer>d__5(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				this.<>1__state = -2;
			}

			bool IEnumerator.MoveNext()
			{
				int num = this.<>1__state;
				Tool tool = this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					this.<>1__state = -1;
				}
				else
				{
					this.<>1__state = -1;
				}
				if (!tool.hoistFsm.Fsm.GetState("Up").Active)
				{
					return false;
				}
				Logger.Debug(string.Format("Sending angle {0}", tool.hoistFsm.Fsm.GetFsmFloat("Angle").Value));
				NetLocalPlayer.Instance.WriteHouseSwitchMessage(tool.gameObject.GetComponent<ObjectSyncComponent>(), PlayerHouse.SwitchIDs.Hoist, true, tool.hoistFsm.Fsm.GetFsmFloat("Angle").Value);
				this.<>2__current = new WaitForEndOfFrame();
				this.<>1__state = 1;
				return true;
			}

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			private int <>1__state;

			private object <>2__current;

			public Tool <>4__this;
		}
	}
}
