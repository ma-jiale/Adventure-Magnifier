using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking; // 新增网络请求命名空间
using System.Collections;
using UnityEngine.UI;

public class RecordVoice : MonoBehaviour
{
    public GameObject info_box;
    public GameObject large_tutor;
    public GameObject small_tutor;
    public Text its_text;
    public Text its_name;
    public Image its_image;
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string microphoneName;

    private byte[] WavData;

    // string url = "http://127.0.0.1:12346/upload";
    private string url = "http://192.168.43.242:12346/upload";
    
    /*在应用启动时获取当前设备的麦克风*/
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("没有找到可用的麦克风");
        }
    }

    /*当按钮按下时开始录音，当按钮释放时结束录音并返回录音数据*/
    public void record_voice_on_pressed()
    {
        if(!isRecording)
        {
            recordedClip = Microphone.Start(microphoneName, false, 5, 44100); //使用使用Microphone类.Start方法调用麦克风录音
            isRecording = true;
            Debug.Log("开始录音...");
        }
    }

    public void record_voice_on_released()
    {
        info_box.SetActive(false);
        if(isRecording)
        {
            Microphone.End(microphoneName); //使用Microphone类.End方法停止录音
            isRecording = false;
            Debug.Log("录音完成");
        }

        // 将录音转换为Wav格式数据并发送到服务器
        WavData = ConvertAudioClipToWav(recordedClip);
        StartCoroutine(SendAudioToServer(WavData)); // 使用协程发送数据
    }

    private IEnumerator SendAudioToServer(byte[] wavData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(wavData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"上传失败: {request.error}");
        }
        else
        {
            Debug.Log("上传成功");
            Debug.Log("服务器返回: " + request.downloadHandler.text);
            show_canvas(request.downloadHandler.text);
        }
    }

    // 将 AudioClip 转换为 WAV 格式并返回字节数组
    private static byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        // 获取音频数据
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 将 WAV 文件数据写入字节数组
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // 写入 WAV 文件头
            WriteWavHeader(memoryStream, clip);

            // 将音频数据写入字节数组
            byte[] byteArray = ConvertSamplesToByteArray(samples);
            memoryStream.Write(byteArray, 0, byteArray.Length);

            // 返回字节数组
            return memoryStream.ToArray();
        }
    }

        // 写入 WAV 文件头到内存流
    private static void WriteWavHeader(MemoryStream memoryStream, AudioClip clip)
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int bitDepth = 16;
        int dataSize = clip.samples * channels * bitDepth / 8;

        // 文件头
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4); // "RIFF"
        memoryStream.Write(BitConverter.GetBytes(36 + dataSize), 0, 4);  // 文件总长度
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4); // "WAVE"

        // 格式块
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);  // "fmt "
        memoryStream.Write(BitConverter.GetBytes(16), 0, 4);  // 子块大小（16 字节）
        memoryStream.Write(BitConverter.GetBytes((short)1), 0, 2);  // 音频格式（1 = PCM）
        memoryStream.Write(BitConverter.GetBytes((short)channels), 0, 2);  // 通道数
        memoryStream.Write(BitConverter.GetBytes(sampleRate), 0, 4);  // 采样率
        memoryStream.Write(BitConverter.GetBytes(sampleRate * channels * bitDepth / 8), 0, 4);  // 字节每秒
        memoryStream.Write(BitConverter.GetBytes((short)(channels * bitDepth / 8)), 0, 2);  // 每个样本的字节数
        memoryStream.Write(BitConverter.GetBytes((short)bitDepth), 0, 2);  // 采样深度

        // 数据块
        memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);  // "data"
        memoryStream.Write(BitConverter.GetBytes(dataSize), 0, 4);  // 数据大小
    }

    // 将音频样本转换为字节数组
    private static byte[] ConvertSamplesToByteArray(float[] samples)
    {
        byte[] byteArray = new byte[samples.Length * 2];  // 每个样本是 2 字节
        for (int i = 0; i < samples.Length; i++)
        {
            short sampleValue = (short)(samples[i] * short.MaxValue);  // 将浮点数样本转换为 16 位整数
            byteArray[i * 2] = (byte)(sampleValue & 0xFF);
            byteArray[i * 2 + 1] = (byte)((sampleValue >> 8) & 0xFF);
        }
        return byteArray;
    }

    private void show_canvas(string name)
    {
        info_box.SetActive(true);
        large_tutor.SetActive(false);
        small_tutor.SetActive(true);
        its_name.text = name;
        its_text.text = find_its_resource(name);
        StartCoroutine(HideCanvasAfterDelay(8f));  // 调用协程，5秒后隐藏
    }

    private IEnumerator HideCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 延迟一段时间
        info_box.SetActive(false);  // 隐藏Canvas
        large_tutor.SetActive(true);
        small_tutor.SetActive(false);
    }

    private string find_its_resource(string name)
    {
        string text;
        Sprite sprite;
        switch (name)
        {
            case "向日葵":
                text = "向日葵是一种总是面向太阳的花，花朵大而黄色，像一个大盘子。它们的种子可以吃，还能做成油。向日葵有很强的生命力，是阳光和温暖的象征！";
                sprite = Resources.Load<Sprite>("Images/Sunflower");
                
                break;
            case "蒲公英":
                text = "蒲公英是一种黄色花朵的植物，花谢后变成带羽毛的种子，随风飘散。它常见于草地，可以帮助植物在不同地方生长。蒲公英的根、叶和花也有药用价值.。";
                sprite = Resources.Load<Sprite>("Images/Dandelion");
                break;
            case "玫瑰":
                text = "玫瑰是一种美丽的花，花朵通常是红色、粉色或白色，香气扑鼻。它是爱情和浪漫的象征，常常被用来送给特别的人。玫瑰的花瓣柔软，刺也很尖，要小心哦！";
                sprite = Resources.Load<Sprite>("Images/Rose");
                break;
            case "郁金香":
                text = "郁金香是一种色彩鲜艳的花，通常有红、黄、粉等多种颜色。它们的花瓣像杯子一样，形状优雅。郁金香常在春天开放，给人一种温暖和欢快的感觉！";
                sprite = Resources.Load<Sprite>("Images/Tulip");
                break;
            case "雏菊":
                text = "雏菊是一种小巧、可爱的花，花瓣通常是白色的，中心是黄色的。它们生长在草地上，喜欢阳光。雏菊象征着纯洁和天真，常被用来制作花环或送给朋友！";
                sprite = Resources.Load<Sprite>("Images/Daisy");
                break;
            case "小猫":
                text = "小猫是一种可爱、活泼的小动物，喜欢跳跃和玩耍。它们有柔软的毛发和灵敏的耳朵，经常发出“喵喵”的声音。它们也很喜欢被人抱着或抚摸，十分亲人！";
                sprite = Resources.Load<Sprite>("Images/Cat");
                break;
            case "绵羊":
                text = "绵羊是一种温顺的动物，身上长着软软的羊毛。它们喜欢成群生活。绵羊的叫声是“咩咩”，它们的毛发可以用来做衣服和毛线，非常有用！";
                sprite = Resources.Load<Sprite>("Images/Sheep");
                break;
            case "大象":
                text = "大象是一种体型巨大的动物，拥有长长的象鼻和大大的耳朵。它们通常生活在草原和森林里，喜欢吃草、树叶。大象非常聪明，能通过鼻子喝水、抓东西！";
                sprite = Resources.Load<Sprite>("Images/Elephant");
                break;
            case "蝴蝶":
                text = "蝴蝶是一种色彩鲜艳的昆虫，翅膀上有美丽的图案。它们从毛毛虫变成蝴蝶，经历了蛹的阶段。蝴蝶喜欢在花丛中飞舞，吸食花蜜，是春天和夏天的代表！";
                sprite = Resources.Load<Sprite>("Images/Butterfly");
                break;
            case "小狗":
                text = "小狗是一种活泼、忠诚的动物，喜欢跟人玩耍和互动。它们有柔软的毛发和温暖的眼睛，常常摇尾巴表示开心。小狗很聪明，还是人们最好的朋友！";
                sprite = Resources.Load<Sprite>("Images/Dog");
                break;                
            default:
                text = "None";
                sprite = null;
                break;
        }
        its_image.sprite = sprite;
        return text;
    }
    
}
