using SkarbnikKlasowy.Model;
using System.Windows;

namespace SkarbnikKlasowy.Windows
{
	public partial class StudentEditWindow : Window
	{
		public Student Student { get; private set; }

		public StudentEditWindow(Student? student = null)
		{
			InitializeComponent();

			if (student != null)
			{
				Student = new Student
				{
					Id = student.Id,
					Name = student.Name,
					SecondName = student.SecondName
				};

				FirstNameTextBox.Text = Student.Name;
				LastNameTextBox.Text = Student.SecondName;

				Title = "Edytuj ucznia";
			}
			else
			{
				Student = new Student();
				Title = "Dodaj ucznia";
			}
		}

		private void SaveButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			string firstName = FirstNameTextBox.Text.Trim();
			string lastName = LastNameTextBox.Text.Trim();

			if (string.IsNullOrWhiteSpace(firstName))
			{
				MessageBox.Show(
					"Podaj imię ucznia.",
					"Brak danych",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				FirstNameTextBox.Focus();
				return;
			}

			if (string.IsNullOrWhiteSpace(lastName))
			{
				MessageBox.Show(
					"Podaj nazwisko ucznia.",
					"Brak danych",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				LastNameTextBox.Focus();
				return;
			}

			Student.Name = firstName;
			Student.SecondName = lastName;

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