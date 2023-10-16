using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBoxController : MonoBehaviour
{
    public int typeofDeck;
    private int lastUpdate;
    private float ySet;
    private Rigidbody rb;
    public int deckSize;
    public int lastCardToShow;
    [SerializeField]
    private Image showCase;
    [SerializeField]
    private CardController cardTop;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ySet = transform.position.y;
    }

    private void Start()
    {
        if (typeofDeck == 5|| typeofDeck == 6)
        {
            RectTransform rt = showCase.GetComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0f, 0f, 0f);
            showCase.gameObject.SetActive(false);
            cardTop.gameObject.SetActive(true);
        }
    }
    void Update()
    {
        switch (typeofDeck)
        {
            case 0: //regular deck
                deckSize = GameManager.instance.inGameDeck.Count;
                break;
            case 1: //land deck
                deckSize = GameManager.instance.inGameLands.Count;
                break;
            case 3: //opponents deck
                deckSize = GameManager.instance.opponentsDeck;
                break;
            case 4: //opponents land deck
                deckSize = GameManager.instance.opponentsLands;
                break;
            case 5:
                deckSize = GameManager.instance.inGameDiscard.Count;
                break;
            case 6:
                deckSize = GameManager.instance.inGameDiscardO.Count;
                break;
        }

        if (deckSize != lastUpdate)
        {
            lastUpdate = deckSize;
            transform.position = new Vector3(transform.position.x,ySet + ((deckSize + 1)/2),transform.position.z);
        }

    }

    private void FixedUpdate()
    {
        rb.position = transform.position;
        rb.rotation = transform.rotation;
    }
}
