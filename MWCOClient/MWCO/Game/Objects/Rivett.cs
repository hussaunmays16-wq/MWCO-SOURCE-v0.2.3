using System;
using MWCO.Game.Components;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class Rivett : ISyncedObject
	{
		public Rivett(GameObject go, ObjectSyncComponent osc)
		{
			this.gameObject = go;
			this.syncComponent = osc;
		}

		public bool CanSync()
		{
			return false;
		}

		public void ConstantSyncChanged(bool newValue)
		{
		}

		public void HandleSyncedVariables(float[] variables, CSteamID sender)
		{
		}

		public Transform ObjectTransform()
		{
			return this.gameObject.transform;
		}

		public void OwnerRemoved()
		{
		}

		public void OwnerSetToRemote()
		{
		}

		public bool PeriodicSyncEnabled()
		{
			return false;
		}

		public float[] ReturnSyncedVariables(bool sendFullSync)
		{
			return null;
		}

		public bool ShouldTakeOwnership()
		{
			return false;
		}

		public void SyncTakenByForce()
		{
		}

		public GameObject gameObject;

		private ObjectSyncComponent syncComponent;
	}
}
