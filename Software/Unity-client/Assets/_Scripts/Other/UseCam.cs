using UnityEngine;
using UnityEngine.UI;

// 本脚本的作用是调用设备的摄像头，将画面呈现在软件中
public class UseCam : MonoBehaviour
{
    int currentCamIndex = 0;
    WebCamTexture tex;
    public RawImage display; // 用于呈现摄像头画面的RawImage游戏对象

    private void StopWebCam()
    {
        display.texture = null;
        tex.Stop();
        tex = null;
    }

        private void StartWebCam()
    {
        WebCamDevice device = WebCamTexture.devices[currentCamIndex];
        tex = new WebCamTexture(device.name);//根据设备名称创建一个新的WebCamTexture的类，并赋值给tex，此时tex已经包含了摄像头的视频信号。
        display.texture = tex;//将摄像头的视频信号传递给RawImage中进行画面显示。

        tex.Play();//播放相机的捕捉图像画面
    }

    /*用来绑定在按钮上，点击切换摄像头开启状态*/
    public void StartStopCam_Clicked()
    {
        if(tex != null)
        {
            StopWebCam();
            Debug.Log("已经关闭摄像头");
        }
        else //打开相机
        {
            StartWebCam();
            Debug.Log("已经打开摄像头");
        }
    }

    public void StopWebCam_clicked()
    {
        if(tex != null)//stop camera
        {
            Debug.Log("已经关闭摄像头");
            StopWebCam();
        }
    }

    /*当设备有多个摄像头时，切换摄像头*/
    public void SwapCam_Clicked()
    {
        if (WebCamTexture.devices.Length > 0 )//WebCamTexture.devices.Length表示接入电脑的摄像机数量，当摄像机数量大于0时，运行下面切换操作。
        {
            currentCamIndex += 1;
            currentCamIndex %= WebCamTexture.devices.Length;//这行代码很有学习价值，它的意思是说设定一个数，计数器未达到这个数时，可以增加，当计数器达到这个数时会被重置为0
        }
    }

    void Start()
    {
        StartStopCam_Clicked();
    }
}


