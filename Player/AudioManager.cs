using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Player Sound Effects")]
    public AudioClip[] playerSFX;

    [Header("Control Flags")]
    [SerializeField] private bool canPlaySFX = true;

    void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        sfxSource = sources[0];
        musicSource = sources[1];
    }

    public void PlayRandomSFX(AudioClip[] clips, float volume = 1.0f)
    {
        if(clips.Length == 0) return;

        int randomInd = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomInd];
        sfxSource.PlayOneShot(clip, volume);
    }

    public IEnumerator PlayRandomSFXAtChanceWithCooldown(AudioClip[] clips, float chance, float cooldown, float volume = 1.0f)
    {
        if (!canPlaySFX || clips.Length == 0) yield break;

        if(Random.value <= chance)
        {
            int randomIndex = Random.Range(0, clips.Length);
            AudioClip clip = clips[randomIndex];
            sfxSource.PlayOneShot(clip, volume);
            canPlaySFX = false;
            yield return new WaitForSeconds(cooldown);
            canPlaySFX = true;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        sfxSource.PlayOneShot(clip, volume);
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