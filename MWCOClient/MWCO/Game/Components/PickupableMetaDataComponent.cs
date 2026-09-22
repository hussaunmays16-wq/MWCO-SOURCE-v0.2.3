using System;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Components
{
	internal class PickupableMetaDataComponent : MonoBehaviour
	{
		public GamePickupableDatabase.PrefabDesc PrefabDescriptor
		{
			get
			{
				ErrorHandling.Assert(this.prefabId != -1, string.Format("Prefab id is not set for {0}, id {1}!", base.name, this.prefabId));
				return GamePickupableDatabase.Instance.GetPickupablePrefab(this.prefabId);
			}
		}

		public PickupableMetaDataComponent()
		{
		}

		public int prefabId = -1;
	}
}
