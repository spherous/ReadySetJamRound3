using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AdjustMixerGroupVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string mixerGroupName;

    public void SetVolume(float silderVal) => 
        mixer.SetFloat(mixerGroupName, Mathf.Log10(silderVal) * 20);
}