using System;
using MWCO.Game.Components;
using MWCO.Network;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class Hose
	{
		public Hose(GameObject go)
		{
			this.hose = go;
			this.osc = this.hose.GetComponent<ObjectSyncComponent>();
			this.HookEvents();
		}

		private void HookEvents()
		{
			if (NetManager.Instance.IsOnline && !NetManager.Instance.IsHost)
			{
				this.osc.RequestObjectSync();
			}
		}

		private GameObject hose;

		private ObjectSyncComponent osc;
	}
}
