using System;
using System.Collections;
using de.chojo.WayFinder.Manager;
using de.chojo.WayFinder.util;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace de.chojo.WayFinder.Menu {
    public class GameControlls : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _roundDurationDisplay;
        [SerializeField] private TextMeshProUGUI _generationDisplay;
        [SerializeField] private TextMeshProUGUI _aiAmountDisplay;
        [SerializeField] private TextMeshProUGUI _aiAmountInGoalDisplay;
        [SerializeField] private Slider _aiAmount;
        [SerializeField] private Slider _roundDuration;
        [SerializeField] private Slider _actionsPerSecond;

        [SerializeField] private Button _learningButton;
        private void Start() {
            _field = Field.GetInstance();
            if (_field.Learning) ChangeColorOfButton(_learningButton, Color.green);
            else ChangeColorOfButton(_learningButton, Color.red);
            StartCoroutine(LoadSliderValues());
        }

        private IEnumerator LoadSliderValues() {
            yield return new WaitForSeconds(0.2f);
            _aiAmount.value = _field.AIsPerRound;
            GetIntAndSetTextForSliderWithTag(_aiAmount, Data.UiUpdateTag);
            _roundDuration.value = _field.RoundLength;
            GetIntAndSetTextForSliderWithTag(_roundDuration, Data.UiUpdateTag);
            _actionsPerSecond.value = _field.ActionsPerSecond;
            GetIntAndSetTextForSliderWithTag(_actionsPerSecond, Data.UiUpdateTag);
        }
