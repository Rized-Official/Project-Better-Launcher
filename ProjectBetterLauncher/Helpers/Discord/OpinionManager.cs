using System.Net;
using System.Text;

namespace ProjectBetterLauncher.Helpers.Discord.RPC;

public class OpinionManager
{
    public static void SendOpinion(string message)
    {
        string webhook = "https://discord.com/api/webhooks/1557419030057459863/XHuMtI63-1GHALOuJ4qEYSbBT-J69xRiWXFlOunqQj67FWmDjZ1Viol4sI1cJHZY1Um4";

        WebClient client = new WebClient();
        client.Headers.Add("Content-Type", "application/json");
        string payload = "{\"content\": \"" + message + "\"}";
        client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
    }
}