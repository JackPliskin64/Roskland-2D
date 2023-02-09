using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    //Parameters

    public float moveX { get; set; }
    public float moveY { get; set; }
    public bool isMoving { get; set; }

    //States

    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;

    //References

    SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        SetFacingDirection(defaultDirection);
        currentAnim = walkDownAnim;
    }

    public void Update()
    {
        var prevAnim = currentAnim;

        if (moveX == 1) {currentAnim = walkRightAnim;}

        else if (moveX == -1) {currentAnim = walkLeftAnim;}

        else if (moveY == 1) {currentAnim = walkUpAnim;}

        else if (moveY == -1) {currentAnim = walkDownAnim;}

        if(currentAnim != prevAnim || isMoving != wasPreviouslyMoving)
        {
            currentAnim.Start();
        }

        if (isMoving)
        {
            currentAnim.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        wasPreviouslyMoving = isMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if (dir == FacingDirection.Up)
        {
            moveY = 1;
        }
        else if (dir== FacingDirection.Down)
        {
            moveY = -1;
        }
        else if (dir== FacingDirection.Left)
        {
            moveX = -1;
        }
        else if (dir == FacingDirection.Right)
        {
            moveX = 1;
        }
    }

    public FacingDirection DefaultDirection { get => defaultDirection; }

    public enum FacingDirection { Up, Down, Left, Right }

}
