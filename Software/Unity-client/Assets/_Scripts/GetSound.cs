using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;

 

public class GetSound : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 12345;
    public AudioSource audioSource;

    public void GetSoundFromServer()
    {
        try
        {
            TcpClient client = new TcpClient(serverIP, serverPort);
            NetworkStream stream = client.GetStream();
            BinaryReader reader = new BinaryReader(stream);
            // 读取语音数据的长度
            int voiceLength = reader.ReadInt32();

            // 读取语音数据
            byte[] voiceData = reader.ReadBytes(voiceLength);
            Debug.Log("已从 Python 服务器收到音频！");
            // 播放音频
            PlayAudio(voiceData);

            // 关闭流和连接
            reader.Close();
            stream.Close();
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("接收语音时出错: " + e.Message);
        }
    }

    private void PlayAudio(byte[] voiceData)
    {
        AudioClip audioClip = Wav2AudioClip(voiceData);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    private AudioClip Wav2AudioClip(byte[] originalData)
    {
        int SampleRate = 16000;
        AudioClip _audioClip = AudioClip.Create("audioClip", SampleRate * 600, 1, SampleRate, false); 
        float[] _clipData = new float[originalData.Length / 2];
        for (int i = 0; i < originalData.Length; i += 2)
        {
        _clipData[i / 2] = (short)((originalData[i + 1] << 8) | originalData[i]) / 32768.0f;
        }
                    
        _audioClip.SetData(_clipData, 0);
        return _audioClip;
    }

    // void Start()
    // {
    //     GetSoundFromServer();
    // }

}


