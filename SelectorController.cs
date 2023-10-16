using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorController : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool move;
    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (move)
        {
            rt.anchoredPosition = Vector3.Lerp(rt.anchoredPosition, targetPosition, 8f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 2 && move)
        {
            move = false;
        }
    }

    public void UpdateTarget(Vector3 newpos)
    {
        targetPosition = newpos;
        move = true;
    }
}
