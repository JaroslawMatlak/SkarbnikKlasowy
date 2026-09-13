using SkarbnikKlasowy.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class GoalModifierEditWindow : Window
	{
		private readonly GoalModifier _modifier;

		public GoalModifierEditWindow(
			GoalModifier modifier,
			IEnumerable<Student> students,
			IEnumerable<Goal> goals)
		{
			InitializeComponent();

			_modifier = modifier;

			StudentComboBox.ItemsSource = students.ToList();
			GoalComboBox.ItemsSource = goals.ToList();

			if (modifier.Student != null &&
				modifier.Student.Id != 0)
			{
				StudentComboBox.SelectedValuePath = "Id";
				StudentComboBox.SelectedValue = modifier.Student.Id;
			}

			if (modifier.Goal != null &&
				modifier.Goal.Id != 0)
			{
				GoalComboBox.SelectedValuePath = "Id";
				GoalComboBox.SelectedValue = modifier.Goal.Id;
			}

			ModifiedValueTextBox.Text =
				modifier.ModifiedValue.ToString(CultureInfo.InvariantCulture);
		}



		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (StudentComboBox.SelectedItem is not Student student)
			{
				MessageBox.Show(
					"Wybierz ucznia.",
					"Brak ucznia",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (GoalComboBox.SelectedItem is not Goal goal)
			{
				MessageBox.Show(
					"Wybierz tytuł.",
					"Brak tytułu",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (string.IsNullOrWhiteSpace(
					ModifiedValueTextBox.Text))
			{
				MessageBox.Show(
					"Podaj zmodyfikowaną kwotę.",
					"Brak kwoty",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (!double.TryParse(
				ModifiedValueTextBox.Text.Replace(',', '.'),
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out double modifiedValue))
			{
				MessageBox.Show(
					"Podaj prawidłową kwotę.",
					"Nieprawidłowa kwota",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			_modifier.Student = student;
			_modifier.Goal = goal;
			_modifier.ModifiedValue = modifiedValue;

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