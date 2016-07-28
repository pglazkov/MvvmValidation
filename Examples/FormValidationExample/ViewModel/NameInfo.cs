namespace FormValidationExample.ViewModel
{
	public class NameInfo
	{
		public NameInfo()
		{
		}

		public NameInfo(string firstName, string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}