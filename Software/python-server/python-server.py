from ultralytics import YOLO
import google.generativeai as genai
from aip import AipSpeech
import uuid
from PIL import Image
import socket
import struct
import io
import numpy as np
import cv2
from pydub import AudioSegment
from openai import OpenAI

# 加载YOLO模型
model = YOLO("models/myModel.pt")

# 配置gemini API
genai.configure(api_key="AIzaSyAFpwjnWz8SbWA-1yPdl-iDqAI7rQVUnjc")
genai_model = genai.GenerativeModel("gemini-1.5-pro")

# 使用DeepSeeek的API密钥和 API 的基础 URL创建一个OpenAI客户端实例
client = OpenAI(api_key="sk-5e89b1cd55f74343b50752fe5fb1d41b", base_url="https://api.deepseek.com")

# 配置baidu语音API
APP_ID = '116958157'
API_KEY = "vWqLVUQfe9BJtpN9go6YtxK4"
SECRET_KEY = "XdkkgTqU967fcIsHnYKfw59xrSZbvVnm"

client_id = str(uuid.uuid4())
# HOST = '127.0.0.1'  # 监听所有可用的接口
HOST = '0.0.0.0'  # 监听所有可用的接口
PORT = 12345        # 与 Unity 中设置的端口号相同

# 存储检测历史
detect_history = []


def YOLO_detect(img):
    """调用YOLO模型检测传入的图片，返回检测结果"""
    # 进行预测
    print("DEBUG:正在对图片进行检测")
    results = model.predict(source=img)
    detected_objects = []

    # 提取检测结果
    for result in results:
        boxes = result.boxes.xyxy  # 边界框坐标
        scores = result.boxes.conf  # 置信度分数
        classes = result.boxes.cls  # 类别索引

        # 如果有类别名称，可以通过类别索引获取
        class_names = [model.names[int(cls)] for cls in classes]

        for box, score, class_name in zip(boxes, scores, class_names):
            # print(f"Class: {class_name}, Score: {score:.2f}, Box: {box}")
            if score > 0.7 and class_name not in detected_objects:
                detected_objects.append(class_name)
    if detected_objects:
        return detected_objects
    else:
        return None

def gemini(text):
    """根据传入的文字内容，调用Gemini大语言模型，返回生成的回复文本"""
    print("DEBUG:正在接收大模型回答文字结果")
    response = genai_model.generate_content(text)
    return response.text

def DeepSeek(text):
    # 使用 client.chat.completions.create 方法向 API 发送一个对话请求。
    print("DEBUG:正在接收大模型回答文字结果")
    response = client.chat.completions.create(
        model="deepseek-chat",
        messages=[
            {"role": "user", "content": text},
        ],
        stream=False
    )
    return response.choices[0].message.content

def baidu_tts(text):
    """根据传入的文字内容，调用百度文字转语音模型，返回生成的语音二进制数据"""
    print("DEBUG:正在将文字结果转换为音频")
    client = AipSpeech(APP_ID, API_KEY, SECRET_KEY)
    voice = client.synthesis(text, 'zh', 6, {'spd': 5,'pit':5, 'vol': 15, 'per': 4100, 'aue':6})
    return voice

def detect_and_generate(img):
    detections = YOLO_detect(img)
    audio = AudioSegment.from_wav("None.wav")
    # 将 AudioSegment 转换为字节流
    byte_io = io.BytesIO()
    audio.export(byte_io, format="wav")
    byte_io.seek(0)  # 重置指针到开头
    voice = byte_io.read()  # 获取字节数据
    if detections is not None:
        for detection in detections:
            detect_history.append(detection)
            print("the history is"+ str(detect_history))
            text = "你是一个小学老师，使用少于100个字向上小学的小朋友科普" + str(detection) + "(请在回答时用中文代替),用：小朋友你好，这是" + str(detection) + "(请在回答时用中文代替)开头。"
            respond = DeepSeek(text)
            voice = baidu_tts(respond)
    return voice
        # os.system("baidu_tts.mp3")



def receive_image(conn):
    """接收来自客户端的图片数据并返回BGR格式图片数据"""
    while True:
        # 接收图片数据长度
        length_bytes = conn.recv(4)
        if not length_bytes:
            print("客户端断开连接")
            break
        # 解包得到图片长度
        image_length = struct.unpack('<i', length_bytes)[0]

        # 接收实际的图片数据
        image_data = b''
        while len(image_data) < image_length:
            remaining = image_length - len(image_data)
            chunk = conn.recv(min(remaining, 4096))  # 每次接收一部分数据
            if not chunk:
                print("接收数据时发生错误")
                break
            image_data += chunk

        if image_data:
            try:
                # 使用 PIL (Pillow) 库处理接收到的图片数据
                image = Image.open(io.BytesIO(image_data))
                print("成功接收到图片，大小:", image.size)
                image_np = np.array(image)
                # 如果图像是 RGB 格式，需要转换为 BGR 格式
                image_bgr = cv2.cvtColor(image_np, cv2.COLOR_RGB2BGR)

                # # 使用 OpenCV 显示图像
                # cv2.imshow("test", image_bgr)
                # cv2.waitKey(0)
                # cv2.destroyAllWindows()

                return image_bgr
            except Exception as e:
                print("处理图片数据时出错:", e)
        else:
            print("未接收到图片数据")
            break


def send_sound(voice, conn):
    """
    将生成的语音通过现有连接发送到 Unity 客户端。
    :param voice: 语音的二进制数据
    :param conn: 已建立的连接，表示与 Unity 客户端的连接
    """
    try:
        # 发送音频数据的长度（int32 类型）
        voice_length = len(voice)
        conn.send(struct.pack('i', voice_length))

        # 发送音频数据
        conn.sendall(voice)
        print("音频数据已发送到客户端！")

    except Exception as e:
        print(f"发送语音时发生错误: {e}")


def send_string_array(string_array, conn):
    """
    将字符串数组通过现有连接发送到 Unity 客户端。
    :param string_array: 包含字符串的数组
    :param conn: 已建立的连接，表示与 Unity 客户端的连接
    """
    try:
        # 发送字符串数组的长度（int32 类型）
        array_length = len(string_array)
        conn.send(struct.pack('i', array_length))

        # 依次发送数组中的每个字符串
        for text in string_array:
            # 将每个字符串编码为字节数据
            encoded_text = text.encode('utf-8')

            # 发送字符串的长度（int32 类型）
            text_length = len(encoded_text)
            conn.send(struct.pack('i', text_length))

            # 发送字符串的字节数据
            conn.sendall(encoded_text)

        print("字符串数组数据已发送到客户端！")

    except Exception as e:
        print(f"发送字符串数组时发生错误: {e}")

def start_server():
    """python服务器主循环"""
    while True:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((HOST, PORT))
            s.listen()
            print(f"Python 服务器正在监听端口 {PORT}...")

            conn, addr = s.accept()
            with conn:
                print(f"与客户端 {addr} 建立连接")
                # 连接建立后，开始接收图片
                img = receive_image(conn)
                voice = detect_and_generate(img)
                send_sound(voice, conn)
                send_string_array(detect_history, conn)
            s.close()
            conn.close()


if __name__ == "__main__":
    start_server()
