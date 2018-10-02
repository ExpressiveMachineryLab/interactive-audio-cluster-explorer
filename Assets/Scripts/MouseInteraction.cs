using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MouseInteraction : EventTrigger
{
    //public static AudioClip clip;
    public static WWW www;
    public static GameObject sourceObject;
    public string filename;

    public override void OnPointerEnter(PointerEventData data)
    {
        //clip = Resources.Load<AudioClip>(filename);
        //WWW www = new WWW("file://" + filename);
        //Debug.Log(www.url);
        //source.clip = clip;
        //source.loop = true;
        //Debug.Log("Is playing: " + source.isPlaying);
        //source.PlayOneShot(clip, 1);
        //sourceObject.GetComponent<AudioSource>().clip = www.GetAudioClip(false, false);
        //sourceObject.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(filename);
        //StartCoroutine(LoadClip());
        StartCoroutine(LoadClipWWW());
        //sourceObject.GetComponent<AudioSource>().PlayOneShot(sourceObject.GetComponent<AudioSource>().clip, 1);
        Debug.Log("Loaded clip and source for " + gameObject.name);
    }

    public IEnumerator LoadClip()
    {
        sourceObject.GetComponents<AudioSource>()[0].clip = null;

        AudioClip clip = Resources.Load<AudioClip>(filename);
        
        yield return clip;

        sourceObject.GetComponents<AudioSource>()[0].clip = clip;

        Debug.Log("Loaded clip for " + gameObject.name + " with " + filename);
    }

    public IEnumerator LoadClipWWW()
    {
        sourceObject.GetComponents<AudioSource>()[0].clip = null;

        if (www != null && www.url != "")
        {
            www.Dispose();
        }

        www = new WWW("file://" + filename);
        yield return www;

        AudioClip clip = www.GetAudioClipCompressed(false, AudioType.WAV);
        yield return clip;

        sourceObject.GetComponents<AudioSource>()[0].clip = clip;
        Debug.Log("Loaded clip for " + gameObject.name + " with " + filename);
    }

    public override void OnPointerExit(PointerEventData data)
    {
        //clip = null;
        //source.clip = null;
        //Debug.Log("Unloaded clip and source for " + gameObject.name);
    }

    public override void OnPointerClick(PointerEventData data)
    {
        //if (sourceObject.GetComponent<AudioSource>().clip != null)
        //{
        //    sourceObject.GetComponent<AudioSource>().PlayOneShot(sourceObject.GetComponent<AudioSource>().clip, 1);
        //    Debug.Log("Played clip for " + gameObject.name);
        //    Debug.Log("Is playing: " + sourceObject.GetComponent<AudioSource>().isPlaying);
        //}
    }
}