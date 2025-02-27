using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine : MonoBehaviour
{
    public enum WrestlerState
    {
        Idle,
        Move,
        Punch,
        Push,
        React,
        Swing, //state for swing remains the same even if back, just the texture/source/base mask changes 
        Jump,
        Fall,
        GetOut
    }

    public WrestlerState currentState;

    // uv mapping script reference
    public UVMappin uvMappingScript;

    // frame sets for each animation state
    private List<Sprite> idleFrames;
    private List<Sprite> moveFrames;
    private List<Sprite> punchFrames;
    private List<Sprite> swingFrames;
    private List<Sprite> swingBackFrames;
    private List<Sprite> pushFrames;
    private List<Sprite> reactFrames;
    private List<Sprite> jumpFrames;
    private List<Sprite> fallFrames;
    private List<Sprite> getOutFrames;

    // Movement variables
    public float moveSpeed = 3f;
    private bool canMove = true;
    public bool actionPlaying;
    public bool swingingBack = false;

    private int curFrame = 0;
    private float frameDuration = 0.1f; // time per frame
    private SpriteRenderer spriteRenderer;

    public WrestlerAnimationSet animationSet;

    Texture2D chosenSourceTexture;

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

        // Pick a random source texture
        if(CompareTag("Enemy") || CompareTag("Goon")){
            int randomIndex = Random.Range(0, 4);  // Pick a number between 0 and 3

            switch (randomIndex)
            {
                case 0: chosenSourceTexture = animationSet.sourceTexture1; break;
                case 1: chosenSourceTexture = animationSet.sourceTexture2; break;
                case 2: chosenSourceTexture = animationSet.sourceTexture3; break;
                case 3: chosenSourceTexture = animationSet.sourceTexture4; break;
                default: chosenSourceTexture = animationSet.sourceTexture1; break;  // Fallback
            }
        } else{
            chosenSourceTexture = animationSet.sourceTexture1;
        }

        // Load optional animations only if they are assigned
        if (animationSet.jumpTexture != null)
        {
            uvMappingScript._texture = animationSet.jumpTexture;
            jumpFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);
        }
        else
        {
            jumpFrames = new List<Sprite>();  // Avoids null reference errors
        }

        if (animationSet.fallTexture != null)
        {
            uvMappingScript._texture = animationSet.fallTexture;
            fallFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);
        }
        else
        {
            fallFrames = new List<Sprite>(); 
        }

        if (animationSet.getOutTexture != null)
        {
            uvMappingScript._texture = animationSet.getOutTexture;
            getOutFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);
        }
        else
        {
            getOutFrames = new List<Sprite>(); 
        }

        uvMappingScript._texture = animationSet.idleTexture;
        idleFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.moveTexture;
        moveFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.punchTexture;
        punchFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.swingTexture;
        swingFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.pushTexture;
        pushFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

        uvMappingScript._texture = animationSet.reactTexture;
        reactFrames = uvMappingScript.MapOntoTexture(chosenSourceTexture, animationSet.baseMask);

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

            case WrestlerState.Jump:
                if (jumpFrames.Count > 0)  // Only play if frames exist
                {
                    actionPlaying = true;
                    PlayAnimation(jumpFrames);
                    if (HasAnimationCompleted(jumpFrames))
                    {
                        actionPlaying = false;
                        ChangeState(WrestlerState.Fall);
                    }
                }
                else
                {
                    ChangeState(WrestlerState.Fall);  // Skip to the next state
                }
                break;

            case WrestlerState.Fall:
                if (fallFrames.Count > 0)
                {
                    actionPlaying = true;
                    PlayAnimation(fallFrames);
                    if (HasAnimationCompleted(fallFrames))
                    {
                        actionPlaying = false;
                        ChangeState(WrestlerState.GetOut);
                    }
                }
                else
                {
                    ChangeState(WrestlerState.GetOut);  // Skip if missing
                }
                break;

            case WrestlerState.GetOut:
                if (getOutFrames.Count > 0)
                {
                    actionPlaying = true;
                    PlayAnimation(getOutFrames);
                    if (HasAnimationCompleted(getOutFrames))
                    {
                        actionPlaying = false;
                        ChangeState(WrestlerState.Idle);
                    }
                }
                else
                {
                    ChangeState(WrestlerState.Idle);  // Skip if missing
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