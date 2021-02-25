//Pipes are used for communication between processes and threads
//Usually there is a unique pipe server where clients can connect to trade messages
//Pipes are named or anonymous. 
// Anonymous pipes: 1. Are unidirectional 2.Can only communicate with a single client 3.Are limited
// to a single a machine (no internet connection)

using System;
using System.IO.Pipes;
using System.Text;
using static System.Console;
using System.Threading.Tasks;

namespace Server_Client_Communication_with_named_Pipes
{
	public class Program
	{
		public static void Main()
		{
			WriteLine("Pipes communication example");
			Task.Run(() => PipeReader("PipeOne"));
			Task.Run(() => PipeWrite("localhost", "PipeOne"));
			ReadLine();
		}

		private static void PipeReader(string pipeName)
		{
			WriteLine($"Pipe Server name: {pipeName}");
			try
			{
				using var pipeReader = new NamedPipeServerStream(pipeName, PipeDirection.In);
				pipeReader.WaitForConnection();
				WriteLine("Reader connected!");
				const int BUFFERSIZE = 256;
				bool terminated = false;
				while (!terminated)
				{
					byte[] buffer = new byte[BUFFERSIZE];
					int nRead = pipeReader.Read(buffer, 0, BUFFERSIZE);
					string line = Encoding.UTF8.GetString(buffer, 0, nRead);
					WriteLine(line);
					if (line == "Bye") terminated = true;
				}
				WriteLine("Channel closed!");
				ReadLine();
			}
			catch (Exception ex) { WriteLine(ex.Message); }
		}

		private static void PipeWrite(string serverName, string pipeName)
		{
			WriteLine($"Pipe Client name: {pipeName}");
			try
			{
				using var pipeWriter = new NamedPipeClientStream(serverName, pipeName, PipeDirection.Out);
				pipeWriter.Connect();
				WriteLine("Writer connected!");
				string input = "This is a message sent through a Pipe";
				byte[] buffer = Encoding.UTF8.GetBytes(input);
				pipeWriter.Write(buffer, 0, buffer.Length);
				input = "Bye";
				buffer = Encoding.UTF8.GetBytes(input);
				pipeWriter.Write(buffer, 0, buffer.Length);
				WriteLine("Messaged terminated!");
			}
			catch (Exception ex) { WriteLine(ex.Message); }
		}

	}
}
