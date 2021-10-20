﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Using_Anonymous_Pipes_for_Communication_between_Threads
{
    public class Program
    {
        private string _pipeHandle;
        private ManualResetEventSlim _pipeHandleSet;
        public static void Main()
        {
            var p = new Program();
            p.Run();
            Console.ReadLine();
        }
        public void Run()
        {
            _pipeHandleSet = new ManualResetEventSlim(initialState: false);
            Task.Run(() => Reader());
            Task.Run(() => Writer());
        }
        private void Writer()
        {
            Console.WriteLine("Anonymous pipe writer");
            _pipeHandleSet.Wait();
            var pipeWriter = new AnonymousPipeClientStream(PipeDirection.Out, _pipeHandle);
            using (var writer = new StreamWriter(pipeWriter))
            {
                writer.AutoFlush = true;
                Console.WriteLine("Sending messages");
                for (int i = 0; i < 5; i++) { writer.WriteLine($"Message {i}"); Task.Delay(500).Wait(); }
                writer.WriteLine("end");
            }
        }
        private void Reader()
        {
            try
            {
                Console.WriteLine("Anonymous Pipe Reader");
                var pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.None);
                using (var reader = new StreamReader(pipeReader))
                {
                    _pipeHandle = pipeReader.GetClientHandleAsString();
                    Console.WriteLine($"pipe handle: {_pipeHandle}");
                    _pipeHandleSet.Set();
                    bool end = false;
                    while (!end)
                    {
                        string line = reader.ReadLine();
                        Console.WriteLine(line);
                        if (line == "end") end = true;
                    }
                    Console.WriteLine("Closing communication");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

