namespace OpcUaEvents;

internal class Program
{
	static void Main(string[] args)
	{
		using var server = new Server();
		using var client = new Client(server.Address);

		client.EventReceived += (o, e) => Console.WriteLine($"Got event: {e}");

		Console.WriteLine($"Server: {server.Address}");

		do
		{
			var input = Console.ReadLine();
			if (string.IsNullOrEmpty(input)) continue;

			if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

			server.SendEvent(input);
		} while (true);
	}
}