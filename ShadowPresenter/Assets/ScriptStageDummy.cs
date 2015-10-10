using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptStageDummy : MonoBehaviour {

	// Use this for initialization

    public GameObject Stage;
    public ScriptStage scriptstage;

    public Camera VirtualLight;
    public script_virtuallight scriptvirtuallight;

    public Mesh mesh;
    public List<Vector3> Positions = new List<Vector3>();
    public List<Vector2> UV = new List<Vector2>();
    public List<int> Indices;

	void Start () {

        if (this.VirtualLight == null) return;
        if (this.Stage == null) return;

        this.scriptvirtuallight = this.VirtualLight.GetComponent<script_virtuallight>();
        

        this.scriptstage = this.Stage.GetComponent<ScriptStage>();
        this.Positions = this.scriptstage.Positions;
        this.UV = this.scriptstage.UV;

        this.mesh = new Mesh();

        this.mesh.vertices = this.Positions.ToArray();
        this.mesh.uv = this.UV.ToArray();
        if (this.Indices.Count == 0)
        {
            this.Indices = this.scriptstage.Indices;
        }

        this.mesh.triangles = this.Indices.ToArray();
        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshFilter>().sharedMesh.name = "myMeshDummy";

	}
	
	// Update is called once per frame
	void Update () {
        try
        {
            if (this.mesh.vertexCount != this.scriptvirtuallight.UVs.Count)
            {
                this.mesh.vertices = this.Positions.ToArray();
                this.mesh.uv = this.UV.ToArray();
                if (this.Indices.Count == 0)
                {
                    this.Indices = this.scriptstage.Indices;
                }

                this.mesh.triangles = this.Indices.ToArray();
                this.mesh.RecalculateNormals();
                this.mesh.RecalculateBounds();

                GetComponent<MeshFilter>().sharedMesh = mesh;
                GetComponent<MeshFilter>().sharedMesh.name = "myMeshDummy";
            }
            this.mesh.uv = this.scriptvirtuallight.UVs.ToArray();
            this.UV = this.scriptvirtuallight.UVs;

            this.mesh.RecalculateNormals();
            this.mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
	}
}
