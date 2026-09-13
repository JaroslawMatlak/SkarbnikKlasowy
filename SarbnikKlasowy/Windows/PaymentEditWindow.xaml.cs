using System.Windows;
using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.Windows
{
	public partial class PaymentEditWindow : Window
	{
		public Payment Payment { get; }

		public PaymentEditWindow(
			Payment payment,
			IEnumerable<Student> students,
			IEnumerable<Goal> goals)
		{
			InitializeComponent();

			Payment = payment;

			DateTextBlock.Text = Payment.PaymentDate.ToString("dd.MM.yyyy HH:mm");

			AmountTextBlock.Text =	$"{Payment.Amount:F2} zł";

			DescriptionTextBlock.Text =	Payment.Name;

			SenderTextBlock.Text = Payment.Sender;

			StudentComboBox.ItemsSource =
				students.ToList();

			GoalComboBox.ItemsSource =
				goals.ToList();

			if (Payment.Student != null &&
				Payment.Student.Id != 0)
			{
				StudentComboBox.SelectedValuePath = "Id";
				StudentComboBox.SelectedValue =
					Payment.Student.Id;
			}

			if (Payment.Goal != null &&
				Payment.Goal.Id != 0)
			{
				GoalComboBox.SelectedValuePath = "Id";
				GoalComboBox.SelectedValue =
					Payment.Goal.Id;
			}
		}

		private void SaveButton_Click(
			object sender,
			RoutedEventArgs e)
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

			Payment.Student = student;
			Payment.Goal = goal;

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