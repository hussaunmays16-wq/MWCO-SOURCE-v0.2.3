using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MWCO.Game.Objects
{
	public class ComponentUtil
	{
		public static T CopyComponent<T>(T original, GameObject destination) where T : Component
		{
			Type type = original.GetType();
			T t = destination.GetComponent(type) as T;
			if (!t)
			{
				t = destination.AddComponent(type) as T;
			}
			foreach (FieldInfo fieldInfo in ComponentUtil.GetAllFields(type))
			{
				if (!fieldInfo.IsStatic)
				{
					fieldInfo.SetValue(t, fieldInfo.GetValue(original));
				}
			}
			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (propertyInfo.CanWrite && propertyInfo.CanWrite && !(propertyInfo.Name == "name"))
				{
					propertyInfo.SetValue(t, propertyInfo.GetValue(original, null), null);
				}
			}
			return t;
		}

		public static IEnumerable<FieldInfo> GetAllFields(Type t)
		{
			if (t == null)
			{
				return Enumerable.Empty<FieldInfo>();
			}
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			return t.GetFields(bindingFlags).Concat(ComponentUtil.GetAllFields(t.BaseType));
		}

		public ComponentUtil()
		{
		}
	}
}
