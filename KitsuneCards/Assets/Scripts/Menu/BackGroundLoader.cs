using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundLoader : MonoBehaviour
{
    public GameObject NormalLevelImage;
    public GameObject BuffandDEbufflevelImage;
    // Start is called before the first frame update
    void Start()
    {
        switch(GameModeConfig.CurrentMode)
        {
            case GameMode.Regular:
                NormalLevelImage.SetActive(true);
                BuffandDEbufflevelImage.SetActive(false);
                break;
            case GameMode.BuffAndDebuff:
                NormalLevelImage.SetActive(false);
                BuffandDEbufflevelImage.SetActive(true);
                break;
            case GameMode.BossOnly:
                NormalLevelImage.SetActive(false);
                BuffandDEbufflevelImage.SetActive(true);
                break;
            default:
                NormalLevelImage.SetActive(true);
                BuffandDEbufflevelImage.SetActive(false);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
