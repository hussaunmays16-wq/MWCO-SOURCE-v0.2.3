using System;
using UnityEngine;

namespace MWCO.Utilities
{
	internal static class IMGUIUtils
	{
		public static void Setup()
		{
			IMGUIUtils.smallLabelStyle.fontSize = 11;
			IMGUIUtils.fillText.SetPixel(0, 0, Color.white);
			IMGUIUtils.fillText.wrapMode = 0;
			IMGUIUtils.fillText.Apply();
		}

		public static void DrawPlainColorRect(Rect rct)
		{
			if (IMGUIUtils.fillText != null)
			{
				GUI.DrawTexture(rct, IMGUIUtils.fillText);
			}
		}

		public static void DrawSmallLabel(string text, Rect rct, Color color, bool shadow = false)
		{
			if (shadow)
			{
				rct.y += 1f;
				rct.x += 1f;
				IMGUIUtils.smallLabelStyle.normal.textColor = Color.black;
				GUI.Label(rct, text, IMGUIUtils.smallLabelStyle);
				rct.y -= 1f;
				rct.x -= 1f;
			}
			IMGUIUtils.smallLabelStyle.normal.textColor = color;
			GUI.Label(rct, text, IMGUIUtils.smallLabelStyle);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static IMGUIUtils()
		{
		}

		private static Texture2D fillText = new Texture2D(1, 1);

		private static GUIStyle smallLabelStyle = new GUIStyle();
	}
}
