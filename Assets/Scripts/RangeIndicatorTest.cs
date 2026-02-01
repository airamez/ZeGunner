using UnityEngine;

public class RangeIndicatorTest : MonoBehaviour
{
    void Start()
    {
        // Create RangeIndicator manually
        GameObject rangeIndicatorObj = new GameObject("RangeIndicator");
        RangeIndicator rangeIndicator = rangeIndicatorObj.AddComponent<RangeIndicator>();
        // Force it to initialize immediately
        rangeIndicator.SendMessage("Start");
    }
}
