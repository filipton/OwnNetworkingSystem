using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestOnLoad : MonoBehaviour
{
    public static DontDestOnLoad DDOLInstance;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        if (DDOLInstance == null)
            DDOLInstance = this;
        else
            Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}