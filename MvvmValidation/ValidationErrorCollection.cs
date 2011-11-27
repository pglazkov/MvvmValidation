using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MvvmValidation
{
	/// <summary>
	/// Represents a collection of <see cref="ValidationError"/> instances.
	/// </summary>
	public class ValidationErrorCollection : Collection<ValidationError>
	{
		internal ValidationErrorCollection()
		{
		}

		internal ValidationErrorCollection(IList<ValidationError> list)
			: base(list)
		{
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			ValidationError[] distinctErrors = this.Distinct(e => e.ErrorText).ToArray();

			if (distinctErrors.Length == 1)
			{
				return this[0].ToString();
			}

			var result = new StringBuilder();
			int counter = 1;

			foreach (ValidationError error in distinctErrors)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, "{0}. {1}", counter, error);
				result.AppendLine();
				counter++;
			}

			return result.ToString().Trim();
		}
	}
}