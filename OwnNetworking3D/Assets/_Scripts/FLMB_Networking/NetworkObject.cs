using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class NetworkObject : MonoBehaviour
{
    [Range(1, 30)]
    public float NetworkSendRate = 1;

    public string uniqueId;
    public string nick;

    public NetworkManager NM;
    public Pos pos;
    public bool CanMove = false;

    public GameObject Glasses;


    public void Awake()
    {
        //uniqueId = Random.Range(0, 1000000).ToString();
        NM = FindObjectOfType<NetworkManager>();
        if (CanMove)
            StartCoroutine(SyncMovement());
    }

    [Rpc]
    public void ObjectMovement(string uId, string x, string y, string z, string rx, string ry, string rz, string rw)
    {
        if (uId == uniqueId)
        {
            this.transform.SetPositionAndRotation(new Vector3 { x = float.Parse(x), y = float.Parse(y), z = float.Parse(z) }, new Quaternion { x = float.Parse(rx), y = float.Parse(ry), z = float.Parse(rz), w = float.Parse(rw) });
            pos.x = x;
            pos.y = y;
            pos.z = z;
            pos.rx = rx;
            pos.ry = ry;
            pos.rz = rz;
            pos.rw = rw;
        }
    }

    void Update()
    {
        if(NM == null)
            NM = FindObjectOfType<NetworkManager>();
    }

    public void MoveRefresh()
    {
        StopCoroutine(SyncMovement());
        if (CanMove)
            StartCoroutine(SyncMovement());
    }

    public void ShowGlasses()
    {
        SetLayer(Glasses, 0);
    }

    public void SetLayer(GameObject gb, int layer)
    {
        gb.layer = layer;

        foreach(Transform t in gb.transform)
        {
            SetLayer(t.gameObject, layer);
        }
    }

    public IEnumerator SyncMovement()
    {
        yield return new WaitForEndOfFrame();
        yes:
        yield return new WaitForSeconds(NetworkSendRate / 1000);
        if (this.transform.position.x.ToString() != pos.x || this.transform.position.y.ToString() != pos.y || this.transform.position.z.ToString() != pos.z || this.transform.rotation.x.ToString() != pos.rx || this.transform.rotation.y.ToString() != pos.ry || this.transform.rotation.z.ToString() != pos.rz || this.transform.rotation.w.ToString() != pos.rw)
        {
            pos.x = this.transform.position.x.ToString();
            pos.y = this.transform.position.y.ToString();
            pos.z = this.transform.position.z.ToString();
            pos.rx = this.transform.rotation.x.ToString();
            pos.ry = this.transform.rotation.y.ToString();
            pos.rz = this.transform.rotation.z.ToString();
            pos.rw = this.transform.rotation.w.ToString();
            NM.SendCommand($"[ObjectMovement] {uniqueId}:{this.transform.position.x}:{this.transform.position.y}:{this.transform.position.z}:{this.transform.rotation.x.ToString()}:{this.transform.rotation.y.ToString()}:{this.transform.rotation.z.ToString()}:{this.transform.rotation.w.ToString()}");
        }
        goto yes;
    }
}

[Serializable]
public class Pos
{
    public string x;
    public string y;
    public string z;
    public string rx;
    public string ry;
    public string rz;
    public string rw;
}