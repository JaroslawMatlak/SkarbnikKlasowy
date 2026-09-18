using ClosedXML.Excel;
using SkarbnikKlasowy.Model;

namespace SkarbnikKlasowy.Services
{
	public static class ExcelReportService
	{
		public static void ExportReport(string filePath)
		{
			var students = CsvService.LoadStudents()
				.OrderBy(x => x.SecondName)
				.ThenBy(x => x.Name)
				.ToList();

			var goals = CsvService.LoadGoals()
				.OrderByDescending(x => x.CreationDate)
				.ToList();

			var payments = CsvService.LoadPayments();
			var modifiers = CsvService.LoadGoalModifiers();
			var overpayments = CsvService.LoadOverpayments();

			using var workbook = new XLWorkbook();

			var worksheet = workbook.Worksheets.Add("Raport");

			// -----------------------------
			// Nagłówki
			// -----------------------------

			worksheet.Cell(1, 1).Value = "Uczeń";
			worksheet.Cell(1, 2).Value = "Nadpłata";

			for (int i = 0; i < goals.Count; i++)
			{
				worksheet.Cell(1, i + 3).Value = goals[i].Name;
			}

			// -----------------------------
			// Dane uczniów
			// -----------------------------

			for (int studentIndex = 0;
				 studentIndex < students.Count;
				 studentIndex++)
			{
				var student = students[studentIndex];

				int row = studentIndex + 2;

				worksheet.Cell(row, 1).Value =
					$"{student.SecondName} {student.Name}";

				double totalOverpayment = 0;

				for (int goalIndex = 0;
					 goalIndex < goals.Count;
					 goalIndex++)
				{
					var goal = goals[goalIndex];

					int column = goalIndex + 3;

					// -----------------------------
					// Zwykłe wpłaty na ten cel
					// -----------------------------

					double paid = payments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.Goal != null &&
							x.Goal.Id == goal.Id)
						.Sum(x => x.Amount);

					// -----------------------------
					// Przekierowania NA ten cel
					// -----------------------------

					double redirectedIn = overpayments
						.Where(x =>
							!x.IsReturned &&
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.TargetGoal != null &&
							x.TargetGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// -----------------------------
					// Przekierowania Z tego celu
					// -----------------------------

					double redirectedOut = overpayments
						.Where(x =>
							x.Student != null &&
							x.Student.Id == student.Id &&
							x.SourceGoal != null &&
							x.SourceGoal.Id == goal.Id)
						.Sum(x => x.Amount);

					// -----------------------------
					// Wymagana kwota
					// Uwzględnia modyfikator
					// -----------------------------

					double requiredAmount =
						GetRequiredAmount(
							student,
							goal,
							modifiers);

					// -----------------------------
					// Rzeczywista kwota przypisana
					// do celu
					// -----------------------------

					double actualAmount =
						paid +
						redirectedIn -
						redirectedOut;

					// -----------------------------
					// Różnica względem wymaganej
					// kwoty
					// -----------------------------

					double difference =
						actualAmount -
						requiredAmount;

					// -----------------------------
					// Status celu
					// -----------------------------

					var cell = worksheet.Cell(row, column);

					if (difference >= 0)
					{
						// Wyświetlamy rzeczywistą kwotę
						// przypisaną do celu
						cell.Value =
							$"{requiredAmount:F2}✓";

						cell.Style.Fill.BackgroundColor =
							XLColor.LightGreen;

						cell.Style.Font.FontColor =
							XLColor.Green;

						cell.Style.Font.Bold = true;

						// Nadpłata pozostająca po opłaceniu celu
						totalOverpayment += difference;
					}
					else
					{
						// Cel nie został opłacony.
						// Wyświetlamy rzeczywistą kwotę,
						// która pozostała na celu.
						cell.Value =
							$"{actualAmount:F2}✗";

						cell.Style.Fill.BackgroundColor =
							XLColor.LightPink;

						cell.Style.Font.FontColor =
							XLColor.Red;

						cell.Style.Font.Bold = true;
					}

					cell.Style.Alignment.Horizontal =
						XLAlignmentHorizontalValues.Center;

					cell.Style.Alignment.Vertical =
						XLAlignmentVerticalValues.Center;
				}

				// -----------------------------
				// Łączna nadpłata ucznia
				// -----------------------------

				worksheet.Cell(row, 2).Value =
					totalOverpayment;

				worksheet.Cell(row, 2)
					.Style.NumberFormat.Format =
					"#,##0.00";
			}

			// -----------------------------
			// Formatowanie nagłówka
			// -----------------------------

			var header = worksheet.Range(
				1,
				1,
				1,
				goals.Count + 2);

			header.Style.Font.Bold = true;

			header.Style.Alignment.Horizontal =
				XLAlignmentHorizontalValues.Center;

			header.Style.Alignment.Vertical =
				XLAlignmentVerticalValues.Center;

			header.Style.Fill.BackgroundColor =
				XLColor.LightGray;

			// -----------------------------
			// Szerokości kolumn
			// -----------------------------

			worksheet.Column(1).Width = 30;
			worksheet.Column(2).Width = 15;

			for (int i = 0; i < goals.Count; i++)
			{
				worksheet.Column(i + 3).Width = 20;
			}

			worksheet.Row(1).Height = 35;

			// -----------------------------
			// Obramowanie tabeli
			// -----------------------------

			var usedRange = worksheet.RangeUsed();

			if (usedRange != null)
			{
				usedRange.Style.Border.OutsideBorder =
					XLBorderStyleValues.Thin;

				usedRange.Style.Border.InsideBorder =
					XLBorderStyleValues.Thin;
			}

			// -----------------------------
			// Zamrożenie nagłówka
			// i dwóch pierwszych kolumn
			// -----------------------------

			worksheet.SheetView.FreezeRows(1);
			worksheet.SheetView.FreezeColumns(2);

			workbook.SaveAs(filePath);
		}

		private static double GetRequiredAmount(
			Student student,
			Goal goal,
			IEnumerable<GoalModifier> modifiers)
		{
			var modifier = modifiers.FirstOrDefault(x =>
				x.Student != null &&
				x.Student.Id == student.Id &&
				x.Goal != null &&
				x.Goal.Id == goal.Id);

			if (modifier == null)
				return goal.Amount;

			return modifier.ModifiedValue;
		}
	}
}