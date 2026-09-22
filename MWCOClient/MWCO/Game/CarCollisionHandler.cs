using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	public class CarCollisionHandler : MonoBehaviour
	{
		private void OnCollisionEnter(Collision collision)
		{
			if (NetLocalPlayer.Instance == null)
			{
				return;
			}
			if (collision.relativeVelocity.magnitude >= ((NetLocalPlayer.Instance.isSeatbelted || base.name.StartsWith("JONNEZ")) ? (this.impactThresholdHigh + 33f) : this.impactThresholdHigh))
			{
				if (collision.collider.tag == "PART" || collision.collider.tag == "TOOL" || collision.collider.tag == "ITEM")
				{
					return;
				}
				if (collision.collider.name.ToLower().StartsWith("grass") && collision.relativeVelocity.magnitude <= this.impactThresholdHigh * 2f)
				{
					return;
				}
				if (NetLocalPlayer.Instance.currentVehicle == this.osc)
				{
					Logger.Debug(string.Format("Died at a collision of {0} by {1}", collision.relativeVelocity.magnitude, collision.collider.name));
					this.HandleDeath();
				}
			}
			if (!this.cooldown && NetLocalPlayer.Instance.currentVehicle == this.osc)
			{
				Logger.Debug("Collided with " + collision.gameObject.name);
				if (collision.gameObject.name.StartsWith("haybale"))
				{
					collision.gameObject.GetComponent<ObjectSyncComponent>().TakeSyncControl();
				}
				this.cooldown = true;
				base.StartCoroutine(this.CooldownTimer());
			}
		}

		private void HandleDeath()
		{
			DevTools.EnableDeath();
		}

		private IEnumerator CooldownTimer()
		{
			CarCollisionHandler.<CooldownTimer>d__5 <CooldownTimer>d__ = new CarCollisionHandler.<CooldownTimer>d__5(0);
			<CooldownTimer>d__.<>4__this = this;
			return <CooldownTimer>d__;
		}

		public CarCollisionHandler()
		{
		}

		public float impactThresholdHigh = 22f;

		public ObjectSyncComponent osc;

		private bool cooldown;

		[CompilerGenerated]
		private sealed class <CooldownTimer>d__5 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <CooldownTimer>d__5(int <>1__state)
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
				CarCollisionHandler carCollisionHandler = this;
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
				carCollisionHandler.cooldown = false;
				carCollisionHandler.StopAllCoroutines();
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

			public CarCollisionHandler <>4__this;
		}
	}
}
