﻿using UnityEngine;
using System.Collections;

public class directionlalightscript : MonoBehaviour {

    public GameObject target;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.LookAt(this.target.transform);
	}
}
