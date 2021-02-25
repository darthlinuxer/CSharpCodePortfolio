using System;
using System.Collections.Generic;
using static System.Console;
using System.Reflection;
using System.Linq;

namespace Replace_If_Then_Else_for_complex_objects__using_Reflection_
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
			IEnumerable<TestCondition<Person>> trueConditions = new CheckIf<Person>(person).PassTheTestConditions();
			foreach (var condition in trueConditions) { condition.Handle(); }

			//BAD IF
			if (person.Name == "Camilo") { WriteLine("BadIf: This person name is Camilo"); }
			if (person.Name == "Aline") { WriteLine("BadIf: this person name is Aline"); }
		}
	}

	public class CheckIf<T>
	{
		private T _obj;
		public CheckIf(T obj) => this._obj = obj;

		public IEnumerable<TestCondition<T>> PassTheTestConditions()
		{
			IEnumerable<Type> childrenOfTestCondition = Assembly.GetExecutingAssembly().GetExportedTypes().Where(e => e.BaseType == typeof(TestCondition<T>));
			IEnumerable<Type> trueConditionTypes = childrenOfTestCondition.Where(c => (Activator.CreateInstance(c, _obj) as TestCondition<T>).isTrue == true);

			foreach (var type in trueConditionTypes)
			{
				yield return Activator.CreateInstance(type, _obj) as TestCondition<T>;
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

	public class TestConditionIfPersonNameIsCamilo : TestCondition<Person>
	{
		public TestConditionIfPersonNameIsCamilo(Person p) : base(p) { this.isTrue = p.Name == "Camilo"; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: This person name is Camilo"); }
	}

	public class TestConditionIfPersonNameIsAline : TestCondition<Person>
	{
		public TestConditionIfPersonNameIsAline(Person p) : base(p) { this.isTrue = p.Name == "Aline"; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: This person name is Aline"); }
	}

}
