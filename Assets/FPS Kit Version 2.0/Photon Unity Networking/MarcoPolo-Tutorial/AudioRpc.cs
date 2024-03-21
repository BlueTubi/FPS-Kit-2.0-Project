using UnityEngine;

public class AudioRpc : Photon.MonoBehaviour
{

    public AudioClip marco;
    public AudioClip polo;

    [RPC]
    void Marco()
    {
        if (!this.enabled)
        {
            return;
        }

        Debug.Log("Marco");

        this.GetComponent<AudioSource>().clip = marco;
        this.GetComponent<AudioSource>().Play();
    }

    [RPC]
    void Polo()
    {
        if (!this.enabled)
        {
            return;
        }

        Debug.Log("Polo");

        this.GetComponent<AudioSource>().clip = polo;
        this.GetComponent<AudioSource>().Play();
    }

    void OnApplicationFocus(bool focus)
    {
        this.enabled = focus;
    }
}
