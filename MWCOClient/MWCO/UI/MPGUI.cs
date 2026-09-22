using System;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.UI
{
	internal class MPGUI : MonoBehaviour
	{
		private void Start()
		{
			MPGUI.Instance = this;
		}

		~MPGUI()
		{
			MPGUI.Instance = null;
		}

		public void ShowCursor(bool show)
		{
			if (show)
			{
				this.cursorCounter++;
				if (!Cursor.visible)
				{
					Cursor.visible = true;
					return;
				}
			}
			else
			{
				ErrorHandling.Assert(this.cursorCounter > 0, "Tried to hide cursor too many times.");
				int num = this.cursorCounter - 1;
				this.cursorCounter = num;
				if (num == 0 && Application.loadedLevelName == "GAME")
				{
					Cursor.visible = false;
				}
			}
		}

		public MPGUI()
		{
		}

		public static MPGUI Instance;

		private int cursorCounter;
	}
}
