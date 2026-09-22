using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game
{
	internal class EventHook
	{
		public EventHook()
		{
			EventHook.Instance = this;
		}

		public static void Add(PlayMakerFSM fsm, string eventName, Func<bool> action, bool actionOnExit = false, bool addToBottom = false)
		{
			if (fsm == null)
			{
				ErrorHandling.Assert(true, "EventHook Add: Failed to hook event. (FSM is null)");
				return;
			}
			FsmState state = fsm.Fsm.GetState(eventName);
			if (state != null)
			{
				EventHook.CheckEnabled(fsm.gameObject);
				PlayMakerUtils.AddNewAction(state, new EventHook.CustomAction(action, eventName, actionOnExit), addToBottom);
				EventHook.CheckOver(fsm.gameObject);
				FsmEvent @event = fsm.Fsm.GetEvent("MP_" + eventName);
				PlayMakerUtils.AddNewGlobalTransition(fsm, @event, eventName);
			}
		}

		private static void CheckEnabled(GameObject go)
		{
			if (go.transform.parent != null)
			{
				EventHook.ParentObject = go.transform.parent;
				go.transform.SetParent(null);
			}
			if (!go.activeSelf)
			{
				go.SetActive(true);
				EventHook.wasEnabled = true;
			}
		}

		private static void CheckOver(GameObject go)
		{
			if (EventHook.ParentObject)
			{
				go.transform.SetParent(EventHook.ParentObject);
			}
			if (EventHook.wasEnabled)
			{
				go.SetActive(false);
				EventHook.wasEnabled = false;
			}
			EventHook.ParentObject = null;
		}

		public static void AddWithSync(PlayMakerFSM fsm, string eventName, Func<bool> action = null, bool addToBottom = false)
		{
			if (fsm == null)
			{
				ErrorHandling.Assert(true, "EventHook AddWithSync: Failed to hook event. (FSM is null)");
			}
			FsmState state = fsm.Fsm.GetState(eventName);
			if (state != null)
			{
				bool flag = false;
				int num = EventHook.Instance.fsmEvents.Count + 1;
				if (EventHook.Instance.fsms.ContainsValue(fsm))
				{
					flag = true;
					IEnumerable<KeyValuePair<int, PlayMakerFSM>> enumerable = EventHook.Instance.fsms;
					Func<KeyValuePair<int, PlayMakerFSM>, bool> <>9__0;
					Func<KeyValuePair<int, PlayMakerFSM>, bool> func;
					if ((func = <>9__0) == null)
					{
						func = (<>9__0 = (KeyValuePair<int, PlayMakerFSM> entry) => entry.Value == fsm);
					}
					using (IEnumerator<KeyValuePair<int, PlayMakerFSM>> enumerator = enumerable.Where(func).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							KeyValuePair<int, PlayMakerFSM> keyValuePair = enumerator.Current;
							num = keyValuePair.Key;
						}
					}
				}
				int count = EventHook.Instance.fsmEvents.Count;
				EventHook.Instance.fsms.Add(EventHook.Instance.fsms.Count + 1, fsm);
				EventHook.Instance.fsmEvents.Add(EventHook.Instance.fsmEvents.Count + 1, eventName);
				EventHook.CheckEnabled(fsm.gameObject);
				PlayMakerUtils.AddNewAction(state, new EventHook.CustomActionSync(EventHook.Instance.fsms.Count, EventHook.Instance.fsmEvents.Count, action), addToBottom);
				EventHook.CheckOver(fsm.gameObject);
				FsmEvent @event = fsm.Fsm.GetEvent("MP_" + eventName);
				PlayMakerUtils.AddNewGlobalTransition(fsm, @event, eventName);
				if (!NetManager.Instance.IsHost && NetManager.Instance.IsOnline && !flag)
				{
					NetLocalPlayer.Instance.RequestEventHookSync(num);
				}
			}
		}

		public static void HandleEventSync(int fsmID, int fsmEventID, string fsmEventName = "none")
		{
			try
			{
				if (fsmEventID == -1)
				{
					EventHook.Instance.fsms[fsmID].SendEvent("MP_" + fsmEventName);
				}
				else
				{
					EventHook.Instance.fsms[fsmID].SendEvent("MP_" + EventHook.Instance.fsmEvents[fsmEventID]);
				}
			}
			catch
			{
				ErrorHandling.Assert(true, string.Format("Handle event sync failed! FSM not found at ID: {0} - Ensure both players are using a new save created on the same version of My Winter Car. Any installed mods could also cause this error.", fsmID));
			}
		}

		public static void SendSync(CSteamID sender, int fsmID)
		{
			try
			{
				string activeStateName = EventHook.Instance.fsms[fsmID].Fsm.ActiveStateName;
				NetLocalPlayer.Instance.SendEventHookSyncToSteamID(sender, fsmID, -1, activeStateName);
			}
			catch
			{
				Logger.Debug("Sync was request for an event, but the FSM wasn't found on this client!");
			}
		}

		public static void SyncAllEvents(PlayMakerFSM fsm, Func<bool> action = null)
		{
			if (fsm == null)
			{
				ErrorHandling.Assert(true, "EventHook SyncAllEvents: Failed to hook event. (FSM is null)");
				return;
			}
			FsmState[] fsmStates = fsm.FsmStates;
			Logger.Debug(string.Format("Asked to SyncAllEvents for {0}({1}) has {2} states.", fsm.gameObject.name, fsm.name, fsmStates.Length));
			for (int i = 0; i < fsmStates.Length; i++)
			{
				if (!fsmStates[i].Name.ToLower().Contains("wait button"))
				{
					EventHook.AddWithSync(fsm, fsmStates[i].Name, () => action != null && action(), false);
				}
			}
		}

		public Dictionary<int, PlayMakerFSM> fsms = new Dictionary<int, PlayMakerFSM>();

		public Dictionary<int, string> fsmEvents = new Dictionary<int, string>();

		public static EventHook Instance;

		private static bool wasEnabled;

		private static bool wasEnabledParent;

		private static Transform ParentObject;

		public class CustomAction : FsmStateAction
		{
			public CustomAction(Func<bool> a, string eName, bool onExit)
			{
				this.action = a;
				this.eventName = eName;
				this.actionOnExit = onExit;
			}

			public bool RunAction(Func<bool> action)
			{
				return action();
			}

			public override void OnEnter()
			{
				if (!this.actionOnExit && this.RunAction(this.action))
				{
					return;
				}
				base.Finish();
			}

			public override void OnExit()
			{
				if (this.actionOnExit && this.RunAction(this.action))
				{
					return;
				}
				base.Finish();
			}

			private Func<bool> action;

			private string eventName;

			private bool actionOnExit;
		}

		public class CustomActionSync : FsmStateAction
		{
			public CustomActionSync(int id, int eventID, Func<bool> a)
			{
				this.fsmID = id;
				this.fsmEventID = eventID;
				this.action = a;
			}

			public bool RunAction(Func<bool> action)
			{
				bool flag;
				try
				{
					flag = action();
				}
				catch (Exception ex)
				{
					string text = string.Format("Could not run function. {0} ", this.fsmID);
					Exception ex2 = ex;
					Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
					flag = false;
				}
				return flag;
			}

			public override void OnEnter()
			{
				try
				{
					if (this.action == null || !this.RunAction(this.action))
					{
						PlayMakerFSM playMakerFSM = EventHook.Instance.fsms[this.fsmID];
						string text;
						if (playMakerFSM == null)
						{
							text = null;
						}
						else
						{
							Fsm fsm = playMakerFSM.Fsm;
							if (fsm == null)
							{
								text = null;
							}
							else
							{
								FsmTransition lastTransition = fsm.LastTransition;
								text = ((lastTransition != null) ? lastTransition.EventName : null);
							}
						}
						if (!(text == "MP_" + EventHook.Instance.fsmEvents[this.fsmEventID]))
						{
							NetLocalPlayer instance = NetLocalPlayer.Instance;
							if (instance != null)
							{
								instance.SendEventHookSync(this.fsmID, this.fsmEventID, "none");
							}
							base.Finish();
						}
					}
				}
				catch (Exception ex)
				{
					string[] array = new string[8];
					array[0] = "[EventHook] Could not OnEnter function. ";
					array[1] = EventHook.Instance.fsms[this.fsmID].gameObject.name;
					array[2] = " Parent? ";
					int num = 3;
					Transform parent = EventHook.Instance.fsms[this.fsmID].transform.parent;
					array[num] = ((parent != null) ? parent.name : null);
					array[4] = " Event(MP_";
					array[5] = EventHook.Instance.fsmEvents[this.fsmEventID];
					array[6] = ")";
					int num2 = 7;
					Exception ex2 = ex;
					array[num2] = ((ex2 != null) ? ex2.ToString() : null);
					Logger.Debug(string.Concat(array));
				}
			}

			private int fsmID;

			private int fsmEventID;

			private Func<bool> action;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public <>c__DisplayClass10_0()
			{
			}

			internal bool <AddWithSync>b__0(KeyValuePair<int, PlayMakerFSM> entry)
			{
				return entry.Value == this.fsm;
			}

			public PlayMakerFSM fsm;

			public Func<KeyValuePair<int, PlayMakerFSM>, bool> <>9__0;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass13_0
		{
			public <>c__DisplayClass13_0()
			{
			}

			internal bool <SyncAllEvents>b__0()
			{
				return this.action != null && this.action();
			}

			public Func<bool> action;
		}
	}
}
