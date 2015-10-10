using UnityEngine;
using System.Collections;

public class SphereMakerScript : MonoBehaviour {
    public Object original;
    public float height;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Instantiate(original, new Vector3(Random.Range(-2f, 2f), height, Random.Range(0, 2f)), new Quaternion());
	}
}
