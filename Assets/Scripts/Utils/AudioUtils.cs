
using UnityEngine;

class AudioUtils
{
    static public AudioSource PlayOnce(AudioClip _clip, Vector3 _pos, float _volume = 1, float _pitch = 1)
    {
        if (_clip)
        {
            GameObject gobj = new GameObject(_clip.name);
            AudioSource source = gobj.AddComponent<AudioSource>();
            AutoDestruct autoDestruct = gobj.AddComponent<AutoDestruct>();

            gobj.transform.position = _pos;

            source.clip = _clip;
            source.volume = _volume;
            source.pitch = _pitch;
            source.Play();

            autoDestruct.delay = _clip.length;

            return source;
        }
        return null;
    }
}

