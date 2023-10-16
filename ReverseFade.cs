using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReverseFade : MonoBehaviour
{
    public Image[] images;
    public TMP_Text words;
    private byte fade = 0;
    private float waitFade;

    private void OnEnable()
    {
        fade = 255;
        waitFade = 2f;
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = new Color32(255, 255, 255, 255);
        }
        words.color = new Color32(255, 255, 255, 255);
    }

    private void Update()
    {
        if (waitFade > 0)
        {
            waitFade -= Time.deltaTime;
        }
        if (waitFade <= 0)
        {
            if (fade > 0)
            {
                fade--;
            }
            if (fade <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (i == 4 || i == 3)
            {
                images[i].color = new Color32(255, 204, 38, fade);
            }
            else
            {
                images[i].color = new Color32(255, 255, 255, fade);
            }
        }
        words.color = new Color32(255, 204, 38, fade);
    }
}
