using System.Collections.Generic;
using UnityEngine;

public class PooledParticleSystem : MonoBehaviour, IPoolable
{
    public bool inPool {get => _inPool; set => _inPool = value;}
    bool _inPool = false;
    public event OnReturnToPool onReturnToPool;
    [SerializeField] private ParticleSystem sparkSystem;
    bool isAlive = false;
    [SerializeField] private AudioSource audioSource;
    public List<AudioClip> clips = new List<AudioClip>();
    public bool resetTransforms = false;
    private void Update() {
        if(isAlive)
        {
            if(!sparkSystem.IsAlive())
            {
                if(resetTransforms)
                {
                    transform.localScale = Vector3.one;
                    for(int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        child.localScale = Vector3.one;
                    }
                }

                isAlive = false;
                onReturnToPool?.Invoke();
            }
        }
    }

    public void Clear() => sparkSystem.Clear();
    public void Play() 
    {
        if(audioSource != null)
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Count)]);

        isAlive = true;
        sparkSystem.Play();
    }
    public void Pause() => sparkSystem.Pause();
    public void Stop() => sparkSystem.Stop();
}
