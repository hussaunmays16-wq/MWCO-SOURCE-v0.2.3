using System;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class CarDoor : ISyncedObject
	{
		public CarDoor(GameObject go)
		{
			this.gameObject = go.transform.parent.gameObject;
			this.rigidbody = this.gameObject.GetComponent<Rigidbody>();
			this.lastRotation = this.gameObject.transform.localEulerAngles.y;
			this.HookEvents(go);
		}

		private void HookEvents(GameObject go)
		{
			this.doorFSM = Utils.GetPlaymakerScriptByName(go, "Use");
			EventHook.AddWithSync(this.doorFSM, "Open door", null, false);
			EventHook.AddWithSync(this.doorFSM, "Open door 2", null, false);
			EventHook.AddWithSync(this.doorFSM, "Open door 3", null, false);
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
			return false;
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
			this.owner = false;
		}

		public void OwnerRemoved()
		{
		}

		public void SyncTakenByForce()
		{
			this.owner = false;
		}

		public void ConstantSyncChanged(bool newValue)
		{
		}

		private GameObject gameObject;

		private Rigidbody rigidbody;

		private float lastRotation;

		private PlayMakerFSM doorFSM;

		private bool owner;
	}
}
