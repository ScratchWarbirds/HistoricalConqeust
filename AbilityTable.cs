using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class AbilityTable
{

    public static void AddToAttack()
    {

    }

    public static void AddtoDeffence()
    {

    }
    public static void EntertheBattleField()
    {

    }

    public static int CheckAtPreCast(string cardName) //returns and int that will tell playercontroller which selection needs to be made
    {
        int set=0;
        if(cardName == "Aaron Burr") //arron needs to select an oppoenents card
        {
            return 1;
        }


        return set;
    }

    public static void CheckEnterTheBattleField(int[] etblist, bool host)
    {
        int index = 0;
        if (etblist.Length <= index) { return; }
        if (etblist[index] == 1) //give the player morale
        {
            index++;
            if (host)
            {
                NetworkTransmission.instance.UpdateMoraleServerRPC(etblist[index], 0);
            }
            else
            {
                NetworkTransmission.instance.UpdateMoraleServerRPC(0, etblist[index]);
            }
            index++;
            if (etblist.Length <= index) { return; }
        }
        if (etblist[index] == 2) //remove the opponents morale
        {
            index++;
            if (host)
            {
                NetworkTransmission.instance.UpdateMoraleServerRPC(0, etblist[index]);
            }
            else
            {
                NetworkTransmission.instance.UpdateMoraleServerRPC(etblist[index], 0);
            }
            index++;
            if (etblist.Length <= index) { return; }
        }
    }


    public static bool CheckAtEndOfCombat()
    {
        if (PlayerControls.instance.activeAbiliites.Contains("Armistead"))
        {
            return true;
        }

        return false;
    }
}
