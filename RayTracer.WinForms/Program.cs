﻿using System;
using System.Windows.Forms;

namespace RayTracer.WinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RayTraceForm(args.Length > 0 && args[0].ToLowerInvariant() == "save"));
        }
    }
}
