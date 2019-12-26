using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _NM_Getter : MonoBehaviour
{
    public NetworkManager NM;

    private void Awake()
    {
        NM = FindObjectOfType<NetworkManager>();
        print("XD1");
    }

    public void Start()
    {
        print("XD2");
    }

    public void OnEnable()
    {
        NM = FindObjectOfType<NetworkManager>();
        print("XD3");
    }

    public void SendCommand(string cmd)
    {
        NM.SendCommand(cmd);
        foreach(Rpcs s in NM.rpclist)
        {
            print(s.cmd);
        }
    }
}