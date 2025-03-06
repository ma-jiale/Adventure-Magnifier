using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking; 



public class PythonServer : MonoBehaviour
{
    // public string serverIP = "127.0.0.1"; 
    public string serverIP = "192.168.43.242"; 
    public int serverPort = 12345;
    public GameObject CamGraph;
    public GameObject AudioPlayer;
    public GameObject TeacherIcon;
    public RawImage rawImage;

    /*从 RawImage 组件中获取纹理（Texture），并将其转换为便于发送PNG格式的字节数组（byte[]）*/
    public byte[] getImageFromTexture()
    {
        Texture texture = rawImage.texture;
        Texture2D textureToSend = TextureToTexture2D(texture);

        // 创建一个临时的可读 Texture2D，如果原始纹理不可读
        Texture2D readableTexture;
        if (!textureToSend.isReadable)
        {
            readableTexture = new Texture2D(textureToSend.width, textureToSend.height, textureToSend.format, false);
            RenderTexture renderTexture = RenderTexture.GetTemporary(textureToSend.width, textureToSend.height, 0);
            Graphics.Blit(textureToSend, renderTexture);
            RenderTexture.active = renderTexture;
            readableTexture.ReadPixels(new Rect(0, 0, textureToSend.width, textureToSend.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        else
        {
            readableTexture = textureToSend;
        }

        byte[] imageData = readableTexture.EncodeToPNG(); // 将 Texture2D 编码为 PNG 字节数组

        if (!textureToSend.isReadable)
        {
            Destroy(readableTexture); // 释放临时纹理
        }
        
        return imageData;
    }

    public void SendImageAndGetSoundOnClick()
    {
        byte[] imageData = getImageFromTexture();
        StartCoroutine(SendImageAndGetSound(imageData));
    }

    private IEnumerator SendImageAndGetSound(byte[] imageData)
    {
        UnityWebRequest request = new UnityWebRequest("http://192.168.43.242:12345/upload", "POST");
        request.uploadHandler = new UploadHandlerRaw(imageData);
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
        // 从响应头获取检测结果
        string detectionResult = request.GetResponseHeader("Detection-Result");
        List<string> detection_results = new List<string>();
        detection_results.Add(detectionResult);
        SaveStringArrayToPlayerPrefs(detection_results);
        Debug.Log("检测结果: " + detectionResult);

        // 获取音频数据并播放
        byte[] audioData = request.downloadHandler.data;
        PlayAudio(audioData);
        }
    }


    // 将texture对象转换为Texture2D对象，因为texture对象无法直接使用EncodeToPNG()函数编码为PNG
    private Texture2D TextureToTexture2D(Texture texture) 
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

/*播放传回来的音频数据*/
private void PlayAudio(byte[] voiceData)
{
    AudioSource audioSource = AudioPlayer.GetComponent<AudioSource>();
    AudioClip audioClip = Wav2AudioClip(voiceData);
    audioSource.clip = audioClip;
    audioSource.Play();
}

/*将wav格式的音频数据转换成Unity的AudioClip格式*/
private AudioClip Wav2AudioClip(byte[] originalData)
{
    int sampleRate = 16000; // 设置音频的采样率
    int totalSamples = originalData.Length / 2; // 每两个字节表示一个采样点（16-bit PCM）

    // 计算音频时长（单位：秒）
    float duration = totalSamples / (float)sampleRate;

    // 使用实际时长来创建音频片段
    AudioClip audioClip = AudioClip.Create("audioClip", totalSamples, 1, sampleRate, false);

    float[] clipData = new float[totalSamples];
    for (int i = 0; i < originalData.Length; i += 2)
    {
        // 将原始字节数据转换为[-1, 1]之间的浮动数据
        clipData[i / 2] = (short)((originalData[i + 1] << 8) | originalData[i]) / 32768.0f;
    }

    // 设置音频数据
    audioClip.SetData(clipData, 0);
    
    // 输出音频的时长（调试）
    Debug.Log("Audio Duration: " + duration + " seconds");

    return audioClip;
}

    void SaveStringArrayToPlayerPrefs(List<string> stringArray)
    {
    // 将列表中的字符串依次保存到 PlayerPrefs
    for (int i = 0; i < stringArray.Count; i++)
    {
        PlayerPrefs.SetString("StringArray_" + i, stringArray[i]);
    }
    PlayerPrefs.SetInt("StringArray_Count", stringArray.Count); // 保存数组的长度

    // 保存 PlayerPrefs
    PlayerPrefs.Save();
    Debug.Log("字符串数组已保存到 PlayerPrefs！");
    }

    public List<string> LoadStringArrayFromPlayerPrefs()
    {
        // 读取存储的字符串数组
        int count = PlayerPrefs.GetInt("StringArray_Count", 0);
        List<string> stringArray = new List<string>();

        for (int i = 0; i < count; i++)
        {
            stringArray.Add(PlayerPrefs.GetString("StringArray_" + i));
        }

        return stringArray;
    }
    
}
