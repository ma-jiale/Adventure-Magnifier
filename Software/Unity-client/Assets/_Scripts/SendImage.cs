using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.IO;


public class SendImage2Server : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 12345;
    public GameObject CamGraph;

    public byte[] getImageFromTexture()
    {
        RawImage rawImage = CamGraph.GetComponent<RawImage>();
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

    public void SendImageOnClick()
    {
        byte[] imageData = getImageFromTexture();

        try
        {
            TcpClient client = new TcpClient(serverIP, serverPort);
            NetworkStream stream = client.GetStream();
            // 首先发送数据长度
            byte[] lengthBytes = BitConverter.GetBytes(imageData.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);

            // 然后发送实际的图像数据
            stream.Write(imageData, 0, imageData.Length);

            Debug.Log("Source Image 已发送到 Python 服务器！");

            stream.Close();
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("发送 Source Image 时出错: " + e.Message);
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



    
}


