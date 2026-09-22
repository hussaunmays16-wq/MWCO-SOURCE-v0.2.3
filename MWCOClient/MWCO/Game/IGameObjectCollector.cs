using System;
using UnityEngine;

namespace MWCO.Game
{
	internal interface IGameObjectCollector
	{
		void CollectGameObject(GameObject gameObject, int assignedID = 0);

		void DestroyObjects();

		void DestroyObject(GameObject gameObject);
	}
}
