from llama_cpp import Llama
from pathlib import Path
import time, sys, subprocess

def load_llm(model_path: str):
    return Llama(
        model_path=model_path,
        n_ctx=512,
        n_threads=12,
        n_gpu_layers=-1,
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

def generate_prompt(llm: Llama, messages: list[dict[str, str]], prompt: str):
    messages.append(
        {
            "role": "user",
            "content": f"{prompt}"
        }
    )

    response = llm.create_chat_completion(
        messages=messages,
        temperature=1.2,
        max_tokens=512,
        top_p=0.9,
        min_p=0.1,
        typical_p=1,
        top_k=40,
        seed=int(time.time())
    ) 

    messages.append(
        {   
            "role": response["choices"][0]["message"]["role"],
            "content": response["choices"][0]["message"]["content"]
        }
    )
    return response["choices"][0]["message"]["role"], response["choices"][0]["message"]["content"]

def main(arg1, arg2):
    # arg1 - model_path
    # arg2 - user message
    
    llm = load_llm(arg1)
    messages = load_messages()
    response = generate_prompt(llm, messages, arg2)

    # Save the conversation to memory
    memory_file_path = Path.cwd() / "memory.txt"
    f = open(memory_file_path, "a")
    f.write(f":end:user: {arg2}\n")
    f.write(f":end:{response[0]}: {response[1]} \n")
    f.close()

    print(f"{response[1]}")

    pass

if __name__ == "__main__":
    # single test
    #args = ["/home/vvik/Documents/programming projects/python/llm_assistant/L3-Dark-Planet-8B-D_AU-q5_k_m.gguf", "Helloo"]
    
    #production
    args = sys.argv[1:]

    
    main(args[0], args[1])