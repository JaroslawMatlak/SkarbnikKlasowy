using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class GoalModifierViewModel
	{
		public GoalModifier Modifier { get; }

		public int Id => Modifier.Id;

		public string StudentName =>
			Modifier.Student == null
				? ""
				: Modifier.Student.FullName;

		public string GoalName =>
			Modifier.Goal?.Name ?? "";

		public double ModifiedValue =>
			Modifier.ModifiedValue;

		public GoalModifierViewModel(GoalModifier modifier)
		{
			Modifier = modifier;
		}
	}
}