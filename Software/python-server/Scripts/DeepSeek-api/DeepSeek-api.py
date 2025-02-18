from openai import OpenAI

# 使用DeepSeeek的API密钥和 API 的基础 URL创建一个OpenAI客户端实例
client = OpenAI(api_key="sk-5e89b1cd55f74343b50752fe5fb1d41b", base_url="https://api.deepseek.com")

def DeepSeek(text):
    # 使用 client.chat.completions.create 方法向 API 发送一个对话请求。
    response = client.chat.completions.create(
        model="deepseek-chat",
        messages=[
            {"role": "user", "content": text},
        ],
        stream=False
    )
    return response.choices[0].message.content

# 将回复的文本内容打印在终端上
print( DeepSeek("hello，你是谁？"))