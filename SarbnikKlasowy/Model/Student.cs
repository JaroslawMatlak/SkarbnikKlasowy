namespace SkarbnikKlasowy.Model
{
	public class Student
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string SecondName { get; set; }	

		public string FullName => $"{Name} {SecondName}";
	}
}
