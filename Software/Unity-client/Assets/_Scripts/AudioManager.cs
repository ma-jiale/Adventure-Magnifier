using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class AudioVisibilityController : MonoBehaviour
{
    public AudioSource audioSource;  // 用来检查音频播放的 AudioSource
    public GameObject TeacherIcon;  // 控制显示/隐藏的 UI 图标

    public GameObject Answer;  // 控制显示/隐藏的 UI 图标

    private bool wasPlaying = false;  // 用来记录音频是否正在播放

    // 需要修改图片的 Image 游戏对象
    public Image Icon;

    public GameObject Load_History;

    List<string> search_history;



    void Update()
    {
        PythonServer receiver = Load_History.GetComponent<PythonServer>();
        if (receiver != null)
        {
            // 调用 LoadStringArrayFromPlayerPrefs 方法
            search_history = receiver.LoadStringArrayFromPlayerPrefs();
        }
        else
        {
            Debug.LogError("找不到 StringArrayReceiver 组件！");
        }

        if (audioSource.clip != null)
        {
            Debug.Log(audioSource.clip.length);
            if (audioSource.isPlaying)
            {
                // 音频播放时触发开始事件
                if (!wasPlaying)
                {
                    wasPlaying = true;
                    if(search_history.Count > 0)
                        ChangeImage(search_history[search_history.Count - 1]);
                        Debug.Log(search_history[search_history.Count - 1]);
                    ToggleVisibilityState(TeacherIcon); // 播放开始时调用
                    ToggleVisibilityState(Answer);
                    Debug.Log("音频开始播放");
                }
            }
            else if (wasPlaying)
            {
                // 检查音频是否结束，通过时间与长度判断
                if (audioSource.time >= audioSource.clip.length)
                {
                    wasPlaying = false;
                    ToggleVisibilityState(TeacherIcon); // 播放结束时调用
                    ToggleVisibilityState(Answer);
                    Debug.Log("音频播放结束");
                }
            }
        }

    }

    public void ToggleVisibilityState(GameObject gameObject)
    {
        if (gameObject != null)
        {
            // 切换游戏对象的激活状态
            gameObject.SetActive(!gameObject.activeSelf);
            Debug.Log("GameObject is now " + (gameObject.activeSelf ? "visible" : "hidden"));
        }
        else
        {
            Debug.LogError("请在 Inspector 中分配目标对象！");
        }
    }

    void ChangeImage(string imageName)
    {
        // 使用 Resources.Load 来加载图片（假设图片存放在 Resources 文件夹下）
        Sprite newImage = Resources.Load<Sprite>("Images/" + imageName);

        // 检查是否成功加载图片
        if (newImage != null)
        {
            // 修改目标 Image 的 sprite
            Icon.sprite = newImage;
            Debug.Log("图片已成功更改！");
        }
        else
        {
            Debug.LogError("无法找到指定的图片： " + imageName);
        }
    }
}

// public class StringArrayReceiver : MonoBehaviour
// {
//     public List<string> LoadStringArrayFromPlayerPrefs()
//     {
//         // 读取存储的字符串数组
//         int count = PlayerPrefs.GetInt("StringArray_Count", 0);
//         List<string> stringArray = new List<string>();

//         for (int i = 0; i < count; i++)
//         {
//             stringArray.Add(PlayerPrefs.GetString("StringArray_" + i));
//         }

//         return stringArray;
//     }
// }




