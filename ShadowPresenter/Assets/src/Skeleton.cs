using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Skeleton
{
    public Bone[] bone { set; get; }
    public Skeleton(int Bonenum = 20)
    {
        this.bone = new Bone[Bonenum];
        for (int i = 0; i < this.bone.Length; i++)
        {
            this.bone[i] = new Bone();
        }
    }

    public void JointBones()
    {
        this.bone[1].Parent = this.bone[0];
        this.bone[2].Parent = this.bone[1];
        this.bone[3].Parent = this.bone[2];

        this.bone[4].Parent = this.bone[2];
        this.bone[5].Parent = this.bone[4];
        this.bone[6].Parent = this.bone[5];
        this.bone[7].Parent = this.bone[6];

        this.bone[8].Parent = this.bone[2];
        this.bone[9].Parent = this.bone[8];
        this.bone[10].Parent = this.bone[9];
        this.bone[11].Parent = this.bone[10];

        this.bone[12].Parent = this.bone[0];
        this.bone[13].Parent = this.bone[12];
        this.bone[14].Parent = this.bone[13];
        this.bone[15].Parent = this.bone[14];

        this.bone[16].Parent = this.bone[0];
        this.bone[17].Parent = this.bone[16];
        this.bone[18].Parent = this.bone[17];
        this.bone[19].Parent = this.bone[18];
    }
}