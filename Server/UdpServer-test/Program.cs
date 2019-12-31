using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpServer_test
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

    class UdpListener : UdpBase
    {
        private IPEndPoint _listenOn;
        private static int port;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, port))
        {
        }

        public UdpListener(IPEndPoint endpoint)
        {
            _listenOn = endpoint;
            port = endpoint.Port;
            Client = new UdpClient(_listenOn);
        }

        public void Reply(string message, IPEndPoint endpoint)
        {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length, endpoint);
        }

    }

    class Program
    {
        public static List<Connections> connections = new List<Connections>();
        public static int port = 7777;
        public static UdpListener server = new UdpListener(new IPEndPoint(IPAddress.Any, port));


        public static List<Object> Objs = new List<Object>();
        public static List<SyncVars> svlist = new List<SyncVars>();
        public static List<OnlineChecher> Ocs = new List<OnlineChecher>();
        public static string ActualScene = "GameLobby";

        public static int uids = 0;

        static void Main(string[] args)
        {
            FileInfo fileInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            var lastmodified = fileInfo.LastWriteTime;
            string ip = GetPublicIpAddress();

            Console.Title = $"FLMB_SERVER (TEST-UDP) STARTED! [0/20]";
            Console.WriteLine("-------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("FLMB - UDP-TEST-GAME-SERVER");
            Console.WriteLine($"BUILD DATE AND TIME: {lastmodified.Day}.{lastmodified.Month}.{lastmodified.Year} {lastmodified.Hour}:{lastmodified.Minute}");
            Console.WriteLine($"SERVER STARTED! IP: {ip}:{port}");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();

            /*Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(50000);
                        foreach (Connections con in connections)
                        {
                            Ocs.Add(new OnlineChecher { Name = con.Name, Online = false });
                        }
                        BrodecastAll("[CheckIfOnline]");
                        Thread.Sleep(5000);
                        for (int x = 0; x < Ocs.Count; x++)
                        {
                            if (Ocs[x].Online == false)
                            {
                                //kick player

                                //send message to client who is kicking (in future)
                                //FUTURE!

                                //delete player from connection list
                                Connections c = connections.Find(y => y.Name.Equals(Ocs[x].Name));
                                connections.Remove(c);

                                //send messages to other clients (client disconected)
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Client Timed Out! [-] IP: {c.ip.ToString()}, NICK: {c.Name}. Clients connected: {connections.Count}");
                                Console.ResetColor();
                                BrodecastAll($"[SyncVarrible] PlayersCount:{connections.Count}");
                                BrodecastAll($"[RemoveClient] {c.Name}");
                                //BrodecastAll($"PLAYER TIMED OUT: {oc.Name}");
                            }
                            else
                            {
                                Console.WriteLine("XD");
                            }
                            Thread.Sleep(500);
                        }
                        Thread.Sleep(1000);
                        Ocs.Clear();
                    }
                    catch { }
                }
            });*/

            //start listening for messages
            Task.Factory.StartNew(async () => {
                while (true)
                {
                    var received = await server.Receive();
                    string command = received.Message;
                    List<string> arguments = command.Replace($"{command.Split(' ')[0]} ", "").Split(':').ToList();
                    int playerindex = connections.FindIndex(x => x.ip.ToString() == received.Sender.ToString());
                    if (playerindex < 0)
                    {
                        if (command.Contains("[InitalizeConnection]")) //command to initalize client with a nick
                        {
                            //Add new client to list and change some varibles
                            if (connections.Find(x => x.Name == arguments[0]) != null)
                            {

                            }
                            else
                            {
                                connections.Add(new Connections { Name = arguments[0], ip = received.Sender });
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Client Connected! [+] IP: {received.Sender.ToString()}, Nick: {arguments[0]}. Clients connected: {connections.Count}");
                                Console.Title = $"FLMB_SERVER (TEST-UDP) STARTED! [{connections.Count}/20]";
                                Console.ResetColor();
                                BrodecastAll($"[SyncVarrible] PlayersCount:{connections.Count}");
                                playerindex = connections.FindIndex(x => x.ip.ToString() == received.Sender.ToString());
                                BrodecastWName("SOMEONE CONNECTED!", connections[playerindex].Name);
                                BrodecastByName("AUTH COMPLETE!", connections[playerindex].Name);

                                BrodecastAll($"[AddClient] {connections[playerindex].Name}");

                                SyncVars ss = svlist.Find(x => x.name == "PlayersCount");
                                if (ss == null)
                                {
                                    svlist.Add(new SyncVars { name = "PlayersCount", var = connections.Count.ToString() });
                                }
                                else
                                {
                                    ss.var = connections.Count.ToString();
                                }


                                //Set object on join and change scene
                                if (ActualScene != null)
                                {
                                    BrodecastSomeOne($"[ChangeScene] {ActualScene}", connections[playerindex].ip);
                                }
                                if (Objs.Count > 0)
                                {
                                    foreach (Object obj in Objs)
                                    {
                                        //Console.WriteLine($"[ObjectMovement] {obj.UniqueID}:{obj.x}:{obj.y}:{obj.z}");
                                        Thread.Sleep(100);
                                        BrodecastSomeOne($"[InitalizeClientObject] {obj.UniqueID}:{obj.x}:{obj.y}:{obj.z}:{obj.rx}:{obj.ry}:{obj.rz}:{obj.rw}:{obj.type}:{obj.OwnerNick}", connections[playerindex].ip);
                                    }
                                }
                                if (svlist.Count > 0)
                                {
                                    foreach(SyncVars sv in svlist)
                                    {
                                        //Console.WriteLine($"{sv.name}:{sv.var}");
                                        BrodecastSomeOne($"[SyncVarrible] {sv.name}:{sv.var}", connections[playerindex].ip);
                                    }
                                }

                                //auto create object
                                /*string uid = uids.ToString();
                                Objs.Add(new Object { UniqueID = uid, x = "0", y = "0", z = "0", rx = "0", ry = "0", rz = "0", rw = "0", type = "player1", OwnerNick = connections[playerindex].Name });
                                Console.WriteLine($"NEW OBJECT ADDED: {uid} {connections[playerindex].Name}");
                                BrodecastAll($"[InitalizeClientObject] {uid}:0:0:0:0:0:0:0:player1:{connections[playerindex].Name}");
                                uids++;*/
                            }
                        }
                        else
                        {
                            //not authorized!
                            //Console.WriteLine("CLIENT NOT AUTHORIZED!");
                        }
                    }
                    else //if player count > 0
                    {
                        //normal commands

                        if (command == "[Disconnect]")
                        {
                            Connections clienttodis = connections.Find(x => x.ip.ToString() == received.Sender.ToString());
                            if (clienttodis != null)
                            {
                                connections.Remove(clienttodis);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Client Disconnected! [-] IP: {received.Sender.ToString()}, NICK: {clienttodis.Name}. Clients connected: {connections.Count}");
                                Console.Title = $"FLMB_SERVER (TEST-UDP) STARTED! [{connections.Count}/20]";
                                Console.ResetColor();

                                //remove object
                                Object on = Objs.Find(x => x.OwnerNick == clienttodis.Name);
                                if(on != null)
                                {
                                    BrodecastAll($"[RemoveObject] {on.UniqueID}");
                                    Objs.Remove(on);
                                }

                                //sync players var
                                BrodecastAll($"[SyncVarrible] PlayersCount:{connections.Count}");

                                //remove client
                                BrodecastAll($"[RemoveClient] {clienttodis.Name}");
                            }
                        }
                        else if (command.Contains("[InitalizeObject] "))
                        {
                            string uid = uids.ToString();
                            Objs.Add(new Object { UniqueID = uid, x = arguments[0], y = arguments[1], z = arguments[2], rx = arguments[3], ry = arguments[4], rz = arguments[5], rw = arguments[6], type = arguments[7], OwnerNick = arguments[8] });
                            Console.WriteLine($"NEW OBJECT ADDED: {uid} {arguments[0]}:{arguments[1]}:{arguments[2]}:{arguments[3]}:{arguments[4]}:{arguments[5]}:{arguments[6]}:{arguments[7]}");
                            BrodecastAll($"[InitalizeClientObject] {uid}:{arguments[0]}:{arguments[1]}:{arguments[2]}:{arguments[3]}:{arguments[4]}:{arguments[5]}:{arguments[6]}:{arguments[7]}:{arguments[8]}");
                            uids++;
                        }
                        else if (command.Contains("[ObjectMovement] "))
                        {
                            Object Obj = Objs.Find(x => x.UniqueID == arguments[0]);
                            if (Obj.OwnerNick == connections.Find(x => x.ip.ToString() == received.Sender.ToString()).Name)
                            {
                                //Console.WriteLine(arguments[0]);
                                if (Obj == null)
                                {
                                    //BrodecastSomeOne("THAT OBJECT DOESNT EXIST... [SCAMMER ALERT]", received.Sender);
                                }
                                else
                                {
                                    Obj.x = arguments[1];
                                    Obj.y = arguments[2];
                                    Obj.z = arguments[3];
                                    Obj.rx = arguments[4];
                                    Obj.ry = arguments[5];
                                    Obj.rz = arguments[6];
                                    Obj.rw = arguments[7];
                                    BrodecastAllWOne($"[ObjectMovement] {arguments[0]}:{arguments[1]}:{arguments[2]}:{arguments[3]}:{arguments[4]}:{arguments[5]}:{arguments[6]}:{arguments[7]}", received.Sender);
                                }
                            }
                        }
                        else if (command.Contains("[ChangeScene] "))
                        {
                            ActualScene = arguments[0];
                            BrodecastAll(command);
                            Console.WriteLine(command);
                            BrodecastAll($"[SyncVarrible] PlayersCount:{connections.Count}");
                        }
                        else if (command.Contains("[SyncVarrible] "))
                        {
                            svlist.Add(new SyncVars { name = arguments[0], var = arguments[1] });
                            BrodecastAll($"[SyncVarrible] {arguments[0]}:{arguments[1]}");
                        }
                        else if(command == "[IsOnline]")
                        {
                            OnlineChecher online = Ocs.Find(x => x.Name.Equals(connections[playerindex].Name));
                            online.Online = true;
                            Console.WriteLine(online.Name + " : " + online.Online);
                        }

                        //BrodecastAllWOne(received.Message, received.Sender);
                    }
                }
            });
        read:
            string s = Console.ReadLine();
            if(s.ToLower() == "quit")
            {
                return;
            }
            else if(s.ToLower() == "clearobjs")
            {
                Objs.Clear();
            }
            goto read;
        }

        private static string GetPublicIpAddress()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }

        static void BrodecastAll(string message)
        {
            foreach (Connections c in connections)
            {
                server.Reply(message, c.ip);
            }
        }

        static void BrodecastSomeOne(string message, IPEndPoint iPEnd)
        {
            server.Reply(message, iPEnd);
        }

        static void BrodecastAllWOne(string message, IPEndPoint iPEnd)
        {
            foreach (Connections c in connections)
            {
                if(c.ip.ToString() != iPEnd.ToString())
                    server.Reply(message, c.ip);
            }
        }

        static void BrodecastByName(string message, string NickName)
        {
            foreach (Connections c in connections)
            {
                if(c.Name == NickName)
                    server.Reply(message, c.ip);
            }
        }

        static void BrodecastWName(string message, string NickName)
        {
            foreach (Connections c in connections)
            {
                if (c.Name != NickName)
                    server.Reply(message, c.ip);
            }
        }
    }

    class Connections
    {
        public string Name;
        public IPEndPoint ip;
    }

    public class Object
    {
        public string UniqueID;
        public string x;
        public string y;
        public string z;
        public string rx;
        public string ry;
        public string rz;
        public string rw;
        public string type;
        public string OwnerNick;
    }

    public class SyncVars
    {
        public string name;
        public string var;
    }

    public class OnlineChecher
    {
        public string Name;
        public bool Online;
    }
}