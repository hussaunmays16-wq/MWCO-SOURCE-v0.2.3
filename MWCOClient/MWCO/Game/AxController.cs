using System;
using UnityEngine;

namespace MWCO.Game
{
	public class AxController : MonoBehaviour
	{
		private void OnEnable()
		{
			MPController.Instance.holdingAx = true;
		}

		private void OnDisable()
		{
			MPController.Instance.holdingAx = false;
		}

		public AxController()
		{
		}
	}
}
