﻿using EA4S.Audio;
using EA4S.MinigamesCommon;

namespace EA4S.Minigames.DancingDots
{
    public class PlayGameState : IState
    {
        DancingDotsGame game;

        float timer;
        bool alarmIsTriggered = false;

        public PlayGameState(DancingDotsGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {
            AudioManager.I.PlayDialogue("DancingDots_Intro", ()=> { game.disableInput = false; });
            this.game.dancingDotsLL.contentGO.SetActive(true);
            game.StartRound();
            timer = game.gameDuration;
        }

        public void ExitState()
        {
            //UnityEngine.Debug.Log(1111111);
            //game.DancingDotsEndGame();
        }

        public void Update(float delta)
        {
            if (!game.isTutRound)
            {
                timer -= delta;
                game.Context.GetOverlayWidget().SetClockTime(timer);
            }

            if (timer < 0)
            {
                game.Context.GetOverlayWidget().OnClockCompleted();
                game.SetCurrentState(game.ResultState);
                AudioManager.I.PlayDialogue("Keeper_TimeUp");
            }

            else if (!alarmIsTriggered && timer < 20)
            {
                alarmIsTriggered = true;
                AudioManager.I.PlayDialogue("Keeper_Time_" + UnityEngine.Random.Range(1, 4));
            }
        }

        public void UpdatePhysics(float delta)
        {
        }
    }
}
