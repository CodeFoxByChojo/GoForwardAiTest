using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
        [SerializeField] private TextMeshProUGUI _logText;

        [SerializeField] private Button _learningButton;
        private Field _field;

        public List<string> log = new List<string>();

        private void Start() {
            _field = Field.GetInstance();
            if (_field.Learning) {
                ChangeColorOfButton(_learningButton, Color.green);
                _learningButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Discovering";
            }
            else {
                ChangeColorOfButton(_learningButton, Color.blue);
                _learningButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Using Way Data";
            }

            StartCoroutine(LoadSliderValues());
        }

        private GameControlls() {
            
        }
        
        public static GameControlls GetInstance() {
            return FindObjectOfType<GameControlls>();
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

        private void Update() {
            _roundDurationDisplay.text = "Round Duration: " + Math.Round(_field.CurrentRoundDuration, 1);
            _generationDisplay.text = _field.Brain.FindQMatrix(_field.Goal).Generations + ". Generation";
            _aiAmountDisplay.text = "AIs on Field: " + _field.AisOnField;
            _aiAmountInGoalDisplay.text = "AIs in Goal: " + _field.AisFoundGoal;
            UpdateLog();
        }

        private void UpdateLog() {
            string text = "";
            foreach (var entry in log) {
                text = string.Concat(text,
                    "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] | " +
                    entry + Environment.NewLine);
            }

            _logText.text = text;
        }

        public void Log(string text) {
            log.Add(text);
            if (log.Count > 30) {
                log.RemoveAt(0);
            }
        }

        public void AdjustCamera(Slider slider) {
            Camera.main.orthographicSize = slider.value;
        }

        public void NewGeneration() {
            Field.GetInstance().ForceNewRound();
        }

        public void GoToMenu() {
            SceneManager.LoadScene(0);
        }

        public void ToggleLearning(Button button) {
            _field.Learning = !_field.Learning;
            if (_field.Learning) {
                ChangeColorOfButton(button, Color.green);
                button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Discovering";
                return;
            }

            button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Using Way Data";
            ChangeColorOfButton(button, Color.blue);
        }

        public void ChangeColorOfButton(Button button, Color color) {
            button.gameObject.GetComponent<Image>().color = color;
        }

        public void AiAmountChanged(Slider slider) {
            _field.AIsPerRound = GetIntAndSetTextForSliderWithTag(slider, tag);
        }

        public void RoundDurationChanged(Slider slider) {
            _field.RoundLength = GetFloatAndSetTextForSliderWithTag(slider, tag);
        }

        public void AiActionsPerSecondChanged(Slider slider) {
            _field.ActionsPerSecond = GetFloatAndSetTextForSliderWithTag(slider, tag);
        }

        private float GetFloatAndSetTextForSliderWithTag(Slider slider, string tag) {
            TextMeshProUGUI[] temp = slider.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (Helper.GetObjectWithTag(temp, Data.UiUpdateTag) != null) {
                Helper.GetObjectWithTag(temp, Data.UiUpdateTag).text = slider.value.ToString();
                return slider.value;
            }

            return 0;
        }

        private int GetIntAndSetTextForSliderWithTag(Slider slider, string tag) {
            TextMeshProUGUI[] temp = slider.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (Helper.GetObjectWithTag(temp, Data.UiUpdateTag) != null) {
                Helper.GetObjectWithTag(temp, Data.UiUpdateTag).text = slider.value.ToString();
                return (int) slider.value;
            }

            return 0;
        }

        public void ChangeHeatMapType(TMP_Dropdown dropdown) {
            _field.HeatMapType = (HeatMapType) dropdown.value;
        }

        public void GenerateNewGoal() {
            _field.GenerateNewGoal(false, true);
        }
    }
}