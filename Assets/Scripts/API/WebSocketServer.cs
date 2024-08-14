using UnityEngine;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

public class WebSocketServer : MonoBehaviour
{
    private WebSocketSharp.Server.WebSocketServer wsServer;
    private ElevenlabsStreaming elevenLabsStreaming;
    private string WebsocketServerHost;
    private int WebsocketServerPort;
    private string WebsocketServerPath;
    void Start()
    {
        string path = Path.Combine(Application.dataPath, "Config/config.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            var configData = JsonUtility.FromJson<ConfigData>(jsonContent);
            WebsocketServerHost = configData.WebsocketServerHost;
            WebsocketServerPort = configData.WebsocketServerPort;
            WebsocketServerPath = configData.WebsocketServerPath;
        }
        else
        {
            Debug.LogError("Config.json file not found in path: " + path);
            return;
        }
        if (!TryGetComponent<ElevenlabsStreaming>(out elevenLabsStreaming))
        {
            Debug.LogError("ElevenlabsStreaming component not found!");
            return;
        }
        wsServer = new WebSocketSharp.Server.WebSocketServer("ws://" + WebsocketServerHost + ":" + WebsocketServerPort);
        wsServer.AddWebSocketService<ChatBehavior>(WebsocketServerPath, () => new ChatBehavior(elevenLabsStreaming));
        wsServer.Start();
        Debug.Log("WebSocket server started at ws://"+ WebsocketServerHost + ":" + WebsocketServerPort + WebsocketServerPath);
    }
    void OnDestroy()
    {
        wsServer.Stop();
    }
}

public class ChatBehavior : WebSocketBehavior
{
    private readonly ElevenlabsStreaming elevenLabsStreaming;
    public ChatBehavior(ElevenlabsStreaming elevenLabsStreaming)
    {
        this.elevenLabsStreaming = elevenLabsStreaming;
    }
    protected override void OnMessage(MessageEventArgs e)
    {
        var messageData = JsonConvert.DeserializeObject<MessageData>(e.Data);
        //Debug.Log(messageData.message);
        if (messageData != null && messageData.message_type == "chat_token" && messageData.streaming_token)
        {
            elevenLabsStreaming.AddChunk(messageData.message);
        }
    }
}

public class MessageData
{
    public string message_author { get; set; }
    public string message { get; set; }
    public string message_type { get; set; }
    public bool sst { get; set; }
    public bool streaming_token { get; set; }
    public string data_from { get; set; }
    public string data_to { get; set; }
}
