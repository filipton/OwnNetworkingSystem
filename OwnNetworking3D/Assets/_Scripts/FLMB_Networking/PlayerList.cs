using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkManager))]
public class PlayerList : MonoBehaviour
{
    public List<Player> Players = new List<Player>();
    NetworkManager _NetworkManager;

    public GameObject PlayerListParent, PlayerPrefab;

    private void Awake()
    {
        _NetworkManager = GetComponent<NetworkManager>();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if(PlayerListParent == null)
        {
            PlayerListParent = GameObject.Find("PlayerParent");
        }
        if(arg1.name == "SampleScene")
        {
            for(int i = 0; i < Players.Count; i++)
            {
                print(Players[i].NickName);
                StartCoroutine(AddClients(Players[i]));
            }
        }
    }

    void PrintAllPlayers()
    {
        string toPrint = "";
        for(int i = 0; i < Players.Count; i++)
        {
            toPrint += $"{Players[i].NickName}, {Players[i].ps.hp}, {Players[i].ps.item}";
            if((i - 1) < Players.Count)
            {
                toPrint += " : ";
            }
        }
        print($"Players Connected: {toPrint}");
    }

    [Rpc]
    public void AddClient(string Nick)
    {
        Player p = new Player { NickName = Nick, ps = new PlayerStats { hp = 100f, item = "sword" } };
        Players.Add(p);
        PrintAllPlayers();
        if(PlayerListParent != null && SceneManager.GetActiveScene().name == "SampleScene")
        {
            StartCoroutine(AddClients(p));
        }
    }

    [Rpc]
    public void RemoveClient(string Nick)
    {
        Player prm = Players.Find(x => x.NickName == Nick);
        Players.Remove(prm);
        PrintAllPlayers();
    }

    public IEnumerator AddClients(Player p)
    {
        yield return new WaitForEndOfFrame();
        GameObject g = Instantiate(PlayerPrefab, PlayerListParent.transform);
        g.GetComponent<TextMeshProUGUI>().text = p.NickName;
    }
}

[Serializable]
public class Player
{
    public string NickName;
    public PlayerStats ps;
    //Something else
}

[Serializable]
public class PlayerStats
{
    public float hp;
    public string item;
}