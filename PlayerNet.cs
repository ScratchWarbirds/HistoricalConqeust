using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerNet : NetworkBehaviour
{

    public NetworkVariable<int> deckSize = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> landSize = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> discardSize = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> cardsinHand = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        Debug.Log("on eneable " + GameManager.instance.isHost);
        if (!IsOwner)
        {
            deckSize.OnValueChanged += StateChange;
            landSize.OnValueChanged += StateChange;
            cardsinHand.OnValueChanged += HandChange;
        }
    }

    private void HandChange(int previousValue, int newValue)
    {
        if (IsOwner) return;
        if (newValue > previousValue)
        {
            GameManager.instance.opponenetsHandsize = cardsinHand.Value;
            OpponentsHand.instance.DrawACard();
        }
    }

    private void StateChange(int previousValue, int newValue)
    {
        GameManager.instance.opponentsDeck = deckSize.Value;
        GameManager.instance.opponentsLands = landSize.Value;    
    }

    private void Update()
    {
        if (!IsOwner) return;
        deckSize.Value = GameManager.instance.inGameDeck.Count;
        landSize.Value = GameManager.instance.inGameLands.Count;
        cardsinHand.Value = PlayerControls.instance.cardsinHand;
    }

}
