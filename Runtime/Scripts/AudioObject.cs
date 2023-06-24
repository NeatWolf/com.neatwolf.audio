using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeatWolf.Audio
{
/// <summary>
/// This class represents a setting for an AudioClip, 
/// holding values for pitch, volume, pan and starting/ending positions.
/// </summary>
[System.Serializable]
public class ClipSettings
{
    [SerializeField]
    private AudioClip audioClip;
    [SerializeField]
    private float pitch;
    [SerializeField]
    private float volume;
    [SerializeField]
    private float panStereo;
    [SerializeField]
    private float startPosition;
    [SerializeField]
    private float endPosition;

    /// <summary>
    /// The AudioClip to be played.
    /// </summary>
    public AudioClip AudioClip => audioClip;

    /// <summary>
    /// The pitch of the AudioClip.
    /// </summary>
    public float Pitch => pitch;

    /// <summary>
    /// The volume of the AudioClip.
    /// </summary>
    public float Volume => volume;

    /// <summary>
    /// The pan position of the AudioClip.
    /// </summary>
    public float PanStereo => panStereo;

    /// <summary>
    /// The starting position in the AudioClip.
    /// </summary>
    public float StartPosition => startPosition;

    /// <summary>
    /// The ending position in the AudioClip.
    /// </summary>
    public float EndPosition => endPosition;
}

/// <summary>
/// This enum represents different play modes for an AudioObject.
/// </summary>
public enum PlayMode
{
    Random,
    RandomDifferent
}

/// <summary>
/// This class represents an AudioObject, holding multiple AudioClip settings and providing methods to play them.
/// </summary>
[CreateAssetMenu(menuName = "Audio/Audio Object")]
public class AudioObject : ScriptableObject
{
    [SerializeField]
    private List<ClipSettings> audioClipsSettings;
    [SerializeField]
    private AudioChannel audioChannel;
    [SerializeField]
    private AudioRolloffMode rolloffMode;
    [SerializeField]
    private float spatialBlend;
    [SerializeField]
    private PlayMode playMode;
    private List<ClipSettings> audioClipsHistory = new List<ClipSettings>();

    /// <summary>
    /// The list of settings for AudioClips in this AudioObject.
    /// </summary>
    public List<ClipSettings> AudioClipsSettings => audioClipsSettings;
    
    /// <summary>
    /// The AudioChannel for this AudioObject.
    /// </summary>
    public AudioChannel AudioChannel => audioChannel;

    /// <summary>
    /// The AudioRolloffMode for this AudioObject.
    /// </summary>
    public AudioRolloffMode RolloffMode => rolloffMode;

    /// <summary>
    /// The spatial blend value for this AudioObject.
    /// </summary>
    public float SpatialBlend => spatialBlend;

    /// <summary>
    /// Returns a ClipSettings following the playMode rules.
    /// </summary>
    /// <param name="clipSettings">The ClipSettings to play.</param>
    /// <returns>ClipSettings to play.</returns>
    public AudioClip GetClipSettings(out ClipSettings clipSettings)
    {
        clipSettings = null;

        switch (playMode)
        {
            case PlayMode.Random:
                clipSettings = GetRandomClip();
                break;
            case PlayMode.RandomDifferent:
                clipSettings = GetRandomDifferentClip();
                break;
        }

        return clipSettings.AudioClip;
    }

    private ClipSettings GetRandomClip()
    {
        int randomIndex = UnityEngine.Random.Range(0, audioClipsSettings.Count);
        return audioClipsSettings[randomIndex];
    }

    private ClipSettings GetRandomDifferentClip()
    {
        List<ClipSettings> nonHistoryClips = audioClipsSettings.Except(audioClipsHistory).ToList();
        if (nonHistoryClips.Count == 0)
        {
            // All clips have been played, clear the history.
            audioClipsHistory.Clear();
            nonHistoryClips = audioClipsSettings;
        }

        int randomIndex = UnityEngine.Random.Range(0, nonHistoryClips.Count);
        ClipSettings chosenClip = nonHistoryClips[randomIndex];
        audioClipsHistory.Add(chosenClip);
        return chosenClip;
    }

    public void PlayAtPoint(Vector3 position)
    {
        AudioManager.Instance.PlaySoundAtPosition(this, position);
    }
}
}
