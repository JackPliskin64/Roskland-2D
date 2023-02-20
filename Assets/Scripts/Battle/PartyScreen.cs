using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    public GameObject messageBox;

    PartyMemberUI[] memberSlots;
    List<Fighter> fighters;

    int selection = 0;

    public Fighter SelectedMember => fighters[selection];

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

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { ++selection; }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        { --selection; }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        { selection += 2; }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        { selection -= 2; }

        selection = Mathf.Clamp(selection, 0, fighters.Count - 1);

        UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
           onBack?.Invoke();
        }
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
