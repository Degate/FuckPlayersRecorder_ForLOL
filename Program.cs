using FuckPlayersRecorder;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Xml.Linq;

namespace FuckPlayersRecorder_ForLOL
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.Run(new FuckPlayersRecorderMainForm());
        }
    }
}