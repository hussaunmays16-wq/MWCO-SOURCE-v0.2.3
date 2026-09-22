using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO
{
	internal static class PlayMakerUtils
	{
		public static void AddNewGlobalTransition(PlayMakerFSM fsm, FsmEvent ev, string stateName)
		{
			FsmTransition[] fsmGlobalTransitions = fsm.FsmGlobalTransitions;
			List<FsmTransition> list = new List<FsmTransition>();
			foreach (FsmTransition fsmTransition in fsmGlobalTransitions)
			{
				list.Add(fsmTransition);
			}
			list.Add(new FsmTransition
			{
				FsmEvent = ev,
				ToState = stateName
			});
			fsm.Fsm.GlobalTransitions = list.ToArray();
		}

		public static void AddNewAction(FsmState state, FsmStateAction action, bool addToBottom = false)
		{
			try
			{
				FsmStateAction[] actions = state.Actions;
				List<FsmStateAction> list = new List<FsmStateAction>();
				if (!addToBottom)
				{
					list.Add(action);
				}
				foreach (FsmStateAction fsmStateAction in actions)
				{
					list.Add(fsmStateAction);
				}
				if (addToBottom)
				{
					list.Add(action);
				}
				state.Actions = list.ToArray();
			}
			catch (Exception ex)
			{
				Logger.Debug("Error addine new action state " + state.Name + " seems to have broken actions." + ex.Message);
			}
		}

		public static void RemoveEvent(PlayMakerFSM fsm, string eventName)
		{
			FsmTransition[] fsmGlobalTransitions = fsm.FsmGlobalTransitions;
			List<FsmTransition> list = new List<FsmTransition>();
			foreach (FsmTransition fsmTransition in fsmGlobalTransitions)
			{
				if (fsmTransition.EventName != eventName)
				{
					list.Add(fsmTransition);
				}
			}
			fsm.Fsm.GlobalTransitions = list.ToArray();
			FsmEvent[] events = fsm.Fsm.Events;
			List<FsmEvent> list2 = new List<FsmEvent>();
			foreach (FsmEvent fsmEvent in events)
			{
				if (fsmEvent.Name != eventName)
				{
					list2.Add(fsmEvent);
				}
			}
			fsm.Fsm.Events = list2.ToArray();
		}

		public static void SetToState(GameObject gameObject, string fsmName, string state)
		{
			string text = state + "-MWCO";
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(gameObject, fsmName);
			FsmEvent @event = playmakerScriptByName.Fsm.GetEvent(text);
			PlayMakerUtils.AddNewGlobalTransition(playmakerScriptByName, @event, state);
			playmakerScriptByName.SendEvent(text);
			PlayMakerUtils.RemoveEvent(playmakerScriptByName, text);
		}
	}
}
