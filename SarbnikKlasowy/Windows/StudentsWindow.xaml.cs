using SkarbnikKlasowy.Model;
using SkarbnikKlasowy.Services;
using SkarbnikKlasowy.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class StudentsWindow : Window
	{
		private readonly ObservableCollection<StudentViewModel> _students = new();

		public StudentsWindow()
		{
			InitializeComponent();

			StudentsGrid.ItemsSource = _students;

			LoadStudents();
		}

		private void LoadStudents()
		{
			_students.Clear();

			var students = CsvService.LoadStudents();

			var payments = CsvService.LoadPayments();
			var goals = CsvService.LoadGoals();
			var modifiers = CsvService.LoadGoalModifiers();
			var overpayments = CsvService.LoadOverpayments();

			foreach (var student in students
				.OrderBy(s => s.SecondName)
				.ThenBy(s => s.Name))
			{
				// -----------------------------
				// Suma wszystkich wpłat ucznia
				// -----------------------------

				double totalPayments = payments
					.Where(x =>
						x.Student != null &&
						x.Student.Id == student.Id)
					.Sum(x => x.Amount);

				// -----------------------------
				// Sprawdzenie wszystkich celów
				// -----------------------------

				double unhandledOverpayments = 0;

				bool hasUnpaidGoals = false;

				foreach (var goal in goals)
				{
					// Zwykłe wpłaty na cel
					double paid = payments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.Goal != null &&
							x.Goal.Id == goal.Id)
						.Sum(x => x.Amount);

					// Nadpłata przekierowana
					// z innych celów NA ten cel
					double redirectedIn = overpayments
						.Where(x =>
							!x.IsReturned &&
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.TargetGoal != null &&
							x.TargetGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// Nadpłata przekierowana
					// Z tego celu na inne cele
					double redirectedOut = overpayments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.SourceGoal != null &&
							x.SourceGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// Wymagana kwota
					double requiredAmount =
						GetRequiredAmount(
							student,
							goal,
							modifiers);

					// Aktualny bilans celu
					double difference =
						paid +
						redirectedIn -
						requiredAmount -
						redirectedOut;

					// Cel nie jest opłacony
					if (difference < 0)
					{
						hasUnpaidGoals = true;
					}
					else
					{
						// Tylko rzeczywiście pozostała nadpłata
						unhandledOverpayments += difference;
					}
				}

				_students.Add(
					new StudentViewModel(
						student,
						totalPayments,
						unhandledOverpayments,
						hasUnpaidGoals));
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

		private void AddStudent_Click(
			object sender,
			RoutedEventArgs e)
		{
			int nextId = _students.Count == 0
				? 1
				: _students.Max(x => x.Student.Id) + 1;

			var newStudent = new Student
			{
				Id = nextId
			};

			var window = new StudentEditWindow(newStudent)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				CsvService.SaveStudents(
					_students
						.Select(x => x.Student)
						.Append(window.Student));

				LoadStudents();
			}
		}

		private void EditStudent_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (StudentsGrid.SelectedItem
				is not StudentViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz ucznia.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var student = selected.Student;

			var window = new StudentEditWindow(student)
			{
				Owner = this
			};

			if (window.ShowDialog() == true)
			{
				student.Name = window.Student.Name;
				student.SecondName = window.Student.SecondName;

				CsvService.SaveStudents(
					_students.Select(x => x.Student));

				LoadStudents();
			}
		}

		private void DeleteStudent_Click(
			object sender,
			RoutedEventArgs e)
		{
			if (StudentsGrid.SelectedItem
				is not StudentViewModel selected)
			{
				MessageBox.Show(
					"Najpierw wybierz ucznia.",
					"Brak wyboru",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				return;
			}

			var student = selected.Student;

			var result = MessageBox.Show(
				$"Czy na pewno chcesz usunąć:\n\n" +
				$"{student.Name} {student.SecondName}?",
				"Usuń ucznia",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
			{
				var students = _students
					.Select(x => x.Student)
					.Where(x => x.Id != student.Id)
					.ToList();

				CsvService.SaveStudents(students);

				LoadStudents();
			}
		}
		private void StudentsGrid_MouseDoubleClick(
			object sender,
			System.Windows.Input.MouseButtonEventArgs e)
		{
			if (StudentsGrid.SelectedItem
				is not StudentViewModel selected)
			{
				return;
			}

			var window = new StudentDetailsWindow(selected.Student)
			{
				Owner = this
			};

			window.ShowDialog();
		}
	}
}