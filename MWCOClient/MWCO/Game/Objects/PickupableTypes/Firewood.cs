using System;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class Firewood
	{
		public Firewood(GameObject go)
		{
			this.itemGO = go;
			this.HookEvents();
		}

		public void HookEvents()
		{
		}

		private GameObject itemGO;
	}
}
