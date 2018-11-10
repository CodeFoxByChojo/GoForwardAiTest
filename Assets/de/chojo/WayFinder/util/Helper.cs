using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace de.chojo.WayFinder.util {
    public static class Helper {
        /// <summary>
        /// Prints a Matrix with a title. No use anymore. Heatmap is better :P
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static string PrintMatrix(float[,] array, string Title) {
            var serializedArray = "";
            var longestNumber = 0;

            //Calculate longest Number
            for (var i = 0; i < array.GetLength(0); i++) {
                for (var j = 0; j < array.GetLength(1); j++) {
                    var number = array[i, j].ToString().Length;
                    if (number > longestNumber) longestNumber = number;
                }
            }

            if (array.GetLength(0) > longestNumber) longestNumber = array.GetLength(0);

            if (array.GetLength(1) > longestNumber) longestNumber = array.GetLength(1);

            if (Title.Length > longestNumber) longestNumber = Title.Length;

            //Write Array
            for (var i = 0; i < array.GetLength(0) + 1; i++) {
                for (var j = 0; j < array.GetLength(1) + 1; j++) {
                    if (i == 0 && j == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, Title, longestNumber);
                        continue;
                    }

                    if (i == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, (j - 1).ToString(), longestNumber);
                        continue;
                    }

                    if (j == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, (i - 1).ToString(), longestNumber);
                        continue;
                    }

                    var number = array[i - 1, j - 1].ToString();
                    serializedArray = AddToStringAndFill(serializedArray, number, longestNumber);
                }

                serializedArray = string.Concat(serializedArray, "/n");
            }

            serializedArray = serializedArray.Replace("/n", Environment.NewLine);

            return serializedArray;
        }

        
        /// <summary>
        /// Fills a string with an amount of spaces
        /// </summary>
        /// <param name="OriginString"></param>
        /// <param name="StringToAdd"></param>
        /// <param name="FillTo"></param>
        /// <returns></returns>
        private static string AddToStringAndFill(string OriginString, string StringToAdd, int FillTo) {
            var fill = FillTo + 1 - StringToAdd.Length;

            for (var k = 0; k < fill; k++) StringToAdd = string.Concat(StringToAdd, " ");

            return string.Concat(OriginString, StringToAdd);
        }

        /// <summary>
        /// Calculates the Average of every number above 0
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        public static double GetAverage(List<double> List) {
            double value = 0;
            double values = 0;
            foreach (var number in List) {
                if (number > 0) {
                    value += number;
                    values++;
                }
            }

            var result = value / values;
            if (double.IsNaN(result)) {
                result = 0;
            }
            
            return result;
        }

        public static Color GetPercentAsColor(double? percent) {
            float per;
            if (percent != null) {
                per = (float) percent;
            }
            else {
                per = 0;
            }

            var blueToCyan = new Gradient();
            var cyanToGreen = new Gradient();
            var greenToYellow = new Gradient();
            var yellowToRed = new Gradient();

            var colorKey1 = new GradientColorKey[2];
            colorKey1[0].color = Color.blue;
            colorKey1[0].time = 0.0f;
            colorKey1[1].color = Color.cyan;
            colorKey1[1].time = 0.25f;

            var colorKey2 = new GradientColorKey[2];
            colorKey2[0].color = Color.cyan;
            colorKey2[0].time = 0.25f;
            colorKey2[1].color = Color.green;
            colorKey2[1].time = 0.5f;

            var colorKey3 = new GradientColorKey[2];
            colorKey3[0].color = Color.green;
            colorKey3[0].time = 0.5f;
            colorKey3[1].color = Color.yellow;
            colorKey3[1].time = 0.75f;

            var colorKey4 = new GradientColorKey[2];
            colorKey4[0].color = Color.yellow;
            colorKey4[0].time = 0.75f;
            colorKey4[1].color = Color.red;
            colorKey4[1].time = 1f;

            var alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1f;
            alphaKey[0].time = 1f;
            alphaKey[1].alpha = 1f;
            alphaKey[1].time = 1f;

            blueToCyan.SetKeys(colorKey1, alphaKey);
            cyanToGreen.SetKeys(colorKey2, alphaKey);
            greenToYellow.SetKeys(colorKey3, alphaKey);
            yellowToRed.SetKeys(colorKey4, alphaKey);

            if (percent <= 0.25f) {
                return blueToCyan.Evaluate(per);
            }

            if (percent > 0.25f && percent <= 0.50f) {
                return cyanToGreen.Evaluate(per);
            }

            if (percent > 0.50f && percent <= 0.75f) {
                return greenToYellow.Evaluate(per);
            }

            return yellowToRed.Evaluate(per);
        }

        public static List<QMatrixMemory> AddListToList(List<QMatrixMemory> firstList, List<QMatrixMemory> secondList) {
            var value = new List<QMatrixMemory>();
            foreach (var entry in firstList) {
                value.Add(entry);
            }

            if (secondList == null) return value;
            foreach (var entry in secondList) {
                value.Add(entry);
            }

            return value;
        }

        public static TextMeshProUGUI GetObjectWithTag(TextMeshProUGUI[] texts, string tag) {
            foreach (var entry in texts) {
                if (entry.tag.ToLower().Equals(tag.ToLower())) {
                    return entry;
                }
            }

            return null;
        }
        public static GameObject GetObjectWithTag(IEnumerable<GameObject> gameObjects, string tag) {
            foreach (var entry in gameObjects) {
                if (entry.tag.Equals(tag)) {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a string to float. Rounds to digits.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="decimalPoints"></param>
        /// <returns>String as float if parse is valid. 0 if not.</returns>
        public static float StringToFloat(string text, int decimalPoints) {
            float result;

            text = text.Replace(",", ".");
            if (float.TryParse(text, out result)) {
                result = (float) Math.Round(result, decimalPoints);
                return result;
            }

            return 0;
        }
        
        /// <summary>
        /// Converts a string to float. Rounds to two digits. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>String as float if parse is valid. 0 if not.</returns>
        public static float StringToFloat(string text) {
            float result;

            text = text.Replace(",", ".");
            if (float.TryParse(text, out result)) {
                result = (float) Math.Round(result, 2);
                return result;
            }

            return 0;
        }
        
        /// <summary>
        /// Converts string to Integer.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>String as int if parse is valid. 0 if not.</returns>
        public static int StringToInt(string text) {
            float result;

            text = text.Replace(",", ".");
            if (float.TryParse(text, out result)) {
                return (int)result;
            }

            return 0;
        }

        public static float ClampFloat(float value, float min, float max, out bool changed) {
            changed = true;
            if (value > max) return max;
            if (value < min) return min;
            changed = false;
            return value;
        }

        public static int ClampInt(int value, int min, int max, out bool changed) {
            changed = true;
            if (value > max) return max;
            if (value < min) return min;
            return value;

        }
        
    }
}


public static class defines {
    public struct pos {
        public int x;
        public int y;
    }
}


public enum Directions {
    Up,
    Down,
    Right,
    Left,
    none
}

public enum HeatMapType{
    BestWay,
    Visits
}

public static class Data {
    private static readonly string _uiUpdateTag = "UiUpdateText";

    public static string UiUpdateTag {
        get { return _uiUpdateTag; }
    }
}