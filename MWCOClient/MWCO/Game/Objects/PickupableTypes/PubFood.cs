using System;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class PubFood
	{
		public PubFood(GameObject go)
		{
			this.FoodGO = go;
			this.HookEvents();
		}

		private void HookEvents()
		{
			EventHook.AddWithSync(Utils.GetPlaymakerScriptByName(this.FoodGO, "Use"), "State 2", null, false);
		}

		private GameObject FoodGO;
	}
}
