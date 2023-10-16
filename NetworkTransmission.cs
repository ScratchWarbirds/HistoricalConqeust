using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransmission : NetworkBehaviour
{
    public static NetworkTransmission instance;

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

    [ServerRpc(RequireOwnership = false)]
    public void IWishToSendAChatServerRPC(string _message, ulong _fromWho)
    {
        ChatFromServerClientRPC(_message, _fromWho);
    }

    [ClientRpc]
    private void ChatFromServerClientRPC(string _message, ulong _fromWho)
    {
        GameManager.instance.SendMessageToChat(_message, _fromWho, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMeToDictionaryServerRPC(ulong _steamId, string _steamName, ulong _clientId,bool _isReady)
    {
        GameManager.instance.SendMessageToChat($"{_steamName} has joined", _clientId, true);
        GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId,_isReady);
        GameManager.instance.UpdateClients();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMeFromDictionaryServerRPC(ulong _steamId)
    {
        RemovePlayerFromDictionaryClientRPC(_steamId);
    }

    [ClientRpc]
    private void RemovePlayerFromDictionaryClientRPC(ulong _steamId)
    {
        //Debug.Log("removing client");
        GameManager.instance.RemovePlayerFromDictionary(_steamId);
    }

    [ClientRpc]
    public void UpdateClientsPlayerInfoClientRPC(ulong _steamId, string _steamName, ulong _clientId,bool _isReady)
    {
        GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId,_isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsTheClientReadyServerRPC(bool _ready, ulong _clientId)
    {
        AClientMightBeReadyClientRPC(_ready, _clientId);
    }

    [ClientRpc]
    private void AClientMightBeReadyClientRPC(bool _ready, ulong _clientId)
    {
        foreach (KeyValuePair<ulong, GameObject> player in GameManager.instance.playCards)
        {
            if (player.Key == _clientId)
            {
                player.Value.GetComponent<PlayerBoxController>().ShowReady(_ready);
                if (NetworkManager.Singleton.IsHost)
                {
                    if (GameManager.instance.CheckifPlayersAreReady())
                    {
                        StartReadyTimerServerRpc(true);
                    }
                    else
                    {
                        StartReadyTimerServerRpc(false);
                    }
                }
                else
                {
                    GameManager.instance.CheckifPlayersAreReady();
                }
            }
        }
        foreach (KeyValuePair<ulong, PlayerInfo> player in GameManager.instance.playerInfo)
        {
            if (player.Key == _clientId)
            {
                player.Value.isReady = _ready;
            }
        }
    }

    [ServerRpc]
    private void StartReadyTimerServerRpc(bool _set)
    {
        StartReadyTimerClientRPC(_set);
    }

    [ClientRpc]
    private void StartReadyTimerClientRPC(bool _set)
    {
        if (_set)
        {
            GameManager.instance.matchStartTimer = 4f;
            GameManager.instance.matchStarting = true;
        }
        else
        {
            GameManager.instance.matchStarting = false;
        }
    }

    [ServerRpc]
    public void HostStartedGameServerRPC()
    {
        PlayerControls.instance.myPlayerTurn = 1;
        ClientStartGameClientRPC();
        UpdateMoraleClientRPC(0, 0);
    }

    [ClientRpc]
    public void ClientStartGameClientRPC()
    {
        if (PlayerControls.instance.myPlayerTurn != 1)
        {
            Debug.Log("make players turn 2");
            PlayerControls.instance.myPlayerTurn = 2;
        }
        GameManager.instance.playersTurn = 1;
        UpdateTurnsServerRPC(3, 0);
        SetClientsUIClientRPC("Moves", "");
        GameManager.instance.SetSelector(1);
        GameManager.instance.StartGameClient();
    }

    [ClientRpc]
    private void ShowPlayerBannerClientRPC(int playersTurn) //sets all gamemanangers to player turn
    {
        GameManager.instance.turnBanner1.SetActive(false);
        GameManager.instance.turnBanner2.SetActive(false);

        GameManager.instance.playersTurn = playersTurn;
        GameManager.instance.gameTurn++;
        if (GameManager.instance.playersTurn==1)
        {

            if (PlayerControls.instance.myPlayerTurn == 1)
            {
                Debug.Log("its your turn");
                PlayerControls.instance.tuckSensor.gameObject.SetActive(true);
                GameManager.instance.turnBanner1.SetActive(true);
                GameManager.instance.phaseManager.gamePhase = 4; //start the players turn  

            }
            if (PlayerControls.instance.myPlayerTurn == 2)
            {
                Debug.Log("its your opponents turn");
                GameManager.instance.turnBanner2.SetActive(true);
            }
        }
        if (GameManager.instance.playersTurn == 2)
        {

            if (PlayerControls.instance.myPlayerTurn == 1)
            {
                GameManager.instance.turnBanner2.SetActive(true);
            }
            if (PlayerControls.instance.myPlayerTurn == 2)
            {
                PlayerControls.instance.tuckSensor.gameObject.SetActive(true);
                GameManager.instance.turnBanner1.SetActive(true);
                GameManager.instance.phaseManager.gamePhase = 4; //start the players turn
            }
        }
        GameManager.instance.SetSelector(playersTurn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TargetPlayerDrawsServerRPC(int amount,ulong player)
    {
        TargetPlayerDrawsClientRPC(amount, new Unity.Netcode.ClientRpcParams { Send = new Unity.Netcode.ClientRpcSendParams { TargetClientIds = new List<ulong> { player } } });
    }

    [ClientRpc]
    private void TargetPlayerDrawsClientRPC(int drawAmount,ClientRpcParams clientRpcParams)
    {
        PlayerControls.instance.SetUpDrawAmount(drawAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyforOpeningLandServerRPC(ulong player)
    {
        //do a check to make sure play should be playing an opening land
        foreach(KeyValuePair<ulong,PlayerInfo> _player in GameManager.instance.playerInfo)
        {
            if(_player.Key == player)
            {
                _player.Value.readyHand = true;
            }
        }
        TargetPlayerPlaysLandClientRPC(new Unity.Netcode.ClientRpcParams { Send = new Unity.Netcode.ClientRpcSendParams { TargetClientIds = new List<ulong> { player } } });
        if (CheckifHandReady())
        {
            //start the next phase of the game
            ShowPlayerBannerClientRPC(1);
        }
    }

    private bool CheckifHandReady()
    {
        foreach (KeyValuePair<ulong, PlayerInfo> _player in GameManager.instance.playerInfo)
        {
            if (!_player.Value.readyHand)
            {
                return false;
            }
        }
        return true;
    }

    [ClientRpc]
    private void TargetPlayerPlaysLandClientRPC( ClientRpcParams clientRpcParams)
    {
        //tells a certain player to auto land first turn
        PlayerControls.instance.PlayLandFromDeck();
    }

    [ServerRpc(RequireOwnership = false)]
    public void LandCardbeingPlayedServerRPC(int card,ulong fromClient)
    {
        //player has played a land and tells the server.. Server tells the player they can play a land then responds with the land they played. 
        PlayerplayedaLandClientRPC(card, new Unity.Netcode.ClientRpcParams { Send = new Unity.Netcode.ClientRpcSendParams { TargetClientIds = GetSendList(fromClient) } });
    }

    [ClientRpc]
    private void PlayerplayedaLandClientRPC(int card, ClientRpcParams clientRpcParams)
    {
        //some opponent will play a land
        OpponentsHand.instance.PlayLandFromDeck(card);
        Debug.Log("opponent played a land: " + card);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiscardingaCardServerRPC(int card, ulong fromClient)
    {
        //player has asked the server to discard a certain card.
        //do some check to make sure the player can discard the card.

        //tell the player to discard and also tell the other players this player has discarded
        PlayerisDiscardingaCardClientRPC(card, fromClient);
    }

    [ClientRpc]
    private void PlayerisDiscardingaCardClientRPC(int _card,ulong fromClient)
    {
        if (GameManager.instance.myClientId == fromClient)
        {
            Debug.Log("Discarding card " + _card);
            PlayerControls.instance.DiscardCard(GetCardinHand(_card));
        }
        else
        {
            Debug.Log("player has discarded a card");
            OpponentsHand.instance.DiscardACard(_card);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiscardingaCardinPlayServerRPC(int row,int slot, ulong fromClient)
    {
        if (GameManager.instance.myClientId == fromClient)
        {
            GameManager.instance.localMoves--;
        }
        else
        {
            GameManager.instance.opMoves--;
        }
        UpdateTurnsClientRPC(GameManager.instance.localMoves, GameManager.instance.opMoves);
        PlayerisDiscardingaCardinPlayClientRPC(row,slot, fromClient);
    }

    [ClientRpc]
    private void PlayerisDiscardingaCardinPlayClientRPC(int row, int slot, ulong fromClient)
    {
        if (GameManager.instance.myClientId == fromClient)
        {
            Debug.Log("Discarding card " + row + "  " + slot);
            PlayerControls.instance.DiscardCardinPlay(row,slot);
        }
        else
        {
            Debug.Log("player has discarded a card");
            OpponentsHand.instance.DiscardACardinPlay(row,slot,fromClient);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerPlayingACardServerRPC(int card,int row,int slot,string ability,bool interupt, ulong fromClient)
    {
        if (!interupt)
        {
            if (GameManager.instance.myClientId == fromClient)
            {
                GameManager.instance.localMoves--;
            }
            else
            {
                GameManager.instance.opMoves--;
            }
            UpdateTurnsClientRPC(GameManager.instance.localMoves, GameManager.instance.opMoves);
        }
        PlayerisPlayingACardClientRPC(card,row,slot,ability,interupt, fromClient);
    }

    [ClientRpc]
    public void PlayerisPlayingACardClientRPC(int _card,int row,int slot, string ability,bool interupt, ulong fromClient)
    {
        if (interupt)
        {
            Debug.Log("interupt card");
        }
        if (GameManager.instance.myClientId == fromClient)
        {
            if (GameManager.instance.playerInfo.Count > 1)
            {
                Debug.Log("Waiting on opponent to accept the play " + interupt);
                if (GameManager.instance.acceptCombatButton.activeInHierarchy)
                {
                    GameManager.instance.acceptCombatButton.SetActive(false);
                }
                GameManager.instance.waitingBanner.SetActive(true);
                //show waiting on player banner and turn off interaction
                PlayerControls.instance.waitingonAccept = true;
                //SendCardrequestToPlayerClientRPC(_card, new Unity.Netcode.ClientRpcParams { Send = new Unity.Netcode.ClientRpcSendParams { TargetClientIds = GetSendList(fromClient) } });
            }
            else
            {
                Debug.Log("no opponents playing card");
                PlayerControls.instance.PlayCard(row,ability);
                GameManager.instance.phaseManager.ButtonSwitch(true);
            }

        }
        else
        {
            Debug.Log("getting request from other player to play a card " + interupt);
            PlayerControls.instance.PasspriorityScreen.SetActive(false);
            OpponentsHand.instance.PlayCard(row, _card, fromClient,"Land",false);
            GameManager.instance.interuptBanner.SetActive(true);
            PlayerControls.instance.CardisBeingPlayed(_card,row,slot,ability,interupt);
            GameManager.instance.phaseManager.ButtonSwitch(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptPlayServerRPC(int cardValue, int row, string ability, bool priorityPass, bool autoplay, ulong fromClient)
    {
        if (DeckManager.instance.cardLibrary[cardValue].etb)
        {
            bool host = false;
            if(fromClient != GameManager.instance.myClientId)
            {
                host = true;
            }
            AbilityTable.CheckEnterTheBattleField(DeckManager.instance.cardLibrary[cardValue].etbList,host);
        }
        AcceptPlayClientRPC(cardValue,row,ability, priorityPass,autoplay, fromClient);
    }

    [ClientRpc]
    private void AcceptPlayClientRPC(int cardValue,int row, string ability,bool priorityPass,bool autoplay, ulong fromClient)
    {
        if (fromClient != GameManager.instance.myClientId) //getting message that the opponent has accept your card
        {
            if (!priorityPass) 
            {
                Debug.Log("trigger 1");
                GameManager.instance.phaseManager.ButtonSwitch(true);

                GameManager.instance.waitingBanner.SetActive(false);
                PlayerControls.instance.PlayCard(row, ability);
                PlayerControls.instance.waitingonAccept = false;
                if (!autoplay)
                {
                    DoWinnerCheckServerRPC();
                }
            }
            else //if the opponent accept your interupt play
            {
                Debug.Log("trigger");
                if (!PlayerControls.instance.attackMode)
                {
                    Debug.Log("trigger 2");
                    GameManager.instance.waitingBanner.SetActive(false);
                }
                else
                {
                    Debug.Log("trigger 8");
                    PlayerControls.instance.playOppurtunity = true;
                    GameManager.instance.interuptBanner.SetActive(true);
                    PlayerControls.instance.PasspriorityScreen.SetActive(true);
                    GameManager.instance.waitingBanner.SetActive(false);
                }
                PlayerControls.instance.PlayCard(row, ability);
                PlayerControls.instance.waitingonAccept = false;
                if (GameManager.instance.gameTurn != PlayerControls.instance.myPlayerTurn) // and its not your turn
                {
                    Debug.Log("trigger 3");
                    PlayerControls.instance.playOppurtunity = true;
                    GameManager.instance.interuptBanner.SetActive(true);
                    if (!PlayerControls.instance.inattackMode)
                    {
                        Debug.Log("trigger 4");
                        PlayerControls.instance.PasspriorityScreen.SetActive(true);
                    }
                }
                else
                {
                    if (!PlayerControls.instance.attackMode)
                    {
                        Debug.Log("trigger 5");
                        GameManager.instance.phaseManager.ButtonSwitch(true);
                    }
                }
            }
        }
        else //if you hit the accept button for a play
        {
            Debug.Log("im accepting  " + priorityPass);
            if (priorityPass)
            {
                Debug.Log("trigger 6");
                if (PlayerControls.instance.myPlayerTurn == GameManager.instance.playersTurn)
                {
                    GameManager.instance.waitingBanner.SetActive(true);
                    PlayerControls.instance.waitingonAccept = true;
                    Debug.Log("trigger");
                }
                else
                {
                    Debug.Log("trigger");
                    PlayerControls.instance.PasspriorityScreen.SetActive(false);
                    PlayerControls.instance.hasPriority = false;
                    PlayerControls.instance.playOppurtunity = false;
                    PlayerControls.instance.interuptShowCase = false;
                    if (PlayerControls.instance.inattackMode)
                    {
                        Debug.Log("trigger 7");
                        GameManager.instance.waitingBanner.SetActive(true);
                        PlayerControls.instance.waitingonAccept = true;
                        //this needs to set the players waiting on opponent
                        //GameManager.instance.acceptCombatButton.SetActive(true);
                    }
                    DoWinnerCheckServerRPC();
                }
            }
                PlayerControls.instance.playOppurtunity = false;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void PassingPriorityServerRPC(ulong fromClient)
    {
        PassingPriorityClientRPC(fromClient);
    }

    [ClientRpc]
    private void PassingPriorityClientRPC(ulong fromClient)
    {
        if (fromClient == GameManager.instance.myClientId)
        {
            PlayerControls.instance.PasspriorityScreen.SetActive(false);
            PlayerControls.instance.hasPriority = false;
            PlayerControls.instance.playOppurtunity = false;
            PlayerControls.instance.interuptShowCase = false;
            Debug.Log("Passing priority");
        }
        else
        {
            if (!PlayerControls.instance.inattackMode)
            {
                GameManager.instance.phaseManager.ButtonSwitch(true);
            }
            else
            {
                GameManager.instance.acceptCombatButton.SetActive(true);
            }

            GameManager.instance.waitingBanner.SetActive(false);
            PlayerControls.instance.waitingonAccept = false;
            PlayerControls.instance.interuptShowCase = false;
            PlayerControls.instance.playOppurtunity = false;
            Debug.Log("Passing priority");
        }
        DoWinnerCheckServerRPC();
    }

    [ServerRpc(RequireOwnership =false)]
    public void PlayerEndingTurnServerRPC(ulong fromClient)
    {
        if (GameManager.instance.playersTurn == 1)
        {
            GameManager.instance.playersTurn = 2;
            UpdateTurnsServerRPC(0, 3);
            SetClientsUIClientRPC("", "Moves");
        }
        else
        {
            GameManager.instance.playersTurn = 1;
            UpdateTurnsServerRPC(3, 0);
            SetClientsUIClientRPC("Moves", "");
        }
        ShowPlayerBannerClientRPC(GameManager.instance.playersTurn);
        //SetUpforTurnFlipClientRPC(fromClient, new Unity.Netcode.ClientRpcParams { Send = new Unity.Netcode.ClientRpcSendParams { TargetClientIds = GetSendList(fromClient) } });
    }

    [ServerRpc(RequireOwnership =false)]
    private void UpdateTurnsServerRPC(int localMoves,int opMoves)
    {
        UpdateTurnsClientRPC(localMoves,opMoves);
    }

    [ClientRpc]
    private void UpdateTurnsClientRPC(int localMoves, int opMoves)
    {
        GameManager.instance.localMoves = localMoves;
        GameManager.instance.opMoves = opMoves;
        if (IsHost)
        {
            GameManager.instance.mymoves = localMoves;
        }
        else
        {
            GameManager.instance.mymoves = opMoves;
        }
    }


    [ServerRpc]
    public void UpdateMoraleServerRPC(int local,int op)  
    {
        UpdateMoraleClientRPC(local, op);

        if(local>= 3000)
        {
            Debug.Log("host has won the game");
        }
        if (op >= 3000)
        {
            Debug.Log("client has won the game");
        }

    }

    [ClientRpc]
    private void UpdateMoraleClientRPC(int local,int op)
    {
        GameManager.instance.UpdateMoraleCounters(local, op);
    }

    private CardController GetCardinHand(int _card)
    {
        CardController returnCard = new CardController();
        foreach (KeyValuePair<int, CardController> card in PlayerControls.instance.inHandList)
        {
            if (card.Value.cardValue == _card)
            {
                Debug.Log("card was found");
                returnCard = PlayerControls.instance.inHandList[card.Key];
            }
        }
        if(returnCard == null)
        {
            Debug.Log("card is null");
        }

        return returnCard;
    }

    [ServerRpc(RequireOwnership =false)]
    public void PlayerChangedingPhaseServerRPC(ulong fromclient,int moves, string phase)
    {
        if (fromclient == GameManager.instance.myClientId)
        {
            SetClientsUIClientRPC(phase,"");
            UpdateTurnsClientRPC(moves, 0);
        }
        else
        {
            SetClientsUIClientRPC("",phase);
            UpdateTurnsClientRPC(0, moves);
        }
    }

    [ClientRpc]
    public void SetClientsUIClientRPC(string local,string op)
    {
        if (IsHost)
        {
            GameManager.instance.SetUIElements(local, op);
        }
        else
        {
            GameManager.instance.SetUIElements(op,local);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void PlayerisAttackingServerRPC(ulong clientId,int attackRow,int opRow)
    {
        if (GameManager.instance.myClientId == clientId)
        {
            GameManager.instance.localMoves--;
        }
        else
        {
            GameManager.instance.opMoves--;
        }
        UpdateTurnsClientRPC(GameManager.instance.localMoves, GameManager.instance.opMoves);
        PlayerisAttackingClientRPC(clientId,attackRow,opRow);
    }

    [ClientRpc]
    private void PlayerisAttackingClientRPC(ulong clientId,int attackRow,int opRow)
    { ///this needs to show which rows are attacking and then an except button
        if (clientId == GameManager.instance.myClientId)
        {
            //highlight the rows
            GameManager.instance.waitingBanner.SetActive(true);
            PlayerControls.instance.waitingonAccept = true;
        }
        else
        {
            //highlight the rows and 
            GameManager.instance.acceptCombatButton.SetActive(true);
            GameManager.instance.interuptBanner.SetActive(true);
            PlayerControls.instance.playOppurtunity = true;
            PlayerControls.instance.inattackMode = true;
            PlayerControls.instance.attackingRow = attackRow;
            PlayerControls.instance.OpponentsRow = opRow;
            PlayerControls.instance.attackSelectors[opRow].SetActive(true);
            OpponentsHand.instance.attackSelectors[attackRow].SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void FinalizeTheAttackServerRPC(ulong clientId, int attackRow, int opRow)
    {
        FinalizeTheAttackClientRPC(clientId, attackRow, opRow);
    }

    [ClientRpc]
    private void FinalizeTheAttackClientRPC(ulong clientId, int attackRow, int opRow)
    {
        Debug.Log("attack row is " + attackRow + " oprow is " + opRow);
        GameManager.instance.waitingBanner.SetActive(false);
        bool local = GameManager.instance.playersTurn == PlayerControls.instance.myPlayerTurn;
        if (clientId == GameManager.instance.myClientId)
        {
            GameManager.instance.ChangeCombatText(false);
            PlayerControls.instance.SendCardsToAttackZone(attackRow, opRow, local);
        }
        else
        {
            GameManager.instance.ChangeCombatText(true);
            PlayerControls.instance.SendCardsToAttackZone(attackRow, opRow, local);
        }
        GameManager.instance.continueCombatButton.SetActive(true);
        GameManager.instance.GenerateAttackShowcase(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptCombatServerRPC(ulong fromClient)
    {
        AcceptCombatClientRPC(fromClient);
    }

    [ServerRpc]
    public void CombatResultsServerRPC(ulong clientId,bool hostwon)
    {
        Debug.Log("Host win? " + hostwon);
        if (hostwon)
        {
            UpdateMoraleServerRPC(100, -100);
        }
        else
        {
            UpdateMoraleServerRPC(-100, 100);
        }    
    }

    [ClientRpc]
    private void AcceptCombatClientRPC(ulong clientId)
    {
        //send card back to rows and loser must discard
        if (clientId == GameManager.instance.myClientId)
        {
            GameManager.instance.waitingBanner.SetActive(true);
            if (IsHost)
            {
                if (GameManager.instance.playerhasAcceptedCombat) //if I accept the combat and im the host and the other player has already accepted
                {
                    GameManager.instance.CombatFinished();
                }
            }
        }
        else
        {
            if (IsHost) //if the opponent accepts the combat
            {
                GameManager.instance.playerhasAcceptedCombat = true; //tell the host im ready
                if (GameManager.instance.waitingBanner.activeInHierarchy)//if the host is already ready.. finish combat
                {
                    GameManager.instance.CombatFinished();
                }
            }
        }
    }

    [ServerRpc]
    public void FinishCombatServerRPC(bool winner)
    {
        FinishCombatClientRPC(winner);
    }

    [ClientRpc]
    private void FinishCombatClientRPC(bool winner)//true if is host
    {
        GameManager.instance.waitingBanner.SetActive(false);
        PlayerControls.instance.waitingonAccept = false;
        PlayerControls.instance.inattackMode = false;

        if (IsHost)
        {
            if (winner)
            {
                //Debug.Log("I am the winner");
            }
            else
            {
                //Debug.Log("I am the loser");
            }
        }
        else
        {
            if (winner)
            {
                //Debug.Log("I am the loser");
            }
            else
            {
                //Debug.Log("I am the winner");
            }
        }

        //depending on how combat goes either this needs to happen of the play will get sent into the discard a card screen and then this will happen anyway
        if (GameManager.instance.playersTurn == PlayerControls.instance.myPlayerTurn)
        {
            //if attacks is greater than zero else show the end turn button
            if (IsHost)
            {
                if (GameManager.instance.localMoves == 0)
                {
                    GameManager.instance.phaseManager.gamePhase = 8;
                }
                else
                {
                    GameManager.instance.phaseManager.SetAttackOptions(GameManager.instance.localMoves);
                }
            }
            else
            {
                if (GameManager.instance.opMoves == 0)
                {
                    GameManager.instance.phaseManager.gamePhase = 8;
                }
                else
                {
                    GameManager.instance.phaseManager.SetAttackOptions(GameManager.instance.opMoves);
                }
            }
        }
        DoWinnerCheckServerRPC();
    }

    [ServerRpc]
    public void DoWinnerCheckServerRPC()
    {
        //Debug.Log("winner check");
        GameManager.instance.CheckMorals();
    }

    [ServerRpc]
    public void SomeoneHasWonServerRPC(bool hostWinner, int reason)
    {
        SomeoneHasWonClientRPC(hostWinner,reason);
    }

    [ClientRpc]
    private void SomeoneHasWonClientRPC(bool hostWinner,int reason)
    {
        GameManager.instance.ShowEndMatchScreen(hostWinner,reason);
    }

    private List<ulong> GetSendList(ulong fromClient)
    {
        List<ulong> playerlist = new List<ulong>();
        foreach (KeyValuePair<ulong, PlayerInfo> player in GameManager.instance.playerInfo)
        {
            if (player.Key != fromClient)
            {
                playerlist.Add(player.Key);
            }

        }
        return playerlist;
    }
}
