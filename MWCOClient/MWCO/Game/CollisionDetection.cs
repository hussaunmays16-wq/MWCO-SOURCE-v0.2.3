using System;
using MWCO.Game.Objects;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	public class CollisionDetection : MonoBehaviour
	{
		private void OnCollisionEnter(Collision collision)
		{
			PlayerVehicle.MPCarController component = collision.gameObject.GetComponent<PlayerVehicle.MPCarController>();
			if (component != null)
			{
				foreach (NetPlayer netPlayer in NetManager.Instance.players)
				{
					if (netPlayer != null && netPlayer.currentVehicle != null && netPlayer.currentVehicle.gameObject.GetTopmostParent().name == collision.gameObject.name)
					{
						Logger.Debug("Not syncing this as it already has an owner!");
						return;
					}
				}
				component.osc.TakeSyncControl();
			}
		}

		public CollisionDetection()
		{
		}
	}
}
