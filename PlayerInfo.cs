using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfo
{
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public bool isReady;
    public bool readyHand;
}
