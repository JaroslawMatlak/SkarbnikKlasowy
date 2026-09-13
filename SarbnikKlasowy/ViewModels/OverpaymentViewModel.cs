using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class OverpaymentViewModel
	{
		public Student Student { get; }
		public Goal Goal { get; }

		public double Paid { get; }
		public double Required { get; }
		public double Available { get; }

		public string StudentName =>
			$"{Student.SecondName} {Student.Name}";

		public string GoalName =>
			Goal.Name;

		public string PaidDisplay =>
			$"{Paid:F2} zł";

		public string RequiredDisplay =>
			$"{Required:F2} zł";

		public string AvailableDisplay =>
			$"{Available:F2} zł";

		public OverpaymentViewModel(
			Student student,
			Goal goal,
			double paid,
			double required,
			double available)
		{
			Student = student;
			Goal = goal;
			Paid = paid;
			Required = required;
			Available = available;
		}
	}
}