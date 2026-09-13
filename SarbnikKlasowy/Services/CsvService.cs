using SkarbnikKlasowy.Model;
using System.Globalization;
using System.IO;
using System.Text;

namespace SkarbnikKlasowy.Services
{
	public static class CsvService
	{

		private static string GetFilePath(string modelName)
		{
			string dataDirectory = Path.Combine(
				AppContext.BaseDirectory,
				"Data");

			Directory.CreateDirectory(dataDirectory);

			return Path.Combine(dataDirectory, $"{modelName}.csv");
		}

		public static List<Student> LoadStudents()
		{
			string filePath = GetFilePath("Student");

			var students = new List<Student>();

			if (!File.Exists(filePath))
				return students;

			var lines = File.ReadAllLines(filePath);

			// Pomijamy nagłówek
			foreach (var line in lines.Skip(1))
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				var fields = line.Split(',');

				if (fields.Length < 3)
					continue;

				if (!int.TryParse(
						fields[0],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int id))
				{
					continue;
				}

				students.Add(new Student
				{
					Id = id,
					Name = fields[1].Trim(),
					SecondName = fields[2].Trim()
				});
			}

			return students;
		}

		public static void SaveStudents(IEnumerable<Student> students)
		{
			string filePath = GetFilePath("Student");

			var lines = new List<string>
			{
				"Id,Name,SecondName"
			};

			foreach (var student in students)
			{
				lines.Add(
					$"{EscapeCsv(student.Id.ToString(CultureInfo.InvariantCulture))}," +
					$"{EscapeCsv(student.Name)}," +
					$"{EscapeCsv(student.SecondName)}");
			}

			File.WriteAllLines(
				filePath,
				lines,
				new UTF8Encoding(false));
		}

		public static List<Goal> LoadGoals()
		{
			string filePath = GetFilePath("Goal");

			if (!File.Exists(filePath))
				return new List<Goal>();

			string content = File.ReadAllText(
				filePath,
				Encoding.UTF8);

			var records = ParseCsv(content);

			var goals = new List<Goal>();

			if (records.Count <= 1)
				return goals;

			foreach (var fields in records.Skip(1))
			{
				// Stara wersja ma 4 pola.
				// Nowa wersja ma 6 pól.
				if (fields.Count < 4)
					continue;

				if (!int.TryParse(
						fields[0],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int id))
				{
					continue;
				}

				if (!double.TryParse(
						fields[1],
						NumberStyles.Float,
						CultureInfo.InvariantCulture,
						out double amount))
				{
					continue;
				}

				if (!DateTime.TryParse(
						fields[3],
						CultureInfo.InvariantCulture,
						DateTimeStyles.RoundtripKind,
						out DateTime creationDate))
				{
					continue;
				}

				// Domyślne wartości dla starego formatu CSV
				DateTime? endDate = null;
				bool isManuallyClosed = false;

				// Nowy format:
				// fields[4] = EndDate
				// fields[5] = IsManuallyClosed

				if (fields.Count >= 5 &&
					!string.IsNullOrWhiteSpace(fields[4]) &&
					fields[4] != "0")
				{
					if (DateTime.TryParse(
							fields[4],
							CultureInfo.InvariantCulture,
							DateTimeStyles.RoundtripKind,
							out DateTime parsedEndDate))
					{
						endDate = parsedEndDate;
					}
				}

				if (fields.Count >= 6 &&
					!string.IsNullOrWhiteSpace(fields[5]))
				{
					bool.TryParse(
						fields[5],
						out isManuallyClosed);
				}

				goals.Add(new Goal
				{
					Id = id,
					Amount = amount,
					Name = fields[2],
					CreationDate = creationDate,
					EndDate = endDate,
					IsManuallyClosed = isManuallyClosed
				});
			}

			return goals;
		}

