using UnityEngine;
using System.Collections; // 引入 Coroutine 所需的命名空间
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class ReviewInteract : MonoBehaviour
{
    // 需要激活的两个游戏对象
    public GameObject firstObject;   // 第一个游戏对象
    public GameObject secondObject;  // 第二个游戏对象

    public GameObject thirdObject;  // 第三个游戏对象

    private AudioSource voice_source; 


    void Start()
    {
        // 启动协程，在场景开始后依次激活两个对象
        StartCoroutine(ActivateObjectsWithDelay());
    
    }

    // 定义一个协程来延时激活两个对象
    private IEnumerator ActivateObjectsWithDelay()
    {
        // 等待 2 秒
        yield return new WaitForSeconds(1f);
        review1();
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
        // 等待 2 秒
        yield return new WaitForSeconds(5f);
        review2();
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
        yield return new WaitForSeconds(1f);
                // 激活第二个游戏对象
        if (thirdObject != null)
        {
            thirdObject.SetActive(true);
            Debug.Log(thirdObject.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("第二个游戏对象为空，无法激活！");
        }
    }

        public void review1()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/review1");;
        voice_source.Play();    
    }

        public void review2()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/review2");;
        voice_source.Play();    
    }

    public void go_to_Round1()
    {
        // 加载新场景
        SceneManager.LoadScene("Round1");    
    }

    public void go_to_Win()
    {
        // 加载新场景
        SceneManager.LoadScene("Win");    
    }
}
