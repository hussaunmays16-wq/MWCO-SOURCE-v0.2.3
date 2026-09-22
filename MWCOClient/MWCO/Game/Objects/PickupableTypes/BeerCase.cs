using System;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class BeerCase
	{
		public int UsedBottles
		{
			get
			{
				return this.beerCaseFSM.FsmVariables.FindFsmInt("DestroyedBottles").Value;
			}
			set
			{
				this.beerCaseFSM.FsmVariables.FindFsmInt("DestroyedBottles").Value = value;
			}
		}

		public BeerCase(GameObject go)
		{
			this.beerCaseGO = go;
			this.osc = this.beerCaseGO.GetComponent<ObjectSyncComponent>();
			ErrorHandling.Assert(this.beerCaseFSM = Utils.GetPlaymakerScriptByName(go, "Use"), "Beer case FSM not found!");
			this.HookEvents();
		}

		private void HookEvents()
		{
			EventHook.AddWithSync(this.beerCaseFSM, "Wait player", null, false);
			EventHook.AddWithSync(this.beerCaseFSM, "State 3", null, false);
			EventHook.AddWithSync(this.beerCaseFSM, "Remove bottle", null, false);
			EventHook.AddWithSync(this.beerCaseFSM, "Check bottles", null, false);
			if (NetManager.Instance.IsOnline && !NetManager.Instance.IsHost)
			{
				this.osc.RequestObjectSync();
			}
		}

		public void RemoveBottles(int count)
		{
			int num = 0;
			while (count > this.UsedBottles)
			{
				if (this.UsedBottles == 23)
				{
					Logger.Error(string.Format("Failed to remove bottle! UsedBottles: {0}", this.UsedBottles));
					return;
				}
				GameObject gameObject = this.beerCaseGO.transform.GetChild(num).gameObject;
				num++;
				if (gameObject != null)
				{
					Object.Destroy(gameObject);
					int usedBottles = this.UsedBottles;
					this.UsedBottles = usedBottles + 1;
				}
				else
				{
					Logger.Error("Failed to remove bottle! No bottle GameObjects found!");
				}
			}
		}

		private GameObject beerCaseGO;

		private ObjectSyncComponent osc;

		private PlayMakerFSM beerCaseFSM;
	}
}
