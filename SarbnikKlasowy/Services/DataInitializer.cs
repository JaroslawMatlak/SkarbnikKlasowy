using System.IO;
using System.Reflection;
using System.Text;
namespace SkarbnikKlasowy.Services
{
	public static class DataInitializer
	{
		public static void Initialize()
		{
			string baseDirectory = AppContext.BaseDirectory; string dataDirectory = Path.Combine(baseDirectory, "Data");
			// Utworzenie katalogu Data, jeśli nie istnieje
			Directory.CreateDirectory(dataDirectory);
			// Pobranie aktualnie wykonywanego assembly
			Assembly assembly = Assembly.GetExecutingAssembly();
			// Znalezienie wszystkich klas znajdujących się w namespace Model
			var modelTypes = assembly
				.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null && t.Namespace.EndsWith(".Model"))
				.ToList();
			foreach (var modelType in modelTypes)
			{
				CreateCsvForModel(modelType, dataDirectory);
			}
		}
		private static void CreateCsvForModel(Type modelType, string dataDirectory)
		{
			string fileName = $"{modelType.Name}.csv";
			string filePath = Path.Combine(dataDirectory, fileName);
			// Jeżeli plik już istnieje, niczego nie zmieniamy.
			// Dzięki temu uruchomienie programu nie skasuje danych.
			if (File.Exists(filePath)) return;
			// Pobieramy publiczne właściwości modelu
			var properties = modelType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead)
				.ToList();
			if (properties.Count == 0) return;
			// Tworzymy nagłówek CSV
			string header = string.Join(
				",",
				properties.Select(p => EscapeCsv(p.Name))
			);
			File.WriteAllText(
				filePath,
				header + Environment.NewLine,
				new UTF8Encoding(false)
			);
		}
		private static string EscapeCsv(string value)
		{
			if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
			{
				return $"\"{value.Replace("\"", "\"\"")}\"";
			}
			return value;
		}
	}
}