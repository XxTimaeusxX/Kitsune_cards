using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameTurnMessager : MonoBehaviour
{
    public static GameTurnMessager instance;
    public TMP_Text messageText;
    private void Awake()
    {
        instance = this;
        if (messageText != null)
            messageText.text = "";
    }

    public void ShowMessage(string msg )
    {
        StopAllCoroutines();
        messageText.text = msg;
        
    }

    private System.Collections.IEnumerator ClearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }
}
