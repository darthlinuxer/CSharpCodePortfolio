using System;
using System.Collections.Generic;
using static System.Console;

namespace Creating_string_Pipes_using_reverse_Pipe_Builder_recursion
{
	public class Program
	{
		public static void Main()
		{
			var pipe = new PipeBuilder((msg) => WriteLine(msg))
				.AddPipe(typeof(PipeUpper))
				.AddPipe(typeof(PipeTrim))
				.Build();
			pipe("Hello      ");
			pipe("     world     ");
		}
	}

	public class PipeBuilder
	{
		Action<string> _mainAction;
		List<Type> _pipeTypes = new List<Type>();
		public PipeBuilder(Action<string> mainAction) => _mainAction = mainAction;

		public PipeBuilder AddPipe(Type pipeType)
		{
			if (!pipeType.GetType().IsInstanceOfType(typeof(Pipe))) throw new Exception();
			_pipeTypes.Add(pipeType);
			return this;
		}

		private Action<string> CreatePipe(int index)
		{
			if (index < _pipeTypes.Count - 1)
			{
				var childPipeHandle = CreatePipe(index + 1);
				var pipe = (Pipe)Activator.CreateInstance(_pipeTypes[index], childPipeHandle);
				return pipe.Handle;
			}
			else
			{
				var finalPipe = (Pipe)Activator.CreateInstance(_pipeTypes[index], _mainAction);
				return finalPipe.Handle;
			}
		}

		public Action<string> Build() => CreatePipe(0);
	}


	public abstract class Pipe
	{
		protected Action<string> _action;
		public Pipe(Action<string> action) => _action = action;
		public abstract void Handle(string msg);
	}

	public class PipeUpper : Pipe
	{
		public PipeUpper(Action<string> action) : base(action) { }
		public override void Handle(string msg) => _action(msg.ToUpper());
	}

	public class PipeTrim : Pipe
	{
		public PipeTrim(Action<string> action) : base(action) { }
		public override void Handle(string msg) => _action(msg.TrimStart().TrimEnd());
	}

}
