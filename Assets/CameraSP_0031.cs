using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSP_0031 : MonoBehaviour {
	float timer;
	float start_time;
	List <Vector3> positions;
	List <Vector3> angles;
	List<float> times;
	List<float> speeds;
	Vector3 lightrot;
	int cam_count;
	int current_cam;
	bool pause = false;
	float pause_start_time;
	[HideInInspector]
	public RouteChoice route_choice;
	public Light sun;
	float FoV = 60f;
	bool initialized = false;
    float scale = 316f;  // scale from old coordinate system to new km-based one

	// Use this for initialization
	void Start () {
		if (initialized) // don't initialize twice
			return;
		positions = new List<Vector3> ();
		angles = new List<Vector3> ();
		times = new List<float> ();
		speeds = new List<float> ();
		initialized = true;
	}

	public void InitView() {
		Start ();
        switch (route_choice)
        {
            case RouteChoice.TransAt:
                // transatlantic view 45 degree
                cam_count = 2;
			    positions.Add (new Vector3 (-7f, 28f, -20f));
			    angles.Add (new Vector3 (51f, 20f, 0f));
			    times.Add (0f);
			    speeds.Add (0.11f);

			    positions.Add (new Vector3 (-10f, 10f, -23f));
			    angles.Add (new Vector3 (-30f, 20f, 0f));
			    times.Add (20f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (-10f, 10f, -23f));
			    angles.Add (new Vector3 (-30f, 20f, 0f));
			    times.Add (100000f);
			    speeds.Add (0.01f);

			    lightrot = new Vector3 (20f, -20f, 0f);
			    FoV = 45f;
                break;
		    case RouteChoice.TransPac:
			    // transpacific view 45 degree
			    cam_count = 2;
			    positions.Add (new Vector3 (-20f, 28f, 25f));
			    angles.Add (new Vector3 (30f, 140f, 0f));
			    times.Add (0f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (-13f, 23f, 22f));
			    angles.Add (new Vector3 (30f, 130f, 45f));
			    times.Add (20f);
			    speeds.Add (0.11f);

			    positions.Add (new Vector3 (-13f, 23f, 22f));
			    angles.Add (new Vector3 (30f, 130f, 45f));
			    times.Add (100000f);
			    speeds.Add (0.01f);
			    lightrot = new Vector3 (20f, 130f, 0f);
			    FoV = 45f;
                break;
		    case RouteChoice.USdense:
		    case RouteChoice.USsparse:
		    // US view 60 degree
			    cam_count = 2;
			    positions.Add (new Vector3 (-25f, 23f, -8f));
			    angles.Add (new Vector3 (45f, 70f, 0f));
			    times.Add (0f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (-25f, 23f, -8f));
			    angles.Add (new Vector3 (45f, 70f, 0f));
			    times.Add (100000f);
			    speeds.Add (0.01f);

			    lightrot = new Vector3 (20f, 130f, 0f);
			    FoV = 60f;
                break;

		    case RouteChoice.TorMia:
			    // US view 60 degree
			    cam_count = 2;
			    positions.Add (new Vector3 (-18f, 18f, -12f));
			    angles.Add (new Vector3 (45f, 60f, 0f));
			    times.Add (0f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (-18f, 18f, -12f));
			    angles.Add (new Vector3 (45f, 60f, 0f));
			    times.Add (100000f);
			    speeds.Add (0.01f);

			    lightrot = new Vector3 (20f, 130f, 0f);
			    FoV = 60f;
                break;

		    case RouteChoice.LonJob:
		    // london joburg 60 degree view
			    /*
			    cam_count = 2;
			    positions.Add (new Vector3 (20f, 15f, -40f));
			    angles.Add (new Vector3 (20f, -30f, 0f));
			    times.Add (0f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (20f, 15f, -40f));
			    angles.Add (new Vector3 (20f, -30f, 0f));
			    times.Add (10000f);
			    speeds.Add (0.01f);
			    break;
			    */


			    // london joburg 45 degree view
			    cam_count = 2;
			    positions.Add (new Vector3 (30f, 16f, -35f));
			    angles.Add (new Vector3 (20f, -40f, 0f));
			    times.Add (0f);
			    speeds.Add (0.01f);

			    positions.Add (new Vector3 (30f, 16f, -35f));
			    angles.Add (new Vector3 (20f, -40f, 0f));
			    times.Add (10000f);
			    speeds.Add (0.01f);
			    FoV = 45f;
                lightrot = new Vector3 (20f, 0f, 0f);
			    break;

            case RouteChoice.Sydney_SFO:
                cam_count = 4;
                positions.Add(new Vector3(-13000, 3000, 3000));
                angles.Add(new Vector3(10f, 100f, 0f));
                times.Add(0f);
                speeds.Add(0.01f);

                positions.Add(new Vector3(-14000, -3000, 12000));
                angles.Add(new Vector3(-10f, 130f, 0f));
                times.Add(20f);
                speeds.Add(0.01f);

                positions.Add(new Vector3(-2000, -4000, 12000));
                angles.Add(new Vector3(-20f, 170f, 0f));
                times.Add(40f);
                speeds.Add(0.01f);

                positions.Add(new Vector3(-2000, -4000, 12000));
                angles.Add(new Vector3(-20f, 170f, 0f));
                times.Add(10000f);
                speeds.Add(0.01f);

                lightrot = new Vector3(20f, 150f, 0f);
                FoV = 60f;
                scale = 1f;
                break;

            case RouteChoice.Sydney_Tokyo:
                cam_count = 2;

                positions.Add(new Vector3(5000, 0, 12000));
                angles.Add(new Vector3(0f, -150f, 0f));
                times.Add(0f);
                speeds.Add(0.01f);

                positions.Add(new Vector3(5000, 0, 12000));
                angles.Add(new Vector3(0f, -150f, 0f));
                times.Add(10000f);
                speeds.Add(0.01f);

                lightrot = new Vector3(20f, 150f, 0f);
                FoV = 60f;
                scale = 1f;
                break;

            case RouteChoice.Sydney_Lima:
                cam_count = 2;
                positions.Add(new Vector3(-16000, -6000, 1000));
                angles.Add(new Vector3(-20f, 100f, 0f));
                times.Add(0f);
                speeds.Add(0.01f);

                positions.Add(new Vector3(-16000, -6000, 1000));
                angles.Add(new Vector3(-20f, 100f, 0f));
                times.Add(10000f);
                speeds.Add(0.01f);
     
                lightrot = new Vector3(20f, 150f, 0f);
                FoV = 45f;
                scale = 1f;
                break;

            case RouteChoice.Followsat:
			    // we just follow a satellite - no need to do anything here.
			    cam_count = 0;
			    FoV = 45f;
			    lightrot = new Vector3 (20f, 0f, 0f);
                break;
		}

		sun.transform.rotation = Quaternion.Euler (lightrot);
		Camera.main.fieldOfView = FoV;

		current_cam = 0;

        if (positions.Count > 0) {
            transform.position = positions[0] * scale;
			transform.rotation = Quaternion.Euler (angles [0]);
		}

		start_time = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space") || Input.GetKeyDown (".")) {
			if (pause == false) {
				pause = true;
				pause_start_time = Time.time;
				print ("space key was pressed");
			} else {
				pause = false;
				start_time += Time.time - pause_start_time;
				print ("space key was pressed");
			}
		}
	}

	// Update is called once per frame
	void LateUpdate () {
		Vector3 pos1, pos, pdiff;
		Vector3 rot1, cur_rot, rot, rdiff;
		if (pause || positions.Count == 0) {
			return;
		}
		// Have we passed the next time?
		if (Time.time - start_time > times [current_cam + 1]) {
			current_cam++;
			Debug.Log ("Current cam: " + current_cam.ToString());
			cam_count--;
		}
		// f goes from zero at times[0] to one at times[1]
		float elapsed_time = Time.time - start_time;
		float f = (elapsed_time - times [current_cam]) / (times [current_cam + 1] - times [current_cam]);
		pos1 = Vector3.Lerp (positions [current_cam]*scale, positions [current_cam + 1]*scale, f);
		pdiff = pos1 - transform.position;
		pos = transform.position + speeds[current_cam + 1] * pdiff;
		rot1 = Vector3.Lerp (angles [current_cam], angles [current_cam + 1], f);
		cur_rot = transform.rotation.eulerAngles;
		rdiff = rot1 - cur_rot;
		if (rdiff.x < -180f) {
			rdiff.x += 360f;
		}
		if (rdiff.y < -180f) {
			rdiff.y += 360f;
		}
		if (rdiff.z < -180f) {
			rdiff.z += 360f;
		}

		rot = cur_rot + speeds[current_cam + 1] * rdiff;

		transform.position = pos;
		transform.rotation = Quaternion.Euler(rot);

	}
}
