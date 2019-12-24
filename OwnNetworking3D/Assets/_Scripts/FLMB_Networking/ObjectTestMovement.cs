using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTestMovement : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            this.transform.localPosition = new Vector3(x: this.transform.localPosition.x, y: (this.transform.localPosition.y + 1), z: this.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.localPosition = new Vector3(x: this.transform.localPosition.x, y: (this.transform.localPosition.y - 1), z: this.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.localPosition = new Vector3(x: (this.transform.localPosition.x - 1), y: this.transform.localPosition.y, z: this.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.localPosition = new Vector3(x: (this.transform.localPosition.x + 1), y: this.transform.localPosition.y, z: this.transform.localPosition.z);
        }
    }
}