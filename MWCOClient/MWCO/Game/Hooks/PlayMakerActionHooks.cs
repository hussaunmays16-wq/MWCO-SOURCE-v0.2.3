using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Hooks
{
	internal static class PlayMakerActionHooks
	{
		private static bool IsWhitelistedFile(string fileName)
		{
			return fileName == "options.txt";
		}

		public static void Install()
		{
			if (PlayMakerActionHooks.hooked)
			{
				return;
			}
			PlayMakerActionHooks.hooked = true;
			ErrorHandling.Invoke("Hook PlayMaker actions", delegate
			{
				Dictionary<string, Type> dictionary = (Dictionary<string, Type>)typeof(ActionData).GetField("ActionTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				dictionary.Add("HutongGames.PlayMaker.Actions.CreateObject", typeof(PlayMakerActionHooks.MyCreateObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.DestroySelf", typeof(PlayMakerActionHooks.MyDestroySelf));
				dictionary.Add("HutongGames.PlayMaker.Actions.DestroyObject", typeof(PlayMakerActionHooks.MyDestroyObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.ActivateGameObject", typeof(PlayMakerActionHooks.MyActivateGameObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.SetPosition", typeof(PlayMakerActionHooks.MySetPosition));
			});
		}

		private static bool hooked;

		private class MyCreateObject : CreateObject
		{
			public override void OnEnter()
			{
				base.OnEnter();
				GameCallbacks.OnPlayMakerObjectCreate onPlayMakerObjectCreate = GameCallbacks.onPlayMakerObjectCreate;
				if (onPlayMakerObjectCreate == null)
				{
					return;
				}
				onPlayMakerObjectCreate(this.storeObject.Value, this.gameObject.Value);
			}

			public MyCreateObject()
			{
			}
		}

		private class MyDestroyObject : DestroyObject
		{
			public override void OnEnter()
			{
				if (this.gameObject.Value != null)
				{
					GameCallbacks.OnPlayMakerObjectDestroy onPlayMakerObjectDestroy = GameCallbacks.onPlayMakerObjectDestroy;
					if (onPlayMakerObjectDestroy != null)
					{
						onPlayMakerObjectDestroy(this.gameObject.Value);
					}
				}
				base.OnEnter();
			}

			public MyDestroyObject()
			{
			}
		}

		private class MyDestroySelf : DestroySelf
		{
			public override void OnEnter()
			{
				GameCallbacks.OnPlayMakerObjectDestroy onPlayMakerObjectDestroy = GameCallbacks.onPlayMakerObjectDestroy;
				if (onPlayMakerObjectDestroy != null)
				{
					onPlayMakerObjectDestroy(base.Owner);
				}
				base.OnEnter();
			}

			public MyDestroySelf()
			{
			}
		}

		private class MyActivateGameObject : ActivateGameObject
		{
			public override void OnEnter()
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
				if (ownerDefaultTarget == null)
				{
					base.Finish();
					return;
				}
				GameCallbacks.OnPlayMakerObjectActivate onPlayMakerObjectActivate = GameCallbacks.onPlayMakerObjectActivate;
				if (onPlayMakerObjectActivate != null)
				{
					onPlayMakerObjectActivate(ownerDefaultTarget, this.activate.Value);
				}
				base.OnEnter();
			}

			public MyActivateGameObject()
			{
			}
		}

		private class MySetPosition : SetPosition
		{
			public override void OnEnter()
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
				if (ownerDefaultTarget == null)
				{
					base.Finish();
					return;
				}
				Vector3 value = this.vector.Value;
				if (!this.x.IsNone)
				{
					value.x = this.x.Value;
				}
				if (!this.y.IsNone)
				{
					value.y = this.y.Value;
				}
				if (!this.z.IsNone)
				{
					value.z = this.z.Value;
				}
				GameCallbacks.OnPlayMakerSetPosition onPlayMakerSetPosition = GameCallbacks.onPlayMakerSetPosition;
				if (onPlayMakerSetPosition != null)
				{
					onPlayMakerSetPosition(ownerDefaultTarget, value, this.space);
				}
				base.OnEnter();
			}

			public MySetPosition()
			{
			}
		}

		private class MySaveAudioClip : SaveAudioClip
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveAudioClip()
			{
			}
		}

		private class MySaveBool : SaveBool
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveBool()
			{
			}
		}

		private class MySaveBoxCollider : SaveBoxCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveBoxCollider()
			{
			}
		}

		private class MySaveCapsuleCollider : SaveCapsuleCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveCapsuleCollider()
			{
			}
		}

		private class MySaveColor : SaveColor
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveColor()
			{
			}
		}

		private class MySaveFloat : SaveFloat
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveFloat()
			{
			}
		}

		private class MySaveInt : SaveInt
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveInt()
			{
			}
		}

		private class MySaveMaterial : SaveMaterial
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveMaterial()
			{
			}
		}

		private class MySaveMeshCollider : SaveMeshCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveMeshCollider()
			{
			}
		}

		private class MySaveQuaternion : SaveQuaternion
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveQuaternion()
			{
			}
		}

		private class MySaveSphereCollider : SaveSphereCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveSphereCollider()
			{
			}
		}

		private class MySaveString : SaveString
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveString()
			{
			}
		}

		private class MySaveTexture : SaveTexture
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveTexture()
			{
			}
		}

		private class MySaveTransform : SaveTransform
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveTransform()
			{
			}
		}

		private class MySaveVector2 : SaveVector2
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveVector2()
			{
			}
		}

		private class MySaveVector3 : SaveVector3
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MySaveVector3()
			{
			}
		}

		public class MyLoadAudioClip : LoadAudioClip
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadAudioClip()
			{
			}
		}

		public class MyLoadBool : LoadBool
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadBool()
			{
			}
		}

		public class MyLoadBoxCollider : LoadBoxCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadBoxCollider()
			{
			}
		}

		public class MyLoadCapsuleCollider : LoadCapsuleCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadCapsuleCollider()
			{
			}
		}

		public class MyLoadColor : LoadColor
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadColor()
			{
			}
		}

		public class MyLoadFloat : LoadFloat
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadFloat()
			{
			}
		}

		public class MyLoadInt : LoadInt
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadInt()
			{
			}
		}

		public class MyLoadMaterial : LoadMaterial
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadMaterial()
			{
			}
		}

		public class MyLoadMeshCollider : LoadMeshCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadMeshCollider()
			{
			}
		}

		public class MyLoadQuaternion : LoadQuaternion
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadQuaternion()
			{
			}
		}

		public class MyLoadSphereCollider : LoadSphereCollider
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadSphereCollider()
			{
			}
		}

		public class MyLoadString : LoadString
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadString()
			{
			}
		}

		public class MyLoadTexture : LoadTexture
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadTexture()
			{
			}
		}

		public class MyLoadTransform : LoadTransform
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadTransform()
			{
			}
		}

		public class MyLoadVector2 : LoadVector2
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadVector2()
			{
			}
		}

		public class MyLoadVector3 : LoadVector3
		{
			public override void OnEnter()
			{
				if (MPController.Instance.CanUseSave || PlayMakerActionHooks.IsWhitelistedFile(this.saveFile.Value))
				{
					base.OnEnter();
					return;
				}
				base.Finish();
			}

			public MyLoadVector3()
			{
			}
		}

		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			// Note: this type is marked as 'beforefieldinit'.
			static <>c()
			{
			}

			public <>c()
			{
			}

			internal void <Install>b__39_0()
			{
				Dictionary<string, Type> dictionary = (Dictionary<string, Type>)typeof(ActionData).GetField("ActionTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				dictionary.Add("HutongGames.PlayMaker.Actions.CreateObject", typeof(PlayMakerActionHooks.MyCreateObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.DestroySelf", typeof(PlayMakerActionHooks.MyDestroySelf));
				dictionary.Add("HutongGames.PlayMaker.Actions.DestroyObject", typeof(PlayMakerActionHooks.MyDestroyObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.ActivateGameObject", typeof(PlayMakerActionHooks.MyActivateGameObject));
				dictionary.Add("HutongGames.PlayMaker.Actions.SetPosition", typeof(PlayMakerActionHooks.MySetPosition));
			}

			public static readonly PlayMakerActionHooks.<>c <>9 = new PlayMakerActionHooks.<>c();

			public static ErrorHandling.InvokeCall <>9__39_0;
		}
	}
}
