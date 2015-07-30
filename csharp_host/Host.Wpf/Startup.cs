namespace Host.Wpf
{
    using Microsoft.Owin.Cors;

    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR<Connection>("/connection");
        }
    }
}
