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
