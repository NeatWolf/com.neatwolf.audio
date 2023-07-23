using UnityEngine;

public class AudioVolumeListener : MonoBehaviour
{
    public static AudioVolumeListener Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one AudioVolumeListener instance detected. Only one AudioVolumeListener should exist in the scene.");
            Destroy(gameObject);
        }
    }
}