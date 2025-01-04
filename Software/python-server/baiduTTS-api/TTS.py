from aip import AipSpeech
import os

APP_ID = '116958157'
API_KEY = "vWqLVUQfe9BJtpN9go6YtxK4"
SECRET_KEY = "XdkkgTqU967fcIsHnYKfw59xrSZbvVnm"


def baidu_tts(text):
    client = AipSpeech(APP_ID, API_KEY, SECRET_KEY)
    voice = client.synthesis(text, 'zh', 6, {'spd': 5,'pit':5, 'vol': 15, 'per': 4100, 'aue':6})
    with open("baidu_tts.wav", 'wb') as fp:
        fp.write(voice)


text = " 孩子们，这是一个AIGC的时代"
baidu_tts(text)
os.system("baidu_tts.wav")