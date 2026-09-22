using System;
using System.Reflection;
using HutongGames.PlayMaker;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO
{
	internal class Utils
	{
		private static void PrintObjectFields(int level, object obj, Utils.PrintInfo print)
		{
			if (obj == null)
			{
				return;
			}
			if (level > 10)
			{
				print(level + 1, "Out of depth limit.");
				return;
			}
			foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
			{
				object value = fieldInfo.GetValue(obj);
				Type fieldType = fieldInfo.FieldType;
				if (value == null)
				{
					print(level, fieldType.FullName + " " + fieldInfo.Name + " = null");
				}
				else
				{
					string text = "";
					if (value is NamedVariable)
					{
						text = text + " [Named variable: " + ((NamedVariable)value).Name + "]";
					}
					print(level, string.Concat(new string[]
					{
						fieldType.FullName,
						" ",
						fieldInfo.Name,
						" = ",
						value.ToString(),
						text
					}));
					if (fieldType.IsClass && (fieldType.Namespace == null || !fieldType.Namespace.StartsWith("System")))
					{
						Utils.PrintObjectFields(level + 1, value, print);
					}
				}
			}
		}

		private static string GetNamedVariableValueAsString(NamedVariable var)
		{
			if (var == null)
			{
				return "null";
			}
			object obj = null;
			if (var is FsmBool)
			{
				obj = ((FsmBool)var).Value;
			}
			if (var is FsmColor)
			{
				obj = ((FsmColor)var).Value;
			}
			if (var is FsmFloat)
			{
				obj = ((FsmFloat)var).Value;
			}
			if (var is FsmGameObject)
			{
				obj = ((FsmGameObject)var).Value;
			}
			if (var is FsmInt)
			{
				obj = ((FsmInt)var).Value;
			}
			if (var is FsmMaterial)
			{
				obj = ((FsmMaterial)var).Value;
			}
			if (var is FsmObject)
			{
				obj = ((FsmObject)var).Value;
			}
			if (var is FsmQuaternion)
			{
				obj = ((FsmQuaternion)var).Value;
			}
			if (var is FsmRect)
			{
				obj = ((FsmRect)var).Value;
			}
			if (var is FsmString)
			{
				obj = ((FsmString)var).Value;
			}
			if (var is FsmTexture)
			{
				obj = ((FsmTexture)var).Value;
			}
			if (var is FsmVector2)
			{
				obj = ((FsmVector2)var).Value;
			}
			if (var is FsmVector3)
			{
				obj = ((FsmVector3)var).Value;
			}
			if (obj == null)
			{
				return "null";
			}
			return obj.ToString();
		}

		private static void PrintPlaymakerFsmComponent(PlayMakerFSM pmfsm, int level, Utils.PrintInfo print)
		{
			pmfsm.Fsm.Init(pmfsm);
			print(level, "PMFSM Name: " + pmfsm.FsmName);
			print(level, "Active state: " + pmfsm.ActiveStateName);
			print(level, string.Format("Initialized: {0}", pmfsm.Fsm.Initialized));
			Logger.Info("EVENTS");
			foreach (FsmEvent fsmEvent in pmfsm.FsmEvents)
			{
				if (fsmEvent == null)
				{
					print(level, "Null event!");
				}
				else
				{
					print(level, string.Concat(new string[] { "Event Name: ", fsmEvent.Name, " (", fsmEvent.Path, ")" }));
				}
			}
			Logger.Info("GT");
			foreach (FsmTransition fsmTransition in pmfsm.FsmGlobalTransitions)
			{
				if (fsmTransition == null)
				{
					print(level, "Null global transition!");
				}
				else
				{
					print(level, "Global transition: " + fsmTransition.EventName + " > " + fsmTransition.ToState);
				}
			}
			Logger.Info("STATES");
			foreach (FsmState fsmState in pmfsm.FsmStates)
			{
				if (fsmState == null)
				{
					print(level, "Null state!");
				}
				else
				{
					Logger.Info("PRE TRANS");
					print(level, string.Format("State Name: {0} (fsm: {1}, go: {2})", fsmState.Name, fsmState.Fsm, fsmState.Fsm.GameObject));
					foreach (FsmTransition fsmTransition2 in fsmState.Transitions)
					{
						if (fsmTransition2 == null)
						{
							print(level + 1, "Null transition!");
						}
						else
						{
							print(level + 1, "Transition: " + fsmTransition2.EventName + " > " + fsmTransition2.ToState);
						}
					}
					Logger.Info("POST TRANS");
					Logger.Info("PRE ACTIONS");
					foreach (FsmStateAction fsmStateAction in fsmState.Actions)
					{
						if (fsmStateAction == null)
						{
							print(level + 1, "Null action!");
						}
						else
						{
							print(level + 1, string.Concat(new string[]
							{
								"Action Name: ",
								fsmStateAction.Name,
								" (",
								fsmStateAction.GetType().FullName,
								")"
							}));
							Utils.PrintObjectFields(level + 2, fsmStateAction, print);
						}
					}
					Logger.Info("POST ACTIONS");
				}
			}
			Logger.Info("VARIABLES");
			print(level, "Variables:");
			foreach (NamedVariable namedVariable in pmfsm.FsmVariables.GetAllNamedVariables())
			{
				print(level + 1, namedVariable.Name + " = " + Utils.GetNamedVariableValueAsString(namedVariable));
			}
		}

		private static void PrintTransformComponents(Transform trans, int level, Utils.PrintInfo print)
		{
			Component[] components = trans.GetComponents<Component>();
			int i = 0;
			while (i < components.Length)
			{
				Component component = components[i];
				print(level + 1, string.Concat(new string[]
				{
					"C ",
					component.GetType().FullName,
					" [",
					component.tag,
					"]"
				}));
				if (component is PlayMakerFSM)
				{
					try
					{
						Utils.PrintPlaymakerFsmComponent((PlayMakerFSM)component, level + 2, print);
						goto IL_DF;
					}
					catch (Exception ex)
					{
						Logger.Info("XXX");
						Logger.Info(ex.StackTrace);
						goto IL_DF;
					}
					goto IL_84;
				}
				goto IL_84;
				IL_DF:
				i++;
				continue;
				IL_84:
				if (component is Animation)
				{
					foreach (object obj in ((Animation)component))
					{
						AnimationState animationState = (AnimationState)obj;
						print(level + 2, "Animation state: " + animationState.name);
					}
					goto IL_DF;
				}
				goto IL_DF;
			}
		}

		private static void PrintTransformChildren(Transform trans, int level, Utils.PrintInfo print)
		{
			for (int i = 0; i < trans.childCount; i++)
			{
				Utils.PrintTransformTree(trans.GetChild(i), level + 1, print);
			}
		}

		public static void PrintTransformTree(Transform trans, int level, Utils.PrintInfo print)
		{
			if (trans == null)
			{
				return;
			}
			print(level, string.Format("> {0} [{1}, {2}, {3}]", new object[]
			{
				trans.name,
				trans.tag,
				trans.gameObject.activeSelf ? "active" : "inactive",
				trans.gameObject.GetInstanceID()
			}));
			Utils.PrintTransformComponents(trans, level, print);
			Utils.PrintTransformChildren(trans, level, print);
		}

		public static PlayMakerFSM GetPlaymakerScriptByName(GameObject go, string name)
		{
			foreach (PlayMakerFSM playMakerFSM in go.GetComponentsInChildren<PlayMakerFSM>())
			{
				if (playMakerFSM.FsmName == name)
				{
					return playMakerFSM;
				}
			}
			return null;
		}

		public static PlayMakerFSM GetPlaymakerScriptByGoName(GameObject go, string name)
		{
			foreach (PlayMakerFSM playMakerFSM in go.GetComponentsInChildren<PlayMakerFSM>())
			{
				if (playMakerFSM.name == name)
				{
					return playMakerFSM;
				}
			}
			return null;
		}

		public static bool IsReceiver(PlayMakerFSM fsm)
		{
			return fsm.Fsm.LastTransition.EventName.StartsWith("MP_");
		}

		public static Vector3Message GameVec3ToNet(Vector3 v3)
		{
			return new Vector3Message
			{
				x = v3.x,
				y = v3.y,
				z = v3.z
			};
		}

		public static Vector3 NetVec3ToGame(Vector3Message msg)
		{
			Vector3 vector = default(Vector3);
			vector.x = msg.x;
			vector.y = msg.y;
			vector.z = msg.z;
			return vector;
		}

		public static QuaternionMessage GameQuatToNet(Quaternion q)
		{
			return new QuaternionMessage
			{
				w = q.w,
				x = q.x,
				y = q.y,
				z = q.z
			};
		}

		public static Quaternion NetQuatToGame(QuaternionMessage msg)
		{
			Quaternion quaternion = default(Quaternion);
			quaternion.w = msg.w;
			quaternion.x = msg.x;
			quaternion.y = msg.y;
			quaternion.z = msg.z;
			return quaternion;
		}

		public static ColorMessage GameColorToNet(Color color)
		{
			return new ColorMessage
			{
				r = color.r,
				g = color.g,
				b = color.b,
				a = color.a
			};
		}

		public static Color NetColorToGame(ColorMessage msg)
		{
			Color color = default(Color);
			color.r = msg.r;
			color.g = msg.g;
			color.b = msg.b;
			color.a = msg.a;
			return color;
		}

		public static int StringJenkinsHash(string str)
		{
			int num = 0;
			int num2 = 0;
			while (num != str.Length)
			{
				num2 += (int)str[num++];
				num2 += num2 << 10;
				num2 ^= num2 >> 6;
			}
			num2 += num2 << 3;
			num2 ^= num2 >> 11;
			return num2 + (num2 << 15);
		}

		public static bool IsGameObjectHierarchyMatching(GameObject obj, string hierarchy)
		{
			Transform transform = obj.transform;
			string[] array = hierarchy.Split(new char[] { '/' });
			for (int i = array.Length; i > 0; i--)
			{
				if (transform == null)
				{
					return false;
				}
				if (!(array[i - 1] == "*"))
				{
					if (transform.name != array[i - 1])
					{
						return false;
					}
					transform = transform.parent;
				}
			}
			return true;
		}

		public static string P2PSessionErrorToString(EP2PSessionError sessionError)
		{
			switch (sessionError)
			{
			case 0:
				return "none";
			case 1:
				return "not running app";
			case 2:
				return "no rights to app";
			case 3:
				return "user not logged in";
			case 4:
				return "timeout";
			default:
				return "unknown";
			}
		}

		public Utils()
		{
		}

		public const int LAYER_DEFAULT = 1;

		public const int LAYER_TRANSPARENT_FX = 2;

		public const int LAYER_IGNORE_RAYCAST = 4;

		public const int LAYER_WATER = 16;

		public const int LAYER_UI = 32;

		public delegate void PrintInfo(int level, string data);
	}
}
