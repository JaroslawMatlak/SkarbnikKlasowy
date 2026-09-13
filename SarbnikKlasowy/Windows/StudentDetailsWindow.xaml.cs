using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class StudentDetailsWindow : Window
	{
		private readonly Student _student;

		public StudentDetailsWindow(Student student)
		{
			InitializeComponent();

			_student = student;

			LoadStudentDetails();
		}

		private void LoadStudentDetails()
		{
			var payments = CsvService.LoadPayments();
			var goals = CsvService.LoadGoals();
			var modifiers = CsvService.LoadGoalModifiers();
			var overpayments = CsvService.LoadOverpayments();

			// -----------------------------
			// Dane ucznia
			// -----------------------------

			StudentNameTextBlock.Text =
				$"{_student.Name} {_student.SecondName}";

			// -----------------------------
			// Suma wpłat
			// -----------------------------

			double totalPayments = payments
				.Where(x =>
					x.Student != null &&
					x.Student.Id == _student.Id)
				.Sum(x => x.Amount);

			// -----------------------------
			// Cele
			// -----------------------------

			var goalStatuses =
				new List<StudentGoalStatusViewModel>();

			double totalOverpayment = 0;

			foreach (var goal in goals)
			{
				double paid = payments
					.Where(x =>
						x.Student != null &&
						x.Student.Id == _student.Id &&
						x.Goal != null &&
						x.Goal.Id == goal.Id)
					.Sum(x => x.Amount);

				double redirectedIn = overpayments
					.Where(x =>
						!x.IsReturned &&
						x.Student != null &&
						x.Student.Id == _student.Id &&
						x.TargetGoal != null &&
						x.TargetGoal.Id == goal.Id)
					.Sum(x => x.Amount);

				double redirectedOut = overpayments
					.Where(x =>
						x.Student != null &&
						x.Student.Id == _student.Id &&
						x.SourceGoal != null &&
						x.SourceGoal.Id == goal.Id)
					.Sum(x => x.Amount);

				double required =
					GetRequiredAmount(
						_student,
						goal,
						modifiers);

				double currentPaid =
					paid +
					redirectedIn -
					redirectedOut;

				if (currentPaid >= required)
				{
					totalOverpayment +=
						currentPaid - required;
				}

				goalStatuses.Add(
					new StudentGoalStatusViewModel(
						goal,
						required,
						currentPaid));
			}

			// -----------------------------
			// Podsumowanie
			// -----------------------------

			TotalPaymentsTextBlock.Text =
				$"{totalPayments:F2} zł";

			OverpaymentsTextBlock.Text =
				$"{totalOverpayment:F2} zł";

			// -----------------------------
			// Lista celów
			// -----------------------------

			GoalsGrid.ItemsSource =
				goalStatuses
					.OrderByDescending(x => x.Goal.CreationDate)
					.ToList();
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

		private void CloseButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			Close();
		}
	}
}