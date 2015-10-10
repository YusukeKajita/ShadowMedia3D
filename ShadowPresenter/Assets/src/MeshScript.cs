using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;

public class MeshScript : MonoBehaviour
{
    private Mesh mesh;
    public string filepath;

    private List<Vector3> Positions;
    private List<Vector2> UV;
    private List<int> Indices;
    private List<int> Weights;

    private Skeleton modelskeleton;

    private Skeleton CurrentSkeleton;

    private CIPC_CS_Unity.CLIENT.CLIENT CIPCClient;

    private byte[] receiveddata;

    private Thread CIPCThread;
    public bool StopThread { private set; get; }

    public string IPAddress;

    public float initpropotion;

    public List<GameObject> BoneObjects;

    public UnityEngine.Object Bonejoints;
    public UnityEngine.Object virtualBonejoints;

    // Use this for initialization
    void Start()
    {


        this.InitClasses();

        this.LoadMesh();

        this.mesh.vertices = this.Positions.ToArray();
        this.mesh.uv = this.UV.ToArray();
        this.mesh.triangles = this.Indices.ToArray();

        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshFilter>().sharedMesh.name = "myMesh";

        this.CIPCClient.Setup(CIPC_CS_Unity.CLIENT.MODE.Receiver);
        this.CIPCThread.Start();
        this.StopThread = false;

    }

    private void InitClasses()
    {
        this.BoneObjects = new List<GameObject>();
        this.Positions = new List<Vector3>();
        this.UV = new List<Vector2>();
        this.Indices = new List<int>();
        this.Weights = new List<int>();

        this.modelskeleton = new Skeleton();
        this.CurrentSkeleton = new Skeleton();
        this.mesh = new Mesh();
        this.CIPCClient = new CIPC_CS_Unity.CLIENT.CLIENT(4120, this.IPAddress, 50000, "Shadow3DUnity", 30);
        this.CIPCThread = new Thread(this.CIPCthreadTask);
    }

