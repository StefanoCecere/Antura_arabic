﻿using UnityEngine;

namespace EA4S.LivingLetters
{
    // TODO refactor: group these behaviours in a folder
    public class SpecialStateEventBehaviour : StateMachineBehaviour
    {
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.SendMessage("OnActionCompleted");
        }
    }
}
