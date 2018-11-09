using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
            var blueToCyan = new Gradient();
            var cyanToGreen = new Gradient();
            var greenToYellow = new Gradient();
            var yellowToRed = new Gradient();

            var colorKey1 = new GradientColorKey[2];
            colorKey1[0].color = Color.blue;
            colorKey1[0].time = 0.0f;
            colorKey1[1].color = Color.cyan;
            colorKey1[1].time = 0.5f;

            var colorKey2 = new GradientColorKey[2];
            colorKey2[0].color = Color.cyan;
            colorKey2[0].time = 0.0f;
            colorKey2[1].color = Color.green;
            colorKey2[1].time = 0.5f;

            var colorKey3 = new GradientColorKey[2];
            colorKey3[0].color = Color.green;
            colorKey3[0].time = 0.0f;
            colorKey3[1].color = Color.yellow;
            colorKey3[1].time = 0.5f;

            var colorKey4 = new GradientColorKey[2];
            colorKey4[0].color = Color.yellow;
            colorKey4[0].time = 0.5f;
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
                return blueToCyan.Evaluate((float) percent);
            }

            if (percent > 25 && percent <= 50) {
                return cyanToGreen.Evaluate((float) percent);
            }

            if (percent > 50 && percent <= 75) {
                return greenToYellow.Evaluate((float) percent);
            }

            return yellowToRed.Evaluate((float) percent);
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