using System;
using UnityEngine;

public static class GameObjectExtensions
{
	public static GameObject GetTopmostParent(this GameObject gameObject)
	{
		Transform transform = gameObject.transform.parent;
		while (transform != null)
		{
			gameObject = transform.gameObject;
			transform = gameObject.transform.parent;
		}
		return gameObject;
	}
}
