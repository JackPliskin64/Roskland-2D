using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string _name;
    [SerializeField] Sprite sprite;
    [SerializeField] Vector2 input;
    Character character;

    public void Awake()
    {
        character = GetComponent<Character>();
    }


    public void HandleUpdate()
    {
        if (!character.IsMoving) 
        { 
        input.x = UnityEngine.Input.GetAxisRaw("Horizontal");
        input.y = UnityEngine.Input.GetAxisRaw("Vertical");

        if (input.x != 0) input.y = 0;

        if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }

    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.moveX, character.Animator.moveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            fighters = GetComponent<FighterParty>().Fighters.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        //Restaurar posición
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        //Restaurar party
        GetComponent<FighterParty>().Fighters = saveData.fighters.Select(s => new Fighter(s)).ToList();
    }

    public string Name { get => _name; }
    public Sprite Sprite { get => sprite; }
    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<FighterSaveData> fighters;
}






