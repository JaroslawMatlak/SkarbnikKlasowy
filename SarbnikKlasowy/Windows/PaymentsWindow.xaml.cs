using System.Collections.ObjectModel;
using System.Windows;
using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;

namespace SkarbnikKlasowy.Windows
{
	public partial class PaymentsWindow : Window
	{
		private readonly ObservableCollection<Payment> _payments = new();

		public PaymentsWindow()
		{
			InitializeComponent();

			LoadPayments();
		}

		private void LoadPayments()
		{
			_payments.Clear();

			var payments = CsvService.LoadPayments();

			foreach (var payment in payments)
				_payments.Add(payment);

			RefreshPayments();
		}

		private void RefreshPayments()
		{
			var viewModels = _payments
				.Select(x => new PaymentViewModel(x))
				.OrderByDescending(x => x.HasProblem)
				.ThenByDescending(x => x.CreationDate)
				.ToList();

			PaymentsGrid.ItemsSource = viewModels;
		}

		private void EditPayment_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (PaymentsGrid.SelectedItem
				is not PaymentViewModel viewModel)
			{
				MessageBox.Show(
					"Najpierw wybierz płatność.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var payment = viewModel.Payment;

			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();

			var window = new PaymentEditWindow(
				payment,
				students,
				goals)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				CsvService.SavePayments(_payments);

				RefreshPayments();
			}
		}

		private void ImportPayments_Click(
			object sender,
			RoutedEventArgs e)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog
			{
				Title = "Wybierz plik z płatnościami",
				Filter = "Pliki Excel (*.xlsx)|*.xlsx",
				Multiselect = false
			};

			if (dialog.ShowDialog() != true)
				return;

			try
			{
				var importedPayments =
					ExcelImportService.ImportPayments(dialog.FileName, _payments);

				if (importedPayments.Count == 0)
				{
					MessageBox.Show(
						"Nie znaleziono żadnych płatności do zaimportowania.",
						"Import płatności",
						MessageBoxButton.OK,
						MessageBoxImage.Information);

					return;
				}

				foreach (var payment in importedPayments)
				{
					payment.Id = _payments.Count == 0
						? 1
						: _payments.Max(x => x.Id) + 1;

					_payments.Add(payment);
				}

				CsvService.SavePayments(_payments);

				RefreshPayments();

				MessageBox.Show(
					$"Zaimportowano {importedPayments.Count} płatności.",
					"Import zakończony",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Wystąpił błąd podczas importu:\n\n{ex.Message}",
					"Błąd importu",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		private void AutoAssignPayments_Click(object sender, RoutedEventArgs e)
		{
			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();

			int assignedStudents = 0;
			int assignedGoals = 0;

			foreach (var payment in _payments)
			{
				string title = payment.Name ?? "";

				// Przypisz ucznia tylko wtedy, gdy jeszcze go nie ma
				if (payment.Student == null || payment.Student.Id == 0)
				{
					var matchingStudent = students.FirstOrDefault(student =>
						ContainsIgnoreCase(title, student.Name) &&
						ContainsIgnoreCase(title, student.SecondName));

					if (matchingStudent != null)
					{
						payment.Student = matchingStudent;
						assignedStudents++;
					}
				}

				// Przypisz tytuł tylko wtedy, gdy jeszcze go nie ma
				if (payment.Goal == null || payment.Goal.Id == 0)
				{
					var matchingGoal = goals.FirstOrDefault(goal =>
						ContainsIgnoreCase(title, goal.Name));

					if (matchingGoal != null)
					{
						payment.Goal = matchingGoal;
						assignedGoals++;
					}
				}
			}

			CsvService.SavePayments(_payments);

			RefreshPayments();

			MessageBox.Show(
				$"Automatyczne przypisywanie zakończone.\n\n" +
				$"Przypisano uczniów: {assignedStudents}\n" +
				$"Przypisano tytułów: {assignedGoals}",
				"Przypisywanie automatyczne",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private static bool ContainsIgnoreCase(string text, string value)
		{
			if (string.IsNullOrWhiteSpace(text) ||
				string.IsNullOrWhiteSpace(value))
				return false;

			return text.Contains(value, StringComparison.OrdinalIgnoreCase);
		}
	}
}