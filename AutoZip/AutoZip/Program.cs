using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AutoZip
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                List<string> folders = new List<string>
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "bfa"),
                    Path.Combine(Directory.GetCurrentDirectory(), "legion"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wod"),
                    Path.Combine(Directory.GetCurrentDirectory(), "mop"),
                    Path.Combine(Directory.GetCurrentDirectory(), "cata"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wotlk"),
                    Path.Combine(Directory.GetCurrentDirectory(),  "bc"),
                    Path.Combine(Directory.GetCurrentDirectory(), "vanilla"),
                };

                foreach (var folder in folders)
                {
                    Console.WriteLine(folder + "...");
                    var zipFile = folder + ".zip";
                    if (File.Exists(zipFile))
                        File.Delete(zipFile);
                    ZipFile.CreateFromDirectory(folder, zipFile);
                    using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            back:
                            foreach (var entry in archive.Entries)
                            {
                                if (entry.Name == ".gitkeep")
                                {
                                    entry.Delete();
                                    goto back;
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Done, press any key to close");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
            Console.ReadKey();
        }
    }
}
