import socket
import wave
import struct

# 设置服务器IP和端口
server_ip = "127.0.0.1"
server_port = 12345

# 创建TCP服务器
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((server_ip, server_port))
server_socket.listen(1)

print(f"等待客户端连接... 在 {server_ip}:{server_port}")

# 等待客户端连接
client_socket, client_address = server_socket.accept()
print(f"客户端 {client_address} 已连接！")

# 打开音频文件
audio_file_path = "baidu_tts.wav"  # 这里指定你的音频文件路径
with wave.open(audio_file_path, 'rb') as audio_file:
    # 获取音频文件的长度
    audio_data = audio_file.readframes(audio_file.getnframes())

    # 发送音频数据的长度（int32 类型）
    voice_length = len(audio_data)
    client_socket.send(struct.pack('i', voice_length))

    # 发送音频数据
    client_socket.sendall(audio_data)
    print("音频数据已发送到客户端！")

# 关闭连接
client_socket.close()
server_socket.close()
