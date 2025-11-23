using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60; // Or -1 to use native refresh rate if supported
    }
}
