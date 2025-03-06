from flask import Flask, request
import websocket
import hashlib
import base64
import hmac
import json
from urllib.parse import urlencode
import time
import ssl
from wsgiref.handlers import format_date_time
from datetime import datetime
from time import mktime
import _thread as thread
import requests
from pydub import AudioSegment
from config import XF_API_Key, XF_API_Secret, XF_APP_ID

app = Flask(__name__)
asr_result = '' # 储存识别结果
key_word = ''

##############################
# 服务器部分
# 从Unity前端获取语音数据
@app.route('/upload', methods=['POST'])
def handle_upload():
    try:
        # 获取音频数据
        audio_data = request.data

        # 保存为 WAV 文件
        with open('temp_audio.wav', 'wb') as f:
            f.write(audio_data)
        convert_wav_to_api_standard('temp_audio.wav', 'temp_audio_standard.wav')
        print(f"{datetime.now().strftime('%H:%M:%S')} 收到并保存了音频文件")
        audio_data2text()
        print(asr_result)
        return key_word, 200


    except Exception as e:
        print(f"保存失败: {str(e)}")
        return 'Error saving audio', 10086


####################################
# 调用讯飞语音识别api部分

STATUS_FIRST_FRAME = 0  # 第一帧的标识
STATUS_CONTINUE_FRAME = 1  # 中间帧标识
STATUS_LAST_FRAME = 2  # 最后一帧的标识
class Ws_Param(object):
    # 初始化
    def __init__(self, APPID, APIKey, APISecret, AudioFile):
        self.APPID = APPID
        self.APIKey = APIKey
        self.APISecret = APISecret
        self.AudioFile = AudioFile

        # 公共参数(common)
        self.CommonArgs = {"app_id": self.APPID}
        # 业务参数(business)，更多个性化参数可在官网查看
        self.BusinessArgs = {"domain": "iat", "language": "zh_cn", "accent": "mandarin", "vinfo":1,"vad_eos":10000}

    # 生成url
    def create_url(self):
        url = 'wss://ws-api.xfyun.cn/v2/iat'
        # 生成RFC1123格式的时间戳
        now = datetime.now()
        date = format_date_time(mktime(now.timetuple()))

        # 拼接字符串
        signature_origin = "host: " + "ws-api.xfyun.cn" + "\n"
        signature_origin += "date: " + date + "\n"
        signature_origin += "GET " + "/v2/iat " + "HTTP/1.1"
        # 进行hmac-sha256进行加密
        signature_sha = hmac.new(self.APISecret.encode('utf-8'), signature_origin.encode('utf-8'),
                                 digestmod=hashlib.sha256).digest()
        signature_sha = base64.b64encode(signature_sha).decode(encoding='utf-8')

        authorization_origin = "api_key=\"%s\", algorithm=\"%s\", headers=\"%s\", signature=\"%s\"" % (
            self.APIKey, "hmac-sha256", "host date request-line", signature_sha)
        authorization = base64.b64encode(authorization_origin.encode('utf-8')).decode(encoding='utf-8')
        # 将请求的鉴权参数组合为字典
        v = {
            "authorization": authorization,
            "date": date,
            "host": "ws-api.xfyun.cn"
        }
        # 拼接鉴权参数，生成url
        url = url + '?' + urlencode(v)
        # print("date: ",date)
        # print("v: ",v)
        # 此处打印出建立连接时候的url,参考本demo的时候可取消上方打印的注释，比对相同参数时生成的url与自己代码生成的url是否一致
        # print('websocket url :', url)
        return url


# 收到websocket消息的处理
def on_message(ws, message):
    global asr_result
    global key_word
    try:
        code = json.loads(message)["code"]
        sid = json.loads(message)["sid"]
        if code != 0:
            errMsg = json.loads(message)["message"]
            print("sid:%s call error:%s code is:%s" % (sid, errMsg, code))

        else:
            data = json.loads(message)["data"]["result"]["ws"]
            is_last = json.loads(message)["data"]["result"]["ls"]
            # print(json.loads(message))
            result = ""
            for i in data:
                for w in i["cw"]:
                    result += w["w"]
            print("sid:%s call success!,data is:%s" % (sid, json.dumps(data, ensure_ascii=False)))
            asr_result += result

            # 当当前语音全部识别为转换为文字完成时，调用模型进行关键词匹配
            if is_last:
                result = send_request_to_model(asr_result)
                if result['output']:
                    key_word =  result['output'][-1]
                print("模型响应:", result['output'][-1])
                print("调用模型提取关键词")
                asr_result = '' # 刷新检测结果
    except Exception as e:
        print("receive msg,but parse exception:", e)



