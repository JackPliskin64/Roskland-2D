using DG.Tweening;
using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState {  Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchFighter, UseItem, Run }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    public event Action<bool> OnBattleOver;

    BattleState state;

    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;

    FighterParty playerParty;
    FighterParty trainerParty;
    Fighter wildFighter;

    bool isTrainerBattle = false;

    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

   public void StartBattle(FighterParty playerParty, Fighter wildFighter)
    {
        this.playerParty = playerParty;
        this.wildFighter = wildFighter;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(FighterParty playerParty, FighterParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;

        player = playerParty.GetComponent <PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //Wild fighter battle
            playerUnit.Setup(playerParty.GetHealthyFighter());
            enemyUnit.Setup(wildFighter);

            dialogBox.SetMoveNames(playerUnit.Fighter.Moves);
            yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Fighter.Base.fighterName} apareció!");
        }
        else
        {
            //Trainer battle

            //Show trainer and player sprites

            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            yield return dialogBox.TypeDialog($"¡{trainer.Name} te desafía a un combate!");

            //Send out first fighter of the trainer
            var enemyFighter = trainerParty.GetHealthyFighter();
            enemyUnit.Setup(enemyFighter);

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            yield return dialogBox.TypeDialog($"{trainer.Name} envía a {enemyFighter.Base.FighterName}.");

            //Send out first fighter of the player
            var playerFighter = playerParty.GetHealthyFighter();
            playerUnit.Setup(playerFighter);

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            yield return dialogBox.TypeDialog($"¡Adelante, {playerFighter.Base.FighterName}!");
            dialogBox.SetMoveNames(playerUnit.Fighter.Moves);

        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Fighters.ForEach(f => f.OnBattleOver());
        OnBattleOver(won);

    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Elige una acción.");
        dialogBox.EnableActionSelector(true);
    }

    IEnumerator AboutToUse(Fighter newFighter)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} va a enviar a {newFighter.Base.FighterName}. ¿Quieres cambiar de luchador?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Fighter fighter, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Elije el movimiento que quieras olvidar.");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(fighter.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;

    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Fighters);
        partyScreen.gameObject.SetActive(true);
        partyScreen.messageBox.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Fighter.CurrentMove = playerUnit.Fighter.Moves[currentMove];
            enemyUnit.Fighter.CurrentMove = enemyUnit.Fighter.GetRandomMove();

            int playerMovePriority = playerUnit.Fighter.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Fighter.CurrentMove.Base.Priority;


            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Fighter.Speed >= enemyUnit.Fighter.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondFighter = secondUnit.Fighter;

            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Fighter.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if(secondFighter.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Fighter.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }

        }

        else
        {
            if (playerAction == BattleAction.SwitchFighter)
            {
                var selectedFighter = playerParty.Fighters[currentMember];
                state = BattleState.Busy;
                yield return SwitchFighter(selectedFighter);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            //Enemy turn
            var enemyMove = enemyUnit.Fighter.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Fighter.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Fighter);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Fighter);


        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Fighter.Base.FighterName} usó {move.Base.MoveName}.");

        if (CheckIfMoveHits(move, sourceUnit.Fighter, targetUnit.Fighter))
        {
        
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveBase.MoveCategory.Estado)
        {
            yield return RunMoveEffects(move.Base.Effects, sourceUnit.Fighter, targetUnit.Fighter, move.Base.Target);
        }

        else
        {
            var damageDetails = targetUnit.Fighter.TakeDamage(move, sourceUnit.Fighter);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Fighter.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Fighter, targetUnit.Fighter, secondary.Target);

                    }
                }
            }

        if (targetUnit.Fighter.HP <= 0)
        {
                yield return HandleFighterFainted(targetUnit);
        }
        }

        else
        {
            yield return dialogBox.TypeDialog($"¡El movimiento de {sourceUnit.Fighter.Base.FighterName} ha fallado!");
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //Statuses like burn or poison will hurt the fighter after the turn
        sourceUnit.Fighter.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Fighter);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Fighter.HP <= 0)
        {
            yield return HandleFighterFainted(sourceUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);

        }
    }
    IEnumerator RunMoveEffects(MoveBase.MoveEffects effects, Fighter source, Fighter target, MoveBase.MoveTarget moveTarget)
    {
        //Stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveBase.MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);

            }

        //Status Condition
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
        }
    }

    bool CheckIfMoveHits(Move move, Fighter source, Fighter target)
    {

        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Precisión];
        int evasion = source.StatBoosts[Stat.Evasión];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if(evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else 
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Fighter fighter)
    {
        while (fighter.StatusChanges.Count > 0)
        {
            var message = fighter.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message); 
        }
    }

    IEnumerator HandleFighterFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Fighter.Base.FighterName} se ha debilitado.");
        faintedUnit.PlayFaintAnimation();

        yield return new WaitForSeconds(2f);

        if (faintedUnit.isEnemy)
        {
            //Exp Gain
            int expYield = faintedUnit.Fighter.Base.ExpYield;
            int enemyLevel = faintedUnit.Fighter.Level;
            float trainerBonus = (isTrainerBattle)? 1.5f : 1f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus) / 7;
            playerUnit.Fighter.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.FighterName} ganó {expGain} puntos de EXP.");
            yield return playerUnit.Hud.SetExpSmooth();


            //Check Level Up
            while (playerUnit.Fighter.CheckForlevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"¡{playerUnit.Fighter.Base.FighterName} subió a nivel {playerUnit.Fighter.Level}!");

                //Try ro learn new move
                var newMove = playerUnit.Fighter.GetLearnableMoveAtCurrentLevel();
                if(newMove != null)
                {
                    if(playerUnit.Fighter.Moves.Count < FighterBase.MaxNumOfMoves)
                    {
                        playerUnit.Fighter.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"¡{playerUnit.Fighter.Base.FighterName} aprendió a usar {newMove.Base.name}!");
                        dialogBox.SetMoveNames(playerUnit.Fighter.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Fighter.Base.FighterName} quiere aprender {newMove.Base.name}.");
                        yield return dialogBox.TypeDialog($"Pero no puede aprender más de {FighterBase.MaxNumOfMoves} movimientos.");
                        yield return ChooseMoveToForget(playerUnit.Fighter, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }
                
                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsEnemy)
        {
                if (!isTrainerBattle)
                {
                    BattleOver(true);
                }

                else
                {
                var nextFighter = trainerParty.GetHealthyFighter();
                if (nextFighter != null) 
                {
                    StartCoroutine(AboutToUse(nextFighter));
                }
                else { BattleOver(true); }
            }

        }

        else
        {
            var nextFighter = playerParty.GetHealthyFighter();
            if (nextFighter != null) { OpenPartyScreen();}
            else { BattleOver(false); }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("¡Un golpe crítico!");
        }

        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("¡Es muy eficaz!");

        }
        
        if(damageDetails.TypeEffectiveness < 1)
        {
            yield return dialogBox.TypeDialog("No es muy eficaz...");

        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleChoiceBox();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == FighterBase.MaxNumOfMoves)
                {
                    //Don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Fighter.Base.FighterName} no aprendió {moveToLearn.MoveName}."));

                }
                else
                {
                    //Forget the selected move and learn new move
                    var selectedMove = playerUnit.Fighter.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Fighter.Base.FighterName} olvidó {selectedMove.MoveName} y aprendió {moveToLearn.MoveName}"));


                    playerUnit.Fighter.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { ++currentAction; }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        { --currentAction; }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        { currentAction += 2; }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        { currentAction -= 2; }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {
                //Fighter
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { ++currentMove; }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        { --currentMove; }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        { currentMove += 2; }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        { currentMove -= 2; }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Fighter.Moves.Count - 1);
        

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Fighter.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Fighter.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }

        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { ++currentMember; }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        { --currentMember; }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        { currentMember += 2; }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        { currentMember -= 2; }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Fighters.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Fighters[currentMember];
            if (selectedMember.HP <= 0) 
            {
                partyScreen.SetMessageText("No puedes enviar a un combatiente debilitado.");
                return; 
            }
            if (selectedMember == playerUnit.Fighter)
            {
                partyScreen.SetMessageText("No puedes cambiar al mismo combatiente.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchFighter));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchFighter(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
           
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (playerUnit.Fighter.HP <= 0)
            {
                partyScreen.SetMessageText("¡Debes elegir un luchador para continuar!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if(partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerFighter());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        }
    }
    void HandleChoiceBox()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //Yes option
                OpenPartyScreen();
            }
            else
            {
                //No option
                StartCoroutine(SendNextTrainerFighter());            
            }
        }
        else if ((Input.GetKeyDown(KeyCode.X))) //(No option)
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerFighter());

        }
    }



    IEnumerator SwitchFighter(Fighter newFighter, bool isTrainerAboutToUse=false)
    {
        if (playerUnit.Fighter.HP > 0)
        {
            yield return dialogBox.TypeDialog($"¡Vuelve, {playerUnit.Fighter.Base.fighterName}!");

            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        }
        
        playerUnit.Setup(newFighter);

        dialogBox.SetMoveNames(newFighter.Moves);

        yield return dialogBox.TypeDialog($"¡Adelante, {newFighter.Base.fighterName}!");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerFighter());
        }
        else
        {
            state = BattleState.RunningTurn;
        }

    }

    IEnumerator SendNextTrainerFighter()
    {
        state = BattleState.Busy;

        var nextFighter = trainerParty.GetHealthyFighter();

        enemyUnit.Setup(nextFighter);
        yield return dialogBox.TypeDialog($"{trainer.Name} envió a {nextFighter.Base.FighterName}.");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"¡No puedes robarle un luchador a tu oponente!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"¡{player.Name} usó una Roscaball!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        //Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 3), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchFighter(enemyUnit.Fighter);

        for (int i = 0; i < Mathf.Min(shakeCount,3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            //Fighter is caught
            yield return dialogBox.TypeDialog($"¡{enemyUnit.Fighter.Base.FighterName} ha sido atrapado!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddFighter(enemyUnit.Fighter);
            yield return dialogBox.TypeDialog($"¡{enemyUnit.Fighter.Base.FighterName} se ha unido a tu equipo!");


            Destroy(pokeball);
            BattleOver(true);

        }
        else
        {
            //Fighter broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"¡{enemyUnit.Fighter.Base.FighterName} se ha escapado!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"¡Casi!");
            }

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchFighter(Fighter fighter)
    {
        float a = (3 * fighter.MaxHP - 2 * fighter.HP) * fighter.Base.CatchRate * ConditionsDB.GetStausBonus(fighter.Status) / (3 * fighter.MaxHP);
        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("¡No puedes huir en un desafío!");
            state = BattleState.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerUnit.Fighter.Speed;
        int enemySpeed = enemyUnit.Fighter.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("¡Escapaste sin problemas!");
            BattleOver(true);
        }

        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog("¡Huiste sin problemas!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("¡No has podido huir!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
