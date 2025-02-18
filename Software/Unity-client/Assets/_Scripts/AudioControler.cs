using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioControler : MonoBehaviour
{
   private AudioSource tutorial_voice; 

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if(sceneName == "Home")
            search_tutorial_audio();
        else if(sceneName == "Search")
            take_photo_audio();
        else if(sceneName == "Fail")
            PlayAudio("wrongAnswer");
        else if(sceneName == "Win")
            PlayAudio("rigntAnswer");
        else if(sceneName == "Fail2")
            PlayAudio("wrongAnswer");

    }

    public void search_tutorial_audio()
    {
        tutorial_voice = gameObject.GetComponent<AudioSource>();
        tutorial_voice.clip = Resources.Load<AudioClip>("Audios/SearchTutorial");;
        tutorial_voice.Play();    
    }

    public void PlayAudio(string audioName)
    {
        // 获取 AudioSource 组件
        tutorial_voice = gameObject.GetComponent<AudioSource>();

        // 动态加载音频文件
        AudioClip clip = Resources.Load<AudioClip>("Audios/" + audioName);

        // 检查音频文件是否加载成功
        if (clip != null)
        {
            // 设置音频剪辑并播放
            tutorial_voice.clip = clip;
            tutorial_voice.Play();
            Debug.Log("正在播放音频: " + audioName);
        }
        else
        {
            Debug.LogWarning("未找到音频文件: " + audioName);
        }
    }

    public void take_photo_audio()
    {
        tutorial_voice = gameObject.GetComponent<AudioSource>();
        tutorial_voice.clip = Resources.Load<AudioClip>("Audios/TakePhoto");;
        tutorial_voice.Play();    
    }


}
