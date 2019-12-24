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

    public struct Received
    {
        public IPEndPoint Sender;
        public string Message;
    }

    abstract class UdpBase
    {
        protected UdpClient Client;

        protected UdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }
    }

    class UdpUser : UdpBase
    {
        private UdpUser() { }

        public static UdpUser ConnectTo(string hostname, int port)
        {
            var connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }

        public void Send(string message)
        {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length);
        }

    }

    UdpUser clientudp;

    //prefabs
    public List<Prefab> Prefabs = new List<Prefab>();

    //ui
    public ServerUiElements Server;

    public List<Rpcs> rpclist = new List<Rpcs>();
    public List<SyncVars> svlist = new List<SyncVars>();
    public List<Object> objects = new List<Object>();

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
        GetRpcs();
        GetSyncVars();
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

            // Retreive the fields from the mono instance
            MethodInfo[] objectFields = monoType.GetMethods();

            // search all fields and find the attribute [Rpc]
            for (int i = 0; i < objectFields.Length; i++)
            {
                RpcAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(RpcAttribute)) as RpcAttribute;

                // if we detect any attribute print out the data.
                if (attribute != null)
                {
                    //add command to "database"
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

            // Retreive the fields from the mono instance
            FieldInfo[] objectFields = monoType.GetFields();

            // search all fields and find the attribute [SyncVar]
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVarAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVarAttribute)) as SyncVarAttribute;

                // if we detect any attribute print out the data.
                if (attribute != null)
                {
                    //add field to "database"
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
                        Console.WriteLine(ex);
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
        SyncVars sv = svlist.Find(x => x.name == varname);
        sv.classInstance.GetType().GetField(varname).SetValue(sv.classInstance, changeto);
        //print($"VAR: {varname}, CHANGETO: {changeto}, NORMAL: {PlayersCount}, CHECK: {sv.classInstance.GetType().GetField(varname).GetValue(sv.classInstance)}");
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}

[Serializable]
public class ServerUiElements
{
    public InputField Ip;
    public InputField Port;
    public InputField Nick;
}

[Serializable]
public class Rpcs
{
    public string cmd;
    public object classInstance;
    public MethodInfo MI;
}

[Serializable]
public class SyncVars
{
    public string name;
    public object classInstance;
    public FieldInfo FI;
}

[Serializable]
public class Prefab
{
    public string name;
    public GameObject Object;
}

[Serializable]
public class Object
{
    public string uid;
    public GameObject gb;
}