﻿using EA4S.Audio;
using EA4S.LivingLetters;
using EA4S.MinigamesCommon;
using EA4S.Tutorial;
using UnityEngine;

namespace EA4S.Minigames.MissingLetter
{

    public class MissingLetterTutorialState : IState
    {

        public MissingLetterTutorialState(MissingLetterGame _game)
        {
            this.m_oGame = _game;
        }

        public void EnterState()
        {
            AudioManager.I.PlayMusic(Music.Theme6);
            m_oGame.m_oRoundManager.SetTutorial(true);
            m_oGame.m_oRoundManager.NewRound();
            m_oGame.m_oRoundManager.onAnswered += OnRoundResult;
        }


        public void ExitState()
        {
            TutorialUI.Clear(true);
            m_oGame.m_oRoundManager.SetTutorial(false);
            m_oGame.m_oRoundManager.onAnswered -= OnRoundResult;
        }

        void OnRoundResult(bool _result)
        {
            if (_result) {
                //TODO: tutorial finito ... mostrare qualcosa a livello di UI ?
                m_oGame.SetCurrentState(m_oGame.PlayState);
            } else {
                var _LL = m_oGame.m_oRoundManager.GetCorrectLLObject();
                _LL.GetComponent<LetterBehaviour>().PlayAnimation(LLAnimationStates.LL_dancing);
            }
        }

        public void Update(float delta)
        {
            m_fDelayTime -= delta;
            if (m_fDelayTime < 0 && !m_bSuggested) {
                if (MissingLetterConfiguration.Instance.Variation == MissingLetterVariation.MissingLetter ||
                    MissingLetterConfiguration.Instance.Variation == MissingLetterVariation.MissingForm) {
                    AudioManager.I.PlayDialogue(Database.LocalizationDataId.MissingLetter_Tuto);
                } else {
                    AudioManager.I.PlayDialogue(Database.LocalizationDataId.MissingLetter_phrases_Tuto);
                }

                m_oGame.m_oRoundManager.GetCorrectLLObject().GetComponent<LetterBehaviour>().PlayAnimation(LLAnimationStates.LL_dancing);
                Vector3 pos = m_oGame.m_oRoundManager.GetCorrectLLObject().transform.position + Vector3.back * 0.8f + Vector3.up * 3;
                TutorialUI.ClickRepeat(pos, 90, 1.5f);
                m_bSuggested = true;
            }
        }

        public void UpdatePhysics(float delta)
        {

        }

        MissingLetterGame m_oGame;
        float m_fDelayTime = 2f;
        bool m_bSuggested = false;
    }
}
