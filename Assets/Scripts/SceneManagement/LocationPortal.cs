using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//Teleports the player to a different position without switching scenes
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    PlayerController player;
    Fader fader;

    public void Start()
    {
        fader = FindObjectOfType<Fader>();
    }
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.isMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);


        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return new WaitForSeconds(0.5f);

        yield return fader.FadeOut(0.5f);

        GameController.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier { A, B, C, D, E }
}
