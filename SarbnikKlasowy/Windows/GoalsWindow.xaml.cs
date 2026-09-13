using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class GoalsWindow : Window
	{
		private readonly ObservableCollection<GoalViewModel> _goals = new();

		public GoalsWindow()
		{
			InitializeComponent();

			GoalsGrid.ItemsSource = _goals;

			LoadGoals();
		}

		private void LoadGoals()
		{
			_goals.Clear();

			var goals = CsvService.LoadGoals()
				.OrderByDescending(g => g.CreationDate)
				.ToList();

			var students = CsvService.LoadStudents();
			var payments = CsvService.LoadPayments();
			var modifiers = CsvService.LoadGoalModifiers();
			var overpayments = CsvService.LoadOverpayments();

			foreach (var goal in goals)
			{
				double expectedAmount = 0;
				double totalIncome = 0;
				int unpaidStudents = 0;

				foreach (var student in students)
				{
					// --------------------------------
					// Kwota wymagana od tego ucznia
					// --------------------------------

					double requiredAmount =
						GetRequiredAmount(
							student,
							goal,
							modifiers);

					expectedAmount += requiredAmount;

					// --------------------------------
					// Rzeczywiste wpłaty na ten cel
					// --------------------------------

					double paid = payments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.Goal != null &&
							x.Goal.Id == goal.Id)
						.Sum(x => x.Amount);

					// --------------------------------
					// Nadpłaty przekierowane
					// NA ten cel
					// --------------------------------

					double redirectedIn = overpayments
						.Where(x =>
							!x.IsReturned &&
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.TargetGoal != null &&
							x.TargetGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// --------------------------------
					// Nadpłaty przekierowane
					// Z tego celu
					// --------------------------------

					double redirectedOut = overpayments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.SourceGoal != null &&
							x.SourceGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// --------------------------------
					// Wpływy na cel
					//
					// Tu pokazujemy rzeczywiste wpływy
					// oraz przekierowania NA cel.
					// --------------------------------

					totalIncome += paid + redirectedIn;

					// --------------------------------
					// Aktualnie dostępna kwota
					// na pokrycie celu
					// --------------------------------

					double available =
						paid +
						redirectedIn -
						redirectedOut;

					if (available < requiredAmount)
					{
						unpaidStudents++;
					}
				}

				_goals.Add(
					new GoalViewModel(
						goal,
						expectedAmount,
						totalIncome,
						unpaidStudents));
			}
		}

		private static double GetRequiredAmount(
			Student student,
			Goal goal,
			IEnumerable<GoalModifier> modifiers)
		{
			var modifier = modifiers.FirstOrDefault(x =>
				x.Student != null &&
				x.Student.Id == student.Id &&
				x.Goal != null &&
				x.Goal.Id == goal.Id);

			return modifier?.ModifiedValue ?? goal.Amount;
		}

		private void AddGoal_Click(
			object sender,
			RoutedEventArgs e)
		{
			int nextId = _goals.Count == 0
				? 1
				: _goals.Max(x => x.Id) + 1;

			var newGoal = new Goal
			{
				Id = nextId
			};

			var window = new GoalEditWindow(newGoal)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				CsvService.SaveGoals(
					_goals
						.Select(x => x.Goal)
						.Append(window.Goal));

				LoadGoals();
			}
		}

		private void EditGoal_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (GoalsGrid.SelectedItem
				is not GoalViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz tytuł.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var goal = selected.Goal;

			var window = new GoalEditWindow(goal)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				CsvService.SaveGoals(
					_goals.Select(x => x.Goal));

				LoadGoals();
			}
		}

		private void DeleteGoal_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (GoalsGrid.SelectedItem
				is not GoalViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz tytuł.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var goal = selected.Goal;

			var result = MessageBox.Show(
				$"Czy na pewno chcesz usunąć tytuł:\n\n" +
				$"{goal.Name}\n" +
				$"Kwota: {goal.Amount:F2} zł?",
				"Usuń tytuł",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
			{
				var goals = _goals
					.Select(x => x.Goal)
					.Where(x => x.Id != goal.Id)
					.ToList();

				CsvService.SaveGoals(goals);

				LoadGoals();
			}
		}
	}
}