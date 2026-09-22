using System;
using System.IO;
using MWCO.Utilities;

namespace MWCO
{
	internal class UserPreferences
	{
		public static void Start()
		{
			try
			{
				if (!File.Exists(UserPreferences.dir))
				{
					using (StreamWriter streamWriter = File.CreateText(UserPreferences.dir))
					{
						streamWriter.WriteLine("SkinCode: 0,0,0,0,0,0");
						streamWriter.WriteLine("Savefile: 0");
						streamWriter.WriteLine("Optimized: 1");
						streamWriter.WriteLine("OptIn: 1");
						streamWriter.Close();
					}
					UserPreferences.usesave = 0;
					UserPreferences.optIn = true;
					UserPreferences.optimized = 1;
				}
				int num = 0;
				StreamReader streamReader = new StreamReader(UserPreferences.dir);
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					if (text.Contains("OptIn"))
					{
						if (int.Parse(text.Split(new char[] { ':' })[1]) == 1)
						{
							UserPreferences.optIn = true;
						}
					}
					else if (text.Contains("SkinCode"))
					{
						UserPreferences.skinCode = text.Split(new char[] { ':' })[1];
					}
					else if (text.Contains("Savefile"))
					{
						UserPreferences.usesave = int.Parse(text.Split(new char[] { ':' })[1]);
					}
					else if (text.Contains("Optimized"))
					{
						UserPreferences.optimized = int.Parse(text.Split(new char[] { ':' })[1]);
					}
					num++;
				}
				streamReader.Close();
			}
			catch (Exception ex)
			{
				string text2 = "Error loading user preferances! ";
				Exception ex2 = ex;
				Logger.Debug(text2 + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		public static void SaveInfo()
		{
			if (MPController.Instance.skinCode == UserPreferences.skinCode)
			{
				return;
			}
			UserPreferences.skinCode = MPController.Instance.skinCode;
			try
			{
				string[] array = File.ReadAllLines(UserPreferences.dir);
				array[0] = "SkinCode:" + UserPreferences.skinCode;
				File.WriteAllLines(UserPreferences.dir, array);
			}
			catch (Exception ex)
			{
				Logger.Debug("Error saving skin - " + ex.Message);
			}
		}

		public UserPreferences()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static UserPreferences()
		{
		}

		public static bool optIn;

		public static string skinCode = "0,0,0,0,0,0";

		public static int usesave;

		public static int optimized = 1;

		public static string currentDirectory = Path.GetDirectoryName(Client.AssemblyPath);

		private static string dir = Path.GetFullPath(Path.Combine(UserPreferences.currentDirectory, "..\\..\\..\\userPref.txt"));
	}
}