# 收到websocket错误的处理
def on_error(ws, error):
    print("### error:", error)


# 收到websocket关闭的处理
def on_close(ws,a,b):
    print("### closed ###")


# 收到websocket连接建立的处理
def on_open(ws):
    def run(*args):
        frameSize = 8000  # 每一帧的音频大小
        intervel = 0.04  # 发送音频间隔(单位:s)
        status = STATUS_FIRST_FRAME  # 音频的状态信息，标识音频是第一帧，还是中间帧、最后一帧

        with open(wsParam.AudioFile, "rb") as fp:
            while True:
                buf = fp.read(frameSize)
                # 文件结束
                if not buf:
                    status = STATUS_LAST_FRAME
                # 第一帧处理
                # 发送第一帧音频，带business 参数
                # appid 必须带上，只需第一帧发送
                if status == STATUS_FIRST_FRAME:

                    d = {"common": wsParam.CommonArgs,
                         "business": wsParam.BusinessArgs,
                         "data": {"status": 0, "format": "audio/L16;rate=16000",
                                  "audio": str(base64.b64encode(buf), 'utf-8'),
                                  "encoding": "raw"}}
                    d = json.dumps(d)
                    ws.send(d)
                    status = STATUS_CONTINUE_FRAME
                # 中间帧处理
                elif status == STATUS_CONTINUE_FRAME:
                    d = {"data": {"status": 1, "format": "audio/L16;rate=16000",
                                  "audio": str(base64.b64encode(buf), 'utf-8'),
                                  "encoding": "raw"}}
                    ws.send(json.dumps(d))
                # 最后一帧处理
                elif status == STATUS_LAST_FRAME:
                    d = {"data": {"status": 2, "format": "audio/L16;rate=16000",
                                  "audio": str(base64.b64encode(buf), 'utf-8'),
                                  "encoding": "raw"}}
                    ws.send(json.dumps(d))
                    time.sleep(1)
                    break
                # 模拟音频采样间隔
                time.sleep(intervel)
        ws.close()

    thread.start_new_thread(run, ())

def audio_data2text():
    """调用语音识别API，将音频数据转化为文字"""
    time1 = datetime.now()
    websocket.enableTrace(False)
    wsUrl = wsParam.create_url()
    ws = websocket.WebSocketApp(wsUrl, on_message=on_message, on_error=on_error, on_close=on_close)
    ws.on_open = on_open
    ws.run_forever(sslopt={"cert_reqs": ssl.CERT_NONE})
    time2 = datetime.now()
    print(time2-time1)

def convert_wav_to_api_standard(input_file, output_file, target_sample_rate=16000):
    """
    将本地 WAV 文件转换为符合 API 要求的格式

    参数:
    input_file (str): 输入 WAV 文件的路径
    output_file (str): 输出 WAV 文件的路径
    target_sample_rate (int): 目标采样率（16000 或 8000）
    """
    try:
        # 加载 WAV 文件
        audio = AudioSegment.from_file(input_file, format="wav")

        # 将音频转换为单声道、16bit、目标采样率
        audio = audio.set_frame_rate(target_sample_rate)  # 设置采样率
        audio = audio.set_channels(1)  # 设置为单声道
        audio = audio.set_sample_width(2)  # 设置为 16bit（2字节）

        # 保存为新的 WAV 文件
        audio.export(output_file, format="wav")

        print(f"文件已成功转换为符合 API 要求的格式，保存为: {output_file}")
        print(f"采样率: {target_sample_rate}Hz, 单声道, 16bit")
    except Exception as e:
        print(f"处理音频时发生错误: {str(e)}")

###############################
# 向学长的模型发送请求获得关键字部分
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

if __name__ == '__main__':
    wsParam = Ws_Param(XF_API_Key, XF_API_Secret, XF_APP_ID,
                       AudioFile='temp_audio_standard.wav')
    app.run(host='0.0.0.0', port=12346)
