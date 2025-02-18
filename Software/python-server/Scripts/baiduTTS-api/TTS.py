from aip import AipSpeech
import os

APP_ID = '116958157'
API_KEY = "vWqLVUQfe9BJtpN9go6YtxK4"
SECRET_KEY = "XdkkgTqU967fcIsHnYKfw59xrSZbvVnm"


def baidu_tts(text):
    """根据传入的文字内容，调用百度文字转语音模型，将生成的语音写入本地文件"""
    client = AipSpeech(APP_ID, API_KEY, SECRET_KEY)
    voice = client.synthesis(text, 'zh', 6, {'spd': 5,'pit':5, 'vol': 15, 'per': 4100, 'aue':6})
    with open("demo.wav", 'wb') as fp:
        fp.write(voice)


text = "你好呀，小朋友"
baidu_tts(text)
os.system("SearchTutorial.wav")