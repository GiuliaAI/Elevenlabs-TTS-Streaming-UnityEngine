[System.Serializable]
public class ConfigData
{
    public string ElevenlabsApiKey;
    public string ElevenlabsVoiceID;
    public string ElevenlabsModelID;
    public float ElevenlabsStability;
    public float ElevenlabsSimilarityBoost;
    public string ElevenlabsLanguage; //Future implementation for v2.5 turbo version
    public string WebsocketServerHost;
    public int WebsocketServerPort;
    public string WebsocketServerPath;
}
