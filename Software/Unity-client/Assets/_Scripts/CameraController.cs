using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraContrller : MonoBehaviour
{
    

    int currentCamIndex = 0;

    WebCamTexture tex;

    public RawImage display;

    public Text startStopText;

    public void SwapCam_Clicked()
    {
        if (WebCamTexture.devices.Length > 0 )//WebCamTexture.devices.Length表示接入电脑的摄像机数量，当摄像机数量大于0时，运行下面切换操作。
        {
            currentCamIndex += 1;
            currentCamIndex %= WebCamTexture.devices.Length;//这行代码很有学习价值，它的意思是说设定一个数，计数器未达到这个数时，可以增加，当计数器达到这个数时会被重置为0
        }
    }


    public void StartStopCam_Clicked()
    {
        if(tex != null)//stop camera
        {
            StopWebCam();
            startStopText.text = "Start Camera";

        }
        else//start camera
        {
            WebCamDevice device = WebCamTexture.devices[currentCamIndex];
            tex = new WebCamTexture(device.name);//根据设备名称创建一个新的WebCamTexture的类，并赋值给tex，此时tex已经包含了摄像头的视频信号。
            display.texture = tex;//将摄像头的视频信号传递给RawImage中进行画面显示。

            tex.Play();//播放
            startStopText.text = "Stop Camera";

        }
    }

    private void StopWebCam()
    {
        display.texture = null;
        tex.Stop();
        tex = null;
    }
}
