//Smallest Client Server Socket Communication

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client_Server_Socket_Communication
{
	public class Program
	{
		public static int port = 8080;

		public static void Main()
		{
			Task.Run(() => InitiateServer());
			Thread.Sleep(5000);
			InitiateClient();
			Console.ReadLine();
		}

		public static void InitiateServer()
		{
			IPHostEntry host = Dns.GetHostEntry("localhost");
			IPAddress ipAddress = host.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
			try
			{
				Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				listener.Bind(localEndPoint);
				listener.Listen(10); //listen to 10 requests before replying busy
				Console.WriteLine("Server: Awaiting Connection...");
				Socket handler = listener.Accept();
				string data = null;
				byte[] bytes = null;
				while (true)
				{
					bytes = new byte[1024];
					int bytesRec = handler.Receive(bytes);
					data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
					if (data.IndexOf("<EOF>") > -1)
						break;
				}

				Console.WriteLine($"Server: Text received ---> Msg: {data}");
				byte[] msg = Encoding.ASCII.GetBytes(data);
				handler.Send(msg);
				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			Console.WriteLine("\n Press something to continue...");
			Console.ReadKey();
		}

		public static void InitiateClient()
		{
			byte[] bytes = new byte[1024];
			try
			{
				IPHostEntry host = Dns.GetHostEntry("localhost");
				IPAddress ipAddress = host.AddressList[0];
				IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
				Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					sender.Connect(remoteEP);
					Console.WriteLine("Client: Socket conected with {0} ", sender.RemoteEndPoint.ToString());
					byte[] msg = Encoding.ASCII.GetBytes("Client sending message <EOF>");
					int bytesSent = sender.Send(msg);
					int bytesRec = sender.Receive(bytes);
					Console.WriteLine("Client: Eco of the message sent received back on Client ---> Msg: {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
					sender.Shutdown(SocketShutdown.Both);
					sender.Close();
				}
				catch (ArgumentNullException ane) { Console.WriteLine("ArgumentNullException : {0}", ane.ToString()); }
				catch (SocketException se) { Console.WriteLine("SocketException : {0}", se.ToString()); }
				catch (Exception e) { Console.WriteLine("Unexpected exception : {0}", e.ToString()); }
			}
			catch (Exception e) { Console.WriteLine(e.ToString()); }
		}
	}
}
