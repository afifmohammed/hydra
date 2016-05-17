using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;

namespace TransportDashboard
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:5368"))
            {
                Console.WriteLine("Server started... press ENTER to shut down");
                Console.ReadLine();
            }
        }
    }
}
