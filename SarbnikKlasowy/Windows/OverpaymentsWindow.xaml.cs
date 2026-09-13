using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;

namespace SkarbnikKlasowy.Windows
{
	public partial class OverpaymentsWindow : Window
	{
		private readonly ObservableCollection<OverpaymentViewModel>
			_overpayments = new();

		public OverpaymentsWindow()
		{
			InitializeComponent();

			LoadOverpayments();
		}

		private void LoadOverpayments()
		{
			_overpayments.Clear();

			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();
			var payments = CsvService.LoadPayments();
			var modifiers = CsvService.LoadGoalModifiers();
			var overpaymentOperations =
				CsvService.LoadOverpayments();

			foreach (var student in students)
			{
				foreach (var goal in goals)
				{
					double paid = payments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.Goal != null &&
							x.Goal.Id == goal.Id)
						.Sum(x => x.Amount);

					double required =
						GetRequiredAmount(
							student,
							goal,
							modifiers);

					double originalOverpayment =
						Math.Max(0, paid - required);

					double usedOverpayment =
						overpaymentOperations
							.Where(x =>
								x.Student != null &&
								x.Student.Id == student.Id &&
								x.SourceGoal != null &&
								x.SourceGoal.Id == goal.Id)
							.Sum(x => x.Amount);

					double available =
						Math.Max(
							0,
							originalOverpayment -
							usedOverpayment);

					if (available <= 0)
						continue;

					_overpayments.Add(
						new OverpaymentViewModel(
							student,
							goal,
							paid,
							required,
							available));
				}
			}

			OverpaymentsGrid.ItemsSource =
				_overpayments
					.OrderBy(x => x.Student.SecondName)
					.ThenBy(x => x.Student.Name)
					.ThenBy(x => x.Goal.Name)
					.ToList();
		}

		private static double GetRequiredAmount(
			Student student,
			Goal goal,
			System.Collections.Generic.IEnumerable<GoalModifier> modifiers)
		{
			var modifier = modifiers.FirstOrDefault(x =>
				x.Student != null &&
				x.Student.Id == student.Id &&
				x.Goal != null &&
				x.Goal.Id == goal.Id);

			return modifier?.ModifiedValue ?? goal.Amount;
		}

		private void RefreshButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			LoadOverpayments();
		}

		private void HandleOverpayment_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (OverpaymentsGrid.SelectedItem
				is not OverpaymentViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz nadpłatę.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var window = new OverpaymentRedirectWindow(
				selected.Student,
				selected.Goal,
				selected.Available)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				LoadOverpayments();
			}
		}

		private void CreateModifier_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (OverpaymentsGrid.SelectedItem
				is not OverpaymentViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz nadpłatę.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var students = CsvService.LoadStudents();
			var goals = CsvService.LoadGoals();
			var modifiers = CsvService.LoadGoalModifiers();

			var existingModifier = modifiers.FirstOrDefault(x =>
				x.Student != null &&
				x.Student.Id == selected.Student.Id &&
				x.Goal != null &&
				x.Goal.Id == selected.Goal.Id);

			GoalModifier modifier;

			if (existingModifier != null)
			{
				// Istniejący modyfikator – otwieramy go do edycji.
				modifier = existingModifier;
			}
			else
			{
				// Brak modyfikatora – tworzymy nowy.
				int nextId = modifiers.Count == 0
					? 1
					: modifiers.Max(x => x.Id) + 1;

				modifier = new GoalModifier
				{
					Id = nextId,
					Student = selected.Student,
					Goal = selected.Goal,

					// Kwota wymagana + dostępna nadpłata
					ModifiedValue =
						selected.Required +
						selected.Available
				};
			}

			var window = new GoalModifierEditWindow(
				modifier,
				students,
				goals)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				// Jeżeli był nowy, dodajemy go do listy.
				if (existingModifier == null)
				{
					modifiers.Add(modifier);
				}

				CsvService.SaveGoalModifiers(modifiers);

				// Przeliczamy dostępne nadpłaty.
				LoadOverpayments();
			}
		}
	}
}