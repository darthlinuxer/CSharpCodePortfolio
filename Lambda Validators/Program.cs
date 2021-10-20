using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lambda_Validators
{
	public class AtomicValidations<T>
	{
		public static readonly Predicate<T> NotNull = (v) => v != null;
		public static readonly Predicate<T> IsPositive = (v) => v as int? > 0;
	}

	public class Validator<T> //receives an object of type T together with an array of validators for that type
	{
		public static bool Validate(T objectToValidate, params Predicate<T>[] validations) => validations.ToList().Where(p => !p(objectToValidate)).Count() == 0; //p is type predicate	
	}

	public class Person
	{
		public string Name { get; set; } = "";
		public int Age { get; set; } = 0;
		public string Email { get; set; } = "";
		public bool IsValid(params Predicate<Person>[] Rules) => Validator<Person>.Validate(this, Rules);
	}

	public class PersonValidator
	{
		public static readonly Predicate<Person>[] Rules =
		{
		(p) => AtomicValidations<string>.NotNull(p.Name),
		(p) => AtomicValidations<string>.NotNull(p.Email),
		(p) => AtomicValidations<int>.IsPositive(p.Age),
		(p) => //Email validation Rule using Regex
		{
			Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
									,RegexOptions.CultureInvariant | RegexOptions.Singleline);
			return regex.IsMatch(p.Email);
		}
	};
	}

	public class Program
	{
		public static void Main()
		{
			Person camilo = new Person { Name = "Camilo", Age = -1, Email = "test@test.com" }; //AGE IS NEGATIVE! That's not possible! 		
			Action<Person> IsValidPerson = camilo.IsValid(PersonValidator.Rules) ? (Action<Person>)Success : (Action<Person>)Error;
			IsValidPerson(camilo);
		}

		public static void Success(Person p) => Console.WriteLine($"Validation Result-> Person: {p.Name} has valid parameters!");
		public static void Error(Person p) => Console.WriteLine($"Validation Result-> Person: {p.Name} has invalid parameters!");
	}
}
