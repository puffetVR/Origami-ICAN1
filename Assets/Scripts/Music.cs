using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Music : MonoBehaviour
{
    public EventReference musicEvent;

    EventInstance musicInstance;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        musicInstance = FMODUnity.RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    public void PlayMusic(string name)
    {
        Debug.Log("Playing " + name);
        musicInstance.setParameterByName(name, 1);
    }

}
