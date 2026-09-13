namespace SkarbnikKlasowy.Model
{
	public class Overpayment
	{
		public int Id { get; set; }

		public Student Student { get; set; } = new();

		public Goal SourceGoal { get; set; } = new();

		public Goal? TargetGoal { get; set; }

		public double Amount { get; set; }

		public bool IsReturned { get; set; }

		public DateTime CreationDate { get; set; }
	}
}