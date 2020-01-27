using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
//using Microsoft.VisualBasic.FileIO;


/// <summary>
/// Spin the object at a specified speed 
/// </summary>
public class OrbitsVLEO: MonoBehaviour {
	[Tooltip("Spin: Yes or No")]
	public bool spin;
	[Tooltip("Spin the parent object instead of the object this script is attached to")]
	public bool spinParent;
	public float speed = 1f; // a value of 1 is realtime
	float simspeed; // actual speed scaled appropriately

	//[HideInInspector]
	public bool clockwise = false;
	[HideInInspector]
	public float direction = 1f;
	[HideInInspector]
	public float directionChangeSpeed = 2f;
	const double earthperiod = 86400f;
	public GameObject orbit;
	public GameObject satellite_vleo;
	public GameObject satellite_low;
	public GameObject satellite_high;
	public GameObject satellite_polar;
	public GameObject laser;
	public Material new_laser_material;
	public Material laser_material;

	GameObject[] orbits;
	double[] orbitalperiod;
	SatelliteVLEO[] satlist;
	Vector3[] orbitaxes;
	int[] plane_sats;
	float[] plane_raan;
	float[] plane_inclination;
	float[] plane_altitude;
	string[] plane_constellation;
	int[] sat_plane;
	float[] sat_anomaly;
	int[] firstsat;
	int[] lastsat;
	int count = 0, satcount = 0;
	int framecount =0;
	public int phase = 1;
	public bool original = false;
	int step = 0;

	int maxsats;
	int maxorbits;
	const int maxlasers = 4;
	double meandist;
	float start_time;
	bool pause = false;
	float pause_start_time;
    public Utilities.SceneField prevscene;
    public Utilities.SceneField nextscene;


    void Start() {
		start_time = Time.time;
		if (original) {
			if (phase == 1) {
				maxsats = 1600;
				maxorbits = 32;
			} else if (phase == 2) {
				maxsats = 3200;
				maxorbits = 64;
			} else if (phase == 3) {
				maxsats = 4425;
				maxorbits = 83;
			} else {
				maxsats = 11944;
				maxorbits = 7603;
			}
		} else {
			if (phase == 1) {
				maxsats = 1584;
				maxorbits = 24;
			} else if (phase == 2) {
				maxsats = 3184;
				maxorbits = 56;
			} else if (phase == 3) {
				maxsats = 4409;
				maxorbits = 83;  // hack - D plane is two planes
			} else {
				maxsats = 11944;
				maxorbits = 7603;
			}
		}
			



		simspeed = speed * 360f / 86400f; // Speed 1 is realtime

		orbits = new GameObject[maxorbits];
		orbitalperiod = new double[maxorbits];
		orbitaxes = new Vector3[maxorbits];

		plane_sats = new int[maxorbits];
		plane_raan = new float[maxorbits];
		plane_inclination = new float[maxorbits];
		plane_constellation = new string[maxorbits];

		for (int i = 0; i < maxorbits; i++) {
			plane_inclination [i] = 0f;
		}
		plane_altitude = new float[maxorbits];

		/* new model */
		satlist = new SatelliteVLEO[maxsats];
		for (int i = 0; i < maxsats; i++) {
			satlist [i] = null;
		}
		sat_plane = new int[maxsats];
		sat_anomaly = new float[maxsats];

		firstsat = new int[1400];
		lastsat = new int[1400];
		for (int i = 0; i < 1400; i++) {
			firstsat[i] = -1;
			lastsat[i] = -1;
		}

		if (original) {
			//CreateSats (32, 50, 53f, 0f, 0f, 5f/32f, 6500.44, 1.180f);
		} else {
            float alt = 6371f + 550f;
			CreateSats (24, 66, 53f, 0f, 0f, 13f / 24f, 5739, alt);
		}
			
		if (phase >= 2) {
            float alt = 6371f + 1110f;
            CreateSats (32, 50, 53.8f, 5.625f, /*0.25f*/0f, 15f/32f, 6448.7, alt);
			//RAAN of plane A_B1 is "5.6" degrees.  Likely 1/2 of 1/32 of 360 = 5.625 
		}

		if (phase >= 3) {
            float alt = 6371f + 1130f;
            CreateSats (8, 25, 74f, 0f, 0f, 4/8f, 6474.55f, alt); // D
			CreateSats (8, 25, 74f, 0f, 0.708f, 4/8f, 6474.55f, alt); // D
            alt = 6371f + 1325f;
            CreateSats (6, 75, 70f, 0f, 0f, 1/6f, 6728.41f, alt); // C
            alt = 6371f + 1275f;
            CreateSats (5, 75, 81f, 0f, 0f, 1/6f, 6663.01f, alt);	// E
            // 1/6 phase for E plane results in a irregular constellation, but that's what the table says.
        }

		if (phase >= 4) {
			fgCSVReader.LoadFromFile("Assets/Orbits/VLEO_planes.csv", new fgCSVReader.ReadLineDelegate(this.ReadLinePlane));
			fgCSVReader.LoadFromFile("Assets/Orbits/VLEO_sats.csv", new fgCSVReader.ReadLineDelegate(this.ReadLineSat));
		}
	}

