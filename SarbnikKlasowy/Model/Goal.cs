public class Goal
{
	public Goal()
	{
		CreationDate = DateTime.Now;
	}

	public int Id { get; set; }

	public double Amount { get; set; }

	public string Name { get; set; } = "";

	public DateTime CreationDate { get; set; }

	/// <summary>
	/// Ostatni dzień, w którym można wpłacać na cel.
	/// Po tej dacie cel jest zakończony.
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	/// Ręczne zakończenie celu.
	/// </summary>
	public bool IsManuallyClosed { get; set; }

	/// <summary>
	/// Czy cel jest zakończony automatycznie lub ręcznie.
	/// </summary>
	public bool IsClosed =>
		IsManuallyClosed ||
		(EndDate.HasValue &&
		 DateTime.Now.Date > EndDate.Value.Date);
}