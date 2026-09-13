using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class StudentGoalStatusViewModel
	{
		public Goal Goal { get; }

		public double Required { get; }
		public double Paid { get; }
		public bool IsPaid { get; }

		public string GoalName => Goal.Name;

		public string RequiredDisplay =>
			$"{Required:F2} zł";

		public string PaidDisplay =>
			$"{Paid:F2} zł";

		public string StatusDisplay =>
			IsPaid ? "✓ Opłacone" : "✗ Nieopłacone";

		public StudentGoalStatusViewModel(
			Goal goal,
			double required,
			double paid)
		{
			Goal = goal;
			Required = required;
			Paid = paid;
			IsPaid = paid >= required;
		}
	}
}