    private void CIPCthreadTask()
    {
        try
        {
            while (!this.StopThread)
            {
                if (this.CIPCClient.IsAvailable > 0)
                {
                    this.ReceiveSkeletonFromCIPCClient();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < this.CurrentSkeleton.bone.Length; i++)
        {
            if (i == 5 || i == 9 || i == 13 || i == 17)
            {
                this.CurrentSkeleton.bone[i]._globalvec = new Vector3f(this.BoneObjects[i].transform.position.x, this.BoneObjects[i].transform.position.y, this.BoneObjects[i].transform.position.z);
            }
            else
            {

                this.BoneObjects[i].transform.position = new Vector3(this.CurrentSkeleton.bone[i]._globalvec.X, this.CurrentSkeleton.bone[i]._globalvec.Y, this.CurrentSkeleton.bone[i]._globalvec.Z);
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Quaternion[] q = new Quaternion[20];
        for (int i = 0; i < q.Length; i++)
        {
            q[i] = Quaternion.FromToRotation(new Vector3(this.modelskeleton.bone[i].LocalVec.X, this.modelskeleton.bone[i].LocalVec.Y, this.modelskeleton.bone[i].LocalVec.Z), new Vector3(this.CurrentSkeleton.bone[i].LocalVec.X, this.CurrentSkeleton.bone[i].LocalVec.Y, this.CurrentSkeleton.bone[i].LocalVec.Z));
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            int weightBone = this.Weights[i];
            Vector3 oldlocalvec = new Vector3(this.Positions[i].x - this.modelskeleton.bone[weightBone].Parent.GlobalVec.X, this.Positions[i].y - this.modelskeleton.bone[weightBone].Parent.GlobalVec.Y, this.Positions[i].z - this.modelskeleton.bone[weightBone].Parent.GlobalVec.Z);
            vertices[i] = q[weightBone] * oldlocalvec + new Vector3(this.CurrentSkeleton.bone[weightBone].Parent.GlobalVec.X, this.CurrentSkeleton.bone[weightBone].Parent.GlobalVec.Y, this.CurrentSkeleton.bone[weightBone].Parent.GlobalVec.Z);
        }
        mesh.vertices = vertices;

    }
    public Vector3 gravity;

    void OnApplicationQuit()
    {
        this.StopThread = true;
        this.CIPCClient.Close();
    }

    private void ReceiveSkeletonFromCIPCClient()
    {

        this.CIPCClient.Update(ref receiveddata);
        UDP_PACKETS_CODER.UDP_PACKETS_DECODER dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER();
        if (this.receiveddata.Length > 1900)
        {
            dec.Source = this.receiveddata;

            dec.get_int();//2
            dec.get_int();//6
            dec.get_int();//0
            for (int i = 0; i < 20; i++)
            {
                float x = dec.get_float();//x
                float y = dec.get_float();//y
                float z = dec.get_float();//z
                int state = dec.get_int();//trackingstate skst => 2

                this.CurrentSkeleton.bone[i].GlobalVec = new Vector3f(x, y, z);

                this.CurrentSkeleton.bone[i].trackingstate = Bone.TrackingState.Tracked;
            }
            Debug.Log("dec.length" + dec.Length.ToString());
        }
    }

    void LoadMesh()
    {
        try
        {
            this.filepath = UnityEditor.EditorUtility.OpenFilePanel("s3dファイルを選択", "", "s3d");
            System.IO.StreamReader sr = new System.IO.StreamReader(filepath, System.Text.Encoding.ASCII);
            char[] separator = new char[] { ';' };
            char[] separator2 = new char[] { ',' };

            this.Positions.Clear();
            this.Indices.Clear();
            this.Weights.Clear();

            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                if (str.Contains("Mesh"))
                {
                    string str_meshnum = sr.ReadLine();
                    string[] str_meshnums = str_meshnum.Split(separator);
                    int meshnum = int.Parse(str_meshnums[0]);
                    for (int i = 0; i < meshnum; i++)
                    {
                        string str_pos = sr.ReadLine();
                        string[] str_poss = str_pos.Split(separator);
                        this.Positions.Add(new Vector3(float.Parse(str_poss[0]) * this.initpropotion, float.Parse(str_poss[1]) * this.initpropotion, float.Parse(str_poss[2]) * this.initpropotion));
                        this.UV.Add(new Vector2(0.0f, 0.0f));
                    }
                    Debug.Log("MeshPoint " + this.Positions.Count.ToString());

                }
                if (str.Contains("Index"))
                {
                    string str_Indexnum = sr.ReadLine();
                    string[] str_Indexnums = str_Indexnum.Split(separator);
                    int Indexnum = int.Parse(str_Indexnums[0]);
                    for (int i = 0; i < Indexnum; i++)
                    {
                        string str_Indecies = sr.ReadLine();
                        string[] strs_Indecies = str_Indecies.Split(separator)[1].Split(separator2);
                        this.Indices.Add(int.Parse(strs_Indecies[0]));
                        this.Indices.Add(int.Parse(strs_Indecies[1]));
                        this.Indices.Add(int.Parse(strs_Indecies[2]));
                    }
                }
                if (str.Contains("Bone"))
                {
                    string str_Bone = sr.ReadLine();
                    string[] strs_Bone = str_Bone.Split(separator);
                    int Bonenum = int.Parse(strs_Bone[0]);
                    this.modelskeleton = new Skeleton(Bonenum);
                    this.CurrentSkeleton = new Skeleton(Bonenum);
                    for (int i = 0; i < Bonenum; i++)
                    {
                        string str_Bonepos = sr.ReadLine();
                        string[] strs_Bonepos = str_Bonepos.Split(separator)[1].Split(separator2);
                        this.modelskeleton.bone[i] = new Bone(new Vector3f(float.Parse(strs_Bonepos[0]) * this.initpropotion, float.Parse(strs_Bonepos[1]) * this.initpropotion, float.Parse(strs_Bonepos[2]) * this.initpropotion), Bone.TrackingState.Tracked);
                        this.CurrentSkeleton.bone[i] = new Bone(new Vector3f(float.Parse(strs_Bonepos[0]) * this.initpropotion, float.Parse(strs_Bonepos[1]) * this.initpropotion, float.Parse(strs_Bonepos[2]) * this.initpropotion), Bone.TrackingState.Tracked);
                        if (i == 5 || i == 9 || i == 13 || i == 17)
                        {
                            this.BoneObjects.Add(
                                Instantiate(this.virtualBonejoints,
                                new Vector3(float.Parse(strs_Bonepos[0]) * this.initpropotion, float.Parse(strs_Bonepos[1]) * this.initpropotion, float.Parse(strs_Bonepos[2]) * this.initpropotion),
                                new Quaternion()) as GameObject);
                        }
                        else
                        {
                            this.BoneObjects.Add(
                                Instantiate(this.Bonejoints,
                                new Vector3(float.Parse(strs_Bonepos[0]) * this.initpropotion, float.Parse(strs_Bonepos[1]) * this.initpropotion, float.Parse(strs_Bonepos[2]) * this.initpropotion),
                                new Quaternion()) as GameObject);
                        }

                    }

                    try
                    {
                        this.BoneObjects[5].GetComponents<SpringJoint>()[0].connectedBody = this.BoneObjects[4].GetComponent<Rigidbody>();
                        this.BoneObjects[5].GetComponents<SpringJoint>()[0].anchor = this.BoneObjects[4].transform.position - this.BoneObjects[5].transform.position;
                        this.BoneObjects[5].GetComponents<SpringJoint>()[1].connectedBody = this.BoneObjects[6].GetComponent<Rigidbody>();
                        this.BoneObjects[5].GetComponents<SpringJoint>()[1].anchor = this.BoneObjects[6].transform.position - this.BoneObjects[5].transform.position;

                        this.BoneObjects[9].GetComponents<SpringJoint>()[0].connectedBody = this.BoneObjects[8].GetComponent<Rigidbody>();
                        this.BoneObjects[9].GetComponents<SpringJoint>()[0].anchor =        this.BoneObjects[8].transform.position - this.BoneObjects[9].transform.position;
                        this.BoneObjects[9].GetComponents<SpringJoint>()[1].connectedBody = this.BoneObjects[10].GetComponent<Rigidbody>();
                        this.BoneObjects[9].GetComponents<SpringJoint>()[1].anchor =        this.BoneObjects[10].transform.position - this.BoneObjects[9].transform.position;

                        this.BoneObjects[13].GetComponents<SpringJoint>()[0].connectedBody =    this.BoneObjects[12].GetComponent<Rigidbody>();
                        this.BoneObjects[13].GetComponents<SpringJoint>()[0].anchor =           this.BoneObjects[12].transform.position - this.BoneObjects[13].transform.position;
                        this.BoneObjects[13].GetComponents<SpringJoint>()[1].connectedBody =    this.BoneObjects[14].GetComponent<Rigidbody>();
                        this.BoneObjects[13].GetComponents<SpringJoint>()[1].anchor =           this.BoneObjects[14].transform.position - this.BoneObjects[13].transform.position;

                        this.BoneObjects[17].GetComponents<SpringJoint>()[0].connectedBody =    this.BoneObjects[16].GetComponent<Rigidbody>();
                        this.BoneObjects[17].GetComponents<SpringJoint>()[0].anchor =           this.BoneObjects[16].transform.position - this.BoneObjects[17].transform.position;
                        this.BoneObjects[17].GetComponents<SpringJoint>()[1].connectedBody =    this.BoneObjects[18].GetComponent<Rigidbody>();
                        this.BoneObjects[17].GetComponents<SpringJoint>()[1].anchor =           this.BoneObjects[18].transform.position - this.BoneObjects[17].transform.position;

                        
                    }
                    catch
                    {
                        Debug.LogError("woops");
                    }
                    Debug.Log("MeshBone " + this.modelskeleton.bone.Length.ToString());

                }
                if (str.Contains("Weight"))
                {
                    string str_Weight = sr.ReadLine();
                    string[] strs_Weight = str_Weight.Split(separator);
                    int Weightsnum = int.Parse(strs_Weight[0]);
                    for (int i = 0; i < Weightsnum; i++)
                    {
                        string str_weight = sr.ReadLine();
                        this.Weights.Add(int.Parse(str_weight.Split(separator)[0]));
                    }
                    Debug.Log("MeshWeight " + this.Weights.Count.ToString());

                }
            }
            sr.Close();
            sr.Dispose();
            this.JointBone();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Connect Each Bone
    /// </summary>
    private void JointBone()
    {
        try
        {
            this.modelskeleton.bone[1].Parent = this.modelskeleton.bone[0];
            this.modelskeleton.bone[2].Parent = this.modelskeleton.bone[1];
            this.modelskeleton.bone[3].Parent = this.modelskeleton.bone[2];

            this.modelskeleton.bone[4].Parent = this.modelskeleton.bone[2];
            this.modelskeleton.bone[5].Parent = this.modelskeleton.bone[4];
            this.modelskeleton.bone[6].Parent = this.modelskeleton.bone[5];
            this.modelskeleton.bone[7].Parent = this.modelskeleton.bone[6];

            this.modelskeleton.bone[8].Parent = this.modelskeleton.bone[2];
            this.modelskeleton.bone[9].Parent = this.modelskeleton.bone[8];
            this.modelskeleton.bone[10].Parent = this.modelskeleton.bone[9];
            this.modelskeleton.bone[11].Parent = this.modelskeleton.bone[10];

            this.modelskeleton.bone[12].Parent = this.modelskeleton.bone[0];
            this.modelskeleton.bone[13].Parent = this.modelskeleton.bone[12];
            this.modelskeleton.bone[14].Parent = this.modelskeleton.bone[13];
            this.modelskeleton.bone[15].Parent = this.modelskeleton.bone[14];

            this.modelskeleton.bone[16].Parent = this.modelskeleton.bone[0];
            this.modelskeleton.bone[17].Parent = this.modelskeleton.bone[16];
            this.modelskeleton.bone[18].Parent = this.modelskeleton.bone[17];
            this.modelskeleton.bone[19].Parent = this.modelskeleton.bone[18];


            this.CurrentSkeleton.bone[1].Parent = this.CurrentSkeleton.bone[0];
            this.CurrentSkeleton.bone[2].Parent = this.CurrentSkeleton.bone[1];
            this.CurrentSkeleton.bone[3].Parent = this.CurrentSkeleton.bone[2];

            this.CurrentSkeleton.bone[4].Parent = this.CurrentSkeleton.bone[2];
            this.CurrentSkeleton.bone[5].Parent = this.CurrentSkeleton.bone[4];
            this.CurrentSkeleton.bone[6].Parent = this.CurrentSkeleton.bone[5];
            this.CurrentSkeleton.bone[7].Parent = this.CurrentSkeleton.bone[6];

            this.CurrentSkeleton.bone[8].Parent = this.CurrentSkeleton.bone[2];
            this.CurrentSkeleton.bone[9].Parent = this.CurrentSkeleton.bone[8];
            this.CurrentSkeleton.bone[10].Parent = this.CurrentSkeleton.bone[9];
            this.CurrentSkeleton.bone[11].Parent = this.CurrentSkeleton.bone[10];

            this.CurrentSkeleton.bone[12].Parent = this.CurrentSkeleton.bone[0];
            this.CurrentSkeleton.bone[13].Parent = this.CurrentSkeleton.bone[12];
            this.CurrentSkeleton.bone[14].Parent = this.CurrentSkeleton.bone[13];
            this.CurrentSkeleton.bone[15].Parent = this.CurrentSkeleton.bone[14];

            this.CurrentSkeleton.bone[16].Parent = this.CurrentSkeleton.bone[0];
            this.CurrentSkeleton.bone[17].Parent = this.CurrentSkeleton.bone[16];
            this.CurrentSkeleton.bone[18].Parent = this.CurrentSkeleton.bone[17];
            this.CurrentSkeleton.bone[19].Parent = this.CurrentSkeleton.bone[18];

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
