using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    public GameObject messageBox;

    PartyMemberUI[] memberSlots;
    List<Fighter> fighters;

    //Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }


    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Fighter> fighters)
    {
        this.fighters = fighters;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < fighters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(fighters[i]);
            }

            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Elije a tu combatiente";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < fighters.Count; i++)
        {
            if (i == selectedMember) { memberSlots[i].SetSelected(true); } 
            else { memberSlots[i].SetSelected(false); }
        }
    }
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
