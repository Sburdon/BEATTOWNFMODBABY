using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WrestlerAnimationSet", menuName = "Animation/WrestlerAnimationSet", order = 1)]
public class WrestlerAnimationSet : ScriptableObject
{
    public Texture2D idleTexture;
    public Texture2D moveTexture;
    public Texture2D punchTexture;
    public Texture2D swingTexture;
    public Texture2D pushTexture;
    public Texture2D reactTexture;

    public Texture2D sourceTexture;
    public Texture2D baseMask;  // common base mask if applicable for all animations
}

public class StateMachine : MonoBehaviour
{
    public enum WrestlerState
    {
        Idle,
        Move,
        Punch,
        Push,
        React,
        Swing
    }

    public WrestlerState currentState;

    // uv mapping script reference
    public UVMappin uvMappingScript;

    // frame sets for each animation state
    private List<Sprite> idleFrames;
    private List<Sprite> moveFrames;
    private List<Sprite> punchFrames;
    private List<Sprite> swingFrames;
    private List<Sprite> pushFrames;
    private List<Sprite> reactFrames;

    // Movement variables
    public float moveSpeed = 3f;
    private bool canMove = true;
    public bool actionPlaying;

    private int curFrame = 0;
    private float frameDuration = 0.1f; // time per frame
    private SpriteRenderer spriteRenderer;

    public WrestlerAnimationSet animationSet;

    private float frameTimer = 0f;    // timer to track time between frames

    // Start is called before the first frame update
    void Start()
    {

        // ensure UV Mapping and SpriteRenderer components are attached
        if (uvMappingScript == null)
        {
            uvMappingScript = GetComponent<UVMappin>();
        }

        if (uvMappingScript == null)
        {
            Debug.LogError("attach uv script!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("attach spriteRenderer!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        uvMappingScript._texture = animationSet.idleTexture;
        idleFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.moveTexture;
        moveFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.punchTexture;
        punchFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.swingTexture;
        swingFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.pushTexture;
        pushFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.reactTexture;
        reactFrames = uvMappingScript.MapOntoTexture(animationSet.sourceTexture, animationSet.baseMask);

        ChangeState(WrestlerState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleState();
    }

    //// handles player input to switch between states
    void HandleInput()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 && currentState != WrestlerState.React && canMove)
        {
            ChangeState(WrestlerState.Move);
        }

        if (Input.GetKeyDown(KeyCode.P) && canMove)
        {
            ChangeState(WrestlerState.Punch);
        }

        if (Input.GetKeyDown(KeyCode.S) && canMove)
        {
            ChangeState(WrestlerState.Swing);
        }

        if (Input.GetKeyDown(KeyCode.O) && canMove)
        {
            ChangeState(WrestlerState.Push);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            ChangeState(WrestlerState.React);
        }
    }

    void HandleState()
    {
        switch (currentState)
        {
            case WrestlerState.Idle:
                if (!actionPlaying)
                {
                    PlayAnimation(idleFrames);  // play idle animation if action not happening
                }
                break;

            case WrestlerState.Move:
                actionPlaying = true;
                PlayAnimation(moveFrames);
                if (HasAnimationCompleted(moveFrames))
                {
                    actionPlaying = false;
                    ChangeState(WrestlerState.Idle);  // return to Idle only after full animation plays
                }
                break;

            case WrestlerState.Punch:
                actionPlaying = true;
                PlayAnimation(punchFrames);
                if (HasAnimationCompleted(punchFrames))
                {
                    actionPlaying = false;
                    ChangeState(WrestlerState.Idle);
                }
                break;

            case WrestlerState.Swing:
                actionPlaying = true;
                PlayAnimation(swingFrames);
                if (HasAnimationCompleted(swingFrames))
                {
                    actionPlaying = false;
                    ChangeState(WrestlerState.Idle);
                }
                break;

            case WrestlerState.Push:
                actionPlaying = true;
                PlayAnimation(pushFrames);
                if (HasAnimationCompleted(pushFrames))
                {
                    actionPlaying = false;
                    ChangeState(WrestlerState.Idle);
                }
                break;

            case WrestlerState.React:
                actionPlaying = true;
                PlayAnimation(reactFrames);
                if (HasAnimationCompleted(reactFrames))
                {
                    actionPlaying = false;
                    ChangeState(WrestlerState.Idle);
                }
                break;
        }
    }

    // move character for move state
    void MoveCharacter()
    {
        float move = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        transform.Translate(move, 0, 0);
    }

    // Plays the animation for the current state
    void PlayAnimation(List<Sprite> frames)
    {
        if (frames == null || frames.Count == 0)
        {
            Debug.LogError("no frames available");
            return;
        }

        frameTimer += Time.deltaTime;

        // Check if enough time has passed before switching to the next frame
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            curFrame++;  // move to next frame

            if (curFrame >= frames.Count)
            {
                curFrame = 0;  // loop animation
            }

            // update sprite renderer with correct frame
            spriteRenderer.sprite = frames[curFrame];
        }
    }

    private bool HasAnimationCompleted(List<Sprite> frames)
    {
        return curFrame >= frames.Count - 1;  // returns true if current frame is last frame in animation
    }

    public void ChangeState(WrestlerState newState)
    {
        // reset frame counter when changing state
        curFrame = 0;
        frameTimer = 0f;  // reset frame timer
        currentState = newState;  // set new state

        // Reset the actionPlaying flag if transitioning to Idle
        if (newState == WrestlerState.Idle)
        {
            actionPlaying = false;
        }
    }
}