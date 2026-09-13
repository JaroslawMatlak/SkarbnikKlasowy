using SkarbnikKlasowy.Model;
using System.Globalization;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class GoalEditWindow : Window
	{
		public Goal Goal { get; private set; }

		public GoalEditWindow(Goal? goal = null)
		{
			InitializeComponent();

			if (goal != null)
			{
				Goal = goal;

				NameTextBox.Text = Goal.Name;
				AmountTextBox.Text = Goal.Amount
					.ToString("0.00", CultureInfo.CurrentCulture);

				Title = "Edytuj tytuł";

				if (Goal.EndDate.HasValue)
					EndDatePicker.SelectedDate = Goal.EndDate.Value;
				
				ManualCloseCheckBox.IsChecked = Goal.IsManuallyClosed;
			}
			else
			{
				Goal = new Goal();

				AmountTextBox.Text = "0,00";

				Title = "Dodaj tytuł";
			}
		}

		private void SaveButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			string name = NameTextBox.Text.Trim();

			if (string.IsNullOrWhiteSpace(name))
			{
				MessageBox.Show(
					"Podaj nazwę tytułu.",
					"Brak danych",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				NameTextBox.Focus();
				return;
			}

			if (!double.TryParse(
					AmountTextBox.Text,
					NumberStyles.Number,
					CultureInfo.CurrentCulture,
					out double amount))
			{
				MessageBox.Show(
					"Podaj prawidłową kwotę.",
					"Nieprawidłowa kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				AmountTextBox.Focus();
				return;
			}

			if (amount < 0)
			{
				MessageBox.Show(
					"Kwota nie może być ujemna.",
					"Nieprawidłowa kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				AmountTextBox.Focus();
				return;
			}

			Goal.Name = name;
			Goal.Amount = amount;
			if(Goal.CreationDate == default)
				Goal.CreationDate = DateTime.Now;

			Goal.EndDate = EndDatePicker.SelectedDate;
			Goal.IsManuallyClosed = ManualCloseCheckBox.IsChecked ?? false;



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