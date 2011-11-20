using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MvvmValidation
{
	public class ValidationErrorCollection : Collection<ValidationError>
	{
		public ValidationErrorCollection()
		{
		}

		public ValidationErrorCollection(IEnumerable<string> errors)
			: this(errors.Select(e => new ValidationError(e, null)).ToList())
		{
		}

		public ValidationErrorCollection(IList<ValidationError> list)
			: base(list)
		{
		}

		public override string ToString()
		{
			var distinctErrors = this.Distinct(e => e.ErrorText).ToArray();

			if (distinctErrors.Length == 1)
			{
				return this[0].ToString();
			}

			var result = new StringBuilder();
			int counter = 1;
			
			foreach (var error in distinctErrors)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, "{0}. {1}", counter, error);
				result.AppendLine();
				counter++;
			}

			return result.ToString().Trim();
		}
	}
}