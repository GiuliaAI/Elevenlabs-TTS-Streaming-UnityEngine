# Elevenlabs API stream for UnityEngine

## ⚠️ Project Status: Work in Progress

**Important Notice:** This project is currently a work in progress and is not yet functional. 

We are actively working on integrating the Elevenlabs API with Unity via WebSocket streaming. However, several features are still under development, including the final audio playback implementation using Unity's `AudioSource`. 

Please be aware that this project may contain bugs, incomplete features, and may not work as intended at this stage.

We appreciate your understanding and patience as we continue to develop and improve this project.



## Introduction

This Unity project is designed to integrate Elevenlabs API with streaming functionality via WebSocket. The project includes a WebSocket server that receives tokenized messages from the client and forwards them to the Elevenlabs API for processing.

Currently, we are using MPV (Media Player) instead of Unity's integrated `AudioSource` for audio playback. This is a temporary setup for testing purposes, as the project is still in development. The final version will integrate audio playback using Unity's `AudioSource` component, providing a more seamless and native audio experience within Unity.

### Unity Version

This project is developed using **Unity 2023.2.8f1**. Ensure that you are using this version or a compatible one to avoid any issues with compatibility.

### Required Plugins

To successfully run this project, you need to have the following plugins:

1. **websocket-sharp**:
   - This plugin is used to manage WebSocket connections within Unity. 
   - You can download it from its GitHub repository: [websocket-sharp](https://github.com/sta/websocket-sharp).

2. **Newtonsoft.Json**:
   - This plugin is used for JSON serialization and deserialization within the project.
   - You can download it via the Unity Asset Store or directly from the [official Newtonsoft.Json GitHub repository](https://github.com/JamesNK/Newtonsoft.Json).

### Download and Configure MPV (Windows)

To set up MPV on Windows, follow these steps:

1. **Download MPV**:
   - Go to the official MPV website: [https://mpv.io/installation/](https://mpv.io/installation/)
   - Scroll down to the **Windows** section and download the latest build. You can use the [SourceForge](https://sourceforge.net/projects/mpv-player-windows/files/) link provided on the site.

2. **Extract MPV**:
   - Extract the downloaded zip file to a location of your choice, such as `C:\Program Files\mpv`.

3. **Add MPV to Your System Path**:
   - Open the Start Menu, search for "Environment Variables," and open the "Edit the system environment variables" option.
   - In the System Properties window, click on the "Environment Variables" button.
   - Under "System variables," find and select the `Path` variable, then click "Edit."
   - Click "New" and add the path to the folder where you extracted MPV (e.g., `C:\Program Files\mpv`).
   - Click "OK" to close all the windows.

4. **Test MPV Installation**:
   - Open a Command Prompt and type `mpv --version` to verify the installation. You should see MPV's version information.

After setting up MPV, the project should be able to use it for audio playback during testing.

Follow the instructions below to set up and configure the project, including setting up API keys, WebSocket server details, and handling message streaming.



## Setup

### Step 1: Create the Config Directory

Make sure there is a `Config` directory within your project's `Assets` folder. If it doesn't exist, create it: Assets/Config/

### Step 2: Create the `config.json` File

Within the `Config` directory, create a new file named `config.json`. This file will contain your API keys and other configuration details.

### Step 3: Use the Example Config File

To help you get started, there's an example configuration file named `config.json.example` in the same directory. You can use this as a template:

1. Copy the `config.json.example` file.
2. Rename the copied file to `config.json`.
3. Open `config.json` in your preferred text editor.

### Step 4: Add Your Configuration Parameters

Replace the placeholders in `config.json` with your actual configuration details. The structure of the file should look like this:

```json
{
    "ElevenlabsApiKey": "sk_",
    "ElevenlabsVoiceID": "21m00Tcm4TlvDq8ikWAM",
    "ElevenlabsModelID": "eleven_turbo_v2_5",
    "ElevenlabsStability": 0.5,
    "ElevenlabsSimilarityBoost": 0.8,
    "ElevenlabsLanguage": "it",
    "WebsocketServerHost":"10.0.0.2",
    "WebsocketServerPort": 4989,
    "WebsocketServerPath": "/ws"
}
```
### Step 5: Save the File

After entering your details, save the `config.json` file. Ensure that it remains in the `Assets/Config/` directory.

## Sending a JSON to the WebSocket

When communicating with the WebSocket server, you need to send a JSON payload in the following format:

```json
{
    "message_author": "user",
    "message": "",
    "message_type": "chat_token",
    "sst": false,
    "streaming_token": true,
    "data_from": "user",
    "data_to": "ElevenlabsAPI"
}
```
### Important Details:
- The `message` field should contain the message in tokenized format. Since the client needs to send tokenized messages, you will typically send multiple JSON payloads to the WebSocket, each representing a part of the complete message.
- Set the `message_author` to `"user"` or the appropriate identifier for the user sending the message.
- The `message_type` should be set to `"chat_token"` to indicate the type of message being sent.
- The `sst` field should be set to `false` unless you are using speech-to-text services.
- The `streaming_token` field should be set to `true` if you want to stream the tokenized message.
- The `data_from` field should specify the origin of the data (e.g., `"user"`).
- The `data_to` field should specify the destination of the data (e.g., `"ElevenlabsAPI"`).

## Important Notes

- **Do not commit your `config.json` file to version control.** This file should be kept out of Git and other version control systems to ensure your API keys and server configurations remain secure.
- The `.gitignore` file should already be configured to ignore `config.json`.



---

If you encounter any issues, please ensure that the `config.json` file is correctly placed and formatted as shown above.
