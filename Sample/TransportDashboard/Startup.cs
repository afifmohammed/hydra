using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.SqlServer;

[assembly: OwinStartup(typeof(TransportDashboard.Startup))]

namespace TransportDashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JobStorage.Current = new SqlServerStorage(
                    nameOrConnectionString: "Hangfire",
                    options: new SqlServerStorageOptions { PrepareSchemaIfNecessary = true });

            app.UseHangfireDashboard();
        }
    }
}
