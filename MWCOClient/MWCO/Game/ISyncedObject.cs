using System;
using Steamworks;
using UnityEngine;

namespace MWCO.Game
{
	public interface ISyncedObject
	{
		Transform ObjectTransform();

		bool PeriodicSyncEnabled();

		bool CanSync();

		bool ShouldTakeOwnership();

		float[] ReturnSyncedVariables(bool sendFullSync);

		void HandleSyncedVariables(float[] variables, CSteamID sender);

		void OwnerSetToRemote();

		void OwnerRemoved();

		void SyncTakenByForce();

		void ConstantSyncChanged(bool newValue);
	}
}
