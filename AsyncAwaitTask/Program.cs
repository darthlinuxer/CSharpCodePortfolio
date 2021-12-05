using static System.Console;
WriteLine(SyncProcesses.MakeTea());
WriteLine("-------------------");
ReadLine();
WriteLine(await ASyncProcesses.MakeTeaAsync(2000));
WriteLine("-------------------");
ReadLine();
WriteLine(await ASyncProcesses.MakeTeaAsync(100));


