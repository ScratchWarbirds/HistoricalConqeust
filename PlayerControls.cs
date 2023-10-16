using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerControls : MonoBehaviour
{
    public int handSize = 5;

    public Camera mainCam;
    public GameObject continueScreen,cardbeingPlayedScreen,cardShowCase,PasspriorityScreen,activateButton,cardSelectionScreen;

    public GameObject cardPrefab,playerHand,playerField;

    public GameObject topofDeck, topofLands,LandTarget,playTarget,cancelButton,cardZoneUI,discoverButton,previewTarget,activeSlot,discardOption,discardTarget,attackTarget;

    public int drawAmount,previewAmount,myPlayerTurn;
    public bool previewOn, previewtoHand,drawCards,inSettings,noShowCase,interuptShowCase,hasPriority,inattackMode;
    private bool tucked = true;

    private PreviewCard previewCard;

    public GameObject[] landSlots,row1Slots, row2Slots, row3Slots, row4Slots,rowSelectors,attackSelectors;

    public bool playerReady,selectingSlot,cardSelected,waitingonAccept,attackMode,playOppurtunity;

    private float timer,drawTimer,scTimer;
    private int drawTick;

    public Image tuckSensor,handScoller,playZone,discardZone;

    public Image[] cardShowCaseImages;

    public CardController discardPreview,selectedCard,choosenCard;

    public int cardsinHand,cardsinPreview,activeSlotCount,attackingRow,OpponentsRow;
    public Dictionary<int, CardController> inHandList = new Dictionary<int, CardController>();
    private CardController mouseCard,playCard;
    private float zCord;
    private Vector3 dragOffset;

    public List<string> activeAbiliites = new List<string>();
    public List<GameObject> searchList = new List<GameObject>();

    public static PlayerControls instance;
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

    public class PreviewCard
    {
        public int cardValue;
        public string cardAbility;
        public int row;
        public int slot;
    }

    private void Update()
    {
        HandlePreview();
        PreviewToHand();
        HandleMouse();

        if (Input.GetMouseButtonDown(0) && continueScreen.activeInHierarchy && !inSettings && !IsPointerOverUIElement("UI"))
        {
            continueScreen.SetActive(false);
            previewtoHand = true;
            playerReady = true; //set the player in card manipulation mode

            if (GameManager.instance.phaseManager.gamePhase == 11) //accept to end turn here <-------
            {
                GameManager.instance.phaseManager.gamePhase = 3;
                EndTurn();
            }
        }

        if (IsPointerOverUIElement("HandScroll") && !inSettings)
        {

        }
        else
        {
            if (!tucked)
            {
                handScoller.gameObject.SetActive(false);
                tuckSensor.gameObject.SetActive(true);
                tucked = true;
                if (!playOppurtunity&& !waitingonAccept)
                {
                    GameManager.instance.phaseManager.ButtonSwitch(true);
                }
                if (inattackMode&& !waitingonAccept&& !interuptShowCase)
                {
                    GameManager.instance.acceptCombatButton.SetActive(true);
                }
                UpdateHandPositions();
            }
        }

        if (IsPointerOverUIElement("TuckedBox") && !inSettings)
        {
            tuckSensor.gameObject.SetActive(false);
            handScoller.gameObject.SetActive(true);
            tucked = false;
            GameManager.instance.phaseManager.ButtonSwitch(false);
            GameManager.instance.acceptCombatButton.SetActive(false);
            UpdateHandPositions();
        }
        if (selectingSlot)
        {
            tuckSensor.gameObject.SetActive(false);
            handScoller.gameObject.SetActive(false);
            GameManager.instance.phaseManager.ButtonSwitch(false);
        }

        if(selectingSlot && !cancelButton.activeInHierarchy)
        {
            cancelButton.SetActive(true);
        }
        if(!selectingSlot && cancelButton.activeInHierarchy)
        {
            cancelButton.SetActive(false);
            discoverButton.SetActive(false);
        }

        if (drawCards)
        {
            DrawCardFunction();
        }

        if (noShowCase)
        {
            scTimer -= Time.deltaTime;
            if (scTimer <= 0)
            {
                noShowCase = false;
            }
        }

    }

    public void SetUpDrawAmount(int amount)
    {
        //do check to see if hand size can fit that many cards
        if (amount + cardsinHand > 5)
        {
            int openslots = 5 - cardsinHand;
            if (openslots == 0) return;
            amount = openslots;
        }
        //Debug.Log("set up draw " + amount + " cards");
        previewOn = true;
        previewAmount = amount;
        drawAmount = amount;
        drawCards = true;
        drawTick = 0;
        drawTimer = 0f;
        playerReady = false;
    }

    public void DrawCardFunction()
    {
        //Debug.Log("running");
        drawTimer += Time.deltaTime;
        if (drawTick == drawAmount) //cards to draw
        {
            drawCards = false;
            drawTick = 0;
            drawAmount = 0;
            drawTimer = 0f;
        }
        if (drawTimer > 0.5f)
        {
            DrawACard();
            drawTimer = 0f;
            drawTick++;
        }
    }


    public void DrawACard()
    {
        if (GameManager.instance.inGameDeck.Count > 0)
        {
            cardsinHand++;
            if (previewAmount > 0)
            {
                cardsinPreview++;
                previewAmount--;
                //Debug.Log("Cards in preview " + cardsinPreview);
            }
            foreach(KeyValuePair<int,CardController> _card in inHandList)
            {
                if (_card.Value.positioninPreview > 0)
                {
                    _card.Value.SetTargets(FindPreviewPosition(_card.Value.positioninPreview), playerHand.transform.rotation);
                }
            }

            int cardValue = GameManager.instance.inGameDeck.Dequeue();
            CardController card = Instantiate(cardPrefab, playerHand.transform).GetComponent<CardController>();
            inHandList.Add(cardsinHand, card);
            //Debug.Log("adding card " + cardsinHand + " to the hand");
            card.transform.position = topofDeck.transform.position;
            card.transform.rotation = Quaternion.Euler(-90f, -170f, 0f);
            card.GetComponent<CardController>().cardValue = cardValue;
            card.GetComponent<CardController>().transformSpeed = 5f;
            card.GetComponent<CardController>().rotateSpeed = 3f;
            card.GetComponent<CardController>().GetValues(cardValue);
            card.GetComponent<CardController>().positioninPreview = cardsinPreview;
            card.GetComponent<CardController>().positioninHand = cardsinHand;
            card.GetComponent<CardController>().SetTargets(FindPreviewPosition(cardsinPreview), playerHand.transform.rotation);
        }
        else
        {
            previewAmount = 0;
        }
    }

    public void PlayLandFromDeck() //grabs a single card from the land deck and auto positions the card
    {
        if (GameManager.instance.inGameLands.Count > 0)
        {
            int useRow = 0;
            int hit = 0;
            for (int i = 0; i < landSlots.Length; i++)
            {
                if (!landSlots[i].gameObject.GetComponent<CardSlotController>().filled)
                {
                    useRow = i;
                    Debug.Log("selecting slot " + i);
                    i = landSlots.Length;
                }
                else
                {
                    hit++;
                }
            }
            if (hit == 4)
            {
                return;
            }

            int cardValue = GameManager.instance.inGameLands.Dequeue();
            CardController card = Instantiate(cardPrefab, playerField.transform).GetComponent<CardController>();
            card.transform.position = topofLands.transform.position;
            card.transform.rotation = Quaternion.Euler(-90f, -170f, 0f);
            card.transform.localScale = new Vector3(1.2f, 0.7f, 0.005f);
            card.GetComponent<CardController>().transformSpeed = 8f;
            card.GetComponent<CardController>().rotateSpeed = 6f;
            card.GetComponent<CardController>().inSlot = true;
            card.GetComponent<CardController>().GetValues(cardValue);
            card.GetComponent<CardController>().SetTargets(LandTarget.transform.position, playerField.transform.rotation);
            card.GetComponent<CardController>().flip = true;
            card.GetComponent<CardController>().landSlot = useRow;
            landSlots[useRow].gameObject.GetComponent<CardSlotController>().filled = true;

            NetworkTransmission.instance.LandCardbeingPlayedServerRPC(cardValue, GameManager.instance.myClientId);
        }
    }

    private void HandlePreview()
    {
        if (previewOn)
        {
            if (tuckSensor.gameObject.activeInHierarchy)
            {
                tuckSensor.gameObject.SetActive(false);
            }

            if (previewAmount == 0)
            {
                timer += Time.deltaTime;
            }

            if (timer > 0.4f)
            {
                //Debug.Log("show click to continue");
                continueScreen.SetActive(true);
                timer = 0f;
                previewOn = false;
            }
        }
    }

    private void PreviewToHand()
    {
        if (previewtoHand)
        {
            timer += Time.deltaTime;
            if (timer > 0.2f)
            {
                //Debug.Log("Cards in preview " + cardsinPreview);
                timer = 0;
                for (int i = 1; i < 6; i++)
                {
                    if (inHandList.ContainsKey(i))
                    {
                        //Debug.Log("dictionary has card" + i);
                        if (inHandList[i].positioninPreview > 0)
                        {
                            //Debug.Log("changing cards settings" + i);
                            inHandList[i].positioninPreview = 0;
                            inHandList[i].transformSpeed = 20f;

                            foreach (KeyValuePair<int, CardController> _card in inHandList)
                            {
                                if (_card.Value.positioninPreview == 0)
                                {
                                    _card.Value.SetTargets(FindHandPosition(_card.Value.positioninHand), playerHand.transform.rotation);
                                }
                            }
                            cardsinPreview--;
                            i = 6;
                        }
                    }
                    else
                    {
                        Debug.Log("no card found" + i);
                    }
                }

                if (cardsinPreview == 0)
                {
                    previewtoHand = false;
                    tuckSensor.gameObject.SetActive(true);
                }
            }
        }
    }
    private Vector3 FindPreviewPosition(int position)
    {
        Vector3 newpos = playerHand.transform.position;
        if (position < 4)
        {
            int _cardsinHand = cardsinPreview;
            if (cardsinPreview > 3)
            {
                _cardsinHand = 3;
            }
            float middle = (_cardsinHand * 10) / 2;
            float startingPoint = middle * -5.2f;
            newpos += new Vector3(startingPoint + ((52 * position)-25),50, 50f);
        }
        else
        {
            int _cardsinHand = cardsinPreview;
            if (cardsinPreview == 4) _cardsinHand = 1;
            if (cardsinPreview == 5) _cardsinHand = 2;
            if (position == 4) position = 1;
            if (position == 5) position = 2;
            float middle = (_cardsinHand * 10) / 2;
            float startingPoint = middle * -5.2f;
            newpos += new Vector3((startingPoint + (52 * position) - 25),40, 25f);
        }

        return newpos;
    }

    private Vector3 FindHandPosition(int position)
    {
        Vector3 newpos = playerHand.transform.position;
        float middle = (cardsinHand * 10) / 2;
        float spacing = 2;
        float height = 0;
        float forward = 0f;
        if (tucked)
        {
            height = position * 2 -50;
        }
        else
        {
            spacing = 5.2f;
            height = -10;
            forward = 20;
        }

        float startingPoint = middle * -spacing;
        newpos += new Vector3((startingPoint + ((spacing*10) * position)-((spacing*10)/2)), height, forward);

        return newpos;
    }

    private Vector3 FindActivePosition(int position)  
    {
        Vector3 newpos = activeSlot.transform.position;
        float forward = position*8;
        Vector3 startingPoint = newpos += new Vector3(0f, 0f, activeSlotCount*8);
        Vector3 returnPoint = startingPoint -= new Vector3(0f, 0f, forward);

        return returnPoint;
    }

    private void UpdateHandPositions()
    {
        for(int i = 1; i < 6; i++)
        {
            if (inHandList.ContainsKey(i))
            {
                if (!inHandList[i].isBeingDragged)
                {
                    if (tucked)
                    {
                        //inHandList[i].inSlot = true;
                    }
                    else
                    {
                        //inHandList[i].inSlot = false;
                    }
                    inHandList[i].SetTargets(FindHandPosition(inHandList[i].positioninHand), playerHand.transform.rotation);
                }
            }
        }
    }

    private void UpdateActivePositions()  
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        Dictionary<int,GameObject> activelist = new Dictionary<int,GameObject>();
        foreach(GameObject card in cards)
        {
            if (card.GetComponent<CardController>().row == 5  && card.GetComponent<CardController>().inSlot && card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
            {
                activelist.Add(card.GetComponent<CardController>().activeSlot,card);
            }
        }
        int pulldown = 0;
        for(int i = 0; i < activeSlotCount+1; i++)
        {
            if(activelist.ContainsKey(i))
            {
                Debug.Log("found + " + i + " in the dictionary");
                GameObject card = activelist[i];
                card.GetComponent<CardController>().activeSlot += pulldown;
                card.GetComponent<CardController>().SetTargets(FindActivePosition(card.GetComponent<CardController>().activeSlot), activeSlot.transform.rotation);
            }
            else
            {
                Debug.Log("did not find " + i + " in the dictionary");
                pulldown++;
            }
        }
    }

    private void HandleMouse()
    {
        if (playerReady)
        {
            if (!cardSelected)
            {
                cardShowCase.SetActive(false);
            }
            if (!noShowCase && !waitingonAccept) // Hovering over a card
            {
                Ray _ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit _hit;
                if (Physics.Raycast(_ray, out _hit))
                {
                    if (_hit.transform.tag == "Card")
                    {
                        CardController card = _hit.transform.GetComponent<CardController>();
                        if (card.inSlot)
                        {
                            cardShowCase.SetActive(true);
                            UpdateCardShowCase(card.cardValue);

                            if (!cardSelected)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    cardShowCaseImages[i].sprite = ImageManager.instance.cardFaces[_hit.transform.GetComponent<CardController>().cardValue];
                                }
                            }
                        }
                    }
                }
            }
            if (Input.GetMouseButtonDown(0) && !waitingonAccept)  //selecting a card in your hand
            {
                Vector3 mousepos = Input.mousePosition;
                mousepos.z = 1000f;
                mousepos = mainCam.ScreenToWorldPoint(mousepos);
                Debug.DrawRay(mainCam.transform.position, mousepos - mainCam.transform.position, Color.blue);

                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.tag == "DiscardOption")
                    {
                        Debug.Log("discarding card " + selectedCard.slot);
                        selectedCard.highLight = false;
                        cardSelected = false;
                        discardOption.SetActive(false);
                        NetworkTransmission.instance.PlayerDiscardingaCardinPlayServerRPC(selectedCard.row,selectedCard.slot, GameManager.instance.myClientId);
                    }
                    else
                    {
                        if (selectedCard != null && !IsPointerOverUIElement("ShowCase"))
                        {
                            DeselectCard();
                        }
                    }
                    if (hit.transform.tag == "Card")
                    {
                        CardController card = hit.transform.GetComponent<CardController>();
                        if ((!card.opponentsHand && !attackMode && GameManager.instance.playersTurn == myPlayerTurn&&GameManager.instance.mymoves>0)||(GameManager.instance.playersTurn != myPlayerTurn && !attackMode && playOppurtunity && IsCardanInterupt(card.cardValue)) || (GameManager.instance.playersTurn == myPlayerTurn && attackMode && playOppurtunity && IsCardanInterupt(card.cardValue)))
                        {
                            if (!card.inSlot)
                            {
                                if (DeckManager.instance.cardLibrary[card.cardValue].type == "Active")
                                {
                                    mouseCard = card;
                                    zCord = mainCam.WorldToScreenPoint(mouseCard.gameObject.transform.position).z;
                                    dragOffset = mouseCard.gameObject.transform.position - GetMouseWorldPos();
                                    card.isBeingDragged = true;
                                    playZone.gameObject.SetActive(true);
                                    discardZone.gameObject.SetActive(true);
                                }
                                else
                                {
                                    mouseCard = card;
                                    zCord = mainCam.WorldToScreenPoint(mouseCard.gameObject.transform.position).z;
                                    dragOffset = mouseCard.gameObject.transform.position - GetMouseWorldPos();
                                    card.isBeingDragged = true;
                                    if (SetSelections(true, DeckManager.instance.cardLibrary[card.cardValue].subType == "Army") > 0 || (DeckManager.instance.cardLibrary[card.cardValue].subType.Contains("Explorer") && GameManager.instance.inGameLands.Count > 0))
                                    {
                                        playZone.gameObject.SetActive(true);
                                    }
                                    SetSelections(false, false);
                                    discardZone.gameObject.SetActive(true);
                                    //cardZoneUI.SetActive(true);
                                }
                            }
                            else
                            { //clicking on a card in play
                                Debug.Log("clicking on a card");
                                if (!noShowCase)
                                {
                                    if ((!card.opponent && GameManager.instance.playersTurn == myPlayerTurn)|| (GameManager.instance.playersTurn != myPlayerTurn && !attackMode && playOppurtunity && IsCardanInterupt(card.cardValue)))
                                    {
                                        selectedCard = card;
                                        card.highLight = true;
                                        cardSelected = true;
                                        if (DeckManager.instance.cardLibrary[card.cardValue].type != "Land" && GameManager.instance.playersTurn == myPlayerTurn && !interuptShowCase)
                                        {
                                            discardOption.SetActive(true);
                                        }
                                        
                                        for (int i = 0; i < 3; i++)
                                        {
                                            cardShowCaseImages[i].sprite = ImageManager.instance.cardFaces[card.cardValue];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (hit.transform.tag == "Row")
                    {
                        if (!attackMode)
                        {
                            int row = hit.transform.GetComponent<RowController>().row;
                            selectingSlot = false;
                            bool op = playOppurtunity;
                            if (DeckManager.instance.cardLibrary[playCard.cardValue].precast)
                            {
                                //the player must do something before casting this card
                                Debug.Log("sending cards to selector screen");
                                SetUpCardSelection(AbilityTable.CheckAtPreCast(DeckManager.instance.cardLibrary[playCard.cardValue].cardName),0);
                            }
                            else
                            {
                                if (playOppurtunity)
                                {
                                    Debug.Log("playing a card in the oppurtunity moment");
                                    PreviewContinueButton(true);//aceepts the previous play and then send a play of our own
                                }
                                PasspriorityScreen.SetActive(false);
                                NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, row, FindSlot(row), "", op, GameManager.instance.myClientId);
                                playOppurtunity = false;
                            }
                            SetSelections(false, false);
                        }
                        else
                        {
                            if (!playOppurtunity)
                            {
                                if (attackingRow == -1)
                                {
                                    Debug.Log("picking row to attack with");
                                    GameManager.instance.phaseManager.ButtonSwitch(false);
                                    tuckSensor.gameObject.SetActive(false);
                                    SetSelections(false, false);
                                    attackingRow = hit.transform.GetComponent<RowController>().row;
                                    attackSelectors[attackingRow].SetActive(true);
                                    //highlight the row then show viable targets that the opponent has.
                                    OpponentsHand.instance.SetSelectionsForAttack(true);
                                }
                                else
                                {
                                    OpponentsRow = hit.transform.GetComponent<RowController>().row;
                                    OpponentsHand.instance.attackSelectors[OpponentsRow].SetActive(true);
                                    Debug.Log("initiate attack with this row " + OpponentsRow);
                                    OpponentsHand.instance.SetSelections(false);
                                    NetworkTransmission.instance.PlayerisAttackingServerRPC(GameManager.instance.myClientId, attackingRow, OpponentsRow);
                                }
                            }
                            else
                            {
                                int row = hit.transform.GetComponent<RowController>().row;
                                selectingSlot = false;
                                bool op = playOppurtunity;
                                if (playOppurtunity)
                                {
                                    Debug.Log("playing a card in the oppurtunity moment");
                                    PreviewContinueButton(true);//aceepts the previous play and then send a play of our own
                                }
                                PasspriorityScreen.SetActive(false);
                                NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, row, FindSlot(row), "", op, GameManager.instance.myClientId);
                                playOppurtunity = false;
                                SetSelections(false, false);
                            }
                        }
                    }
                }
            }
            if (Input.GetMouseButtonUp(0) && !selectingSlot)
            {
                if (mouseCard != null)
                {
                    if (IsPointerOverUIElement("PlayZone"))
                    {
                        playCard = mouseCard;
                        SelectionScreen(playCard); //give the player a choice if there is one or just plays the card in the only slot it can be played
                        GameManager.instance.phaseManager.ButtonSwitch(false);
                    }
                    if (IsPointerOverUIElement("DiscardZone"))
                    {
                        mouseCard.movetoSpot = false;
                        NetworkTransmission.instance.PlayerDiscardingaCardServerRPC(mouseCard.cardValue, GameManager.instance.myClientId);
                        //PlayCard(mouseCard);
                    }
                    mouseCard.isBeingDragged = false;
                    UpdateHandPositions();
                    mouseCard = null;
                    playZone.gameObject.SetActive(false);
                    discardZone.gameObject.SetActive(false);
                }
            }
            if (mouseCard != null)
            {
                mouseCard.SetTargets(GetMouseWorldPos() + dragOffset,playerHand.transform.rotation);
            }
        }
    }

    public void SelectionScreen(CardController card)
    {
        /*
        ShuffleDictionaryDown(card.positioninHand);
        Destroy(card.gameObject);
        */
        scTimer = 1f;
        noShowCase = true;
        ShuffleDictionaryDown(card.positioninHand);
        card.SetTargets(playTarget.transform.position, playTarget.transform.rotation);
        card.inSlot = true;
        if (inattackMode)
        {
            GameManager.instance.acceptCombatButton.SetActive(false);
        }
        string cardType = DeckManager.instance.cardLibrary[card.cardValue].type;
        switch (cardType)
        {
            case "Character":
                Debug.Log("trying to play a character");
                selectingSlot = true;
                //do check first to see if only 1 slot if present or if character is an explorer
                int slots = SetSelections(true, DeckManager.instance.cardLibrary[card.cardValue].subType == "Army");
                if (DeckManager.instance.cardLibrary[card.cardValue].subType.StartsWith("Explorer"))
                {
                    if (slots >= 1)
                    {
                        Debug.Log("its an explorer give the option to play land");
                        if (GameManager.instance.inGameLands.Count > 0 && !CheckLandSlots())
                        {
                            discoverButton.SetActive(true);
                        }
                        if (playCard == null)
                        {
                            Debug.Log("playcard is null");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (!landSlots[i].GetComponent<CardSlotController>().filled)
                            {
                                //tell network to play card here with a land
                                tuckSensor.gameObject.SetActive(false);
                                PasspriorityScreen.SetActive(false);
                                NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, i,FindSlot(i),"Land",playOppurtunity, GameManager.instance.myClientId);
                                i = 4;
                            }

                        }
                    }
                }
                //Debug.Log("open slots " + slots);

                //show slot selection screen
                    break;
            case "Active":
                Debug.Log("playing an active"); //no selection just go straight to active slots


                ///this will change later depending if a selection needs to be made
                if (playOppurtunity)
                {
                    PasspriorityScreen.SetActive(false);
                    Debug.Log("playing a card in the oppurtunity moment" + playOppurtunity);
                    PreviewContinueButton(true); //aceepts the previous play and then send a play of our own
                    NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, 5,activeSlotCount, "",true, GameManager.instance.myClientId);
                }
                else
                {
                    PasspriorityScreen.SetActive(false);
                    NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, 5, activeSlotCount, "", false, GameManager.instance.myClientId);
                }
                tuckSensor.gameObject.SetActive(false);
                playOppurtunity = false;
                break;
        }
    }

    public void AttackSelection()
    {
        noShowCase = true;
        SetSelectionsForAttack(true);
    }

    public int SetSelections(bool _set,bool army) //turns off all rows or turn rows on depending on avaliable slots
    {
        int open = 0;
        if (_set)
        {
            for (int i = 0; i < 4; i++)
            {
                if (landSlots[i].gameObject.GetComponent<CardSlotController>().filled)
                {
                    switch (i)
                    {
                        case 0:
                            if (!row1Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                if (army)
                                {
                                    if (!RowContainArmy(i))
                                    {
                                        open++;
                                        rowSelectors[i].SetActive(_set);
                                    }
                                }
                                else
                                {
                                    open++;
                                    rowSelectors[i].SetActive(_set);
                                }
                            }
                            break;
                        case 1:
                            if (!row2Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                if (army)
                                {
                                    if (!RowContainArmy(i))
                                    {
                                        open++;
                                        rowSelectors[i].SetActive(_set);
                                    }
                                }
                                else
                                {
                                    open++;
                                    rowSelectors[i].SetActive(_set);
                                }
                            }
                            break;
                        case 2:
                            if (!row3Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                if (army)
                                {
                                    if (!RowContainArmy(i))
                                    {
                                        open++;
                                        rowSelectors[i].SetActive(_set);
                                    }
                                }
                                else
                                {
                                    open++;
                                    rowSelectors[i].SetActive(_set);
                                }
                            }
                            break;
                        case 3:
                            if (!row4Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                if (army)
                                {
                                    if (!RowContainArmy(i))
                                    {
                                        open++;
                                        rowSelectors[i].SetActive(_set);
                                    }
                                }
                                else
                                {
                                    open++;
                                    rowSelectors[i].SetActive(_set);
                                }
                            }
                            break;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                rowSelectors[i].SetActive(_set);
            }
        }                    

        return open;
    }
    public int SetSelectionsForAttack(bool _set) //turns off all rows or turn rows on depending on avaliable slots
    {
        int open = 0;
        if (_set)
        {
            for (int i = 0; i < 4; i++)
            {
                if (landSlots[i].gameObject.GetComponent<CardSlotController>().filled)
                {
                    switch (i)
                    {
                        case 0:
                            if (row1Slots[0].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 1:
                            if (row2Slots[0].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 2:
                            if (row3Slots[0].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 3:
                            if (row4Slots[0].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                rowSelectors[i].SetActive(_set);
            }
        }

        return open;
    }

    private bool RowContainArmy(int row)
    {
        bool set = false;
        switch (row)
        {
            case 0:
                for(int i = 0; i < 4; i++)
                {
                    if (row1Slots[i].GetComponent<CardSlotController>().subType == "Army")
                    {
                        set = true;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < 4; i++)
                {
                    if (row2Slots[i].GetComponent<CardSlotController>().subType == "Army")
                    {
                        set = true;
                    }
                }
                break;
            case 2:
                for (int i = 0; i < 4; i++)
                {
                    if (row3Slots[i].GetComponent<CardSlotController>().subType == "Army")
                    {
                        set = true;
                    }
                }
                break;
            case 3:
                for (int i = 0; i < 4; i++)
                {
                    if (row4Slots[i].GetComponent<CardSlotController>().subType == "Army")
                    {
                        set = true;
                    }
                }
                break;
        }

        return set;
    }

    public void UpdateRowSlots()
    {
        for(int i = 0; i < 4; i++)
        {
            row1Slots[i].SetActive(false);
            row2Slots[i].SetActive(false);
            row3Slots[i].SetActive(false);
            row4Slots[i].SetActive(false);
        }
        for(int i = 0; i < 4; i++)
        {
            if (row1Slots[i].GetComponent<CardSlotController>().filled)
            {
                    row1Slots[i].SetActive(true);
            }
            else
            {
                    row1Slots[i].SetActive(true);
                    i = 5;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (row2Slots[i].GetComponent<CardSlotController>().filled)
            {
                row2Slots[i].SetActive(true);
            }
            else
            {
                row2Slots[i].SetActive(true);
                i = 5;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (row3Slots[i].GetComponent<CardSlotController>().filled)
            {
                row3Slots[i].SetActive(true);
            }
            else
            {
                row3Slots[i].SetActive(true);
                i = 5;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (row4Slots[i].GetComponent<CardSlotController>().filled)
            {
                row4Slots[i].SetActive(true);
            }
            else
            {
                row4Slots[i].SetActive(true);
                i = 5;
            }
        }
    }

    public void PlayCard(int row, string ability)  
    {
        if (playCard != null)
        {
            //GameManager.instance.phaseManager.ButtonSwitch(true);
            int useSlot = FindSlot(row);
            if (DeckManager.instance.cardLibrary[playCard.cardValue].subType == "Army")
            {
                //shuffle things upwards
                ShuffleRowForArmy(row);
                useSlot = 0;
            }
            else
            {
                useSlot = CheckForExplorer(row, useSlot);
            }

            if (ability != "")
            {
                playCard.triggerAbility = true;
                playCard.abilityString = ability;
            }

            //Debug.Log("playing card in row " + row + " in slot " + useSlot);
            playCard.GetComponent<CardController>().cardOwner = GameManager.instance.myClientId;
            playCard.transform.parent = playerField.transform;
            playCard.transform.localScale = new Vector3(1.2f, 0.7f, 0.005f);
            playCard.GetComponent<CardController>().transformSpeed = 8f;
            playCard.GetComponent<CardController>().rotateSpeed = 6f;
            playCard.GetComponent<CardController>().inSlot = true;
            playCard.GetComponent<CardController>().row = row;
            playCard.GetComponent<CardController>().slot = useSlot;
            switch (row)
            {
                case 0:
                    playCard.GetComponent<CardController>().SetTargets(row1Slots[useSlot].transform.position, row1Slots[useSlot].transform.rotation);
                    row1Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                    row1Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[playCard.cardValue].subType;
                    break;
                case 1:
                    playCard.GetComponent<CardController>().SetTargets(row2Slots[useSlot].transform.position, row2Slots[useSlot].transform.rotation);
                    row2Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                    row2Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[playCard.cardValue].subType;
                    break;
                case 2:
                    playCard.GetComponent<CardController>().SetTargets(row3Slots[useSlot].transform.position, row3Slots[useSlot].transform.rotation);
                    row3Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                    row3Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[playCard.cardValue].subType;
                    break;
                case 3:
                    playCard.GetComponent<CardController>().SetTargets(row4Slots[useSlot].transform.position, row4Slots[useSlot].transform.rotation);
                    row4Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                    row4Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[playCard.cardValue].subType;
                    break;
                case 5:
                    playCard.GetComponent<CardController>().activeSlot = activeSlotCount;
                    activeSlotCount++;
                    UpdateActivePositions();
                    break;
            }
            tuckSensor.gameObject.SetActive(true);
            if (inattackMode)
            {
                GameManager.instance.acceptCombatButton.SetActive(true);
            }
            Debug.Log("trigger");
            SetSelections(false, false);
            playCard = null;
            selectingSlot = false;
            UpdateRowSlots();
        }
    }

    public void ShowCardForPreview(int cardValue, Vector3 pos)
    {
        CardController card = Instantiate(cardPrefab, playerField.transform).GetComponent<CardController>();
        card.transform.position = pos;
        card.transform.rotation = Quaternion.Euler(-90f, -170f, 0f);
        card.transform.localScale = new Vector3(1.2f, 0.7f, 0.005f);
        card.isPreviewCard = true;
        card.GetComponent<CardController>().transformSpeed = 8f;
        card.GetComponent<CardController>().rotateSpeed = 6f;
        card.GetComponent<CardController>().inSlot = true;
        card.GetComponent<CardController>().GetValues(cardValue);
        card.GetComponent<CardController>().SetTargets(previewTarget.transform.position, previewTarget.transform.rotation);
    }


    public void SendCardsToAttackZone(int row, int opRow, bool local)
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            if ((card.GetComponent<CardController>().row == row && card.GetComponent<CardController>().opponent != local) ||
                (card.GetComponent<CardController>().row == opRow && card.GetComponent<CardController>().opponent == local))
            {
                card.GetComponent<CardController>().transform.parent = attackTarget.transform;
                float x = 0;
                if (card.GetComponent<CardController>().opponent)
                {
                    x = 60;
                }
                else
                {
                    x = -60;
                }
                float y = card.GetComponent<CardController>().slot * 23;
                Vector3 displace = new Vector3(x, y, y + (card.GetComponent<CardController>().slot * 6));
                card.GetComponent<CardController>().SetTargets(attackTarget.transform.position + displace, attackTarget.transform.rotation);
                card.GetComponent<CardController>().inAttackPreview = true;
            }
            if ((card.GetComponent<CardController>().landSlot == row && card.GetComponent<CardController>().slot == -1 && card.GetComponent<CardController>().inSlot && card.GetComponent<CardController>().opponent != local) ||
                (card.GetComponent<CardController>().landSlot == opRow && card.GetComponent<CardController>().slot == -1 && card.GetComponent<CardController>().inSlot && card.GetComponent<CardController>().opponent == local))
            {
                card.GetComponent<CardController>().transform.parent = attackTarget.transform;
                float x = 0;
                if (card.GetComponent<CardController>().opponent)
                {
                    x = 130;
                }
                else
                {
                    x = -130;
                }
                float y = 20;
                Vector3 displace = new Vector3(x, y, y - 10);
                card.GetComponent<CardController>().SetTargets(attackTarget.transform.position + displace, attackTarget.transform.rotation);
                card.GetComponent<CardController>().inAttackPreview = true;
            }
        }
    }

    public void SendCardsBackToHome()  
    {
        Debug.Log("sending cards back");
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            if (card.GetComponent<CardController>().inAttackPreview|| card.GetComponent<CardController>().inSearch)
            {
                int row = card.GetComponent<CardController>().row;
                int useSlot = card.GetComponent<CardController>().slot;
                int cardValue = card.GetComponent<CardController>().cardValue;
                if (!card.GetComponent<CardController>().opponent)
                {
                    card.GetComponent<CardController>().inAttackPreview = false;
                    card.transform.parent = playerField.transform;
                    switch (row)
                    {
                        case -1:
                            card.GetComponent<CardController>().SetTargets(landSlots[card.GetComponent<CardController>().landSlot].transform.position, landSlots[card.GetComponent<CardController>().landSlot].transform.rotation);
                            break;
                        case 0:
                            card.GetComponent<CardController>().SetTargets(row1Slots[useSlot].transform.position, row1Slots[useSlot].transform.rotation);
                            row1Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                            row1Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[cardValue].subType;
                            break;
                        case 1:
                            card.GetComponent<CardController>().SetTargets(row2Slots[useSlot].transform.position, row2Slots[useSlot].transform.rotation);
                            row2Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                            row2Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[cardValue].subType;
                            break;
                        case 2:
                            card.GetComponent<CardController>().SetTargets(row3Slots[useSlot].transform.position, row3Slots[useSlot].transform.rotation);
                            row3Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                            row3Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[cardValue].subType;
                            break;
                        case 3:
                            card.GetComponent<CardController>().SetTargets(row4Slots[useSlot].transform.position, row4Slots[useSlot].transform.rotation);
                            row4Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                            row4Slots[useSlot].GetComponent<CardSlotController>().subType = DeckManager.instance.cardLibrary[cardValue].subType;
                            break;
                    }
                }
                //if(card.GetComponent<CardController>().landSlot)
            }
        }
    }

    public void PreviewContinueButton(bool interupt)  //this is the continue button to accept a played card
    {
        Debug.Log(playOppurtunity);
        NetworkTransmission.instance.AcceptPlayServerRPC(previewCard.cardValue,previewCard.row,previewCard.cardAbility,interuptShowCase,interupt, GameManager.instance.myClientId);
        interuptShowCase = false;
        cardbeingPlayedScreen.SetActive(false);
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach(GameObject card in cards)
        {
            if (card.GetComponent<CardController>().isPreviewCard)
            {
                Debug.Log("card found");
                Destroy(card);
            }
        }
    }

    public void PassPriorityButton()
    {
        if (cardbeingPlayedScreen.activeInHierarchy)
        {
            interuptShowCase = false;
            PreviewContinueButton(false);
        }
        NetworkTransmission.instance.PassingPriorityServerRPC(GameManager.instance.myClientId);
    }

    public void AcceptCombatButton() //player accepts the opponents attack dicision and combat screen pulls up
    {
        GameManager.instance.acceptCombatButton.SetActive(false);
        playOppurtunity = false;
        inattackMode = false;
        NetworkTransmission.instance.FinalizeTheAttackServerRPC(GameManager.instance.myClientId,attackingRow,OpponentsRow);
    }

    public void CombatContinueButton() //after the combat screen pulls up the play will hit the continue button to reset the screen
    {
        GameManager.instance.continueCombatButton.SetActive(false);
        GameManager.instance.attackShowcase.SetActive(false);
        PlayerControls.instance.SendCardsBackToHome();
        OpponentsHand.instance.SendCardsBackToRows();
        ClearAttackRows();
        OpponentsHand.instance.ClearAttackRows();
        NetworkTransmission.instance.AcceptCombatServerRPC(GameManager.instance.myClientId);
    }

    public void FinishCombatButton()
    {
    }
    
    public void CardisBeingPlayed(int cardValue,int row,int slot,string ability,bool interupt) //this activates the continue button for card showcase
    {
        playOppurtunity = true;
        if (interupt)
        {
            Debug.Log("player has played an interrupt");
            interuptShowCase = true; //the next is accept for an interupt
            //GameManager.instance.phaseManager.ButtonSwitch(false);
            tuckSensor.gameObject.SetActive(true);
        }
        cardbeingPlayedScreen.SetActive(true);
        if (inattackMode)
        {
            GameManager.instance.acceptCombatButton.SetActive(false);
        }
        previewCard = new PreviewCard() { cardValue = cardValue, cardAbility = ability, row = row,slot = slot };
    }

    private int FindSlot(int row)
    {
        int slot = 0;
        switch (row)
        {
            case 0:
                for (int i = 0; i < 4; i++)
                {
                    if (!row1Slots[i].GetComponent<CardSlotController>().filled)
                    {
                        slot = i;
                        i = 5;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < 4; i++)
                {
                    if (!row2Slots[i].GetComponent<CardSlotController>().filled)
                    {
                        slot = i;
                        i = 5;
                    }
                }
                break;
            case 2:
                for (int i = 0; i < 4; i++)
                {
                    if (!row3Slots[i].GetComponent<CardSlotController>().filled)
                    {
                        slot = i;
                        i = 5;
                    }
                }
                break;
            case 3:
                for (int i = 0; i < 4; i++)
                {
                    if (!row4Slots[i].GetComponent<CardSlotController>().filled)
                    {
                        slot = i;
                        i = 5;
                    }
                }
                break;
            case 5:
                slot = activeSlotCount;
                break;
        }

        return slot;
    }

    public void CancelPlay()
    {
        selectingSlot = false;
        cardsinHand++;
        inHandList.Add(cardsinHand, playCard);
        Debug.Log("adding card " + cardsinHand + " to the hand");
        playCard.positioninHand = cardsinHand;
        playCard.inSlot = false;
        playCard.SetTargets(FindHandPosition(cardsinHand), playerHand.transform.rotation);
        playCard = null;
        Debug.Log("trigger");
        tuckSensor.gameObject.SetActive(true);
        discoverButton.SetActive(false);
        SetSelections(false,false);
        //send card back to their posisitons aswel.
        cardSelectionScreen.SetActive(false);
    }

    public void DiscardCard(CardController card)
    {
        ShuffleDictionaryDown(card.positioninHand);
        card.discard = true;
        card.useSlerp = true;
        card.movetoSpot = true;
        card.SetTargets(discardPreview.transform.position, discardPreview.transform.rotation,new Vector3(1.2f, 0.7f, 0.005f));
        card.scaleSpeed = 10f;
    }

    public void DiscardCardinPlay(int row,int slot) //gets called when a player selects the discard option
    {
        CardController card = FindCardinPlay(row, slot,GameManager.instance.myClientId);
        card.useSlerp = false;
        card.movetoSpot = true;
        card.transformSpeed = 8f;
        card.rotateSpeed = 6f;
        card.scaleSpeed = 10f;
        card.discardFlip = true;
        card.inSlot = false;
        ShuffleRowDown(row, slot);
        card.SetTargets(discardTarget.transform.position, discardTarget.transform.rotation, new Vector3(1.2f, 0.7f, 0.005f));
    }

    private void ShuffleDictionaryDown(int position)
    {
        inHandList.Remove(position);
        cardsinHand--;
        for(int i = 1; i < 6; i++)
        {
            if (inHandList.ContainsKey(i))
            {
                if(i > position)
                {
                    CardController card = inHandList[i];
                    inHandList.Remove(i);
                    card.positioninHand--;
                    inHandList.Add(card.positioninHand, card);
                }
            }
        }

        UpdateHandPositions();
    }

    private void ShuffleRowDown(int row, int slot)
    {
        Debug.Log("shuffle row down player");
        for (int i = slot; i < 4; i++)
        {
            switch (row)
            {
                case 0:
                    row1Slots[i].GetComponent<CardSlotController>().filled = false;
                    break;
                case 1:
                    row2Slots[i].GetComponent<CardSlotController>().filled = false;
                    break;
                case 2:
                    row3Slots[i].GetComponent<CardSlotController>().filled = false;
                    break;
                case 3:
                    row4Slots[i].GetComponent<CardSlotController>().filled = false;
                    break;
            }
        }
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject _card in cards)
        {
            if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot > slot && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
            {
                int useSlot = _card.GetComponent<CardController>().slot - 1;
                _card.GetComponent<CardController>().slot = useSlot;
                switch (row)
                {
                    case 0:
                        _card.GetComponent<CardController>().SetTargets(row1Slots[useSlot].transform.position, row1Slots[useSlot].transform.rotation);
                        row1Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                        break;
                    case 1:
                        _card.GetComponent<CardController>().SetTargets(row2Slots[useSlot].transform.position, row2Slots[useSlot].transform.rotation);
                        row2Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                        break;
                    case 2:
                        _card.GetComponent<CardController>().SetTargets(row3Slots[useSlot].transform.position, row3Slots[useSlot].transform.rotation);
                        row3Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                        break;
                    case 3:
                        _card.GetComponent<CardController>().SetTargets(row4Slots[useSlot].transform.position, row4Slots[useSlot].transform.rotation);
                        row4Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                        break;

                    case 5:
                        _card.GetComponent<CardController>().activeSlot--;
                        break;
                }
            }
        }
        if (row == 5)
        {
            activeSlotCount--;
            UpdateActivePositions();
        }
    }

    public void ClearAttackRows()
    {
        for (int i = 0; i < 4; i++)
        {
            attackSelectors[i].SetActive(false);
        }
    }

    private int CheckForExplorer(int row, int useSlot)
    {
        //Debug.Log("doing an explorer check");
        int slot = useSlot;
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject _card in cards)
        {
            if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == slot-1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
            {
                if (DeckManager.instance.cardLibrary[_card.GetComponent<CardController>().cardValue].subType.StartsWith("Explorer"))
                {
                    MoveCardUpaSlot(_card.GetComponent<CardController>());
                    slot--;
                }
            }
        }

        return slot;
    }

    private bool CheckLandSlots()
    {
        for(int i = 0; i < 4; i++)
        {
            if (!landSlots[i].GetComponent<CardSlotController>().filled)
            {
                return false;
            }
        }
        return true;
    }

    private void MoveCardUpaSlot(CardController _playCard)
    {
        Debug.Log("moving card up a slot");
        int row = _playCard.row;
        int useSlot = _playCard.slot +1;
        switch (row)
        {
            case 0:
                _playCard.GetComponent<CardController>().SetTargets(row1Slots[useSlot].transform.position, row1Slots[useSlot].transform.rotation);
                row1Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                break;
            case 1:
                _playCard.GetComponent<CardController>().SetTargets(row2Slots[useSlot].transform.position, row2Slots[useSlot].transform.rotation);
                row2Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                break;
            case 2:
                _playCard.GetComponent<CardController>().SetTargets(row3Slots[useSlot].transform.position, row3Slots[useSlot].transform.rotation);
                row3Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                break;
            case 3:
                _playCard.GetComponent<CardController>().SetTargets(row4Slots[useSlot].transform.position, row4Slots[useSlot].transform.rotation);
                row4Slots[useSlot].GetComponent<CardSlotController>().filled = true;
                break;
        }
        _playCard.slot = useSlot;
    }

    public void DiscoverLandButton()
    {
        discoverButton.SetActive(false);
        selectingSlot = false;
        SetSelections(false, false);
        for (int i = 0; i < 4; i++)
        {
            if (!landSlots[i].GetComponent<CardSlotController>().filled)
            {
                if (playCard == null)
                {
                    Debug.Log("playcard is null");
                }
                else
                {
                    NetworkTransmission.instance.PlayerPlayingACardServerRPC(playCard.cardValue, i,FindSlot(i), "Land",false, GameManager.instance.myClientId);
                    i = 5;
                }
                //tell network to play card here with a land
            }
        }
    }

    private void ShuffleRowForArmy(int row)
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        switch (row)
        {
            case 0:
                if (row1Slots[1].GetComponent<CardSlotController>().filled)
                {
                    row1Slots[2].GetComponent<CardSlotController>().subType = row1Slots[1].GetComponent<CardSlotController>().subType;
                    row1Slots[2].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot&&_card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }
                }
                if (row1Slots[0].GetComponent<CardSlotController>().filled)
                {
                    row1Slots[1].GetComponent<CardSlotController>().subType = row1Slots[0].GetComponent<CardSlotController>().subType;
                    row1Slots[1].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }

                    row1Slots[0].GetComponent<CardSlotController>().filled = false;
                    row1Slots[0].GetComponent<CardSlotController>().subType = "";
                }
                break;
            case 1:
                if (row2Slots[1].GetComponent<CardSlotController>().filled)
                {
                    row2Slots[2].GetComponent<CardSlotController>().subType = row2Slots[1].GetComponent<CardSlotController>().subType;
                    row2Slots[2].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }
                }
                if (row2Slots[0].GetComponent<CardSlotController>().filled)
                {
                    row2Slots[1].GetComponent<CardSlotController>().subType = row2Slots[0].GetComponent<CardSlotController>().subType;
                    row2Slots[1].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }

                    row2Slots[0].GetComponent<CardSlotController>().filled = false;
                    row2Slots[0].GetComponent<CardSlotController>().subType = "";
                }
                break;
            case 2:
                if (row3Slots[1].GetComponent<CardSlotController>().filled)
                {
                    row3Slots[2].GetComponent<CardSlotController>().subType = row3Slots[1].GetComponent<CardSlotController>().subType;
                    row3Slots[2].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }
                }
                if (row3Slots[0].GetComponent<CardSlotController>().filled)
                {
                    row3Slots[1].GetComponent<CardSlotController>().subType = row3Slots[0].GetComponent<CardSlotController>().subType;
                    row3Slots[1].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }

                    row3Slots[0].GetComponent<CardSlotController>().filled = false;
                    row3Slots[0].GetComponent<CardSlotController>().subType = "";
                }
                break;
            case 3:
                if (row4Slots[1].GetComponent<CardSlotController>().filled)
                {
                    row4Slots[2].GetComponent<CardSlotController>().subType = row4Slots[1].GetComponent<CardSlotController>().subType;
                    row4Slots[2].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }
                }
                if (row4Slots[0].GetComponent<CardSlotController>().filled)
                {
                    row4Slots[1].GetComponent<CardSlotController>().subType = row4Slots[0].GetComponent<CardSlotController>().subType;
                    row4Slots[1].GetComponent<CardSlotController>().filled = true;
                    foreach (GameObject _card in cards)
                    {
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                        {
                            MoveCardUpaSlot(_card.GetComponent<CardController>());
                        }
                    }

                    row4Slots[0].GetComponent<CardSlotController>().filled = false;
                    row4Slots[0].GetComponent<CardSlotController>().subType = "";
                }
                break;
        }
    }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCord;
        return mainCam.ScreenToWorldPoint(mousePoint);
    }
    public void ClearHand()
    {
        inHandList = new Dictionary<int, CardController>();
    }

    public void ClearSlots()
    {
        for(int i = 0; i < 4; i++)
        {
            landSlots[i].GetComponent<CardSlotController>().filled = false;
        }
        for(int y = 0; y < 4; y++)
        {
            row1Slots[y].GetComponent<CardSlotController>().filled = false;
            row2Slots[y].GetComponent<CardSlotController>().filled = false;
            row3Slots[y].GetComponent<CardSlotController>().filled = false;
            row4Slots[y].GetComponent<CardSlotController>().filled = false;
            row1Slots[y].GetComponent<CardSlotController>().subType = "";
            row2Slots[y].GetComponent<CardSlotController>().subType = "";
            row3Slots[y].GetComponent<CardSlotController>().subType = "";
            row4Slots[y].GetComponent<CardSlotController>().subType = "";
        }
        SetSelections(false, false);
    }

    public void DeselectCard()
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject card in cards)
        {
            card.GetComponent<CardController>().highLight = false;
        }
        cardSelected = false;
        selectedCard = null;
        discardOption.SetActive(false);
    }

    private CardController FindCardinPlay(int row,int slot,ulong cliendId)
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject _card in cards)
        {
            if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == slot && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == cliendId)
            {
                return _card.GetComponent<CardController>();
            }
        }

        return null;
    }

    private void SetUpCardSelection(int search,int row)
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        switch (search)
        {
            case 1: //search opponents field of non lands and non actives
                foreach (GameObject _card in cards)
                {
                    if (_card.GetComponent<CardController>().row == row &&_card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == GameManager.instance.myClientId)
                    {
                        searchList.Add(_card);
                    }
                }
                break;
        }

        foreach (GameObject _card in searchList)
        {
           // _card.GetComponent<CardController>().row
           //send card to selection screen
        }
        cardSelectionScreen.SetActive(true);
    }

    private bool IsCardanInterupt(int value)
    {
        bool set = false;
        switch(DeckManager.instance.cardLibrary[value].abilityType)
        {
            case 1:
                set = true;
                break;
            case 4:
                set = true;
                break;
            case 5:
                set = true;
                break;
        }
        return set;
    }

    private void UpdateCardShowCase(int cardValue)
    {
        if (DeckManager.instance.cardLibrary[cardValue].abilityType == 2)
        {
            activateButton.SetActive(true);
        }
        else
        {
            activateButton.SetActive(false);
        }


    }
    public void EndTurn()
    {
        NetworkTransmission.instance.PlayerEndingTurnServerRPC(GameManager.instance.myClientId);
    }

    public void SettingsSwitch(bool set)
    {
        inSettings = set;
    }

    public bool IsPointerOverUIElement(string tag)
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults(),tag);
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults,string tag)
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
