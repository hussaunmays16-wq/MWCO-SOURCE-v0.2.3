using System;
using System.Runtime.CompilerServices;
using Boo.Lang;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Places
{
	internal class Shop
	{
		public Shop(GameObject shop)
		{
			Shop.Instance = this;
			this.shopGO = shop;
			foreach (Transform transform in this.shopGO.GetComponentsInChildren<Transform>())
			{
				string name = transform.name;
				if (!(name == "ActivateStore"))
				{
					if (!(name == "ActivateBar"))
					{
						if (name == "CashRegisterLogic")
						{
							if (transform.parent.name == "PubCashRegister")
							{
								this.pubRegister = transform.gameObject;
							}
						}
					}
					else
					{
						this.pubProducts = transform.gameObject;
					}
				}
			}
			foreach (PlayMakerFSM playMakerFSM in this.shopGO.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.gameObject.name == "TeimoInBar" && playMakerFSM.FsmName == "Animate")
				{
					string name2 = playMakerFSM.gameObject.name;
					string text = " ";
					PlayMakerFSM playMakerFSM2 = playMakerFSM;
					Logger.Debug(name2 + text + ((playMakerFSM2 != null) ? playMakerFSM2.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "TeimoInSled" && playMakerFSM.FsmName == "Move")
				{
					this.teimoB = playMakerFSM;
					string name3 = playMakerFSM.gameObject.name;
					string text2 = " ";
					PlayMakerFSM playMakerFSM3 = playMakerFSM;
					Logger.Debug(name3 + text2 + ((playMakerFSM3 != null) ? playMakerFSM3.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Bet" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "GFX_Store")
				{
					this.slotBet = playMakerFSM;
					string name4 = playMakerFSM.gameObject.name;
					string text3 = " ";
					PlayMakerFSM playMakerFSM4 = playMakerFSM;
					Logger.Debug(name4 + text3 + ((playMakerFSM4 != null) ? playMakerFSM4.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Start" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "GFX_Store")
				{
					this.slotStart = playMakerFSM;
					string name5 = playMakerFSM.gameObject.name;
					string text4 = " ";
					PlayMakerFSM playMakerFSM5 = playMakerFSM;
					Logger.Debug(name5 + text4 + ((playMakerFSM5 != null) ? playMakerFSM5.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "PayMoney" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "GFX_Store")
				{
					this.slotPay = playMakerFSM;
					string name6 = playMakerFSM.gameObject.name;
					string text5 = " ";
					PlayMakerFSM playMakerFSM6 = playMakerFSM;
					Logger.Debug(name6 + text5 + ((playMakerFSM6 != null) ? playMakerFSM6.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Cashout" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "GFX_Store")
				{
					this.slotCash = playMakerFSM;
					string name7 = playMakerFSM.gameObject.name;
					string text6 = " ";
					PlayMakerFSM playMakerFSM7 = playMakerFSM;
					Logger.Debug(name7 + text6 + ((playMakerFSM7 != null) ? playMakerFSM7.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Bet" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "SlotMachine")
				{
					this.slotBet2 = playMakerFSM;
					string name8 = playMakerFSM.gameObject.name;
					string text7 = " ";
					PlayMakerFSM playMakerFSM8 = playMakerFSM;
					Logger.Debug(name8 + text7 + ((playMakerFSM8 != null) ? playMakerFSM8.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Start" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "SlotMachine")
				{
					this.slotStart2 = playMakerFSM;
					string name9 = playMakerFSM.gameObject.name;
					string text8 = " ";
					PlayMakerFSM playMakerFSM9 = playMakerFSM;
					Logger.Debug(name9 + text8 + ((playMakerFSM9 != null) ? playMakerFSM9.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "PayMoney" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "SlotMachine")
				{
					this.slotPay2 = playMakerFSM;
					string name10 = playMakerFSM.gameObject.name;
					string text9 = " ";
					PlayMakerFSM playMakerFSM10 = playMakerFSM;
					Logger.Debug(name10 + text9 + ((playMakerFSM10 != null) ? playMakerFSM10.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Cashout" && playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.transform.parent.transform.parent.name == "SlotMachine")
				{
					this.slotCash2 = playMakerFSM;
					string name11 = playMakerFSM.gameObject.name;
					string text10 = " ";
					PlayMakerFSM playMakerFSM11 = playMakerFSM;
					Logger.Debug(name11 + text10 + ((playMakerFSM11 != null) ? playMakerFSM11.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "BreakableWindow" && playMakerFSM.FsmName == "Use")
				{
					this.breakShop = playMakerFSM;
					string name12 = playMakerFSM.gameObject.name;
					string text11 = " ";
					PlayMakerFSM playMakerFSM12 = playMakerFSM;
					Logger.Debug(name12 + text11 + ((playMakerFSM12 != null) ? playMakerFSM12.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "BreakableWindowPub" && playMakerFSM.FsmName == "Use")
				{
					this.breakPub = playMakerFSM;
					string name13 = playMakerFSM.gameObject.name;
					string text12 = " ";
					PlayMakerFSM playMakerFSM13 = playMakerFSM;
					Logger.Debug(name13 + text12 + ((playMakerFSM13 != null) ? playMakerFSM13.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "PostOrderBuy" && playMakerFSM.FsmName == "Use")
				{
					this.storePostOrderFSM = playMakerFSM;
					EventHook.Add(this.storePostOrderFSM, "State 1", delegate
					{
						try
						{
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteHostTriggerAction(69425);
							}
							else
							{
								this.receiver = false;
							}
						}
						catch
						{
						}
						return false;
					}, false, false);
					string name14 = playMakerFSM.gameObject.name;
					string text13 = " ";
					PlayMakerFSM playMakerFSM14 = playMakerFSM;
					Logger.Debug(name14 + text13 + ((playMakerFSM14 != null) ? playMakerFSM14.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.gameObject.name == "Handle" && playMakerFSM.FsmName == "Use")
				{
					EventHook.AddWithSync(playMakerFSM, "Check position", null, false);
					string name15 = playMakerFSM.gameObject.name;
					string text14 = " ";
					PlayMakerFSM playMakerFSM15 = playMakerFSM;
					Logger.Debug(name15 + text14 + ((playMakerFSM15 != null) ? playMakerFSM15.ToString() : null) + " [SHOP FSM]");
				}
				else if (playMakerFSM.FsmName == "LOD" && playMakerFSM.gameObject == this.shopGO)
				{
					playMakerFSM.gameObject.AddComponent<LODManager>().Initialize(playMakerFSM, 120f, false, "LOD", "Object");
				}
			}
			this.HookEvents();
		}

		public void PerformBuyOn(int id)
		{
			foreach (PlayMakerFSM playMakerFSM in this.pubBuyFSMs)
			{
				if (playMakerFSM.gameObject.name == (id - 20000).ToString())
				{
					playMakerFSM.SendEvent("MP_Check money");
				}
			}
		}

		private void HookEvents()
		{
			int num = 0;
			PlayMakerFSM[] componentsInChildren = this.pubProducts.GetComponentsInChildren<PlayMakerFSM>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				PlayMakerFSM fsm = componentsInChildren[i];
				if (fsm.FsmName == "Buy")
				{
					this.pubBuyFSMs.Add(fsm);
					EventHook.Add(fsm, "Check money", delegate
					{
						try
						{
							if (!this.receiver)
							{
								fsm.Fsm.GetState("Purchase").Actions[0].Enabled = true;
								NetLocalPlayer.Instance.WriteHostTriggerAction(20000 + int.Parse(fsm.gameObject.name));
							}
							else
							{
								fsm.Fsm.GetState("Purchase").Actions[0].Enabled = false;
								this.receiver = false;
							}
						}
						catch
						{
						}
						return false;
					}, false, false);
					num++;
				}
			}
			this.pubRegisterFSM = Utils.GetPlaymakerScriptByName(this.pubRegister, "Data");
			if (this.slotBet != null)
			{
				EventHook.AddWithSync(this.slotBet, "Compare", null, false);
				EventHook.AddWithSync(this.slotBet, "Set bet", null, false);
			}
			if (this.slotPay != null)
			{
				EventHook.AddWithSync(this.slotPay, "Wait button", null, false);
				EventHook.Add(this.slotPay, "Add coins", delegate
				{
					try
					{
						if (!this.receiver)
						{
							this.slotPay.Fsm.GetState("Add coins").Actions[2].Enabled = true;
							NetLocalPlayer.Instance.WriteHostTriggerAction(69421);
						}
						else
						{
							this.receiver = false;
						}
					}
					catch
					{
					}
					return false;
				}, false, false);
			}
			if (this.slotStart != null)
			{
				EventHook.AddWithSync(this.slotStart, "Check added", null, false);
			}
			if (this.slotCash != null)
			{
				EventHook.AddWithSync(this.slotCash, "Wait button", null, false);
				EventHook.AddWithSync(this.slotCash, "State 1", null, false);
				EventHook.AddWithSync(this.slotCash, "State 2", null, false);
			}
			if (this.slotBet2 != null)
			{
				EventHook.AddWithSync(this.slotBet2, "Compare", null, false);
			}
			if (this.slotPay2 != null)
			{
				EventHook.AddWithSync(this.slotPay2, "Wait button", null, false);
				EventHook.Add(this.slotPay, "Add coins", delegate
				{
					try
					{
						if (!this.receiver)
						{
							this.slotPay2.Fsm.GetState("Add coins").Actions[2].Enabled = true;
							NetLocalPlayer.Instance.WriteHostTriggerAction(69423);
						}
						else
						{
							this.receiver = false;
						}
					}
					catch
					{
					}
					return false;
				}, false, false);
			}
			if (this.slotStart2 != null)
			{
				EventHook.AddWithSync(this.slotStart2, "Check added", null, false);
				EventHook.AddWithSync(this.slotStart2, "Wait button", null, false);
			}
			if (this.slotCash2 != null)
			{
				EventHook.AddWithSync(this.slotCash2, "Wait button", null, false);
				EventHook.AddWithSync(this.slotCash2, "State 1", null, false);
				EventHook.AddWithSync(this.slotCash2, "State 2", null, false);
			}
			if (this.breakShop != null)
			{
				EventHook.AddWithSync(this.breakShop, "Break", null, false);
			}
			if (this.breakPub != null)
			{
				EventHook.AddWithSync(this.breakPub, "Break", null, false);
			}
		}

		[CompilerGenerated]
		private bool <.ctor>b__19_0()
		{
			try
			{
				if (!this.receiver)
				{
					NetLocalPlayer.Instance.WriteHostTriggerAction(69425);
				}
				else
				{
					this.receiver = false;
				}
			}
			catch
			{
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__23_0()
		{
			try
			{
				if (!this.receiver)
				{
					this.slotPay.Fsm.GetState("Add coins").Actions[2].Enabled = true;
					NetLocalPlayer.Instance.WriteHostTriggerAction(69421);
				}
				else
				{
					this.receiver = false;
				}
			}
			catch
			{
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__23_1()
		{
			try
			{
				if (!this.receiver)
				{
					this.slotPay2.Fsm.GetState("Add coins").Actions[2].Enabled = true;
					NetLocalPlayer.Instance.WriteHostTriggerAction(69423);
				}
				else
				{
					this.receiver = false;
				}
			}
			catch
			{
			}
			return false;
		}

		private GameObject shopGO;

		private GameObject pubProducts;

		private GameObject pubRegister;

		private PlayMakerFSM teimoS;

		private PlayMakerFSM teimoB;

		private PlayMakerFSM slotBet;

		private PlayMakerFSM slotStart;

		public PlayMakerFSM slotPay;

		private PlayMakerFSM slotCash;

		private PlayMakerFSM slotBet2;

		private PlayMakerFSM slotStart2;

		public PlayMakerFSM slotPay2;

		private PlayMakerFSM slotCash2;

		private PlayMakerFSM breakPub;

		private PlayMakerFSM breakShop;

		public PlayMakerFSM storeRegisterFSM;

		public PlayMakerFSM storePostOrderFSM;

		public PlayMakerFSM pubRegisterFSM;

		public static Shop Instance;

		public bool receiver;

		public List<PlayMakerFSM> pubBuyFSMs = new List<PlayMakerFSM>();

		[CompilerGenerated]
		private sealed class <>c__DisplayClass23_0
		{
			public <>c__DisplayClass23_0()
			{
			}

			internal bool <HookEvents>b__2()
			{
				try
				{
					if (!this.<>4__this.receiver)
					{
						this.fsm.Fsm.GetState("Purchase").Actions[0].Enabled = true;
						NetLocalPlayer.Instance.WriteHostTriggerAction(20000 + int.Parse(this.fsm.gameObject.name));
					}
					else
					{
						this.fsm.Fsm.GetState("Purchase").Actions[0].Enabled = false;
						this.<>4__this.receiver = false;
					}
				}
				catch
				{
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Shop <>4__this;
		}
	}
}
