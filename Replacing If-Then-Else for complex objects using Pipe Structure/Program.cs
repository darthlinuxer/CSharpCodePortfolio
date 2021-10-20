using System;
using System.Collections.Generic;
using static System.Console;

namespace Replacing_If_Then_Else_for_complex_objects_using_Pipe_Structure
{
	public class Person
	{
		public string Name { get; set; }
		public static bool operator ==(Person a, Person b) => a.Name == b.Name;
		public static bool operator !=(Person a, Person b) => a.Name != b.Name;
		public override bool Equals(Object a)
		{
			if (!(a is Person)) return false;
			return (a as Person).Name == this.Name;
		}

	}


	public class Program
	{
		public static void Main()
		{
			Person person = new Person() { Name = "Camilo" };

			//GOOD IF
			IEnumerable<TestCondition<Person>> trueConditions = new IFBuilder<Person>(person)
														.AddCheckCondition(typeof(PersonNameIsCamilo))
														.AddCheckCondition(typeof(PersonNameIsAline))
														.Build();

			foreach (var condition in trueConditions) { condition.Handle(); }

			//BAD IF
			if (person.Name == "Camilo") WriteLine("BadIf: Person Name is Camilo");
			if (person.Name == "Aline") WriteLine("BadIf: Person Name is Aline");

		}
	}

	public class IFBuilder<T>
	{
		private T _inputVariableToBeTested;
		List<Type> _conditionTypes = new List<Type>();
		public IFBuilder(T input) => _inputVariableToBeTested = input;

		public IFBuilder<T> AddCheckCondition(Type type)
		{
			if (!type.GetType().IsInstanceOfType(typeof(TestCondition<T>))) throw new Exception();
			_conditionTypes.Add(type);
			return this;
		}

		public IEnumerable<TestCondition<T>> Build()
		{
			foreach (var _conditionType in _conditionTypes)
			{
				if ((Activator.CreateInstance(_conditionType, _inputVariableToBeTested) as TestCondition<T>).isTrue)
					yield return (Activator.CreateInstance(_conditionType, _inputVariableToBeTested) as TestCondition<T>);
			}
		}

	}

	public abstract class TestCondition<T>
	{
		public bool isTrue = false;
		protected T _input;
		public TestCondition(T input) => this._input = input;
		public abstract void Handle();
	}

	public class PersonNameIsCamilo : TestCondition<Person>
	{
		public PersonNameIsCamilo(Person p) : base(p) { this.isTrue = p == new Person { Name = "Camilo" }; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: Person Name is Camilo"); }
	}

	public class PersonNameIsAline : TestCondition<Person>
	{
		public PersonNameIsAline(Person p) : base(p) { this.isTrue = p == new Person { Name = "Aline" }; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: Person Name is Aline"); }
	}

}
