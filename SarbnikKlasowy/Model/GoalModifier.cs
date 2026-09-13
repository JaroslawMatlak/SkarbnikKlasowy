namespace SkarbnikKlasowy.Model
{
	public class GoalModifier
	{
		public int Id { get; set; }
		public Student Student { get; set; }
		public Goal Goal { get; set; }
		public double ModifiedValue { get; set; }
	}
}
