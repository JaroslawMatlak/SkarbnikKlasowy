using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class GoalViewModel
	{
		public Goal Goal { get; }

		public string Name => Goal.Name;

		public double Amount => Goal.Amount;

		public DateTime CreationDate => Goal.CreationDate;

		public int Id => Goal.Id;

		public double ExpectedAmount { get; }

		public double TotalIncome { get; }

		public int UnpaidStudents { get; }

		public DateTime? EndDate => Goal.EndDate;

		public bool IsManuallyClosed => Goal.IsManuallyClosed;

		public bool IsClosed => Goal.IsClosed;

		public string EndDateDisplay =>
			EndDate.HasValue
				? EndDate.Value.ToString("dd.MM.yyyy")
				: "—";

		public string ExpectedAmountDisplay =>
			$"{ExpectedAmount:F2} zł";

		public string TotalIncomeDisplay =>
			$"{TotalIncome:F2} zł";

		public string UnpaidStudentsDisplay =>
			UnpaidStudents.ToString();

		public GoalViewModel(
			Goal goal,
			double expectedAmount,
			double totalIncome,
			int unpaidStudents)
		{
			Goal = goal;
			ExpectedAmount = expectedAmount;
			TotalIncome = totalIncome;
			UnpaidStudents = unpaidStudents;
		}
	}
}