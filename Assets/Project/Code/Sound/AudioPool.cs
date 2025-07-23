using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    private Queue<AudioSource> _audioSourcePool = new();

    private static AudioPool _instance;
    public static AudioPool Instance => _instance;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(AudioClip clip, Vector3 position, float pitch = 1.0f)
    {
        if (!_audioSourcePool.TryPeek(out var source))
        {
            source = Instantiate(_audioSource, position, Quaternion.identity);
        }

        // ñ⁄ìIÇÃà íuÇ≈çƒê∂
        source.transform.position = position;
        source.pitch = pitch;
        source.PlayOneShot(clip);

        Release(source, clip.length);
    }

    private async void Release(AudioSource target, float waitTime)
    {
        try
        {
            await UniTask.WaitForSeconds(waitTime);
        }
        catch { return; }

        _audioSourcePool.Enqueue(target);
    }
}
