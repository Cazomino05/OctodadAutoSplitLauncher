using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
namespace Octodad
{
    class Octodad
    {
        Process pOctodad { get; set; }

        System.Timers.Timer tLoadingTimer { get; set; }
        long lFirstLoadMessage { get; set; }
        long lLastLoadMessage { get; set; }
        public string strCurrentLevel { get; set; }

        public long lLoadingTimes { get; set; }

        public Octodad()
        {
            // no current level
            strCurrentLevel = "";

            // Initialise load timer
            tLoadingTimer = new System.Timers.Timer(2000);
            tLoadingTimer.Enabled = false;
            tLoadingTimer.Elapsed += LoadingTimer_Elapsed;

            // Start the game
            StartOctodad();
        }

        void LoadingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Stop our loading timer
            tLoadingTimer.Stop();
            // Calculate how long the loading time took
            long lLoadTime = lLastLoadMessage - lFirstLoadMessage;

            // Add the loading time on
            lLoadingTimes += lLoadTime;

            // Print the loading time in seconds
            Console.WriteLine("Loading time for current level: {0} seconds, total time {1}", lLoadTime / 1000.0f, lLoadingTimes / 1000.0f );

        }

        eLevelID GetLevelIDFromName(String strBuffer)
        {
            // Maps level names to level IDs
            if ("Church_Main.irr" == strBuffer)
            {
                return eLevelID.Church;
            }
            else if ("House_Main.irr" == strBuffer)
            {
                return eLevelID.Home;
            }
            else if ("OpeningCredits.irr" == strBuffer)
            {
                return eLevelID.OpeningCredits;
            }
            else if ("Grocery_Main.irr" == strBuffer)
            {
                return eLevelID.Grocery;
            }
            else if ("Aquarium_Hub.irr" == strBuffer)
            {
                return eLevelID.Aquarium_Hub;
            }
            else if ("Kelp_Main.irr" == strBuffer)
            {
                return eLevelID.Kelp;
            }
            else if ("Aquarium_Dark_Main.irr" == strBuffer)
            {
                return eLevelID.Aqurium_Deep_Sea;
            }
            else if ("Aquarium_Amazon_Main.irr" == strBuffer)
            {
                return eLevelID.Amazon;
            }
            else if ("Boat_Main.irr" == strBuffer)
            {
                return eLevelID.Boat;
            }
            else if ("Aquarium_Swimming_Main.irr" == strBuffer)
            {
                return eLevelID.Aquarium_Swimming;
            }
            else if ("Stealth_Main.irr" == strBuffer)
            {
                return eLevelID.Stealth;
            }
            else if ("Cafeteria_Main.irr" == strBuffer)
            {
                return eLevelID.Cafeteria;
            }
            else if ("EndingCredits.irr" == strBuffer)
            {
                return eLevelID.Credits;
            }
            else if ("MainScreen_Background.irr" == strBuffer)
            {
                return eLevelID.MainMenu;
            }
            return eLevelID.Unknown;
        }

        public void StartOctodad()
        {
            // Start our octodad starting thread
            Thread pThread = new Thread(StartOctodadThread);
            // Needs to be STA for open file dialog
            pThread.SetApartmentState ( ApartmentState.STA );
            // Start our thread
            pThread.Start();
            // Make sure our thread is still running
            if (pThread.ThreadState == System.Threading.ThreadState.Running)
            {
                // wait for the process to launch
                while (pOctodad == null)
                {
                    // Wait 200ms
                    Thread.Sleep(200);
                    // if we abort during startup
                    if (pThread.ThreadState != System.Threading.ThreadState.Running)
                    {
                        // return
                        return;
                    }
                }
                // Wait for the game to exit
                pOctodad.WaitForExit();
            }
        }

