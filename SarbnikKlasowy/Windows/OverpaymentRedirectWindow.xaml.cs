using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;

namespace SkarbnikKlasowy.Windows
{
	public partial class OverpaymentRedirectWindow : Window
	{
		private readonly Student _student;
		private readonly Goal _sourceGoal;
		private readonly double _availableAmount;

		public OverpaymentRedirectWindow(
			Student student,
			Goal sourceGoal,
			double availableAmount)
		{
			InitializeComponent();

			_student = student;
			_sourceGoal = sourceGoal;
			_availableAmount = availableAmount;

			AvailableTextBlock.Text =
				$"Dostępna nadpłata: {availableAmount:F2} zł";

			AmountTextBox.Text =
				availableAmount.ToString("0.##");

			LoadGoals();
		}

		private void LoadGoals()
		{
			var goals = CsvService
				.LoadGoals()
				.Where(x => x.Id != _sourceGoal.Id)
				.OrderByDescending(x => x.CreationDate)
				.ToList();

			GoalComboBox.ItemsSource = goals;

			if (goals.Count > 0)
				GoalComboBox.SelectedIndex = 0;
		}

		private void RedirectButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (!double.TryParse(
					AmountTextBox.Text.Replace(',', '.'),
					System.Globalization.NumberStyles.Float,
					System.Globalization.CultureInfo.InvariantCulture,
					out double amount))
			{
				MessageBox.Show(
					"Podaj prawidłową kwotę.",
					"Nieprawidłowa kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (amount <= 0)
			{
				MessageBox.Show(
					"Kwota musi być większa od zera.",
					"Nieprawidłowa kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (amount > _availableAmount)
			{
				MessageBox.Show(
					$"Kwota nie może być większa niż dostępna nadpłata " +
					$"({_availableAmount:F2} zł).",
					"Za duża kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (GoalComboBox.SelectedItem is not Goal targetGoal)
			{
				MessageBox.Show(
					"Wybierz tytuł, na który ma zostać przekierowana nadpłata.",
					"Brak tytułu",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			var overpayments = CsvService.LoadOverpayments();

			int nextId = overpayments.Count == 0
				? 1
				: overpayments.Max(x => x.Id) + 1;

			overpayments.Add(new Overpayment
			{
				Id = nextId,
				Student = _student,
				SourceGoal = _sourceGoal,
				TargetGoal = targetGoal,
				Amount = amount,
				IsReturned = false,
				CreationDate = DateTime.Now
			});

			CsvService.SaveOverpayments(overpayments);

			DialogResult = true;
		}

		private void CancelButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}