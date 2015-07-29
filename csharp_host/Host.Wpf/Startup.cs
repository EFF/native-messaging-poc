namespace Host.Wpf
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Cors;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR<Connection>("/connection", new ConnectionConfiguration { EnableJSONP = true, });
        }
    }
}
