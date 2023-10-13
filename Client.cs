using Opc.UaFx;
using Opc.UaFx.Client;

namespace OpcUaEvents;

internal sealed class Client : IDisposable
{
	private readonly OpcClient mClient;

	public Client(string serverAddress)
	{
		mClient = new(serverAddress);
		mClient.Connect();

		var eventFilter = OpcFilter.Using(mClient).FromEvents(OpcEventTypes.Event).Select();
		mClient.SubscribeEvent("ns=2;s=Root/Events", eventFilter, OnEventReceivedHandler);
	}
	public void Dispose()
	{
		mClient.Disconnect();
		mClient.Dispose();

		GC.SuppressFinalize(this);
	}

	public event EventHandler<string> EventReceived = delegate { };

	private void OnEventReceivedHandler(object sender, OpcEventReceivedEventArgs e) => EventReceived(this, e.Event.Message);
}
