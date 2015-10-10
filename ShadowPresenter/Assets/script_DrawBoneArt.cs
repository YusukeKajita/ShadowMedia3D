using UnityEngine;
using System.Collections;

public class script_DrawBoneArt : MonoBehaviour {

    public script_CIPC_For_Unity cipc;
    public Skeleton[] skeletons;
    public bool HaveData
    {
        set;
        get;
    }
    
	// Use this for initialization
	void Start () {
        this.skeletons = new Skeleton[6];
        for (int i = 0; i < 6; i++)
        {
            this.skeletons[i] = new Skeleton();
            this.skeletons[i].JointBones();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (this.cipc == null) return;
        UDP_PACKETS_CODER.UDP_PACKETS_DECODER dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER();
        dec.Source = this.cipc.Data;
        if (this.cipc.Data.Length > 1900)
        {
            dec.Source = this.cipc.Data;

            dec.get_int();//2
            dec.get_int();//6
            for (int j = 0; j < 6; j++)
            {
                dec.get_int();//0
                for (int i = 0; i < 20; i++)
                {
                    float x = dec.get_float();//x
                    float y = dec.get_float();//y
                    float z = dec.get_float();//z
                    int state = dec.get_int();//trackingstate skst => 2
                    this.skeletons[j].bone[i].GlobalVec = new Vector3f(x, y, z);
                    this.skeletons[j].bone[i].trackingstate = Bone.TrackingState.Tracked;
                    
                }
            }
            this.HaveData = true;
        }
	}
}
