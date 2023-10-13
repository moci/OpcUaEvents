using System.Text;

using Opc.UaFx;
using Opc.UaFx.Server;

namespace OpcUaEvents;

internal sealed class Server : IDisposable
{
	private readonly OpcServer mServer;
	private readonly OpcEventNode mEventNode;

	public Server()
	{
		var versionNode = new OpcDataVariableNode<string>("Version", "0.0.1");

		// Client must subscribe to this node to receive events
		var eventsNode = new OpcObjectNode("Events");
		var rootNode = new OpcObjectNode("Root", versionNode, eventsNode);

		mServer = new OpcServer(rootNode);

		// The node that represents the actual event
		mEventNode = new OpcEventNode("Event")
		{
			SourceNodeId = eventsNode.Id,
			SourceName = eventsNode.SymbolicName
		};

		eventsNode.AddNotifier(mServer.SystemContext, mEventNode);

		mServer.Start();
	}
	public void Dispose()
	{
		mServer.Stop();
		mServer.Dispose();

		GC.SuppressFinalize(this);
	}

	public string Address => mServer.Address.ToString();

	/// <summary>
	/// Send the message as an event
	/// </summary>
	public void SendEvent(string message)
	{
		mEventNode.EventId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
		mEventNode.Message = message;

		mEventNode.ReportEvent(mServer.SystemContext);
	}
}