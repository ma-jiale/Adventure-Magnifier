import requests

def send_request_to_model(sentence):
    """
    发送请求至模型处理文本

    参数:
    sentence (str): 待处理的文本

    返回:
    dict: 模型的响应结果
    """
    # 定义请求的URL
    url = "http://127.0.0.1:5000/process"

    # 定义请求头
    headers = {
        "Content-Type": "application/json"
    }

    # 定义请求体
    data = {
        "sentence": sentence
    }

    try:
        # 发送POST请求
        response = requests.post(url, json=data, headers=headers)

        # 检查请求是否成功
        if response.status_code == 200:
            return response.json()  # 返回JSON格式的响应
        else:
            return {"error": f"请求失败，状态码: {response.status_code}", "response": response.text}
    except requests.exceptions.RequestException as e:
        return {"error": f"请求异常: {str(e)}"}

# 示例调用
if __name__ == "__main__":
    sentence = "请告诉我向日葵"
    result = send_request_to_model(sentence)
    print("模型响应:", result['output'][0])