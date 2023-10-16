using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardController : MonoBehaviour
{
    [SerializeField] private Image cardBack,cardFace,cardTitle,cardStats,cardCover,aBorder,aBackGround;
    [SerializeField] private TMP_Text cardAbility;
    public string cardName;
    public ulong cardOwner;
    public int cardValue;
    public int positioninHand;
    public int positioninPreview;
    public int landSlot;
    public Vector3 targetPosition,targetScale;
    private Color32 first, second;
    public Quaternion targetRot;
    public float rotateSpeed, transformSpeed,scaleSpeed;
    public bool flip,highLight,discard,useSlerp,movetoSpot,discardFlip;
    public GameObject highlightImage,hiddenOverlay;
    private Rigidbody rb;
    public bool isBeingDragged,inSlot,deckTop,triggerAbility,isPreviewCard,inAttackPreview,inSearch;
    public string abilityString;
    public bool opponent,opponentsHand;
    public int row, slot,activeSlot;

    private void Start()
    {
        row = -1;
        slot = -1;
        rb = GetComponent<Rigidbody>();
        targetScale = transform.localScale;
        if(!deckTop) movetoSpot = true;
    }


    private void Update()
    {

        if (isBeingDragged)
        {
            targetRot.eulerAngles += new Vector3(Input.GetAxis("Mouse Y") * Time.deltaTime * 1000, Input.GetAxis("Mouse X") * Time.deltaTime * 500,0f);
        }

        if (movetoSpot)
        {

            if (!useSlerp)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, transformSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.Slerp(transform.position, targetPosition, transformSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
            }
        }

        if(highLight && !highlightImage.activeInHierarchy)
        {
            highlightImage.SetActive(true);
        }
        if (!highLight && highlightImage.activeInHierarchy)
        {
            highlightImage.SetActive(false);
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 2 && flip)
        {
            flip = false;
            transformSpeed = 3f;
            rotateSpeed = 1f;
            if (!opponent)
            {
                targetPosition = GetLandSlotPosition(landSlot);
            }
            else
            {
                rotateSpeed = 5f;
                targetPosition = GetLandSlotOPosition(landSlot);
            }
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 8 && discardFlip)
        {
            discardFlip = false;
            discard = true;
            if (!opponent)
            {
                targetPosition = PlayerControls.instance.discardPreview.transform.position;
                targetRot = PlayerControls.instance.discardPreview.transform.rotation;
            }
            else
            {
                rotateSpeed = 5f;
                targetPosition = OpponentsHand.instance.discardPreview.transform.position;
                targetRot = OpponentsHand.instance.discardPreview.transform.rotation;
            }
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 8 && discard)
        {
            if (!opponent)
            {
                PlayerControls.instance.discardPreview.GetValues(cardValue);
                GameManager.instance.inGameDiscard.Enqueue(cardValue);
                Debug.Log("adding to discard pile");
                Destroy(this.gameObject);
            }
            else
            {
                OpponentsHand.instance.discardPreview.GetValues(cardValue);
                GameManager.instance.inGameDiscardO.Enqueue(cardValue);
                Debug.Log("adding to discard pile");
                Destroy(this.gameObject);
            }
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 8 && triggerAbility)
        {
            ActivateAbility(abilityString);
            triggerAbility = false;
        }

            if (highLight && !highlightImage.activeInHierarchy)
        {
            highlightImage.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        rb.position = transform.position;
        rb.rotation = transform.rotation;
    }
    public void SetTargets(Vector3 pos, Quaternion rot)
    {
        targetPosition = pos;
        targetRot = rot;
    }

    public void SetTargets(Vector3 pos,Quaternion rot,Vector3 scale)
    {
        targetPosition = pos;
        targetRot = rot;
        targetScale = scale;
    }
    public void GetValues(int card)
    {
        cardFace.sprite = ImageManager.instance.cardFaces[card];
        cardTitle.sprite = ImageManager.instance.cardFaces[card];
        cardStats.sprite = ImageManager.instance.cardFaces[card];
        cardCover.sprite = ImageManager.instance.cardFaces[card];
        aBorder.color = DeckManager.instance.ReturnCardColor(card, 0);
        aBackGround.color = DeckManager.instance.ReturnCardColor(card, 1);
        cardName = DeckManager.instance.cardLibrary[card].cardName;
        cardAbility.text = DeckManager.instance.cardLibrary[card].ability;
        cardValue = card;
    }

    private void ActivateAbility(string ability) //this is made for entering the field and waht the card will accomplish when it enters
    {
        //cards marks by the discover land button come through with land as the ability to promp a new placement of a land card
        //all cards will get previewed and then proc whatever enter the field triggers they have
        //Debug.Log(ability);
        if(ability == "Land")
        {
            if (cardOwner == GameManager.instance.myClientId)
            {
                PlayerControls.instance.PlayLandFromDeck();
            }
        }
        if(ability == "Preview")
        {
            PlayerControls.instance.ShowCardForPreview(cardValue,transform.position);
        }
    }

    private Vector3 GetLandSlotPosition(int land)
    {
        Vector3 newPos = PlayerControls.instance.landSlots[land].transform.position;
        return newPos;
    }

    private Vector3 GetLandSlotOPosition(int land)
    {
        Vector3 newPos = OpponentsHand.instance.landSlots[land].transform.position;
        return newPos;
    }

}
