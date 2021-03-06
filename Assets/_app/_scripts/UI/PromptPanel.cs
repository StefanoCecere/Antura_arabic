﻿using System;
using UnityEngine;
using UnityEngine.UI;
using EA4S.Core;
using EA4S.Audio;
using DG.DeExtensions;
using DG.Tweening;

namespace EA4S.UI
{
    /// <summary>
    /// Shows an interactive YES/NO prompt panel.
    /// Uses callbacks to determine how to react to user input.
    /// </summary>
    public class PromptPanel : MonoBehaviour
    {
        public RectTransform Content;
        public TextRender TfMessageArabic;
        public TextRender TfMessageEnglish;
        public TextRender TfMessageEnglishFull;
        public UIButton BtYes, BtNo;

        Action onYes, onNo;
        Action onCloseAction;
        float defYesX;
        RectTransform btYesRT;
        Tween showTween;

        #region Unity

        void Awake()
        {
            btYesRT = BtYes.GetComponent<RectTransform>();
            defYesX = btYesRT.anchoredPosition.x;
            showTween = DOTween.Sequence().SetUpdate(true).SetAutoKill(false).Pause()
                .Append(this.GetComponent<Image>().DOFade(0, 0.35f).From())
                .Join(Content.DOScale(0.0001f, 0.35f).From().SetEase(Ease.OutBack))
                .OnRewind(() => {
                    this.gameObject.SetActive(false);
                    OnClose();
                });

            BtYes.Bt.onClick.AddListener(() => OnClick(BtYes));
            BtNo.Bt.onClick.AddListener(() => OnClick(BtNo));

            this.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            showTween.Kill();
            BtYes.Bt.onClick.RemoveAllListeners();
            BtNo.Bt.onClick.RemoveAllListeners();
        }

        #endregion

        #region Public Methods

        public void Show(Database.LocalizationDataId id, Action _onYes, Action _onNo)
        {
            var localizationData = LocalizationManager.GetLocalizationData(id);
            AudioManager.I.PlayDialogue(localizationData);
            Show(LocalizationManager.GetTranslation(id),
                localizationData.English, _onYes, _onNo);
        }

        public void Show(string _messageAr, Action _onYes, Action _onNo)
        {
            Show(_messageAr, "", _onYes, _onNo);
        }

        public void Show(string _messageAr, string _messageEn, Action _onYes, Action _onNo)
        {
            onCloseAction = null;
            if (_messageAr.IsNullOrEmpty()) {
                TfMessageEnglishFull.text = _messageEn;
            } else {
                TfMessageArabic.text = _messageAr.IsNullOrEmpty() ? "" : _messageAr;
                TfMessageEnglish.text = _messageEn.IsNullOrEmpty() ? "" : _messageEn;
                TfMessageEnglishFull.text = "";
            }
            onYes = _onYes;
            onNo = _onNo;
            btYesRT.SetAnchoredPosX(_onNo == null ? 0 : defYesX);
            BtNo.gameObject.SetActive(onNo != null);
            showTween.Restart();
            this.gameObject.SetActive(true);
        }

        public void Close()
        {
            showTween.PlayBackwards();
        }

        #endregion

        #region Callbacks

        void OnClick(UIButton _bt)
        {
            if (showTween.IsBackwards()) return;

            if (_bt == BtYes && onYes != null) onCloseAction = onYes;
            else if (_bt == BtNo && onNo != null) onCloseAction = onNo;
            Close();
        }

        void OnClose()
        {
            if (onCloseAction != null) onCloseAction();
            onCloseAction = null;
        }

        #endregion
    }
}