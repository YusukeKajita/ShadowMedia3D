using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScriptStage : MonoBehaviour {

	// Use this for initialization
    public Mesh mesh;
    public List<Vector3> Positions = new List<Vector3>();
    public List<Vector2> UV = new List<Vector2>();
    public List<int> Indices;

    public int NumPerEdge;

	void Start () {
        
        
        this.mesh = new Mesh();
        /*
        this.Positions = new List<Vector3>();
        this.UV = new List<Vector2>();
        this.Indices = new List<int>();
        
        this.Positions.Add(new Vector3(-2f, 0, 2f));
        this.Positions.Add(new Vector3(-2f, 3f, 2f));
        this.Positions.Add(new Vector3(2f, 3f, 2f));
        this.Positions.Add(new Vector3(2f, 0, 2f));

        this.UV.Add(new Vector2(0, 0));
        this.UV.Add(new Vector2(0, 1));
        this.UV.Add(new Vector2(1, 1));
        this.UV.Add(new Vector2(1, 0));

        this.Indices.Add(0);
        this.Indices.Add(2);
        this.Indices.Add(1);

        this.Indices.Add(2);
        this.Indices.Add(0);
        this.Indices.Add(3);
        */

        this.CreateMesh();
        this.mesh.vertices = this.Positions.ToArray();
        this.mesh.uv = this.UV.ToArray();
        this.mesh.triangles = this.Indices.ToArray();

        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshFilter>().sharedMesh.name = "myMesh";
        GetComponent<MeshCollider>().sharedMesh = mesh;

	}

    private void CreateMesh()
    {
        if (this.Positions.Count == 4)
        {
            Vector3[] Points = new Vector3[4];
            
            for(int i = 0; i < this.Positions.Count; i++)
            {
                Points[i] = this.Positions[i];
            }

            this.Positions.Clear();
            this.UV.Clear();
            this.Indices.Clear();

            for (int y = 0; y < this.NumPerEdge; y++)
            {
                for (int x = 0; x < this.NumPerEdge; x++)
                {
                    this.Positions.Add(((Points[3] - Points[0]) * (this.NumPerEdge - x - 1) + (Points[2] - Points[0]) * x) * y / (this.NumPerEdge - 1) / (this.NumPerEdge - 1) + ((this.NumPerEdge - 1) - y) * x * (Points[1] - Points[0]) / (this.NumPerEdge - 1) / (this.NumPerEdge - 1) + Points[0]);
                    this.UV.Add(new Vector2((float)x / this.NumPerEdge, (float)y / this.NumPerEdge));
                }
            }

            for (int y = 0; y < this.NumPerEdge - 1; y++)
            {
                for (int x = 0; x < this.NumPerEdge - 1; x++)
                {
                    this.Indices.Add(x + y*NumPerEdge);
                    this.Indices.Add(x + 1 + y*NumPerEdge);
                    this.Indices.Add(x + this.NumPerEdge + y * NumPerEdge);

                    this.Indices.Add(x + 1 + y * NumPerEdge);
                    this.Indices.Add(x + 1 + this.NumPerEdge + y * NumPerEdge);
                    this.Indices.Add(x + this.NumPerEdge + y * NumPerEdge);
                }
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}
