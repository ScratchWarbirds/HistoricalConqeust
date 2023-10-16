using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewFade : MonoBehaviour
{
    public Image[] images;
    public TMP_Text words;
    private byte fade = 0;

    private void OnEnable()
    {
        fade = 0;
        for(int i = 0; i < images.Length; i++)
        {
            images[i].color = new Color32(255, 255, 255, 0);
        }
        words.color = new Color32(255, 255, 255, 0);
    }

    private void Update()
    {
        if (fade < 255)
        {
            fade++;
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
