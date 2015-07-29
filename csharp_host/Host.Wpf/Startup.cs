namespace Host.Wpf
{
    using Microsoft.AspNet.SignalR;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR<Connection>("/connection", new ConnectionConfiguration { EnableJSONP = true, });
        }
    }
}
