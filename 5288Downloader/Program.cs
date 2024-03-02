using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using DepotDownloader;
using System.Text;

namespace Downloader5288
{
    class Program
    {
        static int Main(string[] args)
        {
            var ret = MainAsync(args).GetAwaiter().GetResult();

            Console.WriteLine("\n\n\n");
            Console.WriteLine("Finished. Press enter to close this window.");
            Console.Write("\n\n\n");
            Console.ReadLine();

            return ret;
        }

        internal static readonly char[] newLineCharacters = ['\n', '\r'];

        static string Prompt(string question, bool password = false)
        {
            if (question.Length >= 50 || question.Contains("\n")) Console.WriteLine(question);
            else Console.Write(question);
            Console.CursorLeft = 50;
            return password ? ReadPassword() : Console.ReadLine();
        }

        static void WriteError(string error)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + error);
            Console.ForegroundColor = oldColor;
        }

        static async Task<int> MainAsync(string[] args)
        {
            Console.Clear();

            Console.WriteLine("\n\n\n");
            Console.WriteLine("Rudimentary Half-Life 2 build 5288 downloader built on top of DepotDownloader.");
            Console.WriteLine();
            Console.WriteLine("Tool by 2838, March 2024.");
            Console.WriteLine("DepotDownloader by its developers and contributors.");
            Console.Write("\n\n");

            try
            {
                AccountSettingsStore.LoadFromFile("account.config");

                Console.WriteLine();
                Console.WriteLine("==== BEFORE WE BEGIN ====");
                Console.WriteLine();
                Console.WriteLine
                (
                    "This tool will download the files for Half-Life 2 build 5288 directly from Valve's servers, like as if we're installing it straight from Steam.\n" +
                    "This means you will need to own the game before continuing, and you will be asked to provide Steam login credentials.\n" +
                    "You may also be asked to allow this tool to access online resources by Windows. Click allow if so.\n" +
                    "Why do we have to do this? To put it loosely, for this build, if it were to be distributed as is, it'd be too much like piracy."
                );
                Console.WriteLine();
                Prompt("Press enter to continue.");

                Console.WriteLine();
                Console.WriteLine("==== CREDENTIALS ====");
                Console.WriteLine();
                Console.WriteLine
                (
                    "You will be providing your Steam username and password below.\n" +
                    "They are necessary for downloading the files, and will only be used for that purpose.\n" +
                    "You may be asked to provide a Steam Guard verification code."
                );
                Console.WriteLine();
                var username = Prompt("Please enter your username:");
                var password = Prompt("Please enter your password:", true);
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("==== OUTPUT DIRECTORY ====");
                Console.WriteLine();
                Console.WriteLine
                (
                    "You will be providing where the files will be put on your computer.\n" +
                    "It is recommended your provide an empty folder, one made specifically to store the game."
                );
                Console.WriteLine();
                var directory = Prompt("Please enter the output directory:");
                ContentDownloader.Config.InstallDirectory = directory;
                ContentDownloader.Config.VerifyAll = true;
                ContentDownloader.Config.MaxServers = 20;
                ContentDownloader.Config.MaxDownloads = 8;

                Console.WriteLine();
                Console.WriteLine("==== DOWNLOADING ====");
                Console.WriteLine();
                Console.WriteLine
                (
                    "The download is about to begin.\n" +
                    "Progess on logging into Steam and on downloading each file will be printed.\n" +
                    "Errors may be thrown, whose fault for them depends, but you'll want to contact 2838 about it first.\n" +
                    "\n" +
                    "The download will commence in 5 seconds.\n"
                );
                Console.WriteLine("");
                Thread.Sleep(5000);

                var locations = new List<(uint, ulong)>()
                {
                    (222, 3151477805868332059),
                    (221, 2694190745568322895),
                };
                if (InitializeSteam(username, password))
                {
                    try
                    {
                        await ContentDownloader.DownloadAppAsync
                        (
                            220,
                            locations,
                            ContentDownloader.DEFAULT_BRANCH,
                            null,
                            null,
                            "english",
                            false,
                            false
                        )
                        .ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is ContentDownloaderException || ex is OperationCanceledException)
                    {
                        WriteError(ex.ToString());
                        return 1;
                    }
                    finally
                    {
                        ContentDownloader.ShutdownSteam3();
                    }
                }
                else
                {
                    WriteError("Steam couldn't be initalized. Bad login credentials or no internet connection?");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
            }
            return 0;
        }

        static bool InitializeSteam(string username, string password)
        {
            return ContentDownloader.InitializeSteam3(username, password);
        }

        static string ReadPassword()
        {
            ConsoleKeyInfo keyInfo;
            var password = new StringBuilder();

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }

                    continue;
                }

                /* Printable ASCII characters only */
                var c = keyInfo.KeyChar;
                if (c >= ' ' && c <= '~')
                {
                    password.Append(c);
                    Console.Write('*');
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            return password.ToString();
        }
    }
}
