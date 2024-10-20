using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    public AudioClip mainMenusMusic;

    public static LevelAudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        musicSource = GetComponent<AudioSource>();
        PlayMusic(mainMenusMusic, 0.1f);
    }

    public void PlayMusic(AudioClip clip, float volume = 1.0f)
    {
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}