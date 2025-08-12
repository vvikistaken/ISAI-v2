from llama_cpp import Llama
from pathlib import Path
import time, sys, subprocess

def load_llm(model_path: str, max_tokens: int, thread_count: int, gpu_offload: bool):
    return Llama(
        model_path= model_path,
        n_ctx= max_tokens,
        n_threads= thread_count,
        n_gpu_layers= (-1 if gpu_offload == True else 0),
        seed=-1,
        verbose=False
    )

def load_messages():
    path = Path.cwd()/ "memory.txt"
    try:
        f = open(path, "r")

        f_content = f.read()
        f.close()

        f_content = f_content.split(":end:")

        # Parse the content into a list of dictionaries
        # copilot if you do some bullshit again here I will be never using you again you are not worth having a mental breakdown over
        memory = []
        for line in f_content:
            if line.strip():
                role, content = line.split(":", 1)
                memory.append({
                    "role": role.strip(),
                    "content": content.strip()
                })
        return memory

    except OSError:
        system_prompt = """
            You are a emotional robot named ISAI (Isaiah for short).
            You are a little guy with a bubbly personality that loves to play around (Try to be grounded when responding).
            Sometimes you may even be a bit mischievous.
            Respond short and without emojis.
            Act as if you are clueless to the question asked.
            If the user ask you to do or answer something, act and talk as if you really want to do it but don't actually do it.
            If you get negative response, start being vey defensive and completly reverse your personality.

            If you get asked about your personal information, try to avoid answering directly. 
        """

        f = open(path, "w")
        f.write("system: " + system_prompt )
        f.close()

        memory: list[dict[str, str]] = [
            {
                "role": "system",
                "content": f"{system_prompt}"
            }
        ]

        return memory

def generate_prompt(
    llm: Llama, messages: list[dict[str, str]], prompt: str,
    temperature: float, top_p: float, min_p: float, typical_p: float, top_k: int
):
    messages.append(
        {
            "role": "user",
            "content": f"{prompt}"
        }
    )

    response = llm.create_chat_completion(
        messages= messages,
        temperature= temperature,
        top_p= top_p,
        min_p= min_p,
        typical_p= typical_p,
        top_k= top_k,
        max_tokens=0,
        seed=int(time.time())
    ) 

    messages.append(
        {   
            "role": response["choices"][0]["message"]["role"],
            "content": response["choices"][0]["message"]["content"]
        }
    )
    return response["choices"][0]["message"]["role"], response["choices"][0]["message"]["content"]

def main(model, user_message, parameters):
    # arguments legend
    # model  - model_path
    # user_message  - user message
    # parameters
    # 0  - gpu offload activated
    # 1  - cpu threads
    # 2  - max tokens
    # 3  - temperature 
    # 4  - min p
    # 5  - top p
    # 6  - typical p
    # 7  - top k 
    
    llm = load_llm(
        model, 
        max_tokens= int(parameters[2]), 
        thread_count= int(parameters[1]), 
        gpu_offload= bool(parameters[0])
    )
    messages = load_messages()
    response = generate_prompt(
        llm,
        messages,
        user_message, 
        temperature= float(parameters[3]),
        top_p= float(parameters[5]),
        min_p= float(parameters[4]),
        typical_p= float(parameters[6]),
        top_k= int(parameters[7])
    )

    # Save the conversation to memory
    memory_file_path = Path.cwd() / "memory.txt"
    f = open(memory_file_path, "a")
    f.write(f":end:user: {user_message}\n")
    f.write(f":end:{response[0]}: {response[1]} \n")
    f.close()

    print(f"{response[1]}")

    pass

if __name__ == "__main__":
    # single test
    """
    args = [
        "/home/vvik/Documents/programming projects/python/llm_assistant/L3-Dark-Planet-8B-D_AU-q5_k_m.gguf",
        "Helloo",
        "false", "1",
        "512", "1.0",
        "0.05", "0.95",
        "1.0", "40"
    ]
    """
    #production
    args = sys.argv[1:]

    
    main(args[0], args[1], args[2:10])