namespace Host.Wpf
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    public class Connection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return base.OnReceived(request, connectionId, data);
        }
    }
}
