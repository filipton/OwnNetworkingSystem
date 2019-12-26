using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

[AttributeUsage(AttributeTargets.Method)]
public class RpcAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field)]
public class SyncVarAttribute : Attribute { }


public class NetworkManager : MonoBehaviour
{
    UdpUser clientudp;

    //prefabs
    public List<Prefab> Prefabs = new List<Prefab>();

    //ui
    public ServerUiElements Server;

    public List<Rpcs> rpclist = new List<Rpcs>();
    public List<SyncVars> svlist = new List<SyncVars>();
    public List<Object> objects = new List<Object>();

    public List<SyncVarQueue> SVQueue = new List<SyncVarQueue>();

    private void Awake()
    {
        if(FindObjectsOfType<NetworkManager>().Length > 1)
            Destroy(this.gameObject);
        else
            DontDestroyOnLoad(this);

#if UNITY_EDITOR
        EditorApplication.quitting += Disconnect;
#endif

        GetRpcs();
        GetSyncVars();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        //Get Rpcs and SyncVars
        GetRpcs();
        GetSyncVars();

        //SyncVarQueue
        List<SyncVarQueue> svQueuelocal = SVQueue;
        for(int i = 0; i < SVQueue.Count; i++)
        {
            SyncVars sv = svlist.Find(x => x.name == SVQueue[i].name);
            if (sv != null)
            {
                sv.classInstance.GetType().GetField(SVQueue[i].name).SetValue(sv.classInstance, SVQueue[i].value);
                svQueuelocal.RemoveAt(i);
            }
        }
        SVQueue = svQueuelocal;
    }

    private void Update()
    {
        
    }

