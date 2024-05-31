# Adaptive Immersive Learning Environment (AILE)

AILE is a project that integrates conversational artificial intelligence into a virtual environment to provide an adaptive, context-aware immersive learning environment. It leverages technologies such as:

- ChatGPT for logical reasoning
- Flask for server-side operations
- Python for backend logic
- Unity for the virtual environment

## Prerequisites 

Ensure you have the following installed:

- Python 3.9+
- Unity 

## Installation

### Python Dependencies

Install the necessary Python packages with the following commands:

```sh
pip install Flask gTTS openai python-dotenv SpeechRecognition
```

### Unity Assets

We recommend the following Unity assets:
[Apartment Kit](https://assetstore.unity.com/packages/3d/environments/apartment-kit-124055)
[Food Items](https://assetstore.unity.com/packages/3d/food-props-163295)
[Food Props](https://assetstore.unity.com/packages/3d/props/food/rpg-food-props-demo-248712)

### Pictograms

We recommend using the pictograms found in [Cboard](https://github.com/cboard-org/cboard).

Place the pictures under `./Resources/Pictograms` the name of the pictures is important, ensure to rename for clarity and improved logical reasoning for the LLM. 

### OpenAI API Key
In the `.env` file in `Server` directory you should provide the ChatGPT OpenAI key like the following:
```sh
OPENAI_API_KEY=YOUR_API_KEY
```

## Features

The system current provides the following functionalities:
- Context Awareness
- Feedback Integration
- Object Interaction
- Event Listeners
- Microphone Listener
- Audio Processing with TTS and SST
- ChatGPT Integration for Logical Reasoning
- Modular Assistant for easily swapping LLM

## Architecture

The Client section is how Unity communicates with the Python Flask Server, which in turn communicates with ChatGPT API.

![image](https://github.com/karlusrex/AILE/assets/90254802/baaf5a9b-1e32-4bb4-9153-3ae4a030a5f0)

## Example 

An example of how pictogram from Cboard is displayed, when the user is supposed to put the water melon in the bowl.

![image](https://github.com/karlusrex/AILE/assets/90254802/f1e14d4e-f8d5-450d-9bf7-c721722377d7)


# Credits

This section is to give credit to code copied, modified, or drawn inspiration from various sources.

### SavWave
Copied from [darktable SavWave.cs](https://gist.github.com/darktable/2317063)

### Microph MicrophoneAudioLevel function
Copied from [darktable MicInput.cs ](https://github.com/dansakamoto/telemouth/blob/master/telemouth-01-livespeech-unity/Assets/MicInput.cs)

### PlayerPickUpDrop
Modified version of [Code Monkey's tutorial on How to Pick up and Drop Objects/Items](https://www.youtube.com/watch?v=2IhzPTS4av4)

### Pictograms
Inspired by [InclusiveRenderScript](https://github.com/niklasenberg/InclusiveRenderScripts)

### Object Tagging system
Inspired by [InclusiveRenderScript](https://github.com/niklasenberg/InclusiveRenderScripts)

### Get closest object
Copied from [How to find the closest object](https://forum.unity.com/threads/how-to-find-the-nearest-object.360952/)
