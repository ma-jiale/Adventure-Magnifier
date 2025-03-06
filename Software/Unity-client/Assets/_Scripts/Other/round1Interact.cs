using UnityEngine;
using System.Collections; // 引入 Coroutine 所需的命名空间
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class ActivateGameObjectsOnSceneStart : MonoBehaviour
{
    // 需要激活的两个游戏对象
    public GameObject firstObject;   // 第一个游戏对象
    public GameObject secondObject;  // 第二个游戏对象

    public GameObject Next;  // 第二个游戏对象

    public GameObject Right;  // 第二个游戏对象

    public GameObject Wrong;  // 第二个游戏对象

    private AudioSource voice_source; 

    public GameObject Sunflower_Obj;
    private Image Sunflower;

    void Start()
    {
        Sunflower = Sunflower_Obj.GetComponent<Image>();
        // 启动协程，在场景开始后依次激活两个对象
        StartCoroutine(ActivateObjectsWithDelay());
    
    }

    // 定义一个协程来延时激活两个对象
    private IEnumerator ActivateObjectsWithDelay()
    {
        round11();
        // 等待 2 秒
        yield return new WaitForSeconds(4f);

        // 激活第一个游戏对象
        if (firstObject != null)
        {
            firstObject.SetActive(true);
            Debug.Log(firstObject.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("第一个游戏对象为空，无法激活！");
        }
        round12();
        // 等待 2 秒
        yield return new WaitForSeconds(18f);

        // 激活第二个游戏对象
        if (secondObject != null)
        {
            secondObject.SetActive(true);
            Debug.Log(secondObject.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("第二个游戏对象为空，无法激活！");
        }
    }




    public void round11()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/round11");;
        voice_source.Play();    
    }

        public void round12()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/round12");;
        voice_source.Play();    
    }

    public void rightAnswer()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/next");;
        voice_source.Play();    
    }

    public void SetActiveRightAndNext()
    {
        rightAnswer();
        ChangeImage(Sunflower,"RightSunflower");
        if (Right != null && Next != null)
        {
            Right.SetActive(true);
            Next.SetActive(true);
            Debug.Log(secondObject.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("游戏对象为空，无法激活！");
        }
    }

        public void SetActiveWrongAndLoadFail()
    {
        if (Right != null && Next != null)
        {
            Wrong.SetActive(true);
            go_to_Fail();   
            Debug.Log(secondObject.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("游戏对象为空，无法激活！");
        }
    }
    public void go_to_Fail()
    {
        // 加载新场景
        SceneManager.LoadScene("Fail");    
    }

    public void go_to_Round2()
    {
        // 加载新场景
        SceneManager.LoadScene("Round2");    
    }

        void ChangeImage(Image Icon,string imageName)
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

