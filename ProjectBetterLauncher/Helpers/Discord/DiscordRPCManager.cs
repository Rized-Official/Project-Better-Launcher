using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace ProjectBetterLauncher.Helpers.Discord.RPC;

public static class DiscordRPCManager
{
    private static DiscordRpcClient? _client;
    public static void Initialize(string clientId)
    {
        if (_client != null && !_client.IsDisposed) return;

        _client = new DiscordRpcClient(clientId);
        
        _client.Logger = new ConsoleLogger { Level = LogLevel.Warning };

        _client.Initialize();
    }
    
    public static void SetDiscordRPC(string details, string state, string largeImageKey, string largeImageText, string smallImageKey = "", string smallImageText = "")
    {
        if (_client == null || _client.IsDisposed)
        {
            Console.WriteLine("Discord RPC isn't initialized");
            return;
        }
        
        var assets = new Assets
        {
            LargeImageKey = largeImageKey,
            LargeImageText = largeImageText
        };

        if (!string.IsNullOrEmpty(smallImageKey))
        {
            assets.SmallImageKey = smallImageKey;
            assets.SmallImageText = smallImageText;
        }
        
        _client.SetPresence(new RichPresence
        {
            Details = details,
            State = state,
            Assets = assets
        });
    }
    
    public static void Dispose()
    {
        if (_client == null) return;
        
        _client.ClearPresence();
        _client.Dispose();
        _client = null;
    }
}