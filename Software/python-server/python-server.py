from ultralytics import YOLO
import google.generativeai as genai
from aip import AipSpeech
import os
import uuid
from PIL import Image
import socket
import struct
import io
import numpy as np
import cv2

# 加载YOLO模型
model = YOLO("models/yolo11s.pt")

# 配置gemini API
genai.configure(api_key="AIzaSyAFpwjnWz8SbWA-1yPdl-iDqAI7rQVUnjc")
genai_model = genai.GenerativeModel("gemini-1.5-pro")

# 配置baidu语音API
APP_ID = '116958157'
API_KEY = "vWqLVUQfe9BJtpN9go6YtxK4"
SECRET_KEY = "XdkkgTqU967fcIsHnYKfw59xrSZbvVnm"

server_address = "127.0.0.1:8188"
client_id = str(uuid.uuid4())
HOST = '127.0.0.1'  # 监听所有可用的接口
PORT = 12345        # 与 Unity 中设置的端口号相同


def YOLO_detect(img):
    # 进行预测
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
    response = genai_model.generate_content(text)
    return response.text

def baidu_tts(text):
    client = AipSpeech(APP_ID, API_KEY, SECRET_KEY)
    voice = client.synthesis(text, 'zh', 6, {'spd': 5,'pit':5, 'vol': 15, 'per': 4100, 'aue':6})
    return voice

def start_server():
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
        s.close()
        conn.close()

def receive_image(conn):
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


def detect_and_generate(img):
    detections = YOLO_detect(img)
    for detection in detections:
        text = "你是一个小学老师，使用少于100个字向上小学的小朋友科普" + str(detection) + "(请在回答时用中文代替),用：小朋友你好，这是" + str(detection) + "(请在回答时用中文代替)开头。"
        respond = gemini(text)
        voice = baidu_tts(respond)
        return voice
        # os.system("baidu_tts.mp3")


if __name__ == "__main__":
    while True:
        start_server()
