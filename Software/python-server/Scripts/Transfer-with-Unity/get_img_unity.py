import uuid
from PIL import Image
import socket
import struct
import io
import numpy as np
import cv2

server_address = "127.0.0.1:8188"
client_id = str(uuid.uuid4())
HOST = '127.0.0.1'  # 监听所有可用的接口
PORT = 12345        # 与 Unity 中设置的端口号相同

def start_server():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()
        print(f"Python 服务器正在监听端口 {PORT}...")

        conn, addr = s.accept()
        with conn:
            print(f"与客户端 {addr} 建立连接")
            # 连接建立后，开始接收图片
            receive_image(conn)

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


if __name__ == "__main__":
       start_server()