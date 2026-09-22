using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class Consumable
	{
		public Consumable(GameObject go)
		{
			this.itemGO = go;
			this.HookEvents();
		}

		public void HookEvents()
		{
			PlayMakerFSM[] components = this.itemGO.GetComponents<PlayMakerFSM>();
			for (int i = 0; i < components.Length; i++)
			{
				PlayMakerFSM fsm = components[i];
				if (fsm.Fsm.Name == "Use")
				{
					EventHook.AddWithSync(fsm, "Destroy", null, false);
					if (this.itemGO.name.StartsWith("juice"))
					{
						EventHook.AddWithSync(fsm, "Empty", delegate
						{
							fsm.Fsm.GetFsmBool("ContainsJuice").Value = false;
							fsm.Fsm.GetFsmBool("ContainsKilju").Value = false;
							return false;
						}, false);
					}
				}
			}
		}

		private GameObject itemGO;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass2_0
		{
			public <>c__DisplayClass2_0()
			{
			}

			internal bool <HookEvents>b__0()
			{
				this.fsm.Fsm.GetFsmBool("ContainsJuice").Value = false;
				this.fsm.Fsm.GetFsmBool("ContainsKilju").Value = false;
				return false;
			}

			public PlayMakerFSM fsm;
		}
	}
}
