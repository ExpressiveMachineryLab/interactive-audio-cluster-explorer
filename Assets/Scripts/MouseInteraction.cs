using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MouseInteraction : EventTrigger
{
    //public static AudioClip clip;
    public static WWW www;
    public static GameObject sourceObject;
    public Point point;
    public InstantiatePoints instantiatePointsComponent;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        instantiatePointsComponent.SetCurrentPoint(point);
        StartCoroutine(LoadClipWWW());
        //Debug.Log("Loaded clip and source for " + gameObject.name);
    }

    public IEnumerator LoadClip()
    {
        sourceObject.GetComponents<AudioSource>()[0].clip = null;

        AudioClip clip = Resources.Load<AudioClip>(point.filename);
        
        yield return clip;

        sourceObject.GetComponents<AudioSource>()[0].clip = clip;

        //Debug.Log("Loaded clip for " + gameObject.name + " with " + point.filename);
    }

    public IEnumerator LoadClipWWW()
    {
        sourceObject.GetComponents<AudioSource>()[0].clip = null;

        if (www != null && www.url != "")
        {
            www.Dispose();
        }

        www = new WWW("file://" + point.filename);
        yield return www;

        AudioClip clip = www.GetAudioClipCompressed(false, AudioType.WAV);
        yield return clip;

        sourceObject.GetComponents<AudioSource>()[0].clip = clip;
        //Debug.Log("Loaded clip for " + gameObject.name + " with " + point.filename);
    }
}