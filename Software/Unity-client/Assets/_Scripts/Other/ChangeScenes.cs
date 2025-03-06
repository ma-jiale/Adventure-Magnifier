using UnityEngine;
using UnityEngine.SceneManagement;

public class change_scence : MonoBehaviour
{
    public void go_to_Home()
    {
        SceneManager.LoadScene("Home_2");
        Debug.Log("go to home");
    }

    public void go_to_Search()
    {
        SceneManager.LoadScene("Search");    
    }

    public void go_to_Round1()
    {
        SceneManager.LoadScene("Round1");    
    }

    public void go_to_Round2()
    {
        SceneManager.LoadScene("Round2");    
    }

    public void go_to_Review()
    {
        SceneManager.LoadScene("Review");    
    }

    public void go_to_VoiceSearch()
    {
        SceneManager.LoadScene("VoiceSearch");    
    }
}
