using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JohnStateMachine : MonoBehaviour
{
    //movement
    public float speed;
    private Rigidbody2D rb; 
    private Vector3 change;

    //animation
    private Animator animator; 

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetFloat("moveX", 0);
        animator.SetFloat("moveY", -1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        change = Vector3.zero;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        UpdateAnimationAndMove();
    }

    void UpdateAnimationAndMove(){
        if(change != Vector3.zero){
            MoveCharacter();
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
        }
        else{
            animator.SetBool("moving", false);
        }
    }

    void MoveCharacter(){
        change.Normalize();
        rb.MovePosition(
            transform.position + change.normalized * speed * Time.deltaTime
        );
    }

}
