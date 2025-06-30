using MyNotepad.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace MyNotepad.Services
{
	internal class SettingsManager
	{
		private static readonly string FilePath = "appsettings.json";

		public static AppSettings LoadSettings()
		{
			if (!File.Exists(FilePath))
				return new AppSettings();

			string json = File.ReadAllText(FilePath);
			return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
		}

		public static void SaveSettings(AppSettings settings)
		{
			Debug.WriteLine($"path: " + Path.GetFullPath(FilePath));
			try
			{
				string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(FilePath, json);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"path: "+ Path.GetFullPath(FilePath));
			}
		}


	}
}
