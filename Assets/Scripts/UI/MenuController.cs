using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
 public void OpenMenu()
    {
        menu.SetActive(true);
    }
}
