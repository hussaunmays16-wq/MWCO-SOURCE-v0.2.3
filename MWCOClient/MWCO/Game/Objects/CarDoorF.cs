using System;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class CarDoorF : ISyncedObject
	{
		public CarDoorF(GameObject go)
		{
			this.gameObject = go.transform.parent.gameObject;
			this.rigidbody = this.gameObject.GetComponent<Rigidbody>();
			this.lastRotation = this.gameObject.transform.localRotation.x;
			this.HookEvents(go);
		}

		private void HookEvents(GameObject go)
		{
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(go, "Use");
			EventHook.AddWithSync(playmakerScriptByName, "Open door", null, false);
			EventHook.AddWithSync(playmakerScriptByName, "Open door 2", null, false);
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
	}
}
