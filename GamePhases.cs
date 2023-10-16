using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePhases : MonoBehaviour
{
    public int gamePhase;
    private float timer;
    private int activeTurnButton;

    private void Update()
    {
        if(gamePhase == 1)
        {
            List<ulong> clientlist = new List<ulong>();
            foreach(KeyValuePair<ulong,PlayerInfo> player in GameManager.instance.playerInfo)
            {
                clientlist.Add(player.Key);
            }
            PlayerControls.instance.UpdateRowSlots();
            OpponentsHand.instance.UpdateRowSlots();
            NetworkTransmission.instance.TargetPlayerDrawsServerRPC(5,GameManager.instance.myClientId);
            gamePhase = 2;
        }
        if(gamePhase==2 && PlayerControls.instance.playerReady) //once hand is drawn play the opening land
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                gamePhase = 3;
                NetworkTransmission.instance.PlayerReadyforOpeningLandServerRPC(GameManager.instance.myClientId);
                timer = 0;
            }
        }
        if (gamePhase == 4) //go into normal playing mode
        {
            if (GameManager.instance.gameTurn > -1) // temp change
            {
                Debug.Log("setting attack button");
                activeTurnButton = 1;
                GameManager.instance.attackPhaseButton.SetActive(true);
                gamePhase++;
            }
            else
            {
                activeTurnButton = 3;
                GameManager.instance.endTurnButton.SetActive(true);
                gamePhase=7;
            }
        }
        if (gamePhase == 6) //attack phase
        {
            SetAttackOptions(2);   
            gamePhase++;
        }
        if (gamePhase == 8)// turn has ended
        {
            gamePhase = 11;
            activeTurnButton = 0;
            PlayerControls.instance.SetSelections(false, false);
            OpponentsHand.instance.SetSelections(false);
            PlayerControls.instance.attackMode = false;
            GameManager.instance.endTurnButton.SetActive(false);
            if (PlayerControls.instance.inHandList.Count == 5)
            {
                gamePhase = 3;
                PlayerControls.instance.EndTurn();
            }
            else
            {
                NetworkTransmission.instance.TargetPlayerDrawsServerRPC(5, GameManager.instance.myClientId);
            }
            //PlayerControls.instance.EndTurn();
        }

    }

    public void SetAttackOptions(int attacks)
    {
        activeTurnButton = 3;
        PlayerControls.instance.tuckSensor.gameObject.SetActive(false);
        NetworkTransmission.instance.PlayerChangedingPhaseServerRPC(GameManager.instance.myClientId,attacks, "Attacks");
        PlayerControls.instance.attackingRow = -1;
        PlayerControls.instance.AttackSelection();
        PlayerControls.instance.attackMode = true;
        GameManager.instance.attackPhaseButton.SetActive(false);
        GameManager.instance.endTurnButton.SetActive(true);
    }

    public void ButtonSwitch(bool set) 
    {
        //Debug.Log("getting called " + set);
        switch (activeTurnButton)
        {
            case 1:
                GameManager.instance.attackPhaseButton.SetActive(set);
                break;
            case 3:
                GameManager.instance.endTurnButton.SetActive(set);
                break;
        }
    }
}
