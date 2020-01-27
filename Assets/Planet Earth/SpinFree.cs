using UnityEngine;
using System.Collections;

/// <summary>
/// Spin the object at a specified speed
/// </summary>
public class SpinFree : MonoBehaviour {
	[Tooltip("Spin: Yes or No")]
	public bool spin;
	[Tooltip("Spin the parent object instead of the object this script is attached to")]
	public bool spinParent;
	public float speed = 10f;

	[HideInInspector]
	public bool clockwise = true;
	[HideInInspector]
	public float direction = 1f;
	[HideInInspector]
	public float directionChangeSpeed = 2f;
	public GameObject orbit;
	public GameObject satellite;
	GameObject[] orbits;
	Vector3[] orbitaxes;
	int count = 0;

	void Start() {
		orbits = new GameObject[83];
		orbitaxes = new Vector3[83];
		for (int i = 0; i < 32; i++) {
			//Quaternion q = Quaternion.Euler(new Vector3(0f, i*11.25f, 53f));
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, 53f);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, i*11.25f);
			orbitaxes [count] = Quaternion.Euler(0, i*11.25f, 0) * (Quaternion.Euler (0, 0, 53f) * Vector3.up);
			Vector3 scale = orbits[count].transform.localScale;
			scale.x *= 1.181f;
			scale.y *= 1.181f;
			scale.z *= 1.181f;
			orbits[count].transform.localScale = scale;
			for (int s = 0; s < 50; s++) {
				Vector3 pos = transform.position;
				pos.x += 4f;
				GameObject sat1 = (GameObject)Instantiate (satellite, pos, transform.rotation);
				sat1.transform.RotateAround (Vector3.zero, Vector3.up, 1.8f + i%2 * 7.2f/2f + s * 360f / 50f);
				sat1.transform.SetParent (orbits[count].transform, false);
			}
			count++;
		}
		for (int i = 0; i < 32; i++) {
			//Quaternion q = Quaternion.Euler(new Vector3(0f, 5.625f + i*11.25f, 53.8f));
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, 53.8f);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, 5.625f + i*11.25f);
			orbitaxes [count] = Quaternion.Euler(0, 5.625f + i*11.25f, 0) * (Quaternion.Euler (0, 0, 53.8f) * Vector3.up);
			Vector3 scale = orbits[count].transform.localScale;
			scale.x *= 1.174f;
			scale.y *= 1.174f;
			scale.z *= 1.174f;
			orbits[count].transform.localScale = scale;
			for (int s = 0; s < 50; s++) {
				Vector3 pos = transform.position;
				pos.x += 4f;
				GameObject sat1 = (GameObject)Instantiate (satellite, pos, transform.rotation);
				sat1.transform.RotateAround (Vector3.zero, Vector3.up, (i+1)%2 * 7.2f/2f + s * 360f / 50f);
				sat1.transform.SetParent (orbits[count].transform, false);
			}
			count++;
		}
		for (int i = 0; i < 8; i++) {
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, 74f);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, 3.3105f + i*45f);
			orbitaxes [count] = Quaternion.Euler(0, 3.3105f + i*45f, 0) * (Quaternion.Euler (0, 0, 74) * Vector3.up);
			Vector3 scale = orbits[count].transform.localScale;
			scale.x *= 1.174f;
			scale.y *= 1.174f;
			scale.z *= 1.174f;
			orbits[count].transform.localScale = scale;
			for (int s = 0; s < 75; s++) {
				Vector3 pos = transform.position;
				pos.x += 4f;
				GameObject sat1 = (GameObject)Instantiate (satellite, pos, transform.rotation);
				sat1.transform.RotateAround (Vector3.zero, Vector3.up, i%2 * 4.8f/2f + s * 360f / 75f);
				sat1.transform.SetParent (orbits[count].transform, false);
			}
			count++;
		}
		for (int i = 0; i < 5; i++) {
			//Quaternion q = Quaternion.Euler(new Vector3(0f, 3.3105f + 17f + i*75f, 81f));
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, 81f);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, 3.3105f + 17f + i*75f);
			orbitaxes [count] = Quaternion.Euler(0, 3.3105f + 17f + i*75f, 0) * (Quaternion.Euler (0, 0, 81) * Vector3.up);
			Vector3 scale = orbits[count].transform.localScale;
			scale.x *= 1.174f;
			scale.y *= 1.174f;
			scale.z *= 1.174f;
			orbits[count].transform.localScale = scale;
			for (int s = 0; s < 75; s++) {
				Vector3 pos = transform.position;
				pos.x += 4f;
				GameObject sat1 = (GameObject)Instantiate (satellite, pos, transform.rotation);
				sat1.transform.RotateAround (Vector3.zero, Vector3.up, i%2 * 4.8f/2f + s * 360f / 75f);
				sat1.transform.SetParent (orbits[count].transform, false);
			}
			count++;
		}
		for (int i = 0; i < 6; i++) {
			//Quaternion q = Quaternion.Euler(new Vector3(0f, 3.3105f + 43f + i*60f, 70f));
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, 70f);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, 3.3105f + 43f + i*60f);
			orbitaxes [count] = Quaternion.Euler(0, 3.3105f + 43f + i*60f, 0) * (Quaternion.Euler (0, 0, 70) * Vector3.up);
			Vector3 scale = orbits[count].transform.localScale;
			scale.x *= 1.174f;
			scale.y *= 1.174f;
			scale.z *= 1.174f;
			orbits[count].transform.localScale = scale;
			for (int s = 0; s < 75; s++) {
				Vector3 pos = transform.position;
				pos.x += 4f;
				GameObject sat1 = (GameObject)Instantiate (satellite, pos, transform.rotation);
				sat1.transform.RotateAround (Vector3.zero, Vector3.up, i%2 * 4.8f/2f + s * 360f / 75f);
				sat1.transform.SetParent (orbits[count].transform, false);
			}
			count++;
		}
	}

	// Update is called once per frame
	void Update() {
		if (direction < 1f) {
			direction += Time.deltaTime / (directionChangeSpeed / 2);
		}

		if (spin) {
			if (clockwise) {
				if (spinParent)
					transform.parent.transform.Rotate(Vector3.up, (speed * direction) * Time.deltaTime);
				else
					transform.Rotate(Vector3.up, (speed * direction) * Time.deltaTime);
			} else {
				if (spinParent)
					transform.parent.transform.Rotate(-Vector3.up, (speed * direction) * Time.deltaTime);
				else
					transform.Rotate(-Vector3.up, (speed * direction) * Time.deltaTime);
			}
		}
		for (int i = 0; i < 83; i++) {
			orbits[i].transform.RotateAround(Vector3.zero, orbitaxes[i], (-16f * speed * direction) * Time.deltaTime);
		}
	}
}