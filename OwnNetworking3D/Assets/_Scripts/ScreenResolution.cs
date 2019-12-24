using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenResolution : MonoBehaviour
{
    public InputField W, H;
    public Toggle FT;

    void Awake()
    {

    }

    public void ChangeResolution()
    {
        Screen.SetResolution(int.Parse(W.text), int.Parse(H.text), FT.isOn);
    }
}