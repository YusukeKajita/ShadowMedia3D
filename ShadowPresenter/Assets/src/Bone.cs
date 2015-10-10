using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Bone
{
    public Vector3f _globalvec;
    public Bone Parent { set; get; }
    public enum TrackingState
    {
        Tracked,
        NotTracked,
        Inferred,
        UnInitialized
    }
    public TrackingState trackingstate { set; get; }

    public Vector3f GlobalVec
    {
        set
        {
            this._globalvec = value;
        }
        get
        {
            return this._globalvec;
        }
    }

    /// <summary>
    /// TODO:
    /// </summary>
    public Vector3f LocalVec
    {
        set
        {
            this._globalvec = this.Parent.GlobalVec + value;
        }
        get
        {
            return Parent != null ? this._globalvec - this.Parent.GlobalVec : this.GlobalVec;
        }
    }

    public Bone()
    {
        this._globalvec = new Vector3f(0, 0, 0);
        this.trackingstate = TrackingState.UnInitialized;
        this.Parent = null;
    }

    public Bone(Vector3f globalvec, TrackingState ts)
    {
        this._globalvec = new Vector3f(globalvec);
        this.trackingstate = ts;
    }

    public Bone(Vector3f globalvec, Bone Parent, TrackingState ts)
    {
        this._globalvec = new Vector3f(globalvec);
        this.Parent = Parent;
        this.trackingstate = ts;
    }

    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="Parent"></param>
    /// <param name="Localvec"></param>
    public Bone(Bone Parent, Vector3f Localvec, TrackingState ts)
    {
        if (Parent != null)
        {
            this.Parent = Parent;
            this.LocalVec = Localvec;
        }
        else
        {
            this.GlobalVec = LocalVec;
        }
        this.trackingstate = ts;

    }
}
