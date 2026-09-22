using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Game.Places;
using MWCO.Network;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class Train : ISyncedObject
	{
		public Train(GameObject go, ObjectSyncComponent osc)
		{
			this.gameObject = go;
			this.syncComponent = osc;
			this.rigidbody = go.GetComponent<Rigidbody>();
			foreach (PlayMakerFSM playMakerFSM in go.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.FsmName == "LOD")
				{
					playMakerFSM.gameObject.AddComponent<LODManager>().Initialize(playMakerFSM, 100f, false, "LOD", "Object");
				}
				if (playMakerFSM.FsmName == "Move")
				{
					this.moveFsm = playMakerFSM;
				}
			}
			this.HookEvents();
		}

		private void HookEvents()
		{
			EventHook.Add(this.moveFsm, "State 1", delegate
			{
				this.moving = true;
				return false;
			}, false, false);
			EventHook.Add(this.moveFsm, "State 2", delegate
			{
				this.moving = true;
				return false;
			}, false, false);
			EventHook.Add(this.moveFsm, "Delay", delegate
			{
				this.moving = false;
				return false;
			}, false, false);
			EventHook.Add(this.moveFsm, "Delay2", delegate
			{
				this.moving = false;
				return false;
			}, false, false);
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
			try
			{
				if (!NetManager.Instance.IsHost)
				{
					this.moveFsm.enabled = false;
					return false;
				}
				this.moveFsm.enabled = true;
			}
			catch
			{
			}
			return this.moving;
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

		[CompilerGenerated]
		private bool <HookEvents>b__6_0()
		{
			this.moving = true;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__6_1()
		{
			this.moving = true;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__6_2()
		{
			this.moving = false;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__6_3()
		{
			this.moving = false;
			return false;
		}

		private GameObject gameObject;

		private Rigidbody rigidbody;

		private ObjectSyncComponent syncComponent;

		private PlayMakerFSM moveFsm;

		private bool moving = true;
	}
}
