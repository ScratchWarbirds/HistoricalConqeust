using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject chatPanel, textObject;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text matchReadyTimer,localName,opName,playerMovesText,opMovesText,localTypeText, opTypeText,localMoraleText,opMoraleText,localPowerText,opPowerText,combatPositionText,resultsText,matchEndText,reasonText;

    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject mainMenu, gameSelectMenu, multiLobby, tunnel, turnIndictor;
    [SerializeField] private GameObject playerBox, deckBox,gameUI,gameBoard,percistantObjects,readyButton,nReadyButton;

    [SerializeField] private GameObject lobbyPlayerBox,playerBoxPrefab;

    [SerializeField] private GameObject playerDeck, playerLandDeck;

    public int moralWinLimit = 1000;

    public GameObject endTurnButton, attackPhaseButton, continueCombatButton,acceptCombatButton,matchEndScreen;

    public GameObject turnBanner1, turnBanner2,waitingBanner,attackShowcase,interuptBanner;
    public SelectorController playerSelector;
    public int playersTurn, localMoves, opMoves,gameTurn,localMoral,opMoral,mymoves;

    public Dictionary<ulong, PlayerInfo> playerInfo = new Dictionary<ulong, PlayerInfo>();
    public Dictionary<ulong, GameObject> playCards = new Dictionary<ulong, GameObject>();

    public Queue<int> inGameDeck = new Queue<int>();
    public Queue<int> inGameLands = new Queue<int>();
    public Queue<int> inGameDiscard = new Queue<int>();
    public Queue<int> inGameDiscardO = new Queue<int>();


    //if more than 2 people will ever play a game together convert these to dictionaries with ulong key's for client access
    //change the playernet to write to these values based on clients id
    public int opponentsDeck;
    public int opponentsLands;
    public int opponenetsHandsize;

    public GamePhases phaseManager;

    [SerializeField]
    private int maxMessages = 20;

    private List<Message> messageList = new List<Message>();

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong myClientId;
    private bool isPaused;

    public bool matchStarting, playerhasAcceptedCombat,hostisWinner;
    public float matchStartTimer;


    /// /////////////////////////////////////////////////

    public string usersName;
    public bool[] ownedSet; //cards that the player has access to
    public Dictionary<int, int[]> customDeck = new Dictionary<int, int[]>();

    /// /////////////////////////////////////////////////
    /// 

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

    private void Update()
    {
        MatchStartTimer();
        HandleUIelements();

        if (inputField.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (inputField.text == " ")
                {
                    inputField.text = "";
                    inputField.DeactivateInputField();
                    return;
                }
                NetworkTransmission.instance.IWishToSendAChatServerRPC(inputField.text, myClientId);
                inputField.text = "";
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                inputField.ActivateInputField();
                inputField.text = " ";
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                pauseMenu.SetActive(true);
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                pauseMenu.SetActive(false);
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        */
    }

    public class Message
    {
        public string text;
        public TMP_Text textObject;
    }


    public void SendMessageToChat(string _text, ulong _fromwho, bool _server)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }
        Message newMessage = new Message();
        string _name = "Server";

        if (!_server)
        {
            if (playerInfo.ContainsKey(_fromwho))
            {
                _name = playerInfo[_fromwho].steamName;
            }
        }

        newMessage.text = _name + ": " + _text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);
    }

    public void ClearChat()
    {
        messageList.Clear();
        GameObject[] chat = GameObject.FindGameObjectsWithTag("ChatMessage");
        foreach (GameObject chit in chat)
        {
            Destroy(chit);
        }
        Debug.Log("clearing chat");
    }

    public void HostCreated()
    {
        isHost = true;
        connected = true;
    }

    public void ConnectedAsClient()
    {
        isHost = false;
        connected = true;
    }


    public void AddPlayerToDictionary(ulong _cliendId, string _steamName, ulong _steamId,bool _isReady)
    {
        if (!playerInfo.ContainsKey(_cliendId))
        {
            PlayerInfo _pi = new PlayerInfo();
            _pi.steamId = _steamId;
            _pi.steamName = _steamName;
            playerInfo.Add(_cliendId, _pi);
            Debug.Log("adding player to dictionary");

            GameObject playerCard = Instantiate(playerBoxPrefab, lobbyPlayerBox.transform);
            playerCard.GetComponent<PlayerBoxController>().SetPlayerInfo(_steamName, 1);
            playCards.Add(_cliendId, playerCard);

            NetworkTransmission.instance.IsTheClientReadyServerRPC(_isReady, myClientId);
        }
    }

    public void UpdateClients()
    {
        Debug.Log("updating clients");
        foreach (KeyValuePair<ulong, PlayerInfo> _player in playerInfo)
        {
            ulong _steamId = _player.Value.steamId;
            string _steamName = _player.Value.steamName;
            ulong _clientId = _player.Key;
            bool _isReady = _player.Value.isReady;

            NetworkTransmission.instance.UpdateClientsPlayerInfoClientRPC(_steamId, _steamName, _clientId,_isReady);

        }
    }

    public void RemovePlayerFromDictionary(ulong _steamId)
    {
        if (multiLobby.activeInHierarchy)
        {
            SetReadyButtons();
            ReadyButton(false);
        }
        PlayerInfo _value = null;
        ulong _key = 100;
        foreach (KeyValuePair<ulong, PlayerInfo> _player in playerInfo)
        {
            if (_player.Value.steamId == _steamId)
            {
                _value = _player.Value;
                _key = _player.Key;
            }
        }
        if (_key != 100)
        {
            playerInfo.Remove(_key);
        }
        if (playCards.ContainsKey(_key))
        {
            Destroy(playCards[_key].gameObject);
            playCards.Remove(_key);
            //Debug.Log("removing card");
            CheckifPlayersAreReady();
        }

    }

    public void ReadyButton(bool _ready)
    {
        NetworkTransmission.instance.IsTheClientReadyServerRPC(_ready, myClientId);
    }

    public bool CheckifPlayersAreReady()
    {
        bool _ready = true;
        int players = 0;
        Debug.Log("player ready check");
        foreach(KeyValuePair<ulong,GameObject> card in playCards)
        {
            players++;
            if (!card.Value.GetComponent<PlayerBoxController>().isReady)
            {
                _ready = false;
            }
        }
        if(players >= 2)
        {
            Debug.Log("seting true");
            readyButton.SetActive(true);
        }
        else
        {
            readyButton.SetActive(false);
        }
                
        return _ready;
    }

    public void SetCamera(bool _set)
    {
        if (_set)
        {
            mainCam.transform.position = new Vector3(0f, 367, -495);
            mainCam.transform.rotation = Quaternion.Euler(57f, 0f, 0f);
        }
        else
        {
            mainCam.transform.position = new Vector3(0f, 0f, 0f);
            mainCam.transform.rotation = Quaternion.Euler(0f, -60f, 0f);
        }
    }

    public void SetReadyButtons()
    {
        nReadyButton.SetActive(false);
        readyButton.SetActive(true);
    }

    public void LoggingIn()
    {
        mainMenu.SetActive(true);
        multiLobby.SetActive(false);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(true);
        deckBox.SetActive(true);
        percistantObjects.SetActive(true);
        SetCamera(false);
        gameUI.SetActive(false);
        gameBoard.SetActive(false);

        DeckManager.instance.InitCards();
        DeckManager.instance.InitTestDeck();
    }

    public void LogOut()
    {
        mainMenu.SetActive(false);
        multiLobby.SetActive(false);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(false);
        deckBox.SetActive(false);
        percistantObjects.SetActive(true);
        SetCamera(false);
        gameUI.SetActive(false);
        gameBoard.SetActive(false);
        customDeck = new Dictionary<int, int[]>();
    }

    public void StartATestGame()
    {
        GameNetworkManager.instance.StartHost(1);
        PlayerControls.instance.myPlayerTurn = 1;
        playersTurn = 1;
        localMoves = 3;
        mainMenu.SetActive(false);
        multiLobby.SetActive(false);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(false);
        deckBox.SetActive(false);
        percistantObjects.SetActive(false);
        SetCamera(true);
        gameUI.SetActive(true);
        gameBoard.SetActive(true);
        tunnel.SetActive(false);
        ReadyTheDeck();
        inGame = true;
        phaseManager.gamePhase = 1;
        SetSelector(1);
        NetworkTransmission.instance.UpdateMoraleServerRPC(0, 0);
    }

    public void StartHost() 
    {
        mainMenu.SetActive(false);
        multiLobby.SetActive(true);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(true);
        deckBox.SetActive(true);
        percistantObjects.SetActive(true);
        SetCamera(false);
        gameUI.SetActive(false);
        gameBoard.SetActive(false);
        readyButton.SetActive(false);
        nReadyButton.SetActive(false);
    }

    public void JoiningGame() //joining another players lobby
    {
        connected = true;
        mainMenu.SetActive(false);
        multiLobby.SetActive(true);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(true);
        deckBox.SetActive(true);
        percistantObjects.SetActive(true);
        SetCamera(false);
        gameUI.SetActive(false);
        gameBoard.SetActive(false);
        readyButton.SetActive(false);
        nReadyButton.SetActive(false);
        tunnel.SetActive(true);
    }

    public void StartGameClient()
    {
        SetDisplayNames();
        mainMenu.SetActive(false);
        multiLobby.SetActive(false);
        gameSelectMenu.SetActive(false);
        playerBox.SetActive(false);
        deckBox.SetActive(false);
        percistantObjects.SetActive(false);
        SetCamera(true);
        gameUI.SetActive(true);
        gameBoard.SetActive(true);
        tunnel.SetActive(false);
        ReadyTheDeck();
        inGame = true;
        phaseManager.gamePhase = 1;
    }

    public void Disconnected()
    {
        foreach (KeyValuePair<ulong, GameObject> card in playCards)
        {
              Destroy(card.Value);
        }
        playerInfo.Clear();
        playCards.Clear();
        CleanTable();
        isHost = false;
        connected = false;
        mainMenu.SetActive(false);
        multiLobby.SetActive(false);
        gameSelectMenu.SetActive(true);
        playerBox.SetActive(true);
        deckBox.SetActive(true);
        percistantObjects.SetActive(true);
        SetCamera(false);
        gameUI.SetActive(false);
        gameBoard.SetActive(false);
        matchStarting = false;
        tunnel.SetActive(true);
    }

    private void CleanTable()
    {
        inGameDeck.Clear();
        inGameLands.Clear();
        inGameDiscard.Clear();
        inGameDiscardO.Clear();
        gameTurn = 0;
        playersTurn = 0;
        localMoral = 0;
        opMoral = 0;
        playerhasAcceptedCombat = false;
        turnBanner1.SetActive(false);
        turnBanner2.SetActive(false);
        attackPhaseButton.SetActive(false);
        endTurnButton.SetActive(false);
        waitingBanner.SetActive(false);
        attackShowcase.SetActive(false);
        continueCombatButton.SetActive(false);
        acceptCombatButton.SetActive(false);
        matchEndScreen.SetActive(false);
        PlayerControls.instance.SettingsSwitch(false);
        PlayerControls.instance.playerReady = false;
        PlayerControls.instance.cardsinHand = 0;
        PlayerControls.instance.cardsinPreview = 0;
        PlayerControls.instance.activeSlotCount = 0;
        PlayerControls.instance.previewOn = false;
        PlayerControls.instance.previewAmount = 0;
        PlayerControls.instance.myPlayerTurn = 0;
        PlayerControls.instance.interuptShowCase = false;
        PlayerControls.instance.waitingonAccept = false;
        PlayerControls.instance.hasPriority = false;
        PlayerControls.instance.continueScreen.SetActive(false);
        PlayerControls.instance.cancelButton.SetActive(false);
        PlayerControls.instance.discoverButton.SetActive(false);
        PlayerControls.instance.PasspriorityScreen.SetActive(false);
        PlayerControls.instance.cardShowCase.SetActive(false);
        PlayerControls.instance.cardbeingPlayedScreen.SetActive(false);
        PlayerControls.instance.cardSelectionScreen.SetActive(false);
        PlayerControls.instance.attackMode = false;
        PlayerControls.instance.ClearHand();
        PlayerControls.instance.ClearSlots();
        PlayerControls.instance.ClearAttackRows();
        if (OpponentsHand.instance != null)
        {
            OpponentsHand.instance.cardsinHand = 0;
            OpponentsHand.instance.activeSlotCount = 0;
            OpponentsHand.instance.ClearHand();
            OpponentsHand.instance.ClearSlots();
            OpponentsHand.instance.ClearAttackRows();
        }
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
    }

    public void ReadyTheDeck()
    {
        //get which deck has been selected through the deckbox
        int decktoUse = 0;
        int length = customDeck[decktoUse].Length;

        Debug.Log("Deck Length is " + length);

        for(int i = 0; i < length; i++)
        {
            int card = customDeck[decktoUse][i];
            if (DeckManager.instance.cardLibrary[card].type == "Land")
            {
                Debug.Log("adding land " + i);
                inGameLands.Enqueue(card);
            }
            else
            {
                //Debug.Log("adding reg cards");
                inGameDeck.Enqueue(card);
            }
        }
    }

    private void MatchStartTimer()
    {
        if (matchStarting && !matchReadyTimer.gameObject.activeInHierarchy) matchReadyTimer.gameObject.SetActive(true);
        if (!matchStarting && matchReadyTimer.gameObject.activeInHierarchy) matchReadyTimer.gameObject.SetActive(false);

        float seconds = Mathf.FloorToInt(matchStartTimer % 60);
        matchReadyTimer.text = seconds.ToString();
        if (matchStarting)
        {
            matchStartTimer -= Time.deltaTime;
            if (matchStartTimer <= 1f)
            {
                matchStarting = false;
                if(isHost) NetworkTransmission.instance.HostStartedGameServerRPC();
            }
        }

    }

    private void SetDisplayNames()
    {
        foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
        {
            if (player.Key != myClientId)
            {
                opName.text = player.Value.steamName;
            }
            if (player.Key == myClientId)
            {
                localName.text = player.Value.steamName;
            }
        }
    }

    public void UpdateMoraleCounters(int local,int op)
    {
        localMoral += local;
        opMoral += op;
        if (isHost)
        {
            localMoraleText.text = localMoral.ToString();
            opMoraleText.text = opMoral.ToString();
        }
        else
        {
            localMoraleText.text = opMoral.ToString();
            opMoraleText.text = localMoral.ToString();
        }
    }

    public void SetSelector(int turn)
    {
        if (PlayerControls.instance.myPlayerTurn == 1)
            if (turn == 1) //to the left
            {
                playerSelector.UpdateTarget(new Vector3(-326f, -127f, 0f));
            }
            else // to the right
            {
                playerSelector.UpdateTarget(new Vector3(326f, -127f, 0f));
            }
        else
        {
            if (turn == 1) //to the right
            {
                playerSelector.UpdateTarget(new Vector3(326f, -127f, 0f));
            }
            else //to the left
            {
                playerSelector.UpdateTarget(new Vector3(-326f, -127f, 0f));
            }
        }
    }

    public void PhaseButtons()
    {
        PlayerControls.instance.DeselectCard();
        phaseManager.gamePhase++;
    }

    public void IncreaseMoralebutton()
    {
        if (isHost)
        {
            localMoral += 1000;
        }
        NetworkTransmission.instance.UpdateMoraleServerRPC(localMoral, opMoral);
    }

    private void HandleUIelements()
    {
        if (PlayerControls.instance.myPlayerTurn == 1)
        {
            if (playersTurn == 1) //if im host and its my turn
            {
                playerMovesText.text = localMoves.ToString();
                opMovesText.text = "";
                localTypeText.gameObject.SetActive(true);
                opTypeText.gameObject.SetActive(false);
            }
            if (playersTurn == 2)
            {
                playerMovesText.text = "";
                opMovesText.text = opMoves.ToString();
                localTypeText.gameObject.SetActive(false);
                opTypeText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (playersTurn == 1)
            {
                playerMovesText.text = "";
                opMovesText.text = localMoves.ToString();
                localTypeText.gameObject.SetActive(false);
                opTypeText.gameObject.SetActive(true);
            }
            else
            {
                playerMovesText.text = opMoves.ToString();
                opMovesText.text = "";
                localTypeText.gameObject.SetActive(true);
                opTypeText.gameObject.SetActive(false);
            }
        }
        if (playersTurn == 0)
        {
            playerMovesText.text = "";
            opMovesText.text = "";
            localTypeText.gameObject.SetActive(false);
            opTypeText.gameObject.SetActive(false);
        }
    }

    public void GenerateAttackShowcase(ulong clientId) //clientid is the attacker
    {
        attackShowcase.SetActive(true);
        int attackPower = 0;
        int defendPower = 0;
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject card in cards)
        {
            if (card.GetComponent<CardController>().inAttackPreview)
            {
                if (card.GetComponent<CardController>().opponent) //opponents side
                {
                    if (clientId == myClientId)//if im the attacker
                    {
                        defendPower += DeckManager.instance.cardLibrary[card.GetComponent<CardController>().cardValue].defence;
                    }
                    else //if the opponents is the attacker
                    {
                        attackPower += DeckManager.instance.cardLibrary[card.GetComponent<CardController>().cardValue].attack;
                    }
                }
                else //your side
                {
                    if (clientId == myClientId)//if im the attacker
                    {
                        attackPower += DeckManager.instance.cardLibrary[card.GetComponent<CardController>().cardValue].attack;
                    }
                    else //if the opponents is the attacker
                    {
                        defendPower += DeckManager.instance.cardLibrary[card.GetComponent<CardController>().cardValue].defence;
                    }
                }
            }
        }
        Debug.Log(attackPower + " defense " + defendPower);
        if (clientId == myClientId) //if I'm the host and im attacking
        {
            if(attackPower >= defendPower)
            {
                resultsText.text = "Battle Won";
            }
            else
            {
                resultsText.text = "Battle Lost";
            }
            localPowerText.text = attackPower.ToString();
            opPowerText.text = defendPower.ToString();
            DeterminCombatWinner(attackPower, defendPower, false);

        }
        else //host and deffending
        {
            if (attackPower >= defendPower)
            {
                resultsText.text = "Battle Lost";
            }
            else
            {
                resultsText.text = "Battle Won";
            }
            localPowerText.text = defendPower.ToString();
            opPowerText.text = attackPower.ToString();
            DeterminCombatWinner(defendPower, attackPower, true);
        }
    }

    private void DeterminCombatWinner(int hostP, int clientP, bool hostTclientF)
    {
        if (hostP > clientP)
        {
            Debug.Log("host is the winner");
            hostisWinner = true;
        }
        else if (hostP < clientP)
        {
            Debug.Log("client is winner");
            hostisWinner = false;
        }
        else if (hostP == clientP)
        {
            if (hostTclientF)
            {
                Debug.Log("host is the winner");
                hostisWinner = true;
            }
            else
            {
                Debug.Log("client is winner");
                hostisWinner = false;
            }
        }
        NetworkTransmission.instance.CombatResultsServerRPC(myClientId, hostisWinner);
    }

    public void ChangeCombatText(bool set)
    {
        if (set)
        {
            combatPositionText.text = "Attacking";
        }
        else
        {
            combatPositionText.text = "Defending";
        }
    }

    public void CombatFinished()
    {
        //Debug.Log("finish combat");
        playerhasAcceptedCombat = false;
        NetworkTransmission.instance.FinishCombatServerRPC(hostisWinner);
    }

    public void CheckMorals()
    {
        if (localMoral >= moralWinLimit)
        {
            Debug.Log("i am the winner of the game");
            NetworkTransmission.instance.SomeoneHasWonServerRPC(true,1);
            
        }
        if (opMoral >= moralWinLimit)
        {
            Debug.Log("client is the winner of the game");
            NetworkTransmission.instance.SomeoneHasWonServerRPC(false,1);
        }
    }

    public void ShowEndMatchScreen(bool hostWinner,int reason)
    {
        
        matchEndScreen.SetActive(true);
        PlayerControls.instance.tuckSensor.gameObject.SetActive(false);
        if(isHost)
        {
            if (hostWinner)
            {
                matchEndText.text = "Victory!";
                reasonText.text = SetReasonText(true, reason);
            }
            else
            {
                matchEndText.text = "Defeat!";
                reasonText.text = SetReasonText(false, reason);
            }
        }
        else
        {
            if (!hostWinner)
            {
                matchEndText.text = "Victory!";
                reasonText.text = SetReasonText(true, reason);
            }
            else
            {
                matchEndText.text = "Defeat!";
                reasonText.text = SetReasonText(false, reason);
            }
        }
    }

    private string SetReasonText(bool Winner, int reason)
    {
        string text = "";
        switch (reason)
        {
            case 1: //morale victory
                if (Winner)
                {
                    text = "You have won by Moral.";
                }
                else
                {
                    text = "Your opponent has won by Moral.";
                }
                break;
        }

        return text;
    }


    public void LeaveMatchButton()
    {
        matchEndScreen.SetActive(false);
        GameNetworkManager.instance.Disconnect();
    }

    public void SetUIElements(string local,string op)
    {
        localTypeText.text = local;
        opTypeText.text = op;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
