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
using System.Threading;

class Program
{
	static async Task Main(string[] args)
	{
		try
		{
			WriteLine("-----> Pipes communication example");
			Task.Run(() => PipeClient("1", "localhost", "PipeOne"));
			Task.Run(() => PipeClient("2", "localhost", "PipeOne"));
			Task.Run(() => PipeClient("3", "localhost", "PipeOne"));
			Task.Run(() => PipeServer("1", "PipeOne"));			
			Task.Run(() => PipeServer("2", "PipeOne"));						
			ReadLine();
		}
		catch (Exception ex)
		{
			WriteLine(ex.Message);
		}
		finally
		{
			WriteLine("Finished execution");
		}
	}

	static async Task PipeServer(string server, string pipeName)
	{
		WriteLine($"Server {server} Created on ThreadId: {Task.CurrentId} , Pipe name: {pipeName}");
		try
		{
			
			while (true)
			{
				using var pipeReader = new NamedPipeServerStream(pipeName: pipeName,
                                                     direction: PipeDirection.In,
                                                     maxNumberOfServerInstances: 1,
                                                     transmissionMode: PipeTransmissionMode.Byte,
                                                     options: PipeOptions.FirstPipeInstance);

				await pipeReader.WaitForConnectionAsync();
				WriteLine($"Server {server}: Client connected: {pipeReader.IsConnected}");
				const int BUFFERSIZE = 256;
				bool terminated = false;
				while (!terminated)
				{
					byte[] buffer = new byte[BUFFERSIZE];
					int nRead = await pipeReader.ReadAsync(buffer, 0, BUFFERSIZE);
					string line = Encoding.UTF8.GetString(buffer, 0, nRead);
					WriteLine(line);
					if (line == "Bye") terminated = true;
				}
				WriteLine($"Server {server}: Channel closed!");
			}

		}
		catch (Exception ex) { WriteLine($"Server: {server} {ex.Message}"); }
	}

	static async Task PipeClient(string clientName, string serverAddr, string pipeName)
	{
		WriteLine($"Client {clientName} created on ThreadId: {Task.CurrentId} -> Pipe:{pipeName}");
		try
		{
			using var pipeWriter = new NamedPipeClientStream(serverAddr, pipeName, PipeDirection.Out);
			await pipeWriter.ConnectAsync();
			WriteLine($"Client {clientName}: Client connected = {pipeWriter.IsConnected}");
			await SendMsgAsync(pipeWriter, $"Client {clientName}: This is a message sent through a Pipe");
			await SendMsgAsync(pipeWriter, $"Client {clientName}: This is a 2nd message sent through a Pipe");
			await SendMsgAsync(pipeWriter, $"Client {clientName}: This is the last message sent through a Pipe");
			await SendMsgAsync(pipeWriter, "Bye");
			WriteLine($"Client {clientName} terminated!");
		}
		catch (Exception ex) { WriteLine($"Client: {clientName} {ex.Message}"); }
	}


	private static Task SendMsgAsync(NamedPipeClientStream pipeWriter, string input, int delay = 1)
	{
		Thread.Sleep(TimeSpan.FromSeconds(delay));
		byte[] buffer = Encoding.UTF8.GetBytes(input);
		return pipeWriter.WriteAsync(buffer, 0, buffer.Length);
	}

}


