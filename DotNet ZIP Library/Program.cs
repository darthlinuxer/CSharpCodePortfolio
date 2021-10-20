using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

public class Program
{
	public static void Main()
	{
		string[] filenames = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json");
		using (ZipOutputStream s = new ZipOutputStream(File.Create("test.zip")))
		{
			s.SetLevel(9); // 0 - store only to 9 - means best compression
			byte[] buffer = new byte[4096];

			foreach (string file in filenames)
			{
				Console.WriteLine($"Adding file: {file}");
				var entry = new ZipEntry(Path.GetFileName(file));
				entry.DateTime = DateTime.Now;
				s.PutNextEntry(entry);

				using (FileStream fs = File.OpenRead(file))
				{
					int sourceBytes;
					do
					{
						sourceBytes = fs.Read(buffer, 0, buffer.Length);
						s.Write(buffer, 0, sourceBytes);
					} while (sourceBytes > 0);
				}
			}

			s.Finish();
			s.Close();
		}

		Console.WriteLine(new String('-', 30));
		Console.WriteLine("Viewing the contents of the Zip");
		byte[] data = new byte[4096];
		using (ZipInputStream s = new ZipInputStream(File.OpenRead("test.zip")))
		{
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{
				Console.WriteLine("Raw Size    Size       Date       Time     Name");
				Console.WriteLine("--------  --------  -----------  ------  ---------");
				Console.WriteLine("{0, -10}{1, -10}{2}  {3}   {4}", theEntry.Size, theEntry.CompressedSize,
																		theEntry.DateTime.ToString("dd MMM yyyy"), theEntry.DateTime.ToString("HH:mm"),
																		theEntry.Name);
				Console.WriteLine("File content...");
				if (theEntry.IsFile)
				{
					int size = s.Read(data, 0, data.Length);
					while (size > 0)
					{
						Console.Write(Encoding.ASCII.GetString(data, 0, size));
						size = s.Read(data, 0, data.Length);
					}
				}
			}
			s.Close();
		}

		//UNZIP to a new directory
		Console.WriteLine();
		Console.WriteLine(new String('-', 30));
		Console.WriteLine("Unzupping the contents of the Zip to a different directory");

		using (ZipInputStream s = new ZipInputStream(File.OpenRead("test.zip")))
		{
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{
				Console.WriteLine($"Unzipping : {theEntry.Name}");
				string directoryName = Directory.GetCurrentDirectory()+"/unzip";
				string fileName = Path.GetFileName(theEntry.Name);

				// create directory
				if (directoryName.Length > 0) Directory.CreateDirectory(directoryName);

				if (fileName != String.Empty)
				{
                    using FileStream streamWriter = File.Create(directoryName+"/"+fileName);
                    int size = 2048;
                    data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0) {streamWriter.Write(data, 0, size);} else break;
					}
                }
			}
		}

		string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory());
		filenames = Directory.GetFiles(Directory.GetCurrentDirectory()+"/unzip");
		foreach (var name in directories) {	Console.WriteLine($"Directory Found: {name}");	}
		foreach (var name in filenames) { Console.WriteLine($"File Found in Unzip Directory: {name}"); }

	}
}
