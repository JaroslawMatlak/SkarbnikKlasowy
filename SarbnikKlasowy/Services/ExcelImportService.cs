using ClosedXML.Excel;
using SkarbnikKlasowy.Model;
using System.Globalization;

namespace SkarbnikKlasowy.Services
{
	public static class ExcelImportService
	{
		public static List<Payment> ImportPayments(
			string filePath,
			IEnumerable<Payment> existingPayments)
		{
			var payments = new List<Payment>();

			using var workbook = new XLWorkbook(filePath);

			var worksheet = workbook.Worksheets.First();

			var headerRow = worksheet.FirstRowUsed();

			if (headerRow == null)
				return payments;

			var headers = new Dictionary<string, int>(
				StringComparer.OrdinalIgnoreCase);

			foreach (var cell in headerRow.CellsUsed())
			{
				string header = cell.GetString().Trim();

				if (!string.IsNullOrWhiteSpace(header))
					headers[header] = cell.Address.ColumnNumber;
			}

			int transactionDateColumn =
				GetColumn(headers, "Data transakcji");

			int amountColumn =
				GetColumn(headers, "Kwota");

			int senderColumn =
				GetColumn(headers, "Nadawca");

			int descriptionColumn =
				GetColumn(headers, "Opis");

			if (transactionDateColumn == 0 ||
				amountColumn == 0 ||
				senderColumn == 0 ||
				descriptionColumn == 0)
			{
				throw new InvalidOperationException(
					"Nie znaleziono wymaganych kolumn: " +
					"Data transakcji, Kwota, Nadawca, Opis.");
			}

			// Zaczynamy od płatności już zapisanych w aplikacji.
			var knownPayments = existingPayments
				.ToList();

			foreach (var row in worksheet.RowsUsed().Skip(1))
			{
				var dateCell =
					row.Cell(transactionDateColumn);

				var amountCell =
					row.Cell(amountColumn);

				var senderCell =
					row.Cell(senderColumn);

				var descriptionCell =
					row.Cell(descriptionColumn);

				if (dateCell.IsEmpty() &&
					amountCell.IsEmpty() &&
					senderCell.IsEmpty() &&
					descriptionCell.IsEmpty())
				{
					continue;
				}

				// -----------------------------------------
				// DATA
				// -----------------------------------------

				DateTime paymentDate;

				if (dateCell.DataType == XLDataType.DateTime)
				{
					paymentDate = dateCell.GetDateTime();
				}
				else if (!DateTime.TryParse(
					dateCell.GetString().Trim(),
					CultureInfo.GetCultureInfo("pl-PL"),
					DateTimeStyles.None,
					out paymentDate))
				{
					continue;
				}

				// -----------------------------------------
				// KWOTA
				// -----------------------------------------

				double amount;

				if (amountCell.DataType == XLDataType.Number)
				{
					amount = amountCell.GetDouble();
				}
				else
				{
					string amountText = amountCell
						.GetString()
						.Trim()
						.Replace("\u00A0", "")
						.Replace(" ", "")
						.Replace(",", ".");

					if (!double.TryParse(
						amountText,
						NumberStyles.Any,
						CultureInfo.InvariantCulture,
						out amount))
					{
						continue;
					}
				}

				// -----------------------------------------
				// NADAWCA / OPIS
				// -----------------------------------------

				string sender =
					senderCell.GetString().Trim();

				string description =
					descriptionCell.GetString().Trim();

				if (string.IsNullOrWhiteSpace(sender) &&
					string.IsNullOrWhiteSpace(description))
				{
					continue;
				}

				// -----------------------------------------
				// SPRAWDZENIE DUPLIKATU
				// -----------------------------------------

				bool alreadyExists = knownPayments.Any(x =>
					string.Equals(
						x.Sender?.Trim(),
						sender,
						StringComparison.Ordinal) &&

					x.Amount == amount &&

					x.PaymentDate == paymentDate &&

					string.Equals(
						x.Name?.Trim(),
						description,
						StringComparison.Ordinal));

				if (alreadyExists)
					continue;

				// -----------------------------------------
				// NOWA PŁATNOŚĆ
				// -----------------------------------------

				var payment = new Payment
				{
					Amount = amount,
					Name = description,
					Sender = sender,
					PaymentDate = paymentDate,

					Student = new Student
					{
						Id = 0
					},

					Goal = new Goal
					{
						Id = 0
					}
				};

				payments.Add(payment);

				// Dodajemy również do knownPayments,
				// żeby dwa identyczne rekordy znajdujące się
				// w tym samym Excelu nie zostały zaimportowane
				// dwukrotnie.
				knownPayments.Add(payment);
			}

			return payments;
		}

		private static int GetColumn(
			Dictionary<string, int> headers,
			string name)
		{
			return headers.TryGetValue(
				name,
				out int column)
				? column
				: 0;
		}
	}
}