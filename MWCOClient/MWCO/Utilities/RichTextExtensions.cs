using System;

namespace MWCO.Utilities
{
	internal static class RichTextExtensions
	{
		public static string Bold(this object obj)
		{
			return "<b>" + obj.ToString() + "</b>";
		}

		public static string Italic(this object obj)
		{
			return "<i>" + obj.ToString() + "</b>";
		}

		public static string SetSize(this object obj, int size)
		{
			return string.Format("<size={0}>{1}</size", size, obj.ToString());
		}

		public static string SetHex(this object obj, string hex)
		{
			return string.Concat(new string[]
			{
				"<color=",
				hex,
				">",
				obj.ToString(),
				"</color>"
			});
		}

		public static string ToAqua(this object obj)
		{
			return "<color=#00FFFF>" + obj.ToString() + "</color>";
		}

		public static string ToBlack(this object obj)
		{
			return "<color=black>" + obj.ToString() + "</color>";
		}

		public static string ToBlue(this object obj)
		{
			return "<color=#378EE1>" + obj.ToString() + "</color>";
		}

		public static string ToBrown(this object obj)
		{
			return "<color=brown>" + obj.ToString() + "</color>";
		}

		public static string ToDarkBlue(this object obj)
		{
			return "<color=darkblue>" + obj.ToString() + "</color>";
		}

		public static string ToGreen(this object obj)
		{
			return "<color=green>" + obj.ToString() + "</color>";
		}

		public static string ToGrey(this object obj)
		{
			return "<color=grey>" + obj.ToString() + "</color>";
		}

		public static string ToLightBlue(this object obj)
		{
			return "<color=lightblue>" + obj.ToString() + "</color>";
		}

		public static string ToLime(this object obj)
		{
			return "<color=lime>" + obj.ToString() + "</color>";
		}

		public static string ToMaroon(this object obj)
		{
			return "<color=maroon>" + obj.ToString() + "</color>";
		}

		public static string ToNavy(this object obj)
		{
			return "<color=navy>" + obj.ToString() + "</color>";
		}

		public static string ToOlive(this object obj)
		{
			return "<color=olive>" + obj.ToString() + "</color>";
		}

		public static string ToOrange(this object obj)
		{
			return "<color=orange>" + obj.ToString() + "</color>";
		}

		public static string ToPurple(this object obj)
		{
			return "<color=purple>" + obj.ToString() + "</color>";
		}

		public static string ToRed(this object obj)
		{
			return "<color=red>" + obj.ToString() + "</color>";
		}

		public static string ToSilver(this object obj)
		{
			return "<color=silver>" + obj.ToString() + "</color>";
		}

		public static string ToTeal(this object obj)
		{
			return "<color=teal>" + obj.ToString() + "</color>";
		}

		public static string ToYellow(this object obj)
		{
			return "<color=yellow>" + obj.ToString() + "</color>";
		}
	}
}
