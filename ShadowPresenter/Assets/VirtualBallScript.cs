using UnityEngine;
using System.Collections;

public class VirtualBallScript : MonoBehaviour {

    public float YLimt = -10;
    public float XZLimit = 10;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (this.transform.position.y < this.YLimt || Mathf.Abs(this.transform.position.x) > this.XZLimit || Mathf.Abs(this.transform.position.z) > this.XZLimit)
        {
            Destroy(this.gameObject);
        }
	}
}