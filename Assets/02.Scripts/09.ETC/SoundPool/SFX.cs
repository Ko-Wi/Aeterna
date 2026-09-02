using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public AudioClip[] sfx;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip _sfx, float _vol)
    {
        audioSource.clip = _sfx;
        audioSource.volume = _vol;
        audioSource.Play();

        SoundPool.Instance.soundPool.Despawn(transform, _sfx.length);
    }

}
