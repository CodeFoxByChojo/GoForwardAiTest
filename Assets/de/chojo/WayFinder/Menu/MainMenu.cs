using de.chojo.WayFinder.Manager;
using de.chojo.WayFinder.util;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace de.chojo.WayFinder.Menu {
    public class MainMenu : MonoBehaviour {
        private int _aiAmount = 1;
        private float _roundDuration = 30;
        private float _aiActionsPerSecond = 0.2f;
        private int _dimensionX = 10;
        private int _dimensionY = 10;
        private int _goalX = 5;
        private int _goalY = 5;

        [SerializeField] private TMP_InputField _dimensionXDisplay;
        [SerializeField] private TMP_InputField _dimensionYDisplay;
        [SerializeField] private TMP_InputField _goalXDisplay;
        [SerializeField] private TMP_InputField _goalYDisplay;
        [SerializeField] private Slider _aiAmountSlider;
        [SerializeField] private Slider _aiActionSlider;
        [SerializeField] private Slider _roundDurationSlider;

        private void Start() {
            _dimensionXDisplay.text = _dimensionX.ToString();
            _dimensionYDisplay.text = _dimensionY.ToString();
            _goalXDisplay.text = _goalX.ToString();
            _goalYDisplay.text = _goalY.ToString();
            AiAmountChanged(_aiAmountSlider);
            RoundDurationChanged(_roundDurationSlider);
            AiActionsPerSecondChanged(_aiActionSlider);
        }

        public void StartGame() {
            PlayerPrefsHandler.SetAiAmount(_aiAmount);
            PlayerPrefsHandler.SetRoundDuration(_roundDuration);
            PlayerPrefsHandler.SetAiActionsPerSecond(_aiActionsPerSecond);
            PlayerPrefsHandler.SetDimensionX(Helper.StringToInt(_dimensionXDisplay.text,0));
            PlayerPrefsHandler.SetDimensionY(Helper.StringToInt(_dimensionYDisplay.text,0));
            PlayerPrefsHandler.SetGoalX(Helper.StringToInt(_goalXDisplay.text,0));
            PlayerPrefsHandler.SetGoalY(Helper.StringToInt(_goalYDisplay.text,0));
            SceneManager.LoadSceneAsync(1);
        }

        public void AiAmountChanged(Slider slider) {
            _aiAmount = GetIntAndSetTextForSliderWithTag(slider, tag);
        }

        public void RoundDurationChanged(Slider slider) {
            _roundDuration = GetFloatAndSetTextForSliderWithTag(slider, tag);
        }

        public void AiActionsPerSecondChanged(Slider slider) {
            _aiActionsPerSecond = GetFloatAndSetTextForSliderWithTag(slider, tag);
        }

        public void DimensionsXChanged(TMP_InputField text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text,1), 10, 250, out changed);
            //TODO: Error Message
            _dimensionX = value;
            _dimensionXDisplay.text = value.ToString();
            _goalX = Helper.ClampInt(_goalX, 0, _dimensionX, out changed);
        }

        public void DimensionsYChanged(TMP_InputField text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text,1), 10, 250, out changed);
            //TODO: Error Message
            _dimensionY = value;
            _dimensionYDisplay.text = value.ToString();
            _goalY = Helper.ClampInt(_goalY, 0, _dimensionY, out changed);
        }

        public void GoalXChanged(TMP_InputField text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text,1), 1, _dimensionX - 1, out changed);
            //TODO: Error Message
            _goalX = value;
            _goalXDisplay.text = value.ToString();
        }

        public void GoalYChanged(TMP_InputField text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text,1), 1, _dimensionY - 1, out changed);
            //TODO: Error Message
            _goalX = value;
            _goalYDisplay.text = value.ToString();
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
    }
}