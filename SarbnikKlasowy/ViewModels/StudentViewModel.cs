using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class StudentViewModel
	{
		public Student Student { get; }

		public string Name => Student.Name;

		public string SecondName => Student.SecondName;

		public double TotalPayments { get; }

		public double UnhandledOverpayments { get; }

		public bool HasUnpaidGoals { get; }

		public string TotalPaymentsDisplay =>
			$"{TotalPayments:F2} zł";

		public string UnhandledOverpaymentsDisplay =>
			$"{UnhandledOverpayments:F2} zł";

		public string UnpaidGoalsDisplay =>
			HasUnpaidGoals ? "✗" : "";

		public StudentViewModel(
			Student student,
			double totalPayments,
			double unhandledOverpayments,
			bool hasUnpaidGoals)
		{
			Student = student;
			TotalPayments = totalPayments;
			UnhandledOverpayments = unhandledOverpayments;
			HasUnpaidGoals = hasUnpaidGoals;
		}
	}
}