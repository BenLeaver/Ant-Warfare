using UnityEngine.Audio;
using UnityEngine;

/// <summary>
/// Represents an individual sound that can be played in the game.
/// Stores configuration data such as clip, volume, pitch, and looping,
/// and holds a runtime AudioSource created by an AudioManager.
/// </summary>
[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
