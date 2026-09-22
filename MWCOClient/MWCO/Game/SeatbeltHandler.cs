using System;
using MWCO.Network;
using UnityEngine;

namespace MWCO.Game
{
	public class SeatbeltHandler : MonoBehaviour
	{
		private void OnEnable()
		{
			NetLocalPlayer.Instance.isSeatbelted = true;
		}

		private void OnDisable()
		{
			NetLocalPlayer.Instance.isSeatbelted = false;
		}

		public SeatbeltHandler()
		{
		}
	}
}
