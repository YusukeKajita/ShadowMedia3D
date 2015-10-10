using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawMesh : MonoBehaviour {

    public script_DrawBoneArt skeletonscript;

    public int PA = 5;
    public int PB = 6;

    public script_DrawSetup drawsetup;

    public bool IsDeleteOld = false;
    public int DeleteNum = 20;

    private List<Vector3> Positions;
    private List<Vector2> UV;
    private List<int> Indices;
    private Mesh mesh;
	// Use this for initialization 
	void Start () {
        this.Init_Classes();
	}

    private List<Vector3> PointA;
    private List<Vector3> PointB;
    
    private void Init_Classes()
    {

        if (this.drawsetup != null)
        {
            this.DeleteNum = this.drawsetup.DeleteNum;
            this.IsDeleteOld = this.drawsetup.IsDeleteOld;
        }
        
        this.Positions = new List<Vector3>();
        this.UV = new List<Vector2>();
        this.Indices = new List<int>();
        this.mesh = new Mesh();

        this.PointA = new List<Vector3>();
        this.PointB = new List<Vector3>();

        this.Positions.Add(new Vector3(0,1,0));
        this.UV.Add(new Vector2(0.0f, 0.0f));
        this.Positions.Add(new Vector3(0,0,1));
        this.UV.Add(new Vector2(1.0f, 1.0f));
        this.Positions.Add(new Vector3(1,0,0));
        this.UV.Add(new Vector2(0.0f, 1.0f));
        this.Positions.Add(new Vector3(0, 0, 0));
        this.UV.Add(new Vector2(1.0f, 0.0f));

        this.Indices.Add(0);
        this.Indices.Add(1);
        this.Indices.Add(2);

        this.Indices.Add(0);
        this.Indices.Add(2);
        this.Indices.Add(3);

        this.Indices.Add(0);
        this.Indices.Add(3);
        this.Indices.Add(1);

        this.Indices.Add(1);
        this.Indices.Add(3);
        this.Indices.Add(2);

        this.mesh.vertices = this.Positions.ToArray();
        this.mesh.uv = this.UV.ToArray();
        this.mesh.triangles = this.Indices.ToArray();

        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshFilter>().sharedMesh.name = "myMesh";

        GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
        GetComponent<SkinnedMeshRenderer>().sharedMesh.name = "myMesh";

    }
	
	// Update is called once per frame
    void Update()
    {
        if (this.skeletonscript == null) return;
        if (this.skeletonscript.HaveData == false) return;

        this.PointA.Add(new Vector3(this.skeletonscript.skeletons[0].bone[this.PA]._globalvec.X, this.skeletonscript.skeletons[0].bone[this.PA]._globalvec.Y, this.skeletonscript.skeletons[0].bone[this.PA]._globalvec.Z));
        this.PointB.Add(new Vector3(this.skeletonscript.skeletons[0].bone[this.PB]._globalvec.X, this.skeletonscript.skeletons[0].bone[this.PB]._globalvec.Y, this.skeletonscript.skeletons[0].bone[this.PB]._globalvec.Z));

        if (this.IsDeleteOld)
        {
            if (this.DeleteNum > 6)
            {
                if (this.PointA.Count > this.DeleteNum)
                {
                    this.PointA.RemoveAt(0);
                }
                if (this.PointB.Count > this.DeleteNum)
                {
                    this.PointB.RemoveAt(0);
                }
            }
        }

        if (PointA.Count < 5) return;

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> UV = new List<Vector2>();

        for (int i = 0; i < this.PointA.Count; i++)
        {
            vertices.Add(this.PointA[i]);
            vertices.Add(this.PointB[i]);
            UV.Add(new Vector2(0.0f, 0.0f));
            UV.Add(new Vector2(1.0f, 1.0f));
        }

        for (int i = 0; i < vertices.Count - 2; i += 2)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 2);




            triangles.Add(i + 3);
            triangles.Add(i + 2);
            triangles.Add(i + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = UV.ToArray();

    }

    public void ResetIndeces()
    {
        this.PointA.Clear();
        this.PointB.Clear();
    }
}
