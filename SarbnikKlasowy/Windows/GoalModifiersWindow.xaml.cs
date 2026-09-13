using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;

namespace SkarbnikKlasowy.Windows
{
	public partial class GoalModifiersWindow : Window
	{
		private readonly ObservableCollection<GoalModifier>
			_modifiers = new();

		public GoalModifiersWindow()
		{
			InitializeComponent();

			LoadModifiers();
		}

		private void LoadModifiers()
		{
			_modifiers.Clear();

			var modifiers =
				CsvService.LoadGoalModifiers();

			foreach (var modifier in modifiers)
				_modifiers.Add(modifier);

			RefreshModifiers();
		}

		private void RefreshModifiers()
		{
			ModifiersGrid.ItemsSource =
				_modifiers
					.Select(x => new GoalModifierViewModel(x))
					.OrderBy(x => x.StudentName)
					.ThenBy(x => x.GoalName)
					.ToList();
		}

		private void AddModifier_Click(
			object sender,
			RoutedEventArgs e)
		{
			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();

			var modifier = new GoalModifier();

			var window = new GoalModifierEditWindow(
				modifier,
				students,
				goals)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				modifier.Id = _modifiers.Count == 0
					? 1
					: _modifiers.Max(x => x.Id) + 1;

				_modifiers.Add(modifier);

				CsvService.SaveGoalModifiers(_modifiers);

				RefreshModifiers();
			}
		}

		private void EditModifier_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (ModifiersGrid.SelectedItem
				is not GoalModifierViewModel viewModel)
			{
				MessageBox.Show(
					"Najpierw wybierz modyfikator.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var modifier = viewModel.Modifier;

			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();

			var window = new GoalModifierEditWindow(
				modifier,
				students,
				goals)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				CsvService.SaveGoalModifiers(_modifiers);

				RefreshModifiers();
			}
		}

		private void DeleteModifier_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (ModifiersGrid.SelectedItem
				is not GoalModifierViewModel viewModel)
			{
				MessageBox.Show(
					"Najpierw wybierz modyfikator.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var result = MessageBox.Show(
				"Czy na pewno chcesz usunąć wybrany modyfikator?",
				"Usuwanie modyfikatora",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes)
				return;

			_modifiers.Remove(viewModel.Modifier);

			CsvService.SaveGoalModifiers(_modifiers);

			RefreshModifiers();
		}
	}
}