using UnityEngine.Audio;
using UnityEngine;
using System;

/// <summary>
/// Manages audio playback for a specific GameObject.
/// Unlike the global AudioManager, this component attaches 3D AudioSources
/// so that sounds originate from the GameObject’s position in the scene.
/// <summary>
public class ObjectAudioManager : MonoBehaviour
{

    public Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.maxDistance = 50;
            s.source.spatialBlend = 1f;
        }
    }

    /// <summary>
    /// Plays a sound by name from this object’s position in the scene.
    /// </summary>
    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }
}
