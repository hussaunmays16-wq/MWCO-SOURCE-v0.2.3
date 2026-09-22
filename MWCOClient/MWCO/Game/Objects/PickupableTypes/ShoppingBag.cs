using System;
using MWCO.Game.Components;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class ShoppingBag
	{
		public ShoppingBag(GameObject go)
		{
			this.ShoppingBagGO = go;
			this.syncComponent = go.GetComponent<ObjectSyncComponent>();
			this.HookEvents();
		}

		private void HookEvents()
		{
			foreach (PlayMakerFSM playMakerFSM in this.ShoppingBagGO.GetComponents<PlayMakerFSM>())
			{
				if (playMakerFSM.FsmName == "Use")
				{
					this.bagFsm = playMakerFSM;
				}
			}
			if (this.bagFsm != null && !this.ShoppingBagGO.name.StartsWith("shopping"))
			{
				this.ShoppingBagGO.name.StartsWith("spark");
			}
		}

		private GameObject ShoppingBagGO;

		private ObjectSyncComponent syncComponent;

		public PlayMakerFSM bagFsm;
	}
}
