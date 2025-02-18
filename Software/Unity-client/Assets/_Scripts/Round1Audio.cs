using UnityEngine;
using UnityEngine.SceneManagement;

public class Round1Audio : MonoBehaviour
{
   private AudioSource voice_source; 


    public void search_tutorial_audio()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/SearchTutorial");;
        voice_source.Play();    
    }

    public void take_photo_audio()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/TakePhoto");;
        voice_source.Play();    
    }

    public void round11()
    {
        voice_source = gameObject.GetComponent<AudioSource>();
        voice_source.clip = Resources.Load<AudioClip>("Audios/round11");;
        voice_source.Play();    
    }
}