    public void GetRpcs()
    {
        rpclist.Clear();

        MonoBehaviour[] sceneActive = GameObject.FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour mono in sceneActive)
        {
            Type monoType = mono.GetType();
            MethodInfo[] objectFields = monoType.GetMethods();

            for (int i = 0; i < objectFields.Length; i++)
            {
                RpcAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(RpcAttribute)) as RpcAttribute;
                if (attribute != null)
                {
                    rpclist.Add(new Rpcs { cmd = objectFields[i].Name, classInstance = mono.GetComponent(mono.GetType()), MI = objectFields[i] });
                }
            }
        }
    }

    public void GetSyncVars()
    {
        svlist.Clear();

        MonoBehaviour[] sceneActive = GameObject.FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour mono in sceneActive)
        {
            Type monoType = mono.GetType();
            FieldInfo[] objectFields = monoType.GetFields();

            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVarAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVarAttribute)) as SyncVarAttribute;
                if (attribute != null)
                {
                    svlist.Add(new SyncVars { name = objectFields[i].Name, classInstance = mono.GetComponent(mono.GetType()), FI = objectFields[i] });
                }
            }
        }
    }

    public void Connect()
    {
        try
        {
            clientudp = UdpUser.ConnectTo(Server.Ip.text, int.Parse(Server.Port.text));
            Task.Factory.StartNew(async () => {
                while (true)
                {
                    try
                    {
                        var received = await clientudp.Receive();
                        string command = received.Message;
                        string[] arguments = command.Replace($"{command.Split(' ')[0]} ", "").Split(':');
                        List<Rpcs> r;

                        //commands
                        if (command.Contains("[InitalizeClientObject] "))
                        {
                            r = rpclist.FindAll(o => o.cmd == "InitalizeClientObject");
                            try
                            {
                                foreach (Rpcs rpc in r)
                                {
                                    rpc.MI.Invoke(rpc.classInstance, arguments);
                                }
                            }
                            catch { }
                        }
                        else if (command.Contains("[ObjectMovement] "))
                        {
                            r = rpclist.FindAll(o => o.cmd == "ObjectMovement");
                            try
                            {
                                foreach (Rpcs rpc in r)
                                {
                                    rpc.MI.Invoke(rpc.classInstance, arguments);
                                }
                            }
                            catch { }
                        }
                        else if(command.Contains("[ChangeScene] "))
                        {
                            r = rpclist.FindAll(o => o.cmd == "ChangeScene");
                            try
                            {
                                foreach (Rpcs rpc in r)
                                {
                                    rpc.MI.Invoke(rpc.classInstance, arguments);
                                }
                            }
                            catch { }
                        }
                        else if(command.Contains("[RemoveObject] "))
                        {
                            r = rpclist.FindAll(o => o.cmd == "RemoveObject");
                            try
                            {
                                foreach (Rpcs rpc in r)
                                {
                                    rpc.MI.Invoke(rpc.classInstance, arguments);
                                }
                            }
                            catch { }
                        }
                        else if(command.Contains("[SyncVarrible] "))
                        {
                            r = rpclist.FindAll(o => o.cmd == "SyncVarrible");
                            try
                            {
                                foreach (Rpcs rpc in r)
                                {
                                    rpc.MI.Invoke(rpc.classInstance, arguments);
                                }
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        print(ex);
                    }
                }
            });
            Thread.Sleep(100);
            SendCommand($"[InitalizeConnection] {Server.Nick.text}");
        }
        catch {  }
    }

    public void Disconnect()
    {
        SendCommand($"[Disconnect]");
    }

    public void SendCommand(string com)
    {
        try
        {
            clientudp.Send(com);
        }
        catch { }
    }

    [Rpc]
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    [Rpc]
    public void InitalizeClientObject(string uid, string x, string y, string z, string rx, string ry, string rz, string rw, string prefabname, string nick)
    {
        GameObject objtoinit = Instantiate(Prefabs.Find(g => g.name == prefabname).Object, new Vector3 { x = float.Parse(x), y = float.Parse(y), z = float.Parse(z) }, new Quaternion { x = float.Parse(rx), y = float.Parse(ry), z = float.Parse(rz), w = float.Parse(rw) }); ;
        //objtoinit.transform.SetPositionAndRotation(new Vector3 { x = float.Parse(x), y = float.Parse(y), z = float.Parse(z) }, new Quaternion { x = float.Parse(rx), y = float.Parse(ry), z = float.Parse(rz), w = float.Parse(rw) });
        objtoinit.GetComponent<NetworkObject>().uniqueId = uid;
        objects.Add(new Object { uid = uid, gb = objtoinit });
        //print($"{nick} == {Server.Nick.text}");
        if (nick == Server.Nick.text)
        {
            objtoinit.GetComponent<NetworkObject>().CanMove = true;
        }
        else
        {
            Destroy(objtoinit.GetComponent<FirstPersonController>());
            Destroy(objtoinit.GetComponent<AudioSource>());
            Destroy(objtoinit.GetComponentInChildren<Camera>().gameObject);
            objtoinit.GetComponent<NetworkObject>().ShowGlasses();
            print($"NICK NOWEGO GRACZA TO: {nick}");
            objtoinit.GetComponent<NetworkObject>().nick = nick;
        }
        objtoinit.GetComponent<NetworkObject>().MoveRefresh();
        GetRpcs();
    }

    [Rpc]
    public void RemoveObject(string uid)
    {
        Destroy(objects.Find(x => x.uid == uid).gb);
    }

    [Rpc]
    public void SyncVarrible(string varname, string changeto)
    {
        if(svlist.Count > 0)
        {
            SyncVars sv = svlist.Find(x => x.name == varname);
            if(sv != null)
                sv.classInstance.GetType().GetField(varname).SetValue(sv.classInstance, changeto);
            else
                SVQueue.Add(new SyncVarQueue { name = varname, value = changeto });
        }
        else
        {
            SVQueue.Add(new SyncVarQueue { name = varname, value = changeto });
        }
        //print($"VAR: {varname}, CHANGETO: {changeto}, NORMAL: {PlayersCount}, CHECK: {sv.classInstance.GetType().GetField(varname).GetValue(sv.classInstance)}");
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}