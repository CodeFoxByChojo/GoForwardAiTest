using de.chojo.WayFinder.Manager;
using de.chojo.WayFinder.util;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public void StartGame() {
            PlayerPrefsHandler.SetAiAmount(_aiAmount);
            PlayerPrefsHandler.SetRoundDuration(_roundDuration);
            PlayerPrefsHandler.SetAiActionsPerSecond(_aiActionsPerSecond);
            PlayerPrefsHandler.SetDimensionX(_dimensionX);
            PlayerPrefsHandler.SetDimensionY(_dimensionY);
            PlayerPrefsHandler.SetGoalX(_goalX);
            PlayerPrefsHandler.SetGoalY(_goalY);
            SceneManager.LoadSceneAsync(1);
            Debug.Log(PlayerPrefsHandler.GetAiAmount().ToString() + PlayerPrefsHandler.GetDimensionX());
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

        public void DimensionsXChanged(TextMeshProUGUI text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text), 10, 250, out changed);
            //TODO: Error Message
            _dimensionX = value;
            text.text = value.ToString();
            _goalX = Helper.ClampInt(_goalX, 0, _dimensionX, out changed);
        }

        public void DimensionsYChanged(TextMeshProUGUI text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text), 10, 250, out changed);
            //TODO: Error Message
            _dimensionY = value;
            text.text = value.ToString();
            _goalY = Helper.ClampInt(_goalY, 0, _dimensionY, out changed);
        }

        public void GoalXChanged(TextMeshProUGUI text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text), 1, _dimensionX - 1, out changed);
            //TODO: Error Message
            _goalX = value;
            text.text = value.ToString();
        }

        public void GoalYChanged(TextMeshProUGUI text) {
            bool changed;
            var value = Helper.ClampInt(Helper.StringToInt(text.text), 1, _dimensionY - 1, out changed);
            //TODO: Error Message
            _goalX = value;
            Debug.Log("Set Goal Y to: " + value);
            text.text = value.ToString();
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