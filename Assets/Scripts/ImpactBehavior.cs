﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactBehavior : StateMachineBehaviour {
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(animator.gameObject.CompareTag("Monster")) {
            animator.gameObject.GetComponent<Monster>().Release();
        }
        else if(animator.gameObject.CompareTag("Projectile"))
            GameManager.Instance.objectManager.ReleaseObject(animator.gameObject);
        else if(animator.gameObject.CompareTag("Soldier"))
            animator.gameObject.GetComponent<Soldier>().Release(true, false);
        else if(animator.gameObject.name.Contains("Canvas"))
            MainMenuManager.Instance.LoadScene(2);
        else if(animator.gameObject.name.Contains("XSign"))
            animator.gameObject.SetActive(false);

    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
