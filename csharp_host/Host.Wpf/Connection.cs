namespace Host.Wpf
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Practices.ServiceLocation;

    public class Connection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            var messageSetter = ServiceLocator.Current.GetInstance<Action<string>>();
            messageSetter("Socket: " + data);

            return base.OnReceived(request, connectionId, data);
        }
    }
}
