using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ElevenlabsStreaming : MonoBehaviour
{
    private string ElevenlabsApiKey;
    private string ElevenlabsVoiceID;
    private string ElevenlabsModelID;
    private float ElevenlabsStability;
    private float ElevenlabsSimilarityBoost;
    private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    void Start()
    {
        string path = Path.Combine(Application.dataPath, "Config/config.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            var configData = JsonUtility.FromJson<ConfigData>(jsonContent);
            ElevenlabsApiKey = configData.ElevenlabsApiKey;
            ElevenlabsVoiceID = configData.ElevenlabsVoiceID;
            ElevenlabsModelID = configData.ElevenlabsModelID;
            ElevenlabsStability = configData.ElevenlabsStability;
            ElevenlabsSimilarityBoost = configData.ElevenlabsSimilarityBoost;
        }
        else
        {
            Debug.LogError("Config.json file not found in path: " + path);
            return;
        }
        Debug.Log("ElevenlabsStreaming starting...");
        Run().ConfigureAwait(false);
    }

    public async Task Run()
    {
        try
        {
            Debug.Log("Run method started.");
            await TextToSpeechInputStreaming(TextIterator());
            Debug.Log("Run method ended.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Run encountered an exception: {ex.Message}");
        }
    }

    public void AddChunk(string textChunk)
    {
        queue.Enqueue(textChunk);
    }

    public async Task TextToSpeechInputStreaming(IAsyncEnumerable<string> textIterator)
    {
        string uri = $"wss://api.elevenlabs.io/v1/text-to-speech/{ElevenlabsVoiceID}/stream-input?model_id="+ElevenlabsModelID;

        using (var websocket = new ClientWebSocket())
        {
            try
            {
                Debug.Log("Connecting to WebSocket...");
                await websocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                Debug.Log("WebSocket connected.");

                var payload = new
                {
                    text = "",
                    voice_settings = new { stability = ElevenlabsStability, similarity_boost = ElevenlabsSimilarityBoost },
                    xi_api_key = ElevenlabsApiKey
                };
                var payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                await websocket.SendAsync(new ArraySegment<byte>(payloadBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                await foreach (var text in textIterator)
                {
                    if (websocket.State != WebSocketState.Open)
                    {
                        Debug.LogWarning("WebSocket is closed unexpectedly.");
                        break;
                    }

                    Debug.Log($"Sending text payload: {text}");
                    var textPayload = new { text = text, try_trigger_generation = true };
                    var textBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(textPayload));

                    try
                    {
                        await websocket.SendAsync(new ArraySegment<byte>(textBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        Debug.Log("Text payload sent successfully.");

                        // Aspetta per ricevere la risposta audio dal server e riprodurla tramite mpv
                        var buffer = new byte[8192];
                        var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        
                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            Debug.Log("Audio received, streaming to mpv...");
                            await Stream(buffer, result.Count);
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        Debug.LogError($"Failed to send text payload: {ex.Message}");
                    }
                }

                if (websocket.State == WebSocketState.Open)
                {
                    Debug.Log("Finalizing text-to-speech input streaming...");
                    var finalPayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { text = "" }));
                    await websocket.SendAsync(new ArraySegment<byte>(finalPayload), WebSocketMessageType.Text, true, CancellationToken.None);
                    Debug.Log("Text payloads sent, closing WebSocket...");
                }
            }
            catch (WebSocketException ex)
            {
                Debug.LogError($"WebSocket exception: {ex.Message}");
            }
            finally
            {
                if (websocket.State == WebSocketState.Open || websocket.State == WebSocketState.CloseSent)
                {
                    await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Debug.Log("WebSocket closed.");
                }
            }
        }
    }

    private async Task Stream(byte[] audioData, int dataSize)
    {
        Debug.Log("Streaming audio data to mpv");
        using (var mpvProcess = new System.Diagnostics.Process())
        {
            mpvProcess.StartInfo.FileName = "mpv";
            mpvProcess.StartInfo.Arguments = "--no-cache --no-terminal --";
            mpvProcess.StartInfo.RedirectStandardInput = true;
            mpvProcess.StartInfo.UseShellExecute = false;
            mpvProcess.StartInfo.CreateNoWindow = true;
            mpvProcess.Start();

            if (mpvProcess.StandardInput.BaseStream.CanWrite)
            {
                await mpvProcess.StandardInput.BaseStream.WriteAsync(audioData, 0, dataSize);
                await mpvProcess.StandardInput.BaseStream.FlushAsync();
            }

            mpvProcess.StandardInput.Close();
            Debug.Log("Waiting for mpv to exit...");
            mpvProcess.WaitForExit();
            Debug.Log("mpv exited");
        }
    }

    private IAsyncEnumerable<string> TextIterator()
    {
        return GetChunksAsync();

        async IAsyncEnumerable<string> GetChunksAsync()
        {
            Debug.Log("Starting text iterator...");
            while (true)
            {
                if (queue.TryDequeue(out var chunk))
                {
                    Debug.Log($"Yielding chunk: {chunk}");
                    yield return chunk;
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
