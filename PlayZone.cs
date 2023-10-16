using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayZone : MonoBehaviour
{
    public Image[] images;
    public TMP_Text words,otherwords;
    private byte fade = 0;
    private byte dZoneFade = 0;
    private byte mouseFade = 0;
    private byte discFade = 0;
    [SerializeField]
    private GameObject playerZoneObj,discardZoneObj;
    public Image mouseOverPlayObj,mouseOverDisObj;

    private void Update()
    {
        images[0].color = new Color32(255, 204, 38, fade);
        images[1].color = new Color32(255, 204, 38, fade);
        images[2].color = new Color32(255, 204, 38, dZoneFade);
        mouseOverPlayObj.color = new Color32(255, 204, 38, mouseFade);
        mouseOverDisObj.color = new Color32(255, 204, 38, discFade);
        words.color = new Color32(255, 204, 38, fade);
        otherwords.color = new Color32(255, 204, 38, dZoneFade);

        if (playerZoneObj.activeInHierarchy)
        {
            ActivatePlayZone();
        }
        else
        {
            DeActivatePlayZone();
        }

        if (discardZoneObj.activeInHierarchy)
        {
            ActivateDiscZone();
        }
        else
        {
            DeActivateDiscZone();
        }

        if (IsPointerOverUIElement("PlayZone"))
        {
            ActivatePlayMouse();
        }
        else
        {
            DeActivatePlayMouse();
        }

        if (IsPointerOverUIElement("DiscardZone"))
        {
            ActivateDiscMouse();
        }
        else
        {
            DeActivateDiscMouse();
        }
    }

    private void ActivatePlayZone()
    {
        if (fade < 100)
        {
            fade++;
        }
    }

    private void DeActivatePlayZone()
    {
        if (fade > 0)
        {
            fade--;
        }
    }

    private void ActivateDiscZone()
    {
        if (dZoneFade < 100)
        {
            dZoneFade++;
        }
    }

    private void DeActivateDiscZone()
    {
        if (dZoneFade > 0)
        {
            dZoneFade--;
        }
    }

    private void ActivatePlayMouse()
    {
        if (mouseFade < 100)
        {
            mouseFade++;
        }
    }

    private void DeActivatePlayMouse()
    {
        if (mouseFade > 0)
        {
            mouseFade--;
        }
    }

    private void ActivateDiscMouse()
    {
        if (discFade < 100)
        {
            discFade++;
        }
    }

    private void DeActivateDiscMouse()
    {
        if (discFade > 0)
        {
            discFade--;
        }
    }

    public bool IsPointerOverUIElement(string tag)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(), tag);
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, string tag)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.tag == tag)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
