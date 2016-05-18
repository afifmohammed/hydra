using System;
using Microsoft.Owin.Hosting;

namespace TransportDashboard
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://localhost:5368");
            using (WebApp.Start<Startup>(uri.AbsoluteUri))
            {
                Console.WriteLine("Dashboard is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the Dashboard Host.");
                Console.ReadLine();
            }
        }
    }
}
