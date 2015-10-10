using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIscript : MonoBehaviour {
    public List<Camera> camera=new List<Camera>();
    public GameObject virtuallight;
    public GameObject KinectModel;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnUpdateText_camera1_pos(string str)
    {
        this.camera[0].transform.position = new Vector3(float.Parse(str.Split(',')[0]), float.Parse(str.Split(',')[1]), float.Parse(str.Split(',')[2]));
    }
    public void OnUpdateText_camera2_pos(string str)
    {
        this.camera[1].transform.position = new Vector3(float.Parse(str.Split(',')[0]), float.Parse(str.Split(',')[1]), float.Parse(str.Split(',')[2]));
    }
    public void OnUpdateText_camera1_rot(string str)
    {
        this.camera[0].transform.rotation = Quaternion.Euler( new Vector3(float.Parse(str.Split(',')[0]), float.Parse(str.Split(',')[1]), float.Parse(str.Split(',')[2])));
    }
    public void OnUpdateText_camera2_rot(string str)
    {
        this.camera[1].transform.rotation = Quaternion.Euler(new Vector3(float.Parse(str.Split(',')[0]), float.Parse(str.Split(',')[1]), float.Parse(str.Split(',')[2])));
    }
    public void OnUpdateText_camera1_viewport(string str)
    {
        this.camera[0].fieldOfView = float.Parse(str);
    }
    public void OnUpdateText_camera2_viewport(string str)
    {
        this.camera[1].fieldOfView = float.Parse(str);
    }
    public void OnUpdateSlider_VirtualLightX(float x)
    {
        this.virtuallight.transform.position = new Vector3(x, this.virtuallight.transform.position.y, this.virtuallight.transform.position.z);
    }
    public void OnUpdateSlider_VirtualLightY(float y)
    {
        this.virtuallight.transform.position = new Vector3(this.virtuallight.transform.position.x, y, this.virtuallight.transform.position.z);
    }
    public void OnUpdateSlider_VirtualLightZ(float z)
    {
        this.virtuallight.transform.position = new Vector3(this.virtuallight.transform.position.x, this.virtuallight.transform.position.y, z);
    }
    public void OnUpdateSlider_KinectModelX(float x)
    {
        this.KinectModel.transform.position = new Vector3(x, this.KinectModel.transform.position.y, this.KinectModel.transform.position.z);
    }
    public void OnUpdateSlider_KinectModelY(float y)
    {
        this.KinectModel.transform.position = new Vector3(this.KinectModel.transform.position.x, y, this.KinectModel.transform.position.z);
    }
    public void OnUpdateSlider_KinectModelZ(float z)
    {
        this.KinectModel.transform.position = new Vector3(this.KinectModel.transform.position.x, this.KinectModel.transform.position.y, z);
    }

    public void OnUpdateSlider_KinectModelrX(float x)
    {
        this.KinectModel.transform.rotation = Quaternion.Euler( new Vector3(x, this.KinectModel.transform.position.y, this.KinectModel.transform.position.z));
    }
    public void OnUpdateSlider_KinectModelrY(float y)
    {
        this.KinectModel.transform.rotation=Quaternion.Euler( new Vector3(this.KinectModel.transform.position.x, y, this.KinectModel.transform.position.z));
    }
    public void OnUpdateSlider_KinectModelrZ(float z)
    {
        this.KinectModel.transform.rotation = Quaternion.Euler( new Vector3(this.KinectModel.transform.position.x, this.KinectModel.transform.position.y, z));
    }
}
