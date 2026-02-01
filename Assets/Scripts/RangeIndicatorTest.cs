using UnityEngine;

public class RangeIndicatorTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("RangeIndicatorTest: Starting manual RangeIndicator creation");
        
        // Create RangeIndicator manually
        GameObject rangeIndicatorObj = new GameObject("RangeIndicator");
        RangeIndicator rangeIndicator = rangeIndicatorObj.AddComponent<RangeIndicator>();
        
        Debug.Log($"RangeIndicatorTest: RangeIndicator created: {rangeIndicator != null}");
        
        // Force it to initialize immediately
        rangeIndicator.SendMessage("Start");
        
        Debug.Log("RangeIndicatorTest: Manual creation complete");
    }
}
