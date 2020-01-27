using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour {
	public LineRenderer line;

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetPos(Vector3 pos1, Vector3 pos2) {
		line.SetPosition (0, pos1);
		line.SetPosition (1, pos2);
	}

	public void SetMaterial(Material mat) {
		line.material = mat;
	}

	public void ChangeMaterial(Material newMat) {
		line.material = newMat;
	}
}
