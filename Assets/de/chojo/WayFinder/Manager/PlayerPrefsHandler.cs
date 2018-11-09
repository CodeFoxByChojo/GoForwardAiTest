using UnityEngine;

namespace de.chojo.WayFinder.Manager {
    public static class PlayerPrefsHandler {
        private static string keyAiAmount = "AiAmount";
        private static string keyRoundDuration = "RoundDuration";
        private static string keyAiActionsPerSecond = "AiActionsPerSecond";
        private static string keyDimensionX = "DimensionX";
        private static string keyDimensionY = "DimensionY";
        private static string keyGoalX = "GoalX";
        private static string keyGoalY = "GoalY";

        public static void SetAiAmount(int aiAmount) {
            PlayerPrefs.SetInt(keyAiAmount, aiAmount);
        }

        public static int GetAiAmount() {
            return PlayerPrefs.GetInt(keyAiAmount);
        }

        public static void SetRoundDuration(float roundDuration) {
            PlayerPrefs.SetFloat(keyRoundDuration, roundDuration);
        }

        public static float GetRoundDuration() {
            return PlayerPrefs.GetFloat(keyRoundDuration);
        }

        public static void SetAiActionsPerSecond(float aiActionsPerSecond) {
            PlayerPrefs.SetFloat(keyAiActionsPerSecond, aiActionsPerSecond);
        }

        public static float GetAiActionsPerSecond() {
            return PlayerPrefs.GetFloat(keyAiActionsPerSecond);
        }

        public static void SetDimensionX(int dimensionx) {
            PlayerPrefs.SetInt(keyDimensionX, dimensionx);
        }

        public static int GetDimensionX() {
            return PlayerPrefs.GetInt(keyDimensionX);
        }

        public static void SetDimensionY(int dimensionY) {
            PlayerPrefs.SetInt(keyDimensionY, dimensionY);
        }

        public static int GetDimensionY() {
            return PlayerPrefs.GetInt(keyDimensionY);
        }

        public static void SetGoalX(int goalX) {
            PlayerPrefs.SetInt(keyGoalX, goalX);
        }

        public static int GetGoalX() {
            return PlayerPrefs.GetInt(keyGoalX);
        }

        public static void SetGoalY(int goalY) {
            PlayerPrefs.SetInt(keyGoalY, goalY);
        }

        public static int GetGoalY() {
            return PlayerPrefs.GetInt(keyGoalY);
        }
    }
}