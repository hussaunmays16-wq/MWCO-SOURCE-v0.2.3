using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class Boot : ISyncedObject
	{
		public Boot(GameObject go)
		{
			this.gameObject = go.transform.parent.gameObject;
			this.rigidbody = this.gameObject.GetComponent<Rigidbody>();
			if (this.gameObject.GetTopmostParent().name == "BACHGLOTZ(1905kg)")
			{
				this.yRotation = true;
			}
			this.lastRotation = (this.yRotation ? this.gameObject.transform.localRotation.y : this.gameObject.transform.localRotation.x);
			this.HookEvents(go);
		}

		private void HookEvents(GameObject go)
		{
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(go, "Use");
			EventHook.AddWithSync(playmakerScriptByName, "Open hood", delegate
			{
				go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}, false);
			EventHook.AddWithSync(playmakerScriptByName, "State 2", delegate
			{
				go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}, false);
			EventHook.AddWithSync(playmakerScriptByName, "Drop", delegate
			{
				go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}, false);
		}

		public Transform ObjectTransform()
		{
			return this.gameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return false;
		}

		public bool CanSync()
		{
			if (this.yRotation)
			{
				if ((double)(this.lastRotation - this.gameObject.transform.localEulerAngles.y) > 0.005 || (double)(this.lastRotation - this.gameObject.transform.localEulerAngles.x) < -0.005)
				{
					this.lastRotation = this.gameObject.transform.localEulerAngles.y;
					return true;
				}
				return false;
			}
			else
			{
				if ((double)(this.lastRotation - this.gameObject.transform.localEulerAngles.x) > 0.005 || (double)(this.lastRotation - this.gameObject.transform.localEulerAngles.x) < -0.005)
				{
					this.lastRotation = this.gameObject.transform.localEulerAngles.x;
					return true;
				}
				return false;
			}
		}

		public bool ShouldTakeOwnership()
		{
			return false;
		}

		public float[] ReturnSyncedVariables(bool sendAllVariables)
		{
			return null;
		}

		public void HandleSyncedVariables(float[] variables, CSteamID sender)
		{
		}

		public void OwnerSetToRemote()
		{
		}

		public void OwnerRemoved()
		{
		}

		public void SyncTakenByForce()
		{
		}

		public void ConstantSyncChanged(bool newValue)
		{
		}

		private GameObject gameObject;

		private Rigidbody rigidbody;

		private float lastRotation;

		private bool yRotation;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass5_0
		{
			public <>c__DisplayClass5_0()
			{
			}

			internal bool <HookEvents>b__0()
			{
				this.go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}

			internal bool <HookEvents>b__1()
			{
				this.go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}

			internal bool <HookEvents>b__2()
			{
				this.go.GetComponent<ObjectSyncComponent>().SyncTakenByForce();
				return false;
			}

			public GameObject go;
		}
	}
}