        public static bool FindGame ( )
        {
            // Create an open file dialog
            OpenFileDialog pOpenFileDialog = new OpenFileDialog();
            // Set our initial directory to hopefully a steam directory or just the C drive if not

            if (File.Exists("C:\\Program Files (x86)\\Steam\\SteamApps\\common\\Octodad Dadliest Catch\\"))
                pOpenFileDialog.InitialDirectory = "C:\\Program Files (x86)\\Steam\\SteamApps\\common\\Octodad Dadliest Catch\\";
            else if (File.Exists("C:\\Program Files (x86)\\Steam\\SteamApps\\common\\Octodad Dadliest Catch\\"))
                pOpenFileDialog.InitialDirectory = "C:\\Program Files\\Steam\\SteamApps\\common\\Octodad Dadliest Catch\\";
            else
                pOpenFileDialog.InitialDirectory = "C:\\";


            // Filter anything but octodad
            pOpenFileDialog.Filter = "Octodad Game (OctodadDadliestCatch.exe)|OctodadDadliestCatch.exe";
            // Set the title
            pOpenFileDialog.Title = "Please find the Octodad Executable";
            // Disable multi select
            pOpenFileDialog.Multiselect = false;
            // make sure it exists
            pOpenFileDialog.CheckFileExists = true;
            // if everything is okay
            if (pOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                // set a registry entry
                RegistryKey rkLocation = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("OctodadLauncher");
                rkLocation.SetValue("Location", pOpenFileDialog.FileName);
                rkLocation.Flush();
                // Success
                return true;
            }
            else
            {
                // Something went wrong possibly user abort
                return false;
            }
        }