		public static void SaveGoals(IEnumerable<Goal> goals)
		{
			string filePath = GetFilePath("Goal");

			var lines = new List<string>
			{
				"Id,Amount,Name,CreationDate"
			};

			foreach (var goal in goals)
			{
				lines.Add(
					$"{goal.Id.ToString(CultureInfo.InvariantCulture)}," +
					$"{goal.Amount.ToString(CultureInfo.InvariantCulture)}," +
					$"{EscapeCsv(goal.Name)}," +
					$"{goal.CreationDate.ToString("O", CultureInfo.InvariantCulture)}," +
					$"{(goal.EndDate.HasValue
						? goal.EndDate.Value.ToString("O", CultureInfo.InvariantCulture)
						: "")}," +
					$"{goal.IsManuallyClosed}");
			}

			File.WriteAllLines(
				filePath,
				lines,
				new UTF8Encoding(false));
		}

		public static List<Payment> LoadPayments()
		{
			string filePath = GetFilePath("Payment");

			var payments = new List<Payment>();

			if (!File.Exists(filePath))
				return payments;

			var students = LoadStudents();
			var goals = LoadGoals();

			var studentDictionary = students
				.ToDictionary(x => x.Id);

			var goalDictionary = goals
				.ToDictionary(x => x.Id);

			string content = File.ReadAllText(
				filePath,
				Encoding.UTF8);

			var records = ParseCsv(content);

			if (records.Count <= 1)
				return payments;

			// Pomijamy nagłówek
			foreach (var fields in records.Skip(1))
			{
				if (fields.Count < 7)
					continue;

				// Id
				if (!int.TryParse(
						fields[0],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int id))
				{
					continue;
				}

				// Kwota
				if (!double.TryParse(
						fields[1],
						NumberStyles.Float,
						CultureInfo.InvariantCulture,
						out double amount))
				{
					continue;
				}

				// Data płatności
				if (!DateTime.TryParse(
						fields[4],
						CultureInfo.InvariantCulture,
						DateTimeStyles.RoundtripKind,
						out DateTime paymentDate))
				{
					continue;
				}

				// StudentId
				if (!int.TryParse(
						fields[5],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int studentId))
				{
					studentId = 0;
				}

				// GoalId
				if (!int.TryParse(
						fields[6],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int goalId))
				{
					goalId = 0;
				}

				// Student
				Student student;

				if (studentId != 0 &&
					studentDictionary.TryGetValue(
						studentId,
						out var foundStudent))
				{
					student = foundStudent;
				}
				else
				{
					student = new Student
					{
						Id = 0
					};
				}

				// Goal
				Goal goal;

				if (goalId != 0 &&
					goalDictionary.TryGetValue(
						goalId,
						out var foundGoal))
				{
					goal = foundGoal;
				}
				else
				{
					goal = new Goal
					{
						Id = 0
					};
				}

				// Payment
				var payment = new Payment
				{
					Id = id,
					Amount = amount,
					Name = fields[2],
					Sender = fields[3],
					PaymentDate = paymentDate,
					Student = student,
					Goal = goal
				};

				payments.Add(payment);
			}

			return payments;
		}

		public static void SavePayments(IEnumerable<Payment> payments)
		{
			string filePath = GetFilePath("Payment");

			var lines = new List<string>
			{
				"Id,Amount,Name,Sender,PaymentDate,StudentId,GoalId"
			};

			foreach (var payment in payments)
			{
				int studentId = payment.Student?.Id ?? 0;
				int goalId = payment.Goal?.Id ?? 0;

				lines.Add(
					$"{payment.Id.ToString(CultureInfo.InvariantCulture)}," +
					$"{payment.Amount.ToString(CultureInfo.InvariantCulture)}," +
					$"{EscapeCsv(payment.Name)}," +
					$"{EscapeCsv(payment.Sender)}," +
					$"{payment.PaymentDate.ToString("O", CultureInfo.InvariantCulture)}," +
					$"{studentId.ToString(CultureInfo.InvariantCulture)}," +
					$"{goalId.ToString(CultureInfo.InvariantCulture)}");
			}

			File.WriteAllLines(
				filePath,
				lines,
				new UTF8Encoding(false));
		}

		public static void SaveGoalModifiers(
	IEnumerable<GoalModifier> modifiers)
		{
			string filePath = GetFilePath("GoalModifier");

			var lines = new List<string>
	{
		"Id,StudentId,GoalId,ModifiedValue"
	};

			foreach (var modifier in modifiers)
			{
				int studentId = modifier.Student?.Id ?? 0;
				int goalId = modifier.Goal?.Id ?? 0;

				lines.Add(
					$"{modifier.Id.ToString(CultureInfo.InvariantCulture)}," +
					$"{studentId.ToString(CultureInfo.InvariantCulture)}," +
					$"{goalId.ToString(CultureInfo.InvariantCulture)}," +
					$"{modifier.ModifiedValue.ToString(CultureInfo.InvariantCulture)}");
			}

			File.WriteAllLines(
				filePath,
				lines,
				new UTF8Encoding(false));
		}


		public static List<GoalModifier> LoadGoalModifiers()
		{
			string filePath = GetFilePath("GoalModifier");

			var modifiers = new List<GoalModifier>();

			if (!File.Exists(filePath))
				return modifiers;

			var students = LoadStudents();
			var goals = LoadGoals();

			var studentDictionary = students.ToDictionary(x => x.Id);
			var goalDictionary = goals.ToDictionary(x => x.Id);

			string content = File.ReadAllText(filePath, Encoding.UTF8);
			var records = ParseCsv(content);

			if (records.Count <= 1)
				return modifiers;

			foreach (var fields in records.Skip(1))
			{
				if (fields.Count < 4)
					continue;

				if (!int.TryParse(
						fields[0],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int id))
					continue;

				if (!int.TryParse(
						fields[1],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int studentId))
					continue;

				if (!int.TryParse(
						fields[2],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int goalId))
					continue;

				if (!double.TryParse(
					fields[3],
					NumberStyles.Float,
					CultureInfo.InvariantCulture,
					out double modifiedValue))
				{
					continue;
				}

				if (!studentDictionary.TryGetValue(studentId, out var student))
					continue;

				if (!goalDictionary.TryGetValue(goalId, out var goal))
					continue;

				modifiers.Add(new GoalModifier
				{
					Id = id,
					Student = student,
					Goal = goal,
					ModifiedValue = modifiedValue
				});
			}

			return modifiers;
		}
		public static List<Overpayment> LoadOverpayments()
		{
			string filePath = GetFilePath("GoalOverpayment");

			var result = new List<Overpayment>();

			if (!File.Exists(filePath))
				return result;

			var students = LoadStudents();
			var goals = LoadGoals();

			var studentDictionary = students.ToDictionary(x => x.Id);
			var goalDictionary = goals.ToDictionary(x => x.Id);

			string content = File.ReadAllText(filePath, Encoding.UTF8);
			var records = ParseCsv(content);

			if (records.Count <= 1)
				return result;

			foreach (var fields in records.Skip(1))
			{
				if (fields.Count < 7)
					continue;

				if (!int.TryParse(
						fields[0],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int id))
					continue;

				if (!int.TryParse(
						fields[1],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int studentId))
					continue;

				if (!int.TryParse(
						fields[2],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int sourceGoalId))
					continue;

				if (!double.TryParse(
						fields[4],
						NumberStyles.Float,
						CultureInfo.InvariantCulture,
						out double amount))
					continue;

				if (!bool.TryParse(fields[5], out bool isReturned))
					continue;

				if (!DateTime.TryParse(
						fields[6],
						CultureInfo.InvariantCulture,
						DateTimeStyles.RoundtripKind,
						out DateTime creationDate))
					continue;

				if (!studentDictionary.TryGetValue(
						studentId,
						out var student))
					continue;

				if (!goalDictionary.TryGetValue(
						sourceGoalId,
						out var sourceGoal))
					continue;

				Goal? targetGoal = null;

				if (int.TryParse(
						fields[3],
						NumberStyles.Integer,
						CultureInfo.InvariantCulture,
						out int targetGoalId) &&
					targetGoalId != 0)
				{
					goalDictionary.TryGetValue(
						targetGoalId,
						out targetGoal);
				}

				result.Add(new Overpayment
				{
					Id = id,
					Student = student,
					SourceGoal = sourceGoal,
					TargetGoal = targetGoal,
					Amount = amount,
					IsReturned = isReturned,
					CreationDate = creationDate
				});
			}

			return result;
		}

		public static void SaveOverpayments(
	IEnumerable<Overpayment> overpayments)
		{
			string filePath = GetFilePath("GoalOverpayment");

			var lines = new List<string>
	{
		"Id,StudentId,SourceGoalId,TargetGoalId,Amount,IsReturned,CreationDate"
	};

			foreach (var item in overpayments)
			{
				int studentId = item.Student?.Id ?? 0;
				int sourceGoalId = item.SourceGoal?.Id ?? 0;
				int? targetGoalId = item.TargetGoal?.Id;

				lines.Add(
					$"{item.Id.ToString(CultureInfo.InvariantCulture)}," +
					$"{studentId.ToString(CultureInfo.InvariantCulture)}," +
					$"{sourceGoalId.ToString(CultureInfo.InvariantCulture)}," +
					$"{(targetGoalId ?? 0).ToString(CultureInfo.InvariantCulture)}," +
					$"{item.Amount.ToString(CultureInfo.InvariantCulture)}," +
					$"{item.IsReturned.ToString(CultureInfo.InvariantCulture)}," +
					$"{item.CreationDate.ToString("O", CultureInfo.InvariantCulture)}");
			}

			File.WriteAllLines(
				filePath,
				lines,
				new UTF8Encoding(false));
		}



		private static string EscapeCsv(string value)
		{
			if (value.Contains(',') ||
				value.Contains('"') ||
				value.Contains('\n') ||
				value.Contains('\r'))
			{
				return $"\"{value.Replace("\"", "\"\"")}\"";
			}

			return value;
		}

		private static List<List<string>> ParseCsv(string content)
		{
			var records = new List<List<string>>();
			var currentRecord = new List<string>();
			var currentField = new System.Text.StringBuilder();

			bool insideQuotes = false;

			for (int i = 0; i < content.Length; i++)
			{
				char c = content[i];

				if (insideQuotes)
				{
					if (c == '"')
					{
						// Podwójny cudzysłów "" oznacza pojedynczy "
						if (i + 1 < content.Length &&
							content[i + 1] == '"')
						{
							currentField.Append('"');
							i++;
						}
						else
						{
							insideQuotes = false;
						}
					}
					else
					{
						// Tutaj może być również \r lub \n.
						// Zachowujemy je w polu.
						currentField.Append(c);
					}

					continue;
				}

				if (c == '"')
				{
					insideQuotes = true;
				}
				else if (c == ',')
				{
					currentRecord.Add(currentField.ToString());
					currentField.Clear();
				}
				else if (c == '\r')
				{
					// Pomijamy \r. Jeśli to CRLF, \n obsłużymy niżej.
					if (i + 1 < content.Length &&
						content[i + 1] == '\n')
					{
						continue;
					}
				}
				else if (c == '\n')
				{
					currentRecord.Add(currentField.ToString());
					currentField.Clear();

					if (currentRecord.Count > 0)
						records.Add(currentRecord);

					currentRecord = new List<string>();
				}
				else
				{
					currentField.Append(c);
				}
			}

			// Ostatnie pole/rekord
			if (currentField.Length > 0 || currentRecord.Count > 0)
			{
				currentRecord.Add(currentField.ToString());
				records.Add(currentRecord);
			}

			return records;
		}

		private static List<string> ParseCsvLine(string line)
		{
			var result = new List<string>();
			var current = new StringBuilder();
			bool insideQuotes = false;

			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];

				if (c == '"')
				{
					if (insideQuotes &&
						i + 1 < line.Length &&
						line[i + 1] == '"')
					{
						current.Append('"');
						i++;
					}
					else
					{
						insideQuotes = !insideQuotes;
					}
				}
				else if (c == ',' && !insideQuotes)
				{
					result.Add(current.ToString());
					current.Clear();
				}
				else
				{
					current.Append(c);
				}
			}

			result.Add(current.ToString());

			return result;
		}
	}
}
