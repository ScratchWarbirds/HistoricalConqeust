using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentsHand : MonoBehaviour
{
    public GameObject cardPrefab, playerHand, playerField;

    public GameObject topofDeck, topofLands, LandTarget,activeSlot,discardTarget;
    public CardController discardPreview;

    public Dictionary<int, CardController> inHandList = new Dictionary<int, CardController>();

    public int cardsinHand,drawAmount,activeSlotCount;
    public bool drawCards;
    public float drawTimer,drawTick;

    public GameObject[] landSlots, row1Slots, row2Slots, row3Slots, row4Slots,rowSelectors,attackSelectors;

    public static OpponentsHand instance;

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
        DrawCardFunction();
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
            foreach (KeyValuePair<int, CardController> _card in inHandList)
            {
                _card.Value.SetTargets(FindHandPosition(_card.Value.positioninHand), playerHand.transform.rotation);
            }

            int cardValue = 0;
            CardController card = Instantiate(cardPrefab, playerHand.transform).GetComponent<CardController>();
            inHandList.Add(cardsinHand, card);
            //Debug.Log("adding card " + cardsinHand + " to the hand");
            card.transform.position = topofDeck.transform.position;
            card.transform.rotation = Quaternion.Euler(-90f, -170f, 0f);
            card.GetComponent<CardController>().transformSpeed = 5f;
            card.GetComponent<CardController>().opponentsHand = true;
            card.GetComponent<CardController>().rotateSpeed = 6f;
            card.GetComponent<CardController>().GetValues(cardValue);
            card.GetComponent<CardController>().positioninHand = cardsinHand;
            card.GetComponent<CardController>().SetTargets(FindHandPosition(cardsinHand), playerHand.transform.rotation);
        }
    }

    public void PlayLandFromDeck(int cardValue) //grabs a single card from the land deck and auto positions the card
    {
        int useSlot = 0;
        for (int i = 0; i < landSlots.Length; i++)
        {
            if (!landSlots[i].gameObject.GetComponent<CardSlotController>().filled)
            {
                useSlot = i;
                Debug.Log("selecting slot " + i);
                i = landSlots.Length;
            }
        }

        CardController card = Instantiate(cardPrefab, playerField.transform).GetComponent<CardController>();
        card.transform.position = topofLands.transform.position;
        card.transform.rotation = Quaternion.Euler(-90f, -170f, 0f);
        card.transform.localScale = new Vector3(1.2f, 0.7f, 0.005f);
        card.GetComponent<CardController>().transformSpeed = 8f;
        card.GetComponent<CardController>().rotateSpeed = 6f;
        card.GetComponent<CardController>().inSlot = true;
        card.GetComponent<CardController>().GetValues(cardValue);
        card.GetComponent<CardController>().opponent = true;
        card.GetComponent<CardController>().SetTargets(LandTarget.transform.position, playerField.transform.rotation);
        card.GetComponent<CardController>().flip = true;
        card.GetComponent<CardController>().landSlot = useSlot;
        landSlots[useSlot].gameObject.GetComponent<CardSlotController>().filled = true;
    }

    public void PlayCard(int row, int cardValue,ulong clientId,string setAbility,bool hidden)
    {
        CardController playCard = FindCardinHand();
        playCard.GetValues(cardValue);
        ShuffleDictionaryDown(playCard.positioninHand);
        playCard.cardOwner = clientId;
        playCard.abilityString = "Preview";
        playCard.triggerAbility = true;
        playCard.opponentsHand = false;
        playCard.opponent = true;
        int useSlot = FindSlot(row);
        if (DeckManager.instance.cardLibrary[playCard.cardValue].subType == "Army")
        {
            //shuffle things upwards
            ShuffleRowForArmy(row,clientId);
            useSlot = 0;
        }
        else
        {
            useSlot = CheckForExplorer(row, useSlot,clientId);
        }

        //Debug.Log("Opponents playing card");
        playCard.transform.parent = playerField.transform;
        playCard.transform.localScale = new Vector3(1.2f, 0.7f, 0.005f);
        playCard.transformSpeed = 8f;
        playCard.rotateSpeed = 6f;
        playCard.inSlot = true;
        playCard.row = row;
        playCard.slot = useSlot;
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
                UpdateActivePositions(clientId);
                break;
        }
        UpdateRowSlots();
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

    private int CheckForExplorer(int row, int useSlot,ulong clientId)
    {
        //Debug.Log("doing an explorer check");
        int slot = useSlot;
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject _card in cards)
        {
            if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == slot - 1 && _card.GetComponent<CardController>().inSlot&& _card.GetComponent<CardController>().cardOwner==clientId)
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

    private Vector3 FindActivePosition(int position)
    {
        Vector3 newpos = activeSlot.transform.position;
        float forward = position * 8;
        Vector3 returnPoint = newpos -= new Vector3(0f, 0f, forward);

        return returnPoint;
    }

    private void UpdateActivePositions(ulong clientId)
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        Dictionary<int, GameObject> activelist = new Dictionary<int, GameObject>();
        foreach (GameObject card in cards)
        {
            if (card.GetComponent<CardController>().row == 5 && card.GetComponent<CardController>().inSlot && card.GetComponent<CardController>().cardOwner == clientId)
            {
                activelist.Add(card.GetComponent<CardController>().activeSlot, card);
            }
        }
        int pulldown = 0;
        for (int i = 0; i < activeSlotCount + 1; i++)
        {
            if (activelist.ContainsKey(i))
            {
                //Debug.Log("found + " + i + " in the dictionary");
                GameObject card = activelist[i];
                card.GetComponent<CardController>().activeSlot += pulldown;
                card.GetComponent<CardController>().SetTargets(FindActivePosition(card.GetComponent<CardController>().activeSlot), activeSlot.transform.rotation);
            }
            else
            {
                //Debug.Log("did not find " + i + " in the dictionary");
                pulldown++;
            }
        }
    }

    private void MoveCardUpaSlot(CardController _playCard)
    {
        //Debug.Log("moving card up a slot");
        int row = _playCard.row;
        int useSlot = _playCard.slot + 1;
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

    private void ShuffleRowForArmy(int row,ulong clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 1 && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == clientId)
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
                        if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot == 0 && _card.GetComponent<CardController>().cardOwner == clientId)
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

    public void DiscardACard(int _card)
    {
        CardController card = FindCardinHand();
        if (card == null) return;

        ShuffleDictionaryDown(card.positioninHand);
        card.discard = true;
        card.useSlerp = true;
        card.cardValue = _card;
        card.GetValues(_card);
        card.movetoSpot = true;
        card.opponent = true;
        card.SetTargets(discardPreview.transform.position, discardPreview.transform.rotation, new Vector3(1.2f, 0.7f, 0.005f));
        card.scaleSpeed = 10f;
    }

    public void DiscardACardinPlay(int row,int slot,ulong clientId)
    {
        Debug.Log("looking for card in row " + row + " and slot " + slot);
        CardController card = FindCardinPlay(row,slot,clientId);
        card.useSlerp = false;
        card.movetoSpot = true;
        card.transformSpeed = 8f;
        card.rotateSpeed = 6f;
        card.scaleSpeed = 10f;
        card.discardFlip = true;
        card.opponent = true;
        card.inSlot = false;
        ShuffleRowDown(row, slot,clientId);
        card.SetTargets(discardTarget.transform.position, discardTarget.transform.rotation, new Vector3(1.2f, 0.7f, 0.005f));
    }

    private CardController FindCardinHand()
    {
        int card = Mathf.RoundToInt(Random.Range(1f,cardsinHand));
        //Debug.Log(card);

        if (inHandList.ContainsKey(card))
        {
            return inHandList[card];
        }

        return null;
    }

    private void ShuffleDictionaryDown(int position)
    {
        inHandList.Remove(position);
        cardsinHand--;
        //Debug.Log("removing " + position);
        for (int i = 1; i < 6; i++)
        {
            if (inHandList.ContainsKey(i))
            {
                if (i > position)
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

    private void ShuffleRowDown(int row, int slot,ulong clientId)
    {
        Debug.Log("shuffle row down OP");
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
            if (_card.GetComponent<CardController>().row == row && _card.GetComponent<CardController>().slot > slot && _card.GetComponent<CardController>().inSlot && _card.GetComponent<CardController>().cardOwner == clientId)
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
            UpdateActivePositions(clientId);
        }
    }
    private Vector3 FindHandPosition(int position)
    {
        Vector3 newpos = playerHand.transform.position;
        float middle = (cardsinHand * 10) / 2;
        float spacing = -2;
        float height = position * -3 - 50;
        float forward = 0f;

        float startingPoint = middle * -spacing;
        newpos += new Vector3((startingPoint + ((spacing * 10) * position) - ((spacing * 10) / 2)), height, forward);

        return newpos;
    }

    private CardController FindCardinPlay(int row, int slot, ulong cliendId)
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

    private void UpdateHandPositions()
    {
        for (int i = 1; i < 6; i++)
        {
            if (inHandList.ContainsKey(i))
            {
                if (!inHandList[i].isBeingDragged)
                {
                    inHandList[i].SetTargets(FindHandPosition(inHandList[i].positioninHand), playerHand.transform.rotation);
                }
            }
        }
    }

    public void ClearHand()
    {
        inHandList = new Dictionary<int, CardController>();
    }


    public void UpdateRowSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            row1Slots[i].SetActive(false);
            row2Slots[i].SetActive(false);
            row3Slots[i].SetActive(false);
            row4Slots[i].SetActive(false);
        }
        for (int i = 0; i < 4; i++)
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

    public void ClearSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            landSlots[i].GetComponent<CardSlotController>().filled = false;
        }
        for (int y = 0; y < 4; y++)
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
        SetSelections(false);
    }

    public int SetSelections(bool _set) //turns off all rows or turn rows on depending on avaliable slots
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
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 1:
                            if (!row2Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 2:
                            if (!row3Slots[3].GetComponent<CardSlotController>().filled)
                            {
                                open++;
                                rowSelectors[i].SetActive(_set);
                            }
                            break;
                        case 3:
                            if (!row4Slots[3].GetComponent<CardSlotController>().filled)
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

    public int SetSelectionsForAttack(bool _set) //turns off all rows or turn rows on depending on avaliable slots
    {
        int open = 0;
        if (_set)
        {
            for (int i = 0; i < 4; i++)
            {
                if (landSlots[i].gameObject.GetComponent<CardSlotController>().filled)
                {
                    open++;
                    rowSelectors[i].SetActive(_set);
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

    public void ClearAttackRows()
    {
        for (int i = 0; i < 4; i++)
        {
            attackSelectors[i].SetActive(false);
        }
    }

    public void SendCardsBackToRows()
    {
        Debug.Log("sending cards back");
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            if (card.GetComponent<CardController>().inAttackPreview)
            {
                int row = card.GetComponent<CardController>().row;
                int useSlot = card.GetComponent<CardController>().slot;
                int cardValue = card.GetComponent<CardController>().cardValue;
                if (card.GetComponent<CardController>().opponent)
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
            }
        }
    }
}
