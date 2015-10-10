using UnityEngine;
using System.Collections;

public class VirtualLightMovingScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = new Vector3(Mathf.Sin(Time.time), 2.0f, Mathf.Cos(Time.time));
	}
}
