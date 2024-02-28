using System.Net.Http.Json;
using Subscriber.Dto;


Console.Write("Enter Channel ID: ");
int channelId = 0;
string input = Console.ReadLine();
if (!int.TryParse(input, out channelId))
{
    Console.WriteLine("Invalid Input");
    Environment.Exit(0);
}

Console.WriteLine("Press Esc to STOP");

do
{
    HttpClient client = new HttpClient();
    Console.WriteLine($"Listening on channel {channelId}...");
    // polling
    while (!Console.KeyAvailable)
    {
        List<int> ids = await GetMessagesAsync(client, channelId);
        Thread.Sleep(2000);
        if (ids.Count() > 0)
        {
            await AckMessagesAsync(client, ids, channelId);
        }
    }
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

static async Task<List<int>> GetMessagesAsync(HttpClient httpClient, int channel)
{
    List<int> ids = new List<int>();
    List<Message>? newMessages = new List<Message>();

    try
    {
        newMessages = await httpClient.GetFromJsonAsync<List<Message>>($"http://localhost:5167/api/subscriptions/{channel}/messages");
    }
    catch
    {
        return ids;
    }

    foreach (var m in newMessages)
    {
        Console.WriteLine($"[{m.MessageStatus}] {m.Id}: {m.TopicMessage}");
        ids.Add(m.Id);
        Console.Beep();
    }

    return ids;
}

static async Task AckMessagesAsync(HttpClient httpClient, List<int> ids, int channel)
{
    var response = await httpClient.PostAsJsonAsync($"http://localhost:5167/api/subscriptions/{channel}/messages", ids);
    var returnedMessage = await response.Content.ReadAsStringAsync();

    Console.WriteLine(returnedMessage);
}