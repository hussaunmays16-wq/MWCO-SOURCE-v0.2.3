using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MWCO.Utilities
{
	internal static class Styles
	{
		public static GUIStyle Watermark
		{
			[CompilerGenerated]
			get
			{
				return Styles.<Watermark>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<Watermark>k__BackingField = value;
			}
		}

		public static GUIStyle LogEntry
		{
			[CompilerGenerated]
			get
			{
				return Styles.<LogEntry>k__BackingField;
			}
			[CompilerGenerated]
			set
			{
				Styles.<LogEntry>k__BackingField = value;
			}
		}

		public static GUIStyle BlackHeader
		{
			[CompilerGenerated]
			get
			{
				return Styles.<BlackHeader>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<BlackHeader>k__BackingField = value;
			}
		}

		public static GUIStyle Header
		{
			[CompilerGenerated]
			get
			{
				return Styles.<Header>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<Header>k__BackingField = value;
			}
		}

		public static GUIStyle Label
		{
			[CompilerGenerated]
			get
			{
				return Styles.<Label>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<Label>k__BackingField = value;
			}
		}

		public static GUIStyle EntryLabel
		{
			[CompilerGenerated]
			get
			{
				return Styles.<EntryLabel>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<EntryLabel>k__BackingField = value;
			}
		}

		public static GUIStyle SubLabel
		{
			[CompilerGenerated]
			get
			{
				return Styles.<SubLabel>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<SubLabel>k__BackingField = value;
			}
		}

		public static GUIStyle InputField
		{
			[CompilerGenerated]
			get
			{
				return Styles.<InputField>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<InputField>k__BackingField = value;
			}
		}

		public static GUIStyle Area
		{
			[CompilerGenerated]
			get
			{
				return Styles.<Area>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Styles.<Area>k__BackingField = value;
			}
		}

		public static void CreateIfMissing()
		{
			if (Styles.Area != null)
			{
				return;
			}
			Styles.Watermark = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				alignment = 6
			};
			Styles.LogEntry = new GUIStyle(GUI.skin.label)
			{
				richText = true
			};
			Styles.BlackHeader = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				fontStyle = 0,
				fontSize = 32,
				alignment = 4
			};
			Styles.Header = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				fontStyle = 0,
				fontSize = 22,
				alignment = 4
			};
			Styles.Label = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				fontStyle = 0,
				fontSize = 16,
				alignment = 4
			};
			Styles.EntryLabel = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				fontStyle = 0,
				fontSize = 14,
				alignment = 4
			};
			Styles.SubLabel = new GUIStyle(GUI.skin.label)
			{
				richText = true,
				fontStyle = 0,
				fontSize = 12,
				alignment = 4
			};
			Styles.InputField = new GUIStyle(GUI.skin.textField)
			{
				fontStyle = 0,
				alignment = 4
			};
			Styles.Area = new GUIStyle();
			Styles.Area.normal.background = Styles.MakeTex(800, 600, new Color(0f, 0f, 0f, 0.5f));
		}

		private static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] array = new Color[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = col;
			}
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		[CompilerGenerated]
		private static GUIStyle <Watermark>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <LogEntry>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <BlackHeader>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <Header>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <Label>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <EntryLabel>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <SubLabel>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <InputField>k__BackingField;

		[CompilerGenerated]
		private static GUIStyle <Area>k__BackingField;
	}
}
