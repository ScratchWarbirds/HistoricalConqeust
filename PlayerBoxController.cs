using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerBoxController : MonoBehaviour
{
    [SerializeField] private Image playerTag;
    [SerializeField] private TMP_Text playerName;

    public GameObject readyImage;
    public bool isReady;


    public void SetPlayerInfo(string _playerName,int _playerTag)
    {
        playerName.text = _playerName;
        playerTag.sprite = ImageManager.instance.playerTags[_playerTag];
    }

    public void ShowReady(bool _set)
    {
        Debug.Log("getting asked " + _set);
        readyImage.SetActive(_set);
        isReady = _set;
    }
}
