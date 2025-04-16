import requests
import os
from dotenv import load_dotenv

load_dotenv()

def explain_question(question: str, prompt_mode: str = "simple") -> str:
    if prompt_mode == "simple":
        prompt = f"请将这道题目中难懂的词语替换为中国六年级小学生能理解的说法，仅替换词语，不要解释题目本身：{question}"
    elif prompt_mode == "step":
        prompt = f"请一步步引导中国六年级小学生思考这道题。每一步最多一句话，文字必须简短清楚，使用简单词语。最多写 4 步，每步用“第1步”、“第2步”开头。不要直接写出答案或计算结果：{question}"
    else:
        prompt = f"请解释这道题目：{question}"

    url = os.getenv("GPT_API_URL")
    api_key = os.getenv("GPT_API_KEY")

    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json"
    }

    payload = {
        "model": "gpt-3.5-turbo",
        "messages": [{"role": "user", "content": prompt}]
    }

    try:
        print("📡 正在调用 GPT 接口", flush=True)
        print("🌐 请求地址:", url, flush=True)
        print("📝 请求头:", headers, flush=True)
        print("📦 请求体:", payload, flush=True)
        response = requests.post(url, json=payload, headers=headers)
        response.raise_for_status()
        data = response.json()
        print("🧠 GPT 解释内容:", data["choices"][0]["message"]["content"], flush=True)
        return data["choices"][0]["message"]["content"]
    except Exception as e:
        if 'response' in locals():
            print("❌ 响应内容:", response.text, flush=True)
        return f"调用失败：{str(e)}"
