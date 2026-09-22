using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class GarageDoor : ISyncedObject
	{
		public GarageDoor(GameObject go)
		{
			this.gameObject = go.transform.parent.gameObject;
			this.rigidbody = this.gameObject.GetComponent<Rigidbody>();
			this.lastRotation = this.gameObject.transform.localRotation.z;
			this.HookEvents(go);
		}

		private void HookEvents(GameObject go)
		{
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(go, "Use");
			EventHook.Add(playmakerScriptByName, "Open", delegate
			{
				go.GetComponent<ObjectSyncComponent>().TakeSyncControl();
				return false;
			}, false, false);
			EventHook.Add(playmakerScriptByName, "Close", delegate
			{
				go.GetComponent<ObjectSyncComponent>().TakeSyncControl();
				return false;
			}, false, false);
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
			if ((double)(this.lastRotation - this.gameObject.transform.localRotation.z) > 0.005 || (double)(this.lastRotation - this.gameObject.transform.localRotation.z) < -0.005)
			{
				this.lastRotation = this.gameObject.transform.localRotation.z;
				return true;
			}
			return false;
		}

		public bool ShouldTakeOwnership()
		{
			return true;
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

		[CompilerGenerated]
		private sealed class <>c__DisplayClass4_0
		{
			public <>c__DisplayClass4_0()
			{
			}

			internal bool <HookEvents>b__0()
			{
				this.go.GetComponent<ObjectSyncComponent>().TakeSyncControl();
				return false;
			}

			internal bool <HookEvents>b__1()
			{
				this.go.GetComponent<ObjectSyncComponent>().TakeSyncControl();
				return false;
			}

			public GameObject go;
		}
	}
}