	void ReadLinePlane(int line_index, List<string> line)
	{
		/*string s = "\n==> Line {0}, {1} column(s) ";
		s += line_index.ToString() + " ";
		s += line.Count.ToString() + "\n";
		for (int i = 0; i < line.Count; i++) {
			s += line [i].ToString () + " ";
		}
		print(s);
*/
		if (line_index >= 1 /*&& line_index < 84*/) {
			int plane_id = int.Parse (line [0]);
			plane_sats [plane_id] = int.Parse (line [1]);
			plane_raan [plane_id] = float.Parse (line [11]);
			plane_inclination [plane_id] = float.Parse (line [3]);
			float period = float.Parse (line [5]);
			plane_altitude [plane_id] = float.Parse (line [6]);
			plane_constellation [plane_id] = line [10];

            float altitude = plane_altitude[plane_id] + 6371f;

            if (/*(plane_inclination [plane_id] == 48 || plane_inclination [plane_id] == 43) &&*/ plane_constellation[plane_id] == "VLEO") {
				CreateOrbit (plane_id, plane_inclination [plane_id], plane_raan [plane_id], period, altitude);
			} else {
				//orbits [plane_id] = null;
			}
		}
	}

	void ReadLineSat(int line_index, List<string> line)
	{
		/*
		string s = "\n==> Line {0}, {1} column(s) ";
		s += line_index.ToString() + " ";
		s += line.Count.ToString() + "\n";
		for (int i = 0; i < line.Count; i++) {
			s += line [i].ToString () + " ";
		}
		print(s);
*/
		if (line_index >= 1) {
			int sat_id = int.Parse (line [0]);
			sat_plane[sat_id]  = int.Parse (line [1]);
			sat_anomaly[sat_id] = float.Parse (line [2]);
			CreateSat (sat_id, sat_plane[sat_id], sat_anomaly[sat_id]);
		}
	}

	void CreateOrbit(int plane_id, float inclination, float raan, double period, float altitude) {
		//print ("CreateOrbit: " + " inclination:" + inclination.ToString() 
		//	+ " raan:" + raan.ToString() + " alt:" + altitude.ToString() + "\n");
		orbits[plane_id] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
		orbits[plane_id].transform.RotateAround (Vector3.zero, Vector3.forward, inclination);
		orbits[plane_id].transform.RotateAround (Vector3.zero, Vector3.up, raan);
		orbitaxes [plane_id] = Quaternion.Euler(0, raan, 0) * (Quaternion.Euler (0, 0, inclination) * Vector3.up);
		orbitalperiod [plane_id] = period;
        orbits[plane_id].transform.localScale = new Vector3(altitude, altitude, altitude);
		if (plane_id > maxorbits) {
			maxorbits = plane_id;
		}
	}

	void CreateSat(int sat_id, int plane_id, float anomaly) {
		if (orbits[plane_id] == null) {
			return;
		}
		float inclination = plane_inclination [plane_id];
		float altitude = plane_altitude [plane_id] + 6371f;
		GameObject orbit = orbits [plane_id];
		SatelliteVLEO newsat;
		int alt_in_km = (int)(plane_altitude [plane_id]);
		if (plane_altitude [plane_id] < 500) {
			newsat = new SatelliteVLEO (satcount, 1, 1, transform, orbit, 
				anomaly, maxlasers, maxsats, 0f, 1, 1,
				altitude, alt_in_km, satellite_vleo, laser);
		} else if (plane_altitude [plane_id] < 600) {
			return;
			newsat = new SatelliteVLEO (satcount, 1, 1, transform, orbit, 
				anomaly, maxlasers, maxsats, 0f, 1, 1,
				altitude, alt_in_km, satellite_low, laser);
		} else if (plane_altitude [plane_id] < 1200) {
			return;
			newsat = new SatelliteVLEO (satcount, 1, 1, transform, orbit, 
				anomaly, maxlasers, maxsats, 0f, 1, 1,
				altitude, alt_in_km, satellite_high, laser);
		} else {
			return;
			newsat = new SatelliteVLEO (satcount, 1, 1, transform, orbit, 
				anomaly, maxlasers, maxsats, 0f, 1, 1,
				altitude, alt_in_km, satellite_polar, laser);
		}
		if (firstsat[alt_in_km] == -1) {
			firstsat[alt_in_km] = sat_id;
		}
		if (sat_id > lastsat[alt_in_km]) {
			lastsat[alt_in_km] = sat_id;
		}
		//print (sat_id);
		satlist [sat_id] = newsat;
		satcount++;
	}
		

