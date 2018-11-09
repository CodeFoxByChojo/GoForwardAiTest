using UnityEngine;
using UnityEngine.UI;

namespace de.chojo.WayFinder.Menu {
    [AddComponentMenu("UI/CustomSlider", 34)]
    public class CustomSlider : Slider {
        [SerializeField] private Text _text;
    }
}