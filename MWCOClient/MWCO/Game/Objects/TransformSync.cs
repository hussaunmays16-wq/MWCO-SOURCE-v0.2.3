using System;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class TransformSync : ISyncedObject
	{
		public TransformSync(GameObject go)
		{
			this.gameObject = go;
			this.HookEvents();
		}

		private void HookEvents()
		{
		}

		public Transform ObjectTransform()
		{
			return this.gameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return true;
		}

		public bool CanSync()
		{
			return true;
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
	}
}
