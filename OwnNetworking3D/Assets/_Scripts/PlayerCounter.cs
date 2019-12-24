using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCounter : MonoBehaviour
{
    public Text CounterText;
    public Image LoadingImage;

    public float speed;

    [SyncVar]
    public string PlayersCount = "0";

    private string m_PlayersCount = "0";

    void Update()
    {
        if (PlayersCount != m_PlayersCount)
        {
            //on var changed
            if (CounterText != null)
            {
                CounterText.text = PlayersCount;
            }

            m_PlayersCount = PlayersCount;
        }

        if(int.Parse(PlayersCount) > 1)
        {
            LoadingImage.fillAmount += (Time.deltaTime / speed);
        }
    }
}