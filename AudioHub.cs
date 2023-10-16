using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHub : MonoBehaviour
{

    public static AudioHub instance;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
