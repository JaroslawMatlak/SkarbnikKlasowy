using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.ViewModels
{
	public class PaymentViewModel
	{
		public Payment Payment { get; }

		public int Id => Payment.Id;
		public double Amount => Payment.Amount;
		public string Name => Payment.Name;
		public DateTime CreationDate => Payment.PaymentDate;

		public string StudentDisplayName =>
			Payment.Student.Id == 0
				? "— brak przypisania —"
				: $"{Payment.Student.Name} {Payment.Student.SecondName}";

		public string GoalDisplayName =>
			Payment.Goal.Id == 0
				? "— brak przypisania —"
				: Payment.Goal.Name;

		public bool HasProblem =>
			Payment.Student.Id == 0 ||
			Payment.Goal.Id == 0;

		public PaymentViewModel(Payment payment)
		{
			Payment = payment;
		}
	}
}