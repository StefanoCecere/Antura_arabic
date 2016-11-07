﻿using System;
using TMPro;
using UnityEngine;
using System.Collections;

namespace EA4S.SickLetters
{
    public class IntroductionGameState : IGameState
    {
        SickLettersGame game;

        float timer = 1;
        public IntroductionGameState(SickLettersGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {
            Debug.Log("enter intro");
        }

        public void ExitState()
        {
            Debug.Log("exit intro");
        }

        public void Update(float delta)
        {
            timer -= delta;

            if (timer < 0)
            {
                game.SetCurrentState(game.QuestionState);
            }
           
        }

        public void UpdatePhysics(float delta)
        {
        }
    }
}