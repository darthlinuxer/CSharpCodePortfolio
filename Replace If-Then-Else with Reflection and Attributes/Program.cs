using System;
using static System.Console;
using System.Reflection;
using System.Linq;


namespace Replace_If_Then_Else_with_Reflection_and_Attributes
{
	public class Program
	{

		public enum State { init, running, closed, cancelled }

		public static void Main()
		{
			int returnedValue = BadIf(State.running);
			WriteLine(returnedValue);
			returnedValue = GoodIf(State.running);
			WriteLine(returnedValue);
		}

		public static int BadIf(State state)
		{
			switch (state)
			{
				case State.init:
					WriteLine("BadIf: Do some init actions");
					return 1;
				case State.cancelled:
					WriteLine("BadIf: App is in cancelled State");
					return 2;
				case State.closed:
					WriteLine("BadIf: App is in closed State");
					return 3;
				case State.running:
					WriteLine("BadIf: App is running State");
					return 4;
			}
			return -1;
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class StateAttribute : System.Attribute { public State StateEnum { get; set; } public StateAttribute(State state) => this.StateEnum = state; }


		public interface IIfCondition
		{
			public int Execute();
		}

		[StateAttribute(State.init)]
		public class InitState : IIfCondition { public int Execute() { WriteLine("GoodIf: Do some init actions"); return 1; } }

		[StateAttribute(State.cancelled)]
		public class CancelledState : IIfCondition { public int Execute() { WriteLine("GoodIf: App is in cancelled State"); return 2; } }

		[StateAttribute(State.closed)]
		public class ClosedState : IIfCondition { public int Execute() { WriteLine("GoodIf: App is in closed State"); return 3; } }

		[StateAttribute(State.running)]
		public class RunningState : IIfCondition { public int Execute() { WriteLine("GoodIf: App is in running State"); return 4; } }


		public static int GoodIf(State state)
		{
			Type iifType = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof(IIfCondition)))
				.First(t => t.GetCustomAttribute<StateAttribute>().StateEnum == state);
			return (Activator.CreateInstance(iifType) as IIfCondition).Execute();
		}

	}


}
