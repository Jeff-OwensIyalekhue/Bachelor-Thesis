using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    #region Vars
    public static MusicManager instance;

    public AudioClip titleMusic;                    //Assign Audioclip for title music loop
    public AudioClip[] mainMusic;                   //Assign Audioclip for main 
    public int songs;
    public AudioMixerSnapshot volumeDown;           //Reference to Audio mixer snapshot in which the master volume of main mixer is turned down
    public AudioMixerSnapshot volumeUp;             //Reference to Audio mixer snapshot in which the master volume of main mixer is turned up

    public AudioMixer mainMixer;

    private AudioSource musicSource;                //Reference to the AudioSource which plays music
    private float resetTime = 1f;                   //Very short time used to fade in near instantly without a click
    #endregion

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(transform.gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //Get a component reference to the AudioSource attached to the UI game object
        musicSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneWasLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneWasLoaded;
    }

    void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayLevelMusic();
    }

    public void PlayLevelMusic()
    {
        //This switch looks at the last loadedLevel number using the scene index in build settings to decide which music clip to play.
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            //If scene index is 0 (usually title scene) assign the clip titleMusic to musicSource
            case 0:
                musicSource.clip = titleMusic;
                break;
            default:
                int r = Random.Range(0, songs);
                musicSource.clip = mainMusic[r];
                break;
        }
        //Fade up the volume very quickly, over resetTime seconds (.01 by default)
        FadeUp(resetTime);
        //Play the assigned music clip in musicSource
        musicSource.Play();
    }

    // Call this function and pass in the float parameter musicLvl to set the volume of the AudioMixerGroup Music in mainMixer
    public void SetMusicLevel(float musicLvl)
    {
        mainMixer.SetFloat("musicVol", musicLvl);
        GameManager.Instance.musicVolume = musicLvl;
    }

    // Call this function and pass in the float parameter sfxLevel to set the volume of the AudioMixerGroup SoundFx in mainMixer
    public void SetSfxLevel(float sfxLevel)
    {
        mainMixer.SetFloat("sfxVol", sfxLevel);
        //GameData.Instance.sfxVolume = sfxLevel;
    }

    // Call this function to very quickly fade up the volume of master mixer
    public void FadeUp(float fadeTime)
    {
        //call the TransitionTo function of the audioMixerSnapshot volume_Up;
        volumeUp.TransitionTo(fadeTime);
        //Debug.Log("Fade down");
    }

    // Call this function to fade the volume to silence over the length of fade_Time
    public void FadeDown(float fadeTime)
    {
        //call the TransitionTo function of the audioMixerSnapshot volumeDown;
        volumeDown.TransitionTo(fadeTime);
    }
}
