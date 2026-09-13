namespace SkarbnikKlasowy.Model
{
	public class Payment
	{
		public int Id { get; set; }
		public double Amount { get; set; } = 0;
		public string Name { get; set; } = string.Empty;
		public string Sender { get; set; } = string.Empty;
		public DateTime PaymentDate { get; set; } 
		public Student Student { get; set; }
		public Goal Goal { get; set; }
	}
}
