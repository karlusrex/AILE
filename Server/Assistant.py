from openai import OpenAI
import os
from dotenv import load_dotenv
load_dotenv()

class Assistant:
    def __init__(self):
        self.client = OpenAI()
        self.client.api_key = os.getenv("OPENAI_API_KEY")
        self.model = "gpt-3.5-turbo-0125"
        
    def evaluate_scenario(self, task_winning_condition, virtual_context):
        return self.client.chat.completions.create(
            model="gpt-4-0125-preview",
            max_tokens=1,
            messages=[
                {"role": "user", "content": """
                Your responsibility is going to be determining if the player's action have met the requirements based on to the best of your ability. 
                The determination is done by either answering "Yes" or "No"
                Do you accept the responsibility?
                """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                The requirment for the player is: {task_winning_condition}.

                the context is: {virtual_context}
                """}
            ]

        ).choices[0].message.content
    
    def help_speech(self, task_instruction, virtual_context, conversation):
        prompt = [
            {"role": "user", "content": f"""
             As an AI assistant, your role is vital in providing support to the user. Please ensure your responses are brief, simple, and clear.
             Do you accept the responsibility?
             """},
            {"role": "system", "content": "Yes, I accept the responsibility."},
            {"role": "user", "content": f"""
             Here are additional instructions for you as an AI assistant:

             If the user is satisfied with the assistance provided, respond with: "Great! If you have any more questions in the future, feel free to ask. Good luck!"
             If the user understands what to do, respond with: "Understood! If you have any more questions in the future, feel free to ask. Good luck!"
             If the user is not satisfied or needs further assistance, respond with: "How can I assist you further?"
             If the user does not understand, respond with: "What aspect don't you understand?"
             """},
            {"role": "system", "content": "I accept the responsibility."},
            {"role": "user", "content": f"""
             The user is currently facing the following task: {task_instruction}.

             Here is the context: {virtual_context}.
             """},
        ]

        messages = []

        for i in conversation: 
            messages.insert(0, i) #LIFO

        # Instead of inserting the prompt list as a single item, extend the messages list with the prompt list
        messages = prompt + messages

        return self.client.chat.completions.create(
            model = "gpt-4-0125-preview",
            messages=messages
        ).choices[0].message.content
    
    #TODO: If not using, remove
    def help_idle(self, task_instruction):
        return self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content":
                    f"""
                    Your role is to reformulate the given task {task_instruction} in a different way, ensuring it is clear and easy to understand.
                    Can you reformulate the current task instruction in another way:  
            
                    Please respond with **ONLY** the reformulated task instruction.

                    """
                }
            ]
        ).choices[0].message.content
    
    def generate_task(self, task_winning_condition, virtual_context):
        return self.client.chat.completions.create(
            model = self.model,
            messages = [
                {"role": "user", "content": f"""
                 You are assisting a player in a game. Your role is crucial in helping them navigate challenges effectively.
                 Your task is to provide the user with easy-to-follow instructions, step by step. Each instruction should be clear and concise, and there should be a clear goal or condition for success associated with each step. 
                 Do you accept the responsibility?
                 """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                 Task format:
                 1. State the task clearly, indicating its objective and the context of the game.
                 2. Specify the steps required to achieve the win condition.
                 3. Each step should consist of clear instructions for the player to follow.
                 4. Each task step should be presented with task instructions followed by winning condition.
                 5. Each step should focus solely on guiding the player through individual steps without explicitly mentioning the overarching objective or success criteria.
                 6. Each step should be on a separate line. DON'T ADD BLANK LINES BETWEEN STEPS IT WILL AFFECT THE PARSING OF THE RESPONSE.
                 7. Special rule: There should not be a "final" step only describing the win condition. The win is achieved by following the previous steps. 

                 Example: 
                 1. Instruction: Pick up the banana. Winning condition: Player holds the banana
                 2. Instruction: Put down the banana on the table. Winning condition: The banana is on the table

                 Do you understand the task format?
                 """},
                 {"role": "system", "content": "Yes, I understand the task format."},
                 {"role": "user", "content": f"""
                 Here are additional instructions for you:

                 The winning conditions should be specific enough to be evaluated only with the context of the game provided by another assistant. 
                 Please be specific and clear in your instructions. For example, say explicitly for example the color of the object, how it relates to other objects in the context, and base the winning condition on that.
                 The player can only hold one object at a time, so it can not pick up multiple objects at the same time, or place down multiple objects at the same time.
                 Do you understand the additional instructions?
                 """
                 },
                 {"role": "system", "content": "Yes, I understand the additional instructions."},
                 {"role": "user", "content": f"""
                  There is a single player involved.
                  The overall win condition for the game is to {task_winning_condition}.
                  The context of the game is: {virtual_context}.
                  """}
            ]
        ).choices[0].message.content
    
    def task_correction(self, task_winning_condition, virtual_context):
        return self.client.chat.completions.create(
            model="gpt-4-0125-preview",
            messages = [
                {"role": "user", "content": f"""
                 The player has deviated from the intended course and needs guidance to realign their actions with the goal. 
                 Your responsiblilty is to provide clear, concise instructions to assist the player in adjusting their actions to meet the objective.
                 There is a single player involved.

                 Do you accept the responsibility?
                 """},
                 {"role": "system", "content": "Yes, I accept the responsibility."},
                 {"role": "user", "content": f"""
                  Here are some additional instructions for you:
                  Your instructions should focus solely on directing the player on how to modify their actions to align with the goal.
                  For instance, if the player is currently holding an apple but the winning condition specifies they should hold a banana:  
                  Put down the apple and pick up the banana.
                  Do not add any additional information.
                  Do you understand these additional instructions?
                  """},
                  {"role": "system", "content": "Yes, I understand the additional instructions."},
                  {"role": "user", "content": f"""
                   The objective is specified as {task_winning_condition}.
                   Context is {virtual_context}.
                   """}
            ]
        ).choices[0].message.content

    def choose_words(self, task_instruction, image_words):
        return self.client.chat.completions.create(
            model = "gpt-4-1106-preview",
            messages = [
                {"role" : "user", "content" : f"""
                 Your task is to select specific words from the given set of words that match the provided instruction. 
                 The chosen words should help convey the instruction clearly and accurately. 
                 Ensure the order of the selected words matches the order of the instruction.
                 Only respond with selected words seperated by whitespace.
                 Do you accept the responsibility?
                 """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                 The words are specified as {image_words}, and the instruction as {task_instruction}.
                 """}
            ]
        ).choices[0].message.content
    
    def object_from_instruction(self, task_instruction, virtual_context):
        return self.client.chat.completions.create(
            model = self.model,
            messages=[
                {"role" : "user", "content" : f"""
                 Your task is to select specific object from the given virtual context that match the provided instruction. 
                 The chosen object needs to be exactly like written, an example, Black Cube, and no additional information should be added.
                 Only respond with the object. 
                 Do you accept the responsibility?
                 """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                 The virtual context is {virtual_context}, and the instruction is {task_instruction}.
                 """}
            ]
        ).choices[0].message.content
    
    def overall_task_goal(self, task_instruction_all, virtual_context):
        return self.client.chat.completions.create(
            model="gpt-4-1106-preview",
            messages=[
                {"role": "user", "content": f"""
                 Given the provided instructions detailing a sequence of actions with specific objects, please provide result of following these instructions. 
                 Do you accept the responsibility?
                 """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                 How should the context be arranged in the end to meet the overall goal?

                 For example, if this is the instructions: 
                 1. Press the grey button
                 2. Press the yellow button
                 3. Press the green button

                 Then the response should be: The player has pressed the grey button, followed by the yellow button and lastly the green button. 
                 Respond as short and specific as possible only with overall task goal. 

                 Do you understand the instructions?
                 """},
                {"role": "system", "content": f"""
                 Yes, I understand the instructions.
                 """},
                {"role": "user", "content": f"""
                 The instructions is defined as {task_instruction_all} and the context as {virtual_context}.
                 """}
            ]
        ).choices[0].message.content
    
        
    def overall_task_brief(self, task_instruction_all):
        return self.client.chat.completions.create(
            model="gpt-4-1106-preview",
            messages=[
                {"role": "user", "content": f"""
                 Given the provided instructions detailing a sequence of actions with specific objects, please provide a brief description of the task. 
                 Do you accept the responsibility?
                 """},
                {"role": "system", "content": "Yes, I accept the responsibility."},
                {"role": "user", "content": f"""
                 Respond with a overall breif simple description on the task in one sentence.
                 Note that you do not have to include specific details. 
                 Example: Your task is to clean the room. 
                 Do you understand the instructions?
                 """},
                {"role": "system", "content": "Yes, I understand the instructions."},
                {"role": "user", "content": f"""
                 The instructions is defined as {task_instruction_all}.
                 """}
            ]
        ).choices[0].message.content