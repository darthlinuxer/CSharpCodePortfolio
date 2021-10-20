using System;
using System.Collections.Generic;
using static System.Console;

namespace Replacing_If_Then_Else_using_Pipe_Builder_structure__simple_input_types_
{
	public class Program
	{
		public static void Main()
		{
			string simpleInput = "isStop";
			//GOOD IF
			IEnumerable<TestCondition<string>> trueConditions = new IFBuilder<string>(simpleInput)
														.AddCheckCondition(typeof(TestIfInputIsInit))
														.AddCheckCondition(typeof(TestIfInputIsTest))
														.AddCheckCondition(typeof(TestIfInputIsStop))
														.Build();

			foreach (var condition in trueConditions) { condition.Handle(); }

			//BAD IF
			if (simpleInput == "isInit") WriteLine("BadIf: Input isInit");
			if (simpleInput == "isTest") WriteLine("BadIf: Input isTest");
			if (simpleInput == "isStop") WriteLine("BadIf: Input isStop");
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

	public class TestIfInputIsInit : TestCondition<string>
	{
		public TestIfInputIsInit(string input) : base(input) { this.isTrue = input == "isInit"; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: Input " + this._input); }
	}

	public class TestIfInputIsTest : TestCondition<string>
	{
		public TestIfInputIsTest(string input) : base(input) { this.isTrue = input == "isTest"; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: Input " + this._input); }
	}

	public class TestIfInputIsStop : TestCondition<string>
	{
		public TestIfInputIsStop(string input) : base(input) { this.isTrue = input == "isStop"; }
		public override void Handle() { if (this.isTrue) WriteLine("GoodIf: Input " + this._input); }
	}


}
