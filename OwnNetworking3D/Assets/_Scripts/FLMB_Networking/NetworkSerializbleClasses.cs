using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
public class SyncVarQueue
{
    public string name;
    public string value;
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