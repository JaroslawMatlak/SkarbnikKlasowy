using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.Windows;

namespace SkarbnikKlasowy
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
		}

		private void StudentsButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			var window = new StudentsWindow()
			{
				Owner = this
			};

			window.ShowDialog();

		}

		private void GoalsButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			var window = new GoalsWindow
			{
				Owner = this
			};

			window.ShowDialog();
		}

		private void PaymentsButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			var window = new PaymentsWindow
			{
				Owner = this
			};
			window.ShowDialog();
		}

		private void ExportReport_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				Title = "Zapisz raport",
				Filter = "Plik Excel (*.xlsx)|*.xlsx",
				FileName = $"Raport_{DateTime.Now:yyyy-MM-dd}.xlsx"
			};

			if (dialog.ShowDialog() != true)
				return;

			try
			{
				ExcelReportService.ExportReport(dialog.FileName);

				MessageBox.Show(
					"Raport został wyeksportowany.",
					"Eksport raportu",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Wystąpił błąd podczas eksportowania raportu:\n\n{ex.Message}",
					"Błąd eksportu",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}
		private void GoalModifiers_Click(
			object sender,
			RoutedEventArgs e)
		{
			var window = new GoalModifiersWindow
			{
				Owner = this
			};

			window.ShowDialog();
		}

		private void OverpaymentsButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			var window = new OverpaymentsWindow
			{
				Owner = this
			};

			window.ShowDialog();
		}

	}
}