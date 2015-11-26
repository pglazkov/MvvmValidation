using System;

namespace MvvmValidation.Tests.Fakes
{
	public class FakeException : Exception
	{
		public FakeException()
			: base("Test exception")
		{
			
		}
	}
}