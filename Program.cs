using System.Text;

using Opc.UaFx;
using Opc.UaFx.Client;
using Opc.UaFx.Server;

namespace OpcUaEvents;

internal class Program
{
	static void Main(string[] args)
	{
		using var server = new Server();
		using var client = new OpcClient(server.Address);

		client.Connect();

		Console.WriteLine($"Server: {server.Address}");
		client.SubscribeEvent(server.EventsSourceNodeId, OnEventReceived);

		do
		{
			var input = Console.ReadLine();
			if (string.IsNullOrEmpty(input)) continue;

			if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

			server.Send(input);
		} while (true);
	}

	private static void OnEventReceived(object sender, OpcEventReceivedEventArgs e) => Console.WriteLine($"Got event: {e.Event.Message}");
}

internal sealed class Server : IDisposable
{
	private readonly OpcServer mServer;
	private readonly OpcEventNode mEventNode;

	public Server()
	{
		var versionNode = new OpcDataVariableNode<string>("Version", "0.0.1");
		var eventsNode = new OpcObjectNode("Events");
		var rootNode = new OpcObjectNode("Root", versionNode, eventsNode);

		mServer = new OpcServer(rootNode);
		mServer.Start();

		mEventNode = new OpcEventNode("Event")
		{
			SourceNodeId = eventsNode.Id,
			SourceName = eventsNode.SymbolicName
		};
		eventsNode.AddNotifier(mServer.SystemContext, mEventNode);
		EventsSourceNodeId = eventsNode.Id;
	}
	public void Dispose()
	{
		mServer.Stop();
		mServer.Dispose();
	}

	public string Address => mServer.Address.ToString();
	public OpcNodeId EventsSourceNodeId { get; }
	public void Send(string message)
	{
		mEventNode.EventId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
		mEventNode.Message = message;

		mEventNode.ReportEvent(mServer.SystemContext);
	}
}