	void CreateSats(int num_orbits, int num_sats, float inclination, float plane0_raan, 
		float sat_phase_offset, float sat_phase_stagger, double period, float altitude) {
		float orbit_angle_step = 360f / num_orbits;
		//print ("CreateSats: num_orbits:" + num_orbits.ToString() + " num_sats:" + num_sats.ToString()
		//	+ " inclinations:" + inclination.ToString() + " raan:" + plane0_raan.ToString() + "\n");
		for (int i = 0; i < num_orbits; i++) {
			//Quaternion q = Quaternion.Euler(new Vector3(0f, i*11.25f, 53f));
			orbits[count] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.forward, inclination);
			float orbit_angle = plane0_raan + i * orbit_angle_step;
			orbits[count].transform.RotateAround (Vector3.zero, Vector3.up, orbit_angle);
			orbitaxes [count] = Quaternion.Euler(0, orbit_angle, 0) * (Quaternion.Euler (0, 0, inclination) * Vector3.up);
			orbitalperiod [count] = period;
            orbits[count].transform.localScale = new Vector3(altitude, altitude, altitude);

            for (int s = 0; s < num_sats; s++) {
				double sat_angle_step = 360f / num_sats;
				double sat_angle = (-1f * sat_phase_offset * sat_angle_step) + (i * sat_angle_step * sat_phase_stagger) + (s * sat_angle_step);
				SatelliteVLEO newsat;
				if (altitude < 6371f + 600f) {
					newsat = new SatelliteVLEO (satcount, s, i, transform, orbits [count], 
						sat_angle, maxlasers, maxsats, sat_phase_stagger, num_sats, num_orbits,
						altitude, 550, satellite_low, laser);
				} else if (inclination > 54f) {
					newsat = new SatelliteVLEO (satcount, s, i, transform, orbits [count], 
						sat_angle, maxlasers, maxsats, sat_phase_stagger, num_sats, num_orbits,
						altitude, 1100, satellite_polar, laser);
				} else {
					newsat = new SatelliteVLEO (satcount, s, i, transform, orbits [count], 
						sat_angle, maxlasers, maxsats, sat_phase_stagger, num_sats, num_orbits,
						altitude, 1100, satellite_high, laser);
				} 
				satlist [satcount] = newsat;
				satcount++;
			}

			count++;
		}
	}


	float longitude(int satnum) {
		Vector3 pos = satlist [satnum].position ();
		float l = (float)(Mathf.Atan2 (pos.x, pos.z) * (180/Mathf.PI));
		if (satnum == 4526) {
			print("pos: " + pos.ToString() + " long: " + l.ToString());
		}
		return l;
	}

	float latitude(int satnum) {
		Vector3 pos = satlist [satnum].position ();
		float edist = Mathf.Sqrt (pos.x * pos.x + pos.z * pos.z);
		float lat = (float)(Mathf.Atan2 (pos.y, edist) * (180/Mathf.PI));
		if (satnum == 4526) {
			print("pos: " + pos.ToString() + " lat: " + lat.ToString());
		}
		return lat;
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown ("space") || Input.GetKeyDown (".")) {
			if (pause == false) {
				pause = true;
				pause_start_time = Time.time;
			} else {
				pause = false;
				start_time += Time.time - pause_start_time;
			}
		}
        if (Input.GetKeyDown("b") || Input.GetKeyDown(KeyCode.PageUp))
        {
            SceneManager.LoadScene(prevscene);
            return;
        }
        if (Input.GetKeyDown("n") || Input.GetKeyDown(KeyCode.PageDown))
        {
            SceneManager.LoadScene(nextscene);
            return;
        }

		int i=0;
		if (direction < 1f) {
			direction += Time.deltaTime / (directionChangeSpeed / 2);
		}

		if (spin) {
			if (clockwise) {
				if (spinParent)
					transform.parent.transform.Rotate (Vector3.up, (simspeed * direction) * Time.deltaTime);
				else
					transform.Rotate (Vector3.up, (simspeed * direction) * Time.deltaTime);
			} else {
				if (spinParent)
					transform.parent.transform.Rotate (-Vector3.up, (simspeed * direction) * Time.deltaTime);
				else
					transform.Rotate (-Vector3.up, (simspeed * direction) * Time.deltaTime);
			}
		}
		for (i = 0; i < maxorbits; i++) {
			if (orbits [i] != null) {
				orbits [i].transform.RotateAround (Vector3.zero, orbitaxes [i], 
					(float)(-1f * earthperiod / orbitalperiod [i] * simspeed * direction) * Time.deltaTime);
			}
		}
		float elapsed_time = Time.time - start_time;
        if (pause)
        {
            elapsed_time -= (Time.time - pause_start_time);
        }
	}
}


