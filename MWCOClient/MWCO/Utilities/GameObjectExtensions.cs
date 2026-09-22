using System;
using UnityEngine;

namespace MWCO.Utilities
{
	public static class GameObjectExtensions
	{
		public static Vector3 SetX(this Vector3 vec, float x)
		{
			vec..ctor(x, vec.y, vec.z);
			return vec;
		}

		public static Vector3 SetY(this Vector3 vec, float y)
		{
			return new Vector3(vec.x, y, vec.z);
		}

		public static Vector3 SetZ(this Vector3 vec, float z)
		{
			return new Vector3(vec.x, vec.y, z);
		}

		public static GameObject MakePersistent(this GameObject self)
		{
			Object.DontDestroyOnLoad(self);
			return self;
		}

		public static bool IsPrefab(this GameObject gameObject)
		{
			return !(gameObject == null) && !gameObject.activeInHierarchy && gameObject.transform.root == gameObject.transform;
		}

		public static Transform GetChild(this Transform transform, params int[] indexes)
		{
			Transform transform2 = transform;
			for (int i = 0; i < indexes.Length; i++)
			{
				transform2 = transform2.GetChild(indexes[i]);
			}
			return transform2;
		}

		public static void DestroyIndexes(this Transform transform, params int[] indexes)
		{
			for (int i = 0; i < indexes.Length; i++)
			{
				Object.Destroy(transform.GetChild(indexes[i]).gameObject);
			}
		}

		public static GameObject[] InstantiateMultiple(this Transform transform, int count, Vector3 newPosition = default(Vector3), bool includeOriginalAtStart = false)
		{
			GameObject[] array = new GameObject[includeOriginalAtStart ? (count + 1) : count];
			for (int i = ((includeOriginalAtStart > false) ? 1 : 0); i < array.Length; i++)
			{
				array[i] = Object.Instantiate<GameObject>(transform.gameObject);
				array[i].transform.SetParent(transform.parent, false);
				if (newPosition != default(Vector3))
				{
					array[i].transform.localPosition = newPosition;
				}
			}
			if (includeOriginalAtStart)
			{
				array[0] = transform.gameObject;
			}
			return array;
		}

		public static T EnsureComponentExists<T>(this GameObject gameObject) where T : Component
		{
			T component = gameObject.GetComponent<T>();
			if (component)
			{
				return component;
			}
			return gameObject.AddComponent<T>();
		}

		public static string ConstructFullPathToObject(this GameObject gameObject)
		{
			return gameObject.transform.ConstructFullPathToObject();
		}

		public static string ConstructFullPathToObject(this Transform transform)
		{
			if (!transform)
			{
				return string.Empty;
			}
			string text = transform.name;
			transform = transform.parent;
			while (transform)
			{
				text = transform.name + "/" + text;
				transform = transform.parent;
			}
			return text;
		}
	}
}