        public void StartOctodadThread()
        {
            // Set our location to blank for now
            String strLocation = "";
            try
            {
                // Open our registry key
                RegistryKey rkLocation = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey ("OctodadLauncher");
                // Get our location
                strLocation = (String)rkLocation.GetValue("Location");
            }
            catch (Exception)
            {
                // Ask the user for the game path
                if (FindGame())
                {
                    // Open our registry key
                    RegistryKey rkLocation = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("OctodadLauncher");
                    // Get our location
                    strLocation = (String)rkLocation.GetValue("Location");
                }
                else
                {
                    // Something went wrong possibly user abort
                    return;
                }
            }

            // if the path is valid and the location ends with the right executable name
            if (!File.Exists(strLocation) || !strLocation.EndsWith("OctodadDadliestCatch.exe"))
            {
                // Ask the user for the game path
                if ( FindGame() )
                {
                    // Open our registry key
                    RegistryKey rkLocation = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("OctodadLauncher");
                    // Get our location
                    strLocation = (String)rkLocation.GetValue("Location");
                }
                else
                {
                    // Something went wrong possibly user abort
                    return;
                }
            }

            try
            {
                // Start our process
                ProcessStartInfo pStartInfo = new ProcessStartInfo(strLocation)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(strLocation)
                };
                // Start a new process
                pOctodad = new Process();
                // pass the start info
                pOctodad.StartInfo = pStartInfo;
                // Start the game
                pOctodad.Start();

                // Fire off a thread to process the console
                Thread pThread = new Thread(ProcessConsole);
                // Start the thread
                pThread.Start();
            }
            catch
            {
                // Find the game
                FindGame();
                // Restart at the top of this method
                StartOctodadThread();
            }
        }

        public void ProcessConsole()
        {
            // Create a stopwatch with which to time the overhead of this
            Stopwatch stopWatch = new Stopwatch();
            
            // were we previously at the main menu
            bool bPreviouslyMainMenu = false;
            // previous level is now unkown
            eLevelID PreviousLevel = eLevelID.Unknown;
            // Inf game loop
            while (pOctodad.HasExited == false)
            {
                // Read a line from the output
                String strLine = pOctodad.StandardOutput.ReadLine();

                // game probably quit
                if (strLine == null)
                    return;

                // if the length isn't greater than 47 we aren't a loading message
                if (strLine.Length > 47 && strLine[0] == '~' && strLine[5] == '~')
                {
                    // Restart the stopwatch
                    stopWatch.Restart();

                    // Get the sub string to ignore the useless parts (mainly path)
                    strCurrentLevel = strLine.Substring ( 47 );

                    // get the level from the name
                    eLevelID Level = GetLevelIDFromName(strCurrentLevel);

                    // if the level is church and we just came from the menu reset
                    if ( Level == eLevelID.Church && bPreviouslyMainMenu )
                    {
                        // Start a reset the run thread
                        Thread pThread = new Thread(new ThreadStart(SendKey.ResetRun));
                        // run the thread
                        pThread.Start();

                        // Set the previous level to this level
                        PreviousLevel = Level;
                        // set the first loading messageto now
                        lFirstLoadMessage = Environment.TickCount;
                        // reset our loading time to 0
                        lLoadingTimes = 0;

#if DEBUG
                        // print we started the level
                        Console.WriteLine("Starting level: " + strCurrentLevel.Substring ( 0, strCurrentLevel.Length - 4 ) );
                        // print we reset the split
                        Console.WriteLine("Reset splitting on level: " + strCurrentLevel.Substring(0, strCurrentLevel.Length - 4));
#endif
                        // Start a loading timer
                        tLoadingTimer.Start();
                        // we aren't previously at the main menu
                        bPreviouslyMainMenu = false;
                    }
                    else
                    {
#if DEBUG
                        // print we started the level
                        Console.WriteLine("Starting level: " + strCurrentLevel.Substring(0, strCurrentLevel.Length - 4) + " from: " + PreviousLevel.ToString());
#endif
                        // we aren't previously at the main menu
                        bPreviouslyMainMenu = false;

                        if (PreviousLevel == Level)
                        {
                            // print we aren't going to split and why
                            Console.WriteLine("Split refused on level: " + strCurrentLevel.Substring ( 0, strCurrentLevel.Length - 4 ) + " already in this level" );
                            // same level, don't go any further and restart the loop from the top
                            continue;
                        }
                        // ignore main menu and credits
                        // amazon is ignored because it is skipped by going to hub
                        // deep sea is ignored because it is skipped by going into hub
                        // boat is ignored because it is skipped by going into hub
                        // Handle checking previous level such that it splits properly for hub
                        if (Level == eLevelID.MainMenu || Level == eLevelID.OpeningCredits 
                            || Level == eLevelID.Amazon || Level == eLevelID.Aqurium_Deep_Sea || Level == eLevelID.Boat
                            || ( Level == eLevelID.Aquarium_Hub && PreviousLevel != eLevelID.Amazon && PreviousLevel != eLevelID.Grocery && PreviousLevel != eLevelID.Kelp && PreviousLevel != eLevelID.Aqurium_Deep_Sea) )
                        {
                            // don't remember main menu as previous as this causes issues with the above checks
                            if (Level != eLevelID.MainMenu)
                            {
                                // store our previous level
                                PreviousLevel = Level;
                            }
                            else 
                            {
                                // looks like we are the main menu, remember that
                                bPreviouslyMainMenu = true;
                            }
                            // print we aren't going to split and why
                            Console.WriteLine("Split refused on level: " + strCurrentLevel.Substring ( 0, strCurrentLevel.Length - 4 ) + " from: " + PreviousLevel.ToString ( ) );
                            // don't skip, restart the loop from the top
                            continue;
                        }
                        else if (Level == eLevelID.Credits)
                        {
                            // Finished
                            // previous level is now unknown
                            PreviousLevel = eLevelID.Unknown;

                            // Print the loading time in seconds
                            Console.WriteLine("Loading time over entire run: {0} seconds", lLoadingTimes / 1000.0f);
                        }
                        // store our previous level
                        PreviousLevel = Level;

                        // reset our first loading message
                        lFirstLoadMessage = Environment.TickCount;

                        // Create a split press thread
                        Thread pThread = new Thread(new ThreadStart(SendKey.SplitRun));
                        // Start the thread
                        pThread.Start();

                        // Start our loading timer
                        tLoadingTimer.Start();
#if DEBUG
                        // print we split the level
                        Console.WriteLine("Splitting on level: " + strCurrentLevel.Substring ( 0, strCurrentLevel.Length - 4 ) );
                        // print what level this is
                        Console.WriteLine(strCurrentLevel);
                        // reset the stopwatch
                        stopWatch.Stop();

                        // calculate the overhead
                        long lOverheadTime = stopWatch.ElapsedTicks;
                        // format the overhead
                        String strElapsedTime = String.Format("Autosplit overhead: {0} us", lOverheadTime / (Stopwatch.Frequency / (1000L * 1000L)));
                        // print the overhead
                        Console.WriteLine(strElapsedTime);
#endif
                    }
                }
                // if the loading timer is enabled and this is a loaded message
                else if (tLoadingTimer.Enabled && strLine.StartsWith("Loaded "))
                {
                    // set our last load message to now
                    lLastLoadMessage = Environment.TickCount;
                    // restart the loading timer
                    tLoadingTimer.Stop();
                    tLoadingTimer.Start();
                    //Console.WriteLine(strLine);
                }
            }
        }
        

    }
}
