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
using System.IO;

public class NetworkObject : MonoBehaviour
{
    [Range(1, 30)]
    public float NetworkSendRate = 1;

    public string uniqueId;
    public string nick;

    public NetworkManager NM;
    public Pos pos;
    public bool CanMove = false;

    public GameObject Body;

    public void Awake()
    {
        //uniqueId = Random.Range(0, 1000000).ToString();
        NM = FindObjectOfType<NetworkManager>();
    }

    [Rpc]
    public void ObjectMovement(string x, string y, string z, string rx, string ry, string rz, string rw)
    {
        try
        {
            float ox, oy, oz, orx, ory, orz, orw;
            if (float.TryParse(x, out ox) && float.TryParse(y, out oy) && float.TryParse(z, out oz) && float.TryParse(rx, out orx) && float.TryParse(ry, out ory) && float.TryParse(rz, out orz) && float.TryParse(rw, out orw))
            {
                //this.transform.SetPositionAndRotation(new Vector3 { x = float.Parse(x), y = float.Parse(y), z = float.Parse(z) }, new Quaternion { x = float.Parse(rx), y = float.Parse(ry), z = float.Parse(rz), w = float.Parse(rw) });
                Vector3 vector = new Vector3 { x = ox, y = oy, z = oz };
                Quaternion rotation = new Quaternion { x = orx, y = ory, z = orz, w = orw };
                this.transform.position = vector;
                this.transform.rotation = rotation;
                pos.x = ox;
                pos.y = oy;
                pos.z = oz;
                pos.rx = orx;
                pos.ry = ory;
                pos.rz = orz;
                pos.rw = orw;
            }
        }
        catch(Exception e)
        {
            print(e);
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
        SetLayer(Body, 0);
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
        if (this.transform.position.x != pos.x || this.transform.position.y != pos.y || this.transform.position.z != pos.z || this.transform.rotation.x != pos.rx || this.transform.rotation.y != pos.ry || this.transform.rotation.z != pos.rz || this.transform.rotation.w != pos.rw)
        {
            pos.x = this.transform.position.x;
            pos.y = this.transform.position.y;
            pos.z = this.transform.position.z;
            pos.rx = this.transform.rotation.x;
            pos.ry = this.transform.rotation.y;
            pos.rz = this.transform.rotation.z;
            pos.rw = this.transform.rotation.w;
            NM.SendCommand($"[ObjectMovement] {uniqueId}:{this.transform.position.x}:{this.transform.position.y}:{this.transform.position.z}:{this.transform.rotation.x.ToString()}:{this.transform.rotation.y.ToString()}:{this.transform.rotation.z.ToString()}:{this.transform.rotation.w.ToString()}");
        }
        goto yes;
    }
}

[Serializable]
public class Pos
{
    public float x;
    public float y;
    public float z;
    public float rx;
    public float ry;
    public float rz;
    public float rw;
}