using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class script_virtuallight : MonoBehaviour {

    public Camera virtuallight;
    public GameObject target;
    public MeshFilter targetmesh;
    public List<Vector3> Vec3;
    public List<Vector2> UVs;

    public int texturewidth;
    public int textureheight;

	// Use this for initialization
	void Start () {
        //this.virtuallight.projectionMatrix *= Matrix4x4.Scale(new Vector3(-1, -1, -1));
        this.UVs = new List<Vector2>();
        this.UVs.Add(new Vector2(0, 0));
        this.UVs.Add(new Vector2(0, 1));
        this.UVs.Add(new Vector2(1, 1));
        this.UVs.Add(new Vector2(1, 0));
        this.targetmesh = target.GetComponent<MeshFilter>();
        this.Update();
        this.textureheight = this.virtuallight.targetTexture.height;
        this.texturewidth= this.virtuallight.targetTexture.width;
	}
	
	// Update is called once per frame
	void Update () {

        
        Vector3 center  =new Vector3();
        int counter = 0 ;
        this.Vec3.Clear();
        foreach (var p in this.targetmesh.mesh.vertices)
        {
            center += p;
            counter++;
            this.Vec3.Add( this.virtuallight.WorldToViewportPoint(p));
        }
        center /= counter;
        this.transform.LookAt(center);
        
        this.UVs.Clear();
        foreach (var p in this.Vec3)
        {
            this.UVs.Add(new Vector2(p.x , p.y));
        }
	}
}
