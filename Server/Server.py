import json
import io
from collections import deque
import speech_recognition as sr # https://pypi.org/project/SpeechRecognition/
from flask import Flask, request, send_file
from gtts import gTTS
from Assistant import Assistant

app = Flask(__name__)
assistant = Assistant()
    
@app.route("/api/v1/speech-to-text", methods=['GET'])
def speech_to_text():
    """
    Opens saved audio file and uses speech recognition to convert it to text.
    """
    recognizer = sr.Recognizer()
    
    with sr.AudioFile("recording.wav") as source:
        audio_data = recognizer.record(source)
        text = recognizer.recognize_google(audio_data)
        return text

@app.route("/api/v1/text-to-speech", methods=['POST'])
def text_to_speech():
    """
    Makes a request to gTTS with provided string to recieve audiofile.
    Returns audiofile and stores it in project folder.
    """
    data = request.form.get("optional_string") 
    print("Text-To-Speech: ", data)
    tts = gTTS(data, lang='en', slow=False)
    
    audio_file = "audio_output.mp3"
    tts.save(audio_file)
    return send_file(audio_file, mimetype='audio/mpeg')

@app.route("/api/v1/get-instructions-from-ollama", methods=['POST'])
def get_instructions_from_ollama():
    """
    Makes request to chatbot with provided winning condition and context data,
    returns the response as JSON. 
    """     
    task_winning_condition = request.form.get("optional_string") 
    virtual_context = request.form.get("virtual_context")
    print("Get-Instructions-From-Ollama:")
    print("Task-Winning-Condition: ", task_winning_condition)
    print("Virtual-Context: ", virtual_context)
    return assistant.generate_task(task_winning_condition=task_winning_condition, virtual_context=virtual_context)


@app.route("/api/v1/get-correction-instruction", methods=['POST'])
def get_correction_instruction():
    """
    Makes request to chatbot with provided expected winning condition and context data,
    returns a correction instruction to get the user back on track.
    """     
    task_winning_condition = request.form.get("task_winning_condition")
    virtual_context = request.form.get("virtual_context")
    print("Get-Correction-Instruction:")
    print("Task-Winning-Condition: ", task_winning_condition)
    print("Virtual-Context: ", virtual_context)
    return assistant.task_correction(task_winning_condition=task_winning_condition, virtual_context=virtual_context)
    
@app.route("/api/v1/evaluation", methods=['POST'])
def evaluate_action():
    """
    Evaluates the users action by comparing the context and the win condition 
    """
    task_winning_condition = request.form.get("task_winning_condition")
    virtual_context = request.form.get("virtual_context")
    print("Evaluation:")
    print("Task-Winning-Condition: ", task_winning_condition)
    print("Virtual-Context: ", virtual_context)
    response = assistant.evaluate_scenario(task_winning_condition=task_winning_condition, virtual_context=virtual_context)
    print(response)
    if 'Yes' in response:
        return 'true' #can't return a boolean
    if "yes" in response:
        return "true"
    return 'false'

conversation = []
introduction = True
@app.route("/api/v1/task-helper-question", methods=['POST'])
def get_task_helper_question():
    """
    Gives support based on users question and context
    """
    global conversation
    global introduction
    question = request.form.get("optional_string") 
    print("QUESTION: ", question)

    if introduction:
        task_instruction = request.form.get("task_instruction_all")
        temp = { "role": "system", "content": f"Do you understand the task?" }
        conversation.insert(0, temp)
    else:
        task_instruction = request.form.get("task_instruction")

    virtual_context = request.form.get("virtual_context")

    """
    print("Task-Helper-Question:")
    print("Optional-String: ", question) #yes
    print("Task-Instruction: ", task_instruction)
    """
    
    user = { "role": "user", "content": f"{question}" }
    conversation.insert(0, user)

    response = assistant.help_speech(task_instruction=task_instruction, 
                                     virtual_context=virtual_context,
                                     conversation=conversation)

    #save user input and AI output for conversation history
    system = { "role": "system", "content": f"{response}" }
    conversation.insert(0, system)

    #check if conversation should end
    end_conversation = "If you have any more questions in the future, feel free to ask. Good luck!"

    if end_conversation in response: 
        conversation = []
        introduction = False
        print("Conversation ends")

    return response

@app.route("/api/v1/task-helper-idle", methods=['POST']) 
def get_task_idle_support():
    """
    Calls the LLM when player is idle 
    """
    task_instruction = request.form.get("task_instruction")
    print("Task-Helper-Idle:")
    print("Task-Instruction: ", task_instruction)
    return assistant.help_idle(task_instruction=task_instruction)

@app.route("/api/v1/instruction_image_support", methods=['POST'])
def get_image_support():
    """
    Calls the LLM to pick out available images based on image name
    """
    task_instruction = request.form.get("task_instruction")
    image_words = request.form.get("image_words")
    print("Instruction-Image-Support:")
    print("Task-Instruction: ", task_instruction)
    print("Image-Words: ", image_words)
    print(task_instruction, image_words)
    words = assistant.choose_words(task_instruction=task_instruction, image_words=image_words)
    print("Words: ", words)
    return words

@app.route("/api/v1/get-object-from-instruction", methods=['POST'])
def get_object_from_instruction():
    """
    Calls the LLM to get the object mentioned in instruction
    """
    task_instruction = request.form.get("task_instruction")
    virtual_context = request.form.get("virtual_context")
    print("Get-Object-From-Instruction:")
    print("Task-Instruction: ", task_instruction)
    print("Virtual-Context: ", virtual_context)
    print(task_instruction, virtual_context)
    return assistant.object_from_instruction(task_instruction=task_instruction, virtual_context=virtual_context)

@app.route("/api/v1/get-overall-task-goal", methods=['POST'])
def get_overall_task_goal():
    """
    Calls the LLM to get the overall task goal
    """
    task_instruction_all = request.form.get("task_instruction_all")
    virtual_context = request.form.get("virtual_context")
    print("Get-Overall-Task-Goal:")
    print("Task-Instruction-All: ", task_instruction_all)
    print("Virtual-Context: ", virtual_context)
    print(task_instruction_all, virtual_context)
    return assistant.overall_task_goal(task_instruction_all=task_instruction_all, virtual_context=virtual_context)

@app.route("/api/v1/clear-conversation", methods=['GET'])
def clear_conversation():
    """
    Clears the conversation history
    """
    global conversation
    conversation = []
    print("Conversation cleared")
    return "Conversation cleared"

@app.route("/api/v1/task-brief", methods=['POST'])
def task_brief():
    """
    Calls the LLM to get short description of task to perform
    """
    task_instruction_all = request.form.get("task_instruction_all")
    response = assistant.overall_task_brief(task_instruction_all=task_instruction_all)
    return response

if __name__ == "__main__":
    app.run(debug=True)