using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MWCO.Network;
using UnityEngine;

namespace MWCO.Game
{
	public class SleepRequest : MonoBehaviour
	{
		public void Start()
		{
			this.fsm = base.GetComponent<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in GameObject.Find("GUI").GetComponentsInChildren<PlayMakerFSM>())
			{
				if (playMakerFSM.FsmName == "SetText" && playMakerFSM.gameObject.name == "Interaction")
				{
					this.textFsm = playMakerFSM;
					return;
				}
			}
		}

		public void Update()
		{
			if (Input.GetKeyDown(120) && !this.fsm.enabled && !this.Allowed)
			{
				NetManager.Instance.CancelSleepRequest();
				this.fsm.enabled = true;
				this.fsm.Fsm.GetFsmBool("PlayerStop").Value = false;
				this.fsm.Fsm.GetFsmBool("PlayerSleeps").Value = false;
				this.fsm.SendEvent("MP_State 1");
			}
			if (!this.fsm.enabled && !this.Allowed)
			{
				this.textFsm.Fsm.GetFsmString("GUIinteraction").Value = "Press 'X' To Cancel Sleep Request";
			}
		}

		public void GetPositions()
		{
			if (!this.Allowed)
			{
				NetManager.Instance.SleepRequest(this);
				this.fsm.Fsm.GetFsmBool("PlayerStop").Value = true;
				this.fsm.enabled = false;
				return;
			}
			this.fsm.enabled = true;
			base.StartCoroutine(this.SleepActivate());
		}

		private IEnumerator SleepActivate()
		{
			SleepRequest.<SleepActivate>d__6 <SleepActivate>d__ = new SleepRequest.<SleepActivate>d__6(0);
			<SleepActivate>d__.<>4__this = this;
			return <SleepActivate>d__;
		}

		public SleepRequest()
		{
		}

		public bool Allowed;

		private PlayMakerFSM fsm;

		private PlayMakerFSM textFsm;

		[CompilerGenerated]
		private sealed class <SleepActivate>d__6 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <SleepActivate>d__6(int <>1__state)
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
				SleepRequest sleepRequest = this;
				if (num == 0)
				{
					this.<>1__state = -1;
					this.<>2__current = new WaitForSeconds(0.2f);
					this.<>1__state = 1;
					return true;
				}
				if (num != 1)
				{
					return false;
				}
				this.<>1__state = -1;
				sleepRequest.fsm.enabled = true;
				sleepRequest.fsm.SendEvent("MP_Get positions");
				return false;
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

			public SleepRequest <>4__this;
		}
	}
}
