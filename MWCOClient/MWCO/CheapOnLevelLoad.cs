using System;
using UnityEngine;

namespace MWCO
{
	public class CheapOnLevelLoad : MonoBehaviour
	{
		private void Start()
		{
			MPController.Instance.OnGameLoad();
		}

		private void Update()
		{
		}

		public CheapOnLevelLoad()
		{
		}
	}
}
