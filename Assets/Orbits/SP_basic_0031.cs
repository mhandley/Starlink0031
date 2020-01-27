using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public enum RouteChoice {TransAt, TransPac, LonJob, USsparse, USdense, TorMia, Sydney_SFO, Sydney_Tokyo, Sydney_Lima, Followsat};

public class ActiveISL {
	public SatelliteSP0031 sat1, sat2;
	public Node node1, node2;
	public ActiveISL(SatelliteSP0031 sat1_, Node node1_, SatelliteSP0031 sat2_, Node node2_) {
		sat1 = sat1_;
		sat2 = sat2_;
		node1 = node1_;
		node2 = node2_;
	}
}

public class ActiveRF {
	public SatelliteSP0031 sat;
	public GameObject city;
	public Node node1, node2;
	public ActiveRF(GameObject city_, Node node1_, SatelliteSP0031 sat_, Node node2_) {
		city = city_;
		sat = sat_;
		node1 = node1_;
		node2 = node2_;
	}
}

public class SP_basic_0031: MonoBehaviour {
	public Camera cam;
	[Tooltip("Spin: Yes or No")]
	public bool spin;
	[Tooltip("Speed 1 is realtime")]
	public float speed = 1f; // a value of 1 is realtime
    float orig_speed = 1f;
	float simspeed; // actual speed scaled appropriately

	[Tooltip("Enable use of inter-sat lasers")]
	public bool use_isls = true;
	[Tooltip("Enable use of ground relays")]
	public bool use_relays = false;
	public float relay_dist_step = 1f;
	[HideInInspector]
	public float direction = 1f;
	[HideInInspector]
	public float directionChangeSpeed = 2f;
	const double earthperiod = 86400f;
	public GameObject orbit;
	public GameObject satellite;
	public GameObject laser;
    public GameObject thin_laser;
	public GameObject city_prefab;
	public GameObject beam_prefab;
	public GameObject beam_prefab2;

	GameObject[] orbits;
	double[] orbitalperiod;
	SatelliteSP0031[] satlist;
	Vector3[] orbitaxes;
	int orbitcount = 0, satcount = 0;
	public Material isl_material;
	public Material yellowMaterial;
	public Material cityMaterial;
	public Text txt;
	public Text countdown;
	public Text rightbottom;
    float elapsed_time;
    float last_speed_change;
    float last_elapsed_time;
	int framecount =0;
	int maxsats; // total number of satellites
	int phase1_sats;  // number of satellites in phase 1
	public int maxorbits;
	const int maxlasers = 4;
	int isl_plane_shift = 0;
	int isl_plane_step = 0;
	bool isl_connect_plane = true;
	int isl_plane2_shift = 0;
	int isl_plane2_step = 0;
	double meandist;
	float start_time;
	bool pause = false;
	float pause_start_time;
	Node[] nodes;
	RouteGraph rg;
	float km_per_unit;
	//float km_per_unit2;
	GameObject london, new_york, san_francisco, singapore, johannesburg, athens, auckland, sydney;
	GameObject northbend, conrad, merrillan, greenville,redmond,hawthorne, bismarck, toronto, 
		thunderbay, columbus, lisbon, miami, majorca, tokyo, chicago, lima;

	//GameObject beam1 = null, beam2 = null;
	GameObject[] lasers;
	List <GameObject> relays;
	string prevpath = "";

	bool route_init = false;
	Vector3 sat0pos;

	int lastpath; // used for multipaths
	List<ActiveISL> used_isl_links;
	List<ActiveRF> used_rf_links;
	int followsat_id = 0;
	float last_dist_calc = -1000f; // last time we update the set of nearest routers
	SatelliteSP0031[] nearest_sats;  // used for calculating collision distances
    float mindist = Node.INFINITY; // used for calculating collision distances

    System.IO.StreamWriter logfile;
	float maxdist = 0f;
	float beam_radius = 0f;
	float margin = 100f;
	GroundGrid grid;
	long elapsed_sum = 0;
	int elapsed_count = 0;

	public RouteChoice route_choice;
	public enum ConstellationChoice {P24_S66_A550, P72_S22_A550, P32_S50_A1100};
	public ConstellationChoice constellation;
	public int no_of_paths = 1;

	public int decimator;
	public float raan0 = 0f;


	public enum LogChoice {None, RTT, Distance};
	public LogChoice log_choice = LogChoice.None;
	public string log_filename = "/users/mjh/docs/starlink/rtt.txt";
	public enum BeamChoice {AllOff, AllOn, SrcDstOn};
	public BeamChoice beam_on;
	public bool graph_on;
	public int speedup;
	public Utilities.SceneField prevscene;
	public Utilities.SceneField nextscene;



	void Start() {

        // handle time in a way that copes with speed change mid sim
        last_elapsed_time = Time.time;
        last_speed_change = last_elapsed_time;
        elapsed_time = 0f;

		orbitcount = 0;
		satcount = 0;
		Application.runInBackground = true;
		sat0pos = Vector3.zero; // center of earth!
		if (log_choice != LogChoice.None) {
			logfile = new System.IO.StreamWriter (@log_filename);
		}
		start_time = Time.time;

		/* ask the camera to view the same area as our route */
		CameraSP_0031 camscript = (CameraSP_0031)cam.GetComponent (typeof(CameraSP_0031));
		camscript.route_choice = route_choice;
		camscript.InitView ();

		int satsperorbit = 0;
		float sat0alt = 0;  // altitiude of sat 0 (other shells may have different altitudes)
		int beam_angle = 0;  // angle above horizon sat is reachable (likely 25 or 40 degrees)
		int orbital_period = 0;
		float phase_offset = 0f;

		switch (constellation) {
		case ConstellationChoice.P24_S66_A550:
			// phase 1 from 2019 FCC filing
			maxorbits = 24 / decimator;
			satsperorbit = 66;
			sat0alt = 550f;
			beam_angle = 25;
			maxdist = 1123f; // max RF distance from sat to ground station
			beam_radius = 940f;
			orbital_period = 5739; // seconds
			isl_connect_plane = true;
			isl_plane_shift = -1;  // isl offset to next plane
			isl_plane_step = 1;
			phase_offset = 13f / 24f;
			break;
		case ConstellationChoice.P72_S22_A550:
			// phase 1 from 2018 FCC filing
			maxorbits = 72 / decimator;
			satsperorbit = 22;
			sat0alt = 550f;
			beam_angle = 25;
			maxdist = 1123f;
			beam_radius = 940f;
			orbital_period = 5739; // seconds	
			isl_connect_plane = false;
			isl_plane_shift = -1; 
			isl_plane_step = 1;
			isl_plane2_shift = -1;
			isl_plane2_step = 2;
			phase_offset = 39f / 72f;
			break;
		case ConstellationChoice.P32_S50_A1100:
			// phase 1 from 2016 FCC filing
			maxorbits = 32 / decimator;
			satsperorbit = 50;
			sat0alt = 1150f;
			beam_angle = 40;
			maxdist = 1600f;  // rough guess 1150*sqrt(2), but check this value
			beam_radius = 1060f;
			orbital_period = 6500; // seconds
			isl_connect_plane = true;
			isl_plane_shift = 0; 
			isl_plane_step = 1;
			phase_offset = 11f / 32f;
			break;
		}
		maxsats = maxorbits * satsperorbit;
		phase1_sats = maxsats;  // will differ if simulating multiple phases

        orig_speed = speed;
		simspeed = speed * 360f / 86400f; // Speed 1 is realtime
		rightbottom.text = speed.ToString() + "x realtime";

		orbits = new GameObject[maxorbits];
		orbitalperiod = new double[maxorbits];
		orbitaxes = new Vector3[maxorbits];

		/* new model */
		satlist = new SatelliteSP0031[maxsats];

		const float earth_r = 6371f; // earth radius
		float sat0r = sat0alt + earth_r;  // sat radius from earth centre

		switch (constellation) {
		case ConstellationChoice.P24_S66_A550:
			CreateSats (maxorbits, satsperorbit, 53f, 0f, 0f, phase_offset, orbital_period, sat0r, 
						beam_angle, beam_radius);
			break;

		case ConstellationChoice.P72_S22_A550:
			CreateSatsDirect (maxorbits, satsperorbit, 53f, 0f, phase_offset, 
							  decimator * phase_offset * 360f / 22f, 
							  decimator * 360f / 72f, 360 / 22f, orbital_period, sat0r, 
							  beam_angle, beam_radius);
			break;

		case ConstellationChoice.P32_S50_A1100:
			CreateSats (maxorbits, satsperorbit, 53f, 0f, 0f, phase_offset, orbital_period, sat0r, 
						beam_angle, beam_radius);
			break;
		}

		float earthdist = Vector3.Distance (satlist[0].gameobject.transform.position, transform.position);
		km_per_unit = sat0r / earthdist;  // sim scale factor

		grid = new GroundGrid (10, maxdist, margin, km_per_unit, city_prefab, transform);  // 5 degrees is 550km (in lat)
		InitCities ();
		rg = new RouteGraph();
		InitRoute(rg);


		Debug.Assert (satcount == maxsats);

		for (int satnum = 0; satnum < maxsats; satnum++) {	
			satlist [satnum].glow = false;
			for (int s2 = 0; s2 < maxsats; s2++) {
				if (satnum != s2) {
					satlist [satnum].AddSat (satlist [s2]);
				}
			}
		}

		meandist = 0;
		for (int satnum = 1; satnum < maxsats; satnum++) {
			meandist += Vector3.Distance(satlist[0].position(), satlist[satnum].position());
		}
		meandist /= (maxsats - 1);
		for (int satnum = 0; satnum < maxsats; satnum++) {
			if (use_isls) {
				if (isl_connect_plane) {
					// connect lasers along orbital plane
					satlist [satnum].PreAssignLasersOrbitalPlane ();
				} else {
					satlist [satnum].PreAssignLasersBetweenPlanes (isl_plane2_shift, isl_plane2_step);
				}
				satlist [satnum].PreAssignLasersBetweenPlanes (isl_plane_shift, isl_plane_step);
			}
		}

		used_isl_links = new List<ActiveISL>(); // keep track of links we've highlighted
		used_rf_links = new List<ActiveRF>();

		if (route_choice == RouteChoice.Followsat) {
			// We're going to follow a satellite.
			// Need to glue the camera to the satellite.
			followsat_id = 0;
			switch (constellation) {
			case ConstellationChoice.P24_S66_A550:
				followsat_id = 78;
				break;
			case ConstellationChoice.P72_S22_A550:
				followsat_id = 1505;
				break;
			case ConstellationChoice.P32_S50_A1100:
				followsat_id = 105;
				break;
			}
			SatelliteSP0031 followsat = satlist[followsat_id];
			cam.transform.position = new Vector3 (100, 0, -60);
			cam.transform.rotation = Quaternion.Euler (0, 300, -90);
			cam.transform.SetParent (followsat.gameobject.transform,false);
			followsat.ChangeMaterial (yellowMaterial);
			nearest_sats = new SatelliteSP0031[4];
		}

		//float earthradius = Vector3.Distance (transform.position, london.gameObject.transform.position);
		//km_per_unit2 = (12756f / 2) / earthradius;
		//Debug.Log("km_per_unit: " + km_per_unit.ToString() + " km_per_unit2: " + km_per_unit2.ToString());

	}

	void InitCities() {
		// N and W are +ve
		relays = new List<GameObject> ();
		london = CreateCity (51.5f, 0f, false);
		new_york = CreateCity (40.76f, 73.98f, false);
		san_francisco = CreateCity (37.733795f, 122.446747f, false);
		singapore = CreateCity (1.290270f,-103.851959f, false);
		johannesburg = CreateCity (-26.1633f, -28.0328f, false);
		athens = CreateCity (37.983810f, -23.727539f, false);
		auckland = CreateCity (-36.84846f, -174.763336f, false);
        sydney = CreateCity(-33.865143f, -151.209900f, false);
        redmond = CreateCity (47.69408f, 122.03222f, false);
		miami = CreateCity (25.768125f, 80.197006f, false);
		tokyo = CreateCity (35.652832f, -139.839478f, false);
		chicago = CreateCity (41.881832f, 87.623177f, false);
		toronto = CreateCity (43.70011f, 79.4163f, false);


		/*
		//hawthorne = CreateCity (33.92119f, 118.32608f, false);
		*/

		/* transatlantic relays */
		float longdist = 0f;
		float latdist = 40007f / 360f;
		float areasum = 0f;

		if (use_relays) {
			switch (route_choice) { // trans-atlantic
			case RouteChoice.TransAt:
				CreateCity (53f, 24f, true);
				CreateCity (51f, 41f, true);

				//CreateCity (42f, 39f, true);

				/* Ireland, west coast */
				CreateCity (52f, 10f, true);
				CreateCity (53f, 10f, true);

				/* Nova Scotia */
				CreateCity (46f, 60f, true);
				CreateCity (44f, 65f, true);

				/* Maine */
				CreateCity (44f, 70f, true);

				/* Newfoundland */
				CreateCity (48f, 58f, true);
				CreateCity (49f, 54f, true);
				CreateCity (51.374132f, 55.581248f, true); // St Anthony

				/* Quebec City */
				CreateCity (46.804304f, 71.212131f, true);
				CreateCity (46.804304f, 72.212131f, true);

				CreateCity (50.132574f, 61.801297f, true); // Pointe-Parent, Quebec

				/* Cloridorme, New Brunswick */
				CreateCity (49.178832f, 64.836269f, true);


				/* Azores */
				CreateCity (39.449375f, 31.209317f, true);
				CreateCity (37.831072f, 25.141654f, true);

				/* Madeira */
				CreateCity (32.857646f, 17.198472f, true);

				/* Finistere, Spain */
				CreateCity (42.923941f, 9.279052f, true);

				/* Marrakesh*/
				CreateCity (31.655479f, 7.977596f, true);

				/* Laayoune, Western Sahara */
				CreateCity (27.156665f, 13.233910f, true);

				/* Las Palmas, Canary Islands */
				CreateCity (28.100000f, 15.451542f, true);

				/* Lisbon */
				lisbon = CreateCity (38.709687f, 9.485385f, true);
				break;


			case RouteChoice.LonJob:
				//relay_dist_step = 2.0f;
				/* Madeira */
				CreateCity (32.857646f, 17.198472f, true);

				/* Finistere, Spain */
				CreateCity (42.923941f, 9.279052f, true);

				/* Marrakesh*/
				CreateCity (31.655479f, 7.977596f, true);

				/* Laayoune, Western Sahara */
				CreateCity (27.156665f, 13.233910f, true);

				/* Las Palmas, Canary Islands */
				CreateCity (28.100000f, 15.451542f, true);

				/* Lisbon */
				lisbon = CreateCity (38.709687f, 9.485385f, true);

				// mediterranean
				CreateCity (45.4655f, -9.1865f, true);
				CreateCity (41.9028f, -12.4964f, true);
				CreateCity (40.8518f, -14.2681f, true);
				CreateCity (37.3979f, -14.6588f, true);
				CreateCity (41.0128f, -8.875f, true);
				CreateCity (35.9375f, -14.3754f, true);
				CreateCity (39.6953f, -3.0176f, true);

				// N Africa
				CreateCity (36.8188f, -10.166f, true);  // Tunis
				CreateCity (32.894548f, -13.182726f, true); // Tripoli
				CreateCity (18.063929f, 15.969875f, true); //Nouakchott, Mauritania
				CreateCity (16.953450f, 0.351920f, true); // Bourem, Mali
				CreateCity (13.523209f, -2.120887f, true); // Niamey, Miger
				CreateCity (16.968058f, -7.989515f, true); //Agadaz, Niger
				CreateCity (22.889711f, -4.847936f, true); // Abalessa, Algeria
				CreateCity (27.879648f, 0.287662f, true); // Adrar, Algeria
				CreateCity (26.719830f, 0.170543f, true); // Reggane, Algeria
				CreateCity (26.587697f, -12.775448f, true); // Ubari, Libya
				CreateCity (27.036485f, -14.422205f, true); // Sabha, Libya
				CreateCity (32.092705f, -20.087222f, true); // Benghazi, Libya
				CreateCity (31.204647f, -16.583952f, true); // Sirte, Libya
				CreateCity (29.030742f, -21.549600f, true); // Jalu, Libya
				CreateCity (25.672835f, -21.074561f, true); // Tazirbu, Libya
				CreateCity (14.423622f, -6.044345f, true); // Bouza, Niger
				CreateCity (18.739735f, -7.392920f, true); // Arlit, Niger
				CreateCity (16.771889f, 3.006630f, true); // Timbuktu, Mali
				CreateCity (16.613272f, 7.256843f, true); // Nema, Mauritania
				CreateCity (20.512436f, 13.045014f, true); //Atar, Mauritania
				CreateCity (26.739824f, 11.680986f, true); //Samara, W. Sahara
				CreateCity (14.253680f, -13.114089f, true); //N'Guigmi, Niger
				CreateCity (12.632408f, 8.009227f, true); // Bamaki, Mali
				CreateCity (14.492812f, 4.192475f, true); // Mopti, Mali
				CreateCity (14.947206f, 3.893295f, true); // Kona, Mali
				CreateCity (14.119974f, -15.312130f, true); // Mao, Chad
				CreateCity (13.643129f, -16.492301f, true); // Moussoro, Chad
				CreateCity (14.972823f, -8.880027f, true); // Tanout, Niger
				CreateCity (16.277464f, 0.046189f, true); // Gao, Mali
				CreateCity (15.181080f, -0.720703f, true); // Ouatagouna, Mali
				CreateCity (30.052325f, -31.234923f, true); // Cairo
				CreateCity (29.069891f, -31.095649f, true); // Ben Sweif, Egypt
				CreateCity (24.088748f, -32.896148f, true); // Aswan, Egypt
				CreateCity (22.371821f, -31.610475f, true); // Abu Simbel, Egypt
				CreateCity (15.508254f, -32.519168f, true); // Khartoum, Sudan
				for (float lat = 38f; lat < 44f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					for (float lng = 2f; lng < 10f; lng += relay_dist_step) {
						CreateCity (lat, lng, true);
						areasum += area;
					}
				}

				for (float lng = -10f; lng < 0f; lng += relay_dist_step) {
					CreateCity (36f, lng, true);
				}
				for (float lat = 32f; lat < 36f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					for (float lng = -10f; lng < 8f; lng += relay_dist_step) {
						CreateCity (lat, lng, true);
						areasum += area;
					}
				}
				for (float lat = 6f; lat < 14f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					for (float lng = -40f; lng < 14f; lng += relay_dist_step) {
						CreateCity (lat, lng, true);
						areasum += area;
					}
				}
				for (float lat = 0f; lat < 6f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					for (float lng = -40f; lng < -14f; lng += relay_dist_step) {
						CreateCity (lat, lng, true);
						areasum += area;
					}
				}
				for (float lat = -20f; lat < 0f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					for (float lng = -40f; lng < -12f; lng += relay_dist_step) {
						CreateCity (lat, lng, true);
						areasum += area;
					}
				}
				break;


			case RouteChoice.TransPac:
                    // transpacific relays
                    TransPacCities();
				    //CreateCity (51f, 145f, true);  // ship, gulf of alaska
				    break;

			case RouteChoice.USsparse:
				northbend = CreateCity (47.48244f, 121.76131f, true);
				conrad = CreateCity (48.2033f, 111.94527f, true);
				bismarck = CreateCity (46.80833f, 100.78374f, true);
				merrillan = CreateCity (43.40633f, 90.81427f, true);
				greenville = CreateCity (41.43355f, 80.33322f, true);
				CreateCity (51.17622f, 115.56982f, true); // Banff
				CreateCity (49.8844f, 97.14704f, true); // Winnipeg
				CreateCity (52.11679f, 106.63452f, true); // Saskatoon
				CreateCity (39.11417f, 94.62746f, true); // Kansas city
				CreateCity (39.73915f, 104.9847f, true); // Denver
				CreateCity (36.253368f, 115.066733f, true); // Vegas
				toronto = CreateCity (43.70011f, 79.4163f, true);
				thunderbay = CreateCity (48.38202f, 89.25018f, true);
				columbus = CreateCity (39.96118f, 82.99879f, true);
				break;

			case RouteChoice.TorMia:
			case RouteChoice.USdense:
				int[] startrows = { 13, 13, 13, 12, 11, 9, 8, 8, 7, 6, 6, 5, 3, 3, 2 };
				int[] endrows = { 45, 45, 49, 50, 50, 53, 54, 55, 55, 56, 56, 57, 57, 57, 57, 57, 57, 57, 57 };
				int[] holestart = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 19, 19, 14, 0, 18, 19, 0, 0, 10, 0, 0, 0 };
				int[] holestop  = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 20, 20, 16, 0, 20, 22, 0, 0, 26, 30, 31, 35 };

				//relay_dist_step = 1.0f;
				float latcount = 0;
				for (float lat = 30f; lat < 55f; lat += relay_dist_step) {
					longdist = (40075f / 360f) * Mathf.Cos (Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
					float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
					float lngcount = 0;
					for (float lng = 68f; lng < 126f; lng += relay_dist_step) {
						if (latcount >= startrows.Length || lngcount > startrows [(int)latcount]) {
							if (latcount >= endrows.Length || lngcount < endrows [(int)latcount]) {
								if (lngcount >= holestop [(int)latcount] || lngcount < holestart [(int)latcount]) {
									CreateCity (lat, lng, true);
									areasum += area;
								}
							}
						}
						lngcount += relay_dist_step;
					}
					latcount += relay_dist_step;
				}
				break;
            case RouteChoice.Sydney_SFO:
                    TransPacCities();
                    WPacificCities();
                    AusCities();
                    break;

                case RouteChoice.Sydney_Tokyo:
                    TransPacCities();
                    WPacificCities();
                    AusCities();
                    break;

                case RouteChoice.Sydney_Lima:
                    SPacificCities();
                    AusCities();

                    break;
                case RouteChoice.Followsat:
				// we just follow a satellite - no need for cities
				break;
			}

		}
	}

    void TransPacCities() {
        float longdist = 0f;
        float latdist = 40007f / 360f;
        float areasum = 0f;
        // transpacific relays
        CreateCity(52.834065f, -173.171531f, true);  //Attu Station, AK, USA
        CreateCity(51.867554f, 176.638798f, true);  //Adak, Alaska, USA
        CreateCity(52.212841f, 174.207698f, true);  //Atka, AK 99547, USA
        CreateCity(52.939563f, 168.860887f, true);  //Nikolski, AK 99638, USA
        CreateCity(53.889189f, 166.538114f, true);  //Amaknak Island, Unalaska, AK, USA
        CreateCity(54.133662f, 165.776601f, true);  //Akutan, AK 99553, USA
        CreateCity(55.059223f, 162.312326f, true);  //King Cove, AK, USA
        CreateCity(56.944874f, 154.168453f, true);  //Akhiok, AK 99615, USA
        CreateCity(57.204254f, 153.304328f, true);  //Old Harbor, AK, USA
        CreateCity(57.754380f, 152.492349f, true);  //Kodiak Station, AK 99615, USA
        CreateCity(59.349383f, 151.829452f, true);  //Homer, AK 99603, USA
        CreateCity(59.797965f, 144.598995f, true);  //Kayak Island State Marine Park, Alaska, USA
        CreateCity(59.509158f, 139.670835f, true);  //Yakutat, AK 99689, USA
        CreateCity(57.960182f, 136.230894f, true);  //Pelican, AK 99832, USA
        CreateCity(56.247291f, 134.647728f, true);  //Port Alexander, AK 99836, USA
        CreateCity(54.310081f, 130.323739f, true);  //Prince Rupert, BC V8J 3K8, Canada
        CreateCity(43.384242f, -145.810615f, true);  //Nosappu, Nemuro, Hokkaido 087-0165, Japan
        CreateCity(43.869647f, -146.828961f, true);  //Unnamed Road, Shikotan-mura 694520
        CreateCity(50.676459f, -156.139535f, true);  //Severo-Kurilsky District, Sakhalin Oblast, Russ
        CreateCity(39.641144f, -141.952581f, true);  //Ōdōri, Miyako-shi, Iwate-ken 027-0083, Japan
        CreateCity(42.011085f, -143.150735f, true);  //Erimo-chō, Horoizumi-gun, Hokkaidō 058-0203, Japan
        CreateCity(53.253489f, 132.117766f, true);  //Queen Charlotte F, BC, Canada
        CreateCity(50.723849f, 127.496263f, true);  //Port Hardy, BC V0N 2P0, Canada
                                                    //relay_dist_step = 2.0f;
        for (float lat = 30f; lat < 55f; lat += relay_dist_step) {
            longdist = (40075f / 360f) * Mathf.Cos(Mathf.Deg2Rad * (lat + relay_dist_step / 2f));
            float area = (latdist * relay_dist_step) * (longdist * relay_dist_step);
            for (float lng = 72f; lng < 122f; lng += relay_dist_step) {
                CreateCity(lat, lng, true);
                areasum += area;
            }
        }
    }

    void WPacificCities() {
        CreateCity(-9.459888f, -147.187468f, true);  // Port Morsby, PNG
        CreateCity(-0.875620f, -131.246103f, true);  // Sorong, West Papua
        CreateCity(-8.511734f, -126.015478f, true); // Manatuto, Timor-Leste
        CreateCity(-2.605694f, -140.677133f, true);  // Jayapura, Indonesia
        CreateCity(0.787670f, -127.386715f, true);  // Ternate City, Indonesia
        CreateCity(7.079770f, -125.618577f, true);  // Davao, Phillipines
        CreateCity(14.587267f, -120.992825f, true);  // Manilla, Phillipines
        CreateCity(18.582077f, -120.785173f, true);  // Burayoc, Phillipines
        CreateCity(22.002509f, -120.743952f, true);  // Hengchung, Taiwan
        CreateCity(25.153802f, -121.747830f, true);  // Keelung, Taiwan
        CreateCity(30.370269f, -130.882104f, true);  // Nishino, Kagoshima, Japan
        CreateCity(31.563340f, -130.553916f, true);  // Kagoshima City, Japan
    }

    void AusCities() {
        CreateCity(-12.463968f, -130.842777f, true);  // Darwin, AUS
        CreateCity(-16.920180f, -145.769174f, true);  // Cairns, AUS
        CreateCity(-27.501833f, -153.060355f, true);  // Brisbane, AUS
        CreateCity(-12.185451f, -136.788007f, true);  // Nuhlunbuy, AUS
        CreateCity(-17.671119f, -141.078417f, true);  // Normanton, QND, AUS
        CreateCity(-23.704273f, -133.875712f, true);  // Alice Springs, AUS
        CreateCity(-31.525911f, -159.065333f, true);  // Lord Howe Island, AUS
    }

    void SPacificCities() {
        CreateCity(-29.031895f, -167.946841f, true);  // Burnt Pine, Norfolk Island
        CreateCity(-22.274674f, -166.452682f, true); // Noumea, New Caledonia
        CreateCity(-18.143231f, -178.435968f, true); // Suva, Fiji
        CreateCity(-13.834423f, 171.760831f, true);  // Apia, Samoa
        CreateCity(-14.220016f, 169.423424f, true);  // Maia, Manu'a, American Samoa
        CreateCity(-21.207587f, 159.783645f, true);  // Cook Islands
        CreateCity(-21.926098f, 157.955635f, true);  // Oneroa, Cook Islands
        CreateCity(-17.558133f, 149.600364f, true);  // F'a'a', French Polynesia
        CreateCity(-14.959108f, 147.646249f, true);  // Rangiroa, French Polynesia
        CreateCity(-16.067234f, 145.614420f, true);  // Rotoava, French Polynesia
        CreateCity(-17.354274f, 138.444553f, true);  // Tatakoto, French Polynesia
        CreateCity(-23.122389f, 134.968636f, true);  // Rikitea, French Polynesia
        CreateCity(-25.066740f, 130.099011f, true);  // Adamstown, Pitcairn Islands
        CreateCity(-24.333670f, 128.303854f, true);  // Henderson Island, Pitcairn Islands (uninhabited)
        CreateCity(-24.674048f, 124.777367f, true);  // Ducie, Pitcairn Islands (uninhabited)
        CreateCity(-25.91f, 117.1f, true);  // ship
        CreateCity(-27.149430f, 109.428944f, true);  // Easter Island
        //CreateCity(-22.14f, 98.75f, true); // ship
        //CreateCity(-17.03f, 87.7f, true); // ship
        //CreateCity(-21.523945f, 92.142192f, true); // ship
        CreateCity(-12.073062f, 77.065722f, true); // Lima, Peru
        CreateCity(-18f, 103f, true); // ship
        CreateCity(-9f, 97f, true); // ship
        CreateCity(-0.956546f, 90.968258f, true); // Puerto Villamil, Galapagos, Ecuador
        lima = CreateCity(-1.069440f, 80.907160f, true); // San Lorenzo, Ecuador
    }

    GameObject CreateCity(float latitude, float longitude, bool is_relay) {
		GameObject city = (GameObject)Instantiate (city_prefab, new Vector3(0f, 0f, -6382.2f), transform.rotation);
		float long_offset = 20f;
		city.transform.RotateAround (Vector3.zero, Vector3.up, longitude - long_offset);
		Vector3 lat_axis = Quaternion.Euler(0f, -90f, 0f) * city.transform.position;
		city.transform.RotateAround (Vector3.zero, lat_axis, latitude);
		city.transform.SetParent (transform, false);
		CityScript cs = (CityScript)city.GetComponent (typeof(CityScript));
		cs.longitude = longitude;
		cs.latitude = latitude;
		if (is_relay) {
			grid.AddCity (latitude, longitude, relays.Count, city);
			relays.Add (city);
		}
		return city;
	}

	// Default way to create a constellation
	void CreateSats(int num_orbits, int sats_per_orbit, float inclination, float orbit_phase_offset, 
					float sat_phase_offset, float sat_phase_stagger, double period, float altitude, 
					int beam_angle, float beam_radius) {
		float orbit_angle_step = 360f / num_orbits;
		for (int i = 0; i < num_orbits; i++) {
			orbits[orbitcount] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[orbitcount].transform.RotateAround (Vector3.zero, Vector3.forward, inclination);
			float orbit_angle = orbit_phase_offset * orbit_angle_step + i * orbit_angle_step  + raan0;
			orbits[orbitcount].transform.RotateAround (Vector3.zero, Vector3.up, orbit_angle);
			orbitaxes [orbitcount] = Quaternion.Euler(0, orbit_angle, 0) * (Quaternion.Euler (0, 0, inclination) * Vector3.up);
			orbitalperiod [orbitcount] = period;
			orbits [orbitcount].transform.localScale = new Vector3 (altitude, altitude, altitude);
			for (int s = 0; s < sats_per_orbit; s++) {
				double sat_angle_step = 360f / sats_per_orbit;
				double sat_angle =  (-1f * sat_phase_offset * sat_angle_step) + (i * sat_angle_step*sat_phase_stagger) + (s * sat_angle_step);
				SatelliteSP0031 newsat = new SatelliteSP0031 (satcount, s, i, transform, orbits[orbitcount], 
					sat_angle, maxlasers, maxsats, phase1_sats, sat_phase_stagger, sats_per_orbit, num_orbits,
					altitude, beam_angle, beam_radius, satellite, beam_prefab, beam_prefab2, laser, thin_laser);
				satlist [satcount] = newsat;
				if (beam_on == BeamChoice.AllOn) {
					newsat.BeamOn ();
				}
				satcount++;

			}
			orbitcount++;
		}
	}

	/* Alternative way to create a constellation (used for August 2019 constellation, as its high inter-plane 
	 * phase offset and high numnber of orbits cause wrapping with CreateSats) */
	void CreateSatsDirect(int num_orbits, int sats_per_orbit, float inclination, float orbit_phase_offset, 
						  float sat_phase_offset, float sat_angle_stagger /* degrees */, 
						  float orbit_angle_step, float sat_angle_step, double period, float altitude, 
						  int beam_angle, float beam_radius ) {

		for (int i = 0; i < num_orbits; i++) {
			orbits[orbitcount] = (GameObject)Instantiate (orbit, transform.position, transform.rotation);
			orbits[orbitcount].transform.RotateAround (Vector3.zero, Vector3.forward, inclination);
			float orbit_angle = -1 * (orbit_phase_offset * orbit_angle_step + i * orbit_angle_step) + raan0;
			orbits[orbitcount].transform.RotateAround (Vector3.zero, Vector3.up, orbit_angle);
			orbitaxes [orbitcount] = Quaternion.Euler(0, orbit_angle, 0) * (Quaternion.Euler (0, 0, inclination) * Vector3.up);
			orbitalperiod [orbitcount] = period;
			orbits [orbitcount].transform.localScale = new Vector3 (altitude, altitude, altitude);
			if (satcount < maxsats) {
				for (int s = 0; s < sats_per_orbit; s++) {
					double sat_angle = (-1 * i * sat_angle_stagger) + (-1 * s * sat_angle_step);
					while (sat_angle < sats_per_orbit * -360f / 22f) { // xxx
						sat_angle += sats_per_orbit * 360f / 22f;
					}
					sat_angle += 90f;
					SatelliteSP0031 newsat = 
						new SatelliteSP0031 (satcount, s, i, transform, orbits[orbitcount], sat_angle, maxlasers, 
											 maxsats, phase1_sats, sat_phase_offset, sats_per_orbit, num_orbits, altitude, 
											 beam_angle, beam_radius, satellite, beam_prefab, beam_prefab2, laser, thin_laser);
					satlist [satcount] = newsat;
					if (beam_on == BeamChoice.AllOn) {
						newsat.BeamOn ();
					}
					satcount++;
				}
			}
			orbitcount++;
		}
	}

	void DeleteSats(int num_orbits) {
		for (int satnum = 0; satnum < maxsats; satnum++) {
			satlist [satnum].clearrefs ();
			satlist [satnum] = null;
		}
		satcount = 0;
		for (int i = 0; i < num_orbits; i++) {
			MonoBehaviour.Destroy (orbits [i]);
			orbits [i] = null;
		}
		orbitcount = 0;
	}


	void UpdateLasers() {
		/* assign all the lasers that both sides agree on */
		for (int satnum = 0; satnum < maxsats; satnum++) {
			satlist [satnum].ClearAssignment ();
		}
			
		for (int satnum = 0; satnum < maxsats; satnum++) {
			satlist [satnum].UsePreAssigned ();
		}
			
		/* finalize the choices, and draw the lasers */
		for (int satnum = 0; satnum < maxsats; satnum++) {
			if (route_choice == RouteChoice.Followsat && followsat_id == satnum) {
				satlist [satnum].FinalizeLasers (speed, yellowMaterial);
			} else {
				satlist [satnum].FinalizeLasers (speed, isl_material);
			}
		}
	}

	GameObject get_relay(int nodeid) {
		int relaynum = -(nodeid + 1000);
		return relays [relaynum];
	}

	int get_relay_id(int relaynum) {
		return ((-1000) - relaynum);
	}

	void InitRoute(RouteGraph rg) {
		rg.Init (maxsats, relays.Count, maxdist, km_per_unit);
		// Plus 2 for start and end city
		for (int satnum = 0; satnum < maxsats; satnum++) {
			rg.NewNode(satlist [satnum].satid, satlist [satnum].gameobject);
		}
		rg.AddEndNodes ();
		int relaycount = 0;
		foreach (GameObject relay in relays) {
			rg.NewNode(get_relay_id(relaycount), relay);
			relaycount++;
		}
	}

	void ResetRoute(GameObject city1, GameObject city2) {
		rg.ResetNodes (city1, city2);
		for (int satnum = 0; satnum < maxsats; satnum++) {
			satlist [satnum].LinkOff ();
		}
	}

	void ResetRoutePos(GameObject city1, GameObject city2) {
		rg.ResetNodesPos (city1, city2);
		for (int satnum = 0; satnum < maxsats; satnum++) {
			satlist [satnum].LinkOff ();
		}
	}

	void LockRoute() {
		foreach (ActiveISL pair in used_isl_links) {
			pair.node1.LockLink (pair.node2);
			pair.node2.LockLink (pair.node1);
		}
		foreach (ActiveRF pair in used_rf_links) {
			pair.node1.LockLink (pair.node2);
			pair.node2.LockLink (pair.node1);
		}
	}

	void ClearRoute() {
		while (used_isl_links.Count > 0) {
			ActiveISL isl = used_isl_links[0];
			isl.sat1.ColourLink (isl.sat2, isl_material);
			isl.sat2.ColourLink (isl.sat1, isl_material);
			used_isl_links.RemoveAt (0);
		}
		while (used_rf_links.Count > 0) {
			// xxx remove graphic
			ActiveRF rf = used_rf_links[0];
			if (beam_on == BeamChoice.SrcDstOn) {
				rf.sat.BeamOff ();
			}
			used_rf_links.RemoveAt (0);
		}
	}

	// Illuminate the coverge areas for first and last satellite on a route (if selected) 
	void CreateSatBeams(SatelliteSP0031 sat1, GameObject city1, SatelliteSP0031 sat2, GameObject city2) {
		if (beam_on == BeamChoice.SrcDstOn) {
			sat1.BeamOn ();
			sat2.BeamOn ();
		}
	}

	void ChangeCityMaterial(GameObject city, Material mat) {
		Renderer[] renderers = city.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.material = mat;
		}
	}

	void BuildRouteGraph(RouteGraph rgph, GameObject city1, GameObject city2, float maxdist, float margin) {
		for (int satnum = 0; satnum < maxsats; satnum++) {
			for (int i = 0; i < satlist [satnum].assignedcount; i++) {
				rgph.AddNeighbour (satnum, satlist [satnum].assignedsats [i].satid);
			}

			// Add start city
			float radiodist = Vector3.Distance (satlist [satnum].gameobject.transform.position, 
				city1.transform.position);
			if (radiodist * km_per_unit < maxdist) { 
				rgph.AddNeighbour (maxsats, satnum, radiodist);
			} else if (radiodist * km_per_unit < maxdist + margin) { 
				rgph.AddNeighbour (maxsats, satnum, Node.INFINITY);
			}

			// Add end city
			radiodist = Vector3.Distance (satlist [satnum].gameobject.transform.position, 
				city2.transform.position);
			if (radiodist * km_per_unit < maxdist) {
				rgph.AddNeighbour (maxsats + 1, satnum, radiodist);
			} else if (radiodist * km_per_unit < maxdist + margin) {
				rgph.AddNeighbour (maxsats + 1, satnum, Node.INFINITY);
			}

			// Add relays
			if (graph_on) {
				satlist [satnum].GraphReset ();
			}

			List<List<City>> in_range = grid.FindInRange (satlist [satnum].gameobject.transform.position);
			foreach (List<City> lst in in_range) {
				foreach (City relay in lst) {
					radiodist = Vector3.Distance (satlist [satnum].gameobject.transform.position, relay.gameobject.transform.position);
					if (radiodist * km_per_unit < maxdist) {
						rgph.AddNeighbour (maxsats + 2 + relay.relayid, satnum, radiodist);
						if (graph_on) {
							satlist [satnum].GraphOn (relay.gameobject, null);
						}
					} else if (radiodist * km_per_unit < maxdist + margin) {
						rgph.AddNeighbour (maxsats + 2 + relay.relayid, satnum, Node.INFINITY);
					}
				}
			}
			if (graph_on) {
				satlist [satnum].GraphDone ();
			}
		}
	}

    void highlight_reachable() {
        for (int satnum = 0; satnum < maxsats; satnum++) {
            satlist[satnum].BeamOff();
        };
        Node[] reachable = rg.GetReachableNodes();
        print("reachable: " + reachable.Length.ToString());
        for (int i = 0; i < reachable.Length; i++) {
            Node rn = reachable[i];
            if (rn.Id >= 0) {
                SatelliteSP0031 sat = satlist[rn.Id];
                sat.BeamOn();
            }
        }
    }

	float Route (int pathnum, GameObject city1, GameObject city2, string name, float rtt, float actualdist) {
		DateTime startdate = DateTime.Now;

		Node rn = rg.endnode;
		SatelliteSP0031 sat = null, prevsat;
		int id = -1;

		bool reset_route = false;
		if (pathnum <= lastpath) {
			// new route attempt
			ClearRoute ();
			reset_route = true;
		} else {
			// additional route attempt in a multiroute
			LockRoute ();
		} 
		lastpath = pathnum;

		int uplinks = 0, downlinks = 0;
		if (reset_route) {

			float satmoveddist = Vector3.Distance (satlist [0].gameobject.transform.position, sat0pos);
			if (route_init == false || satmoveddist * km_per_unit > margin || graph_on) {
				ResetRoute (city1, city2);
				sat0pos = satlist [0].gameobject.transform.position;
				BuildRouteGraph (rg, city1, city2, maxdist, margin);
				route_init = true;
			} else {
				/* reset dijkstra, but not neighbours */
				ResetRoutePos (city1, city2);
			}
		} else {
			rg.ResetNodeDistances ();
		}

		rg.ComputeRoutes ();

		string path = "";
		// figure out the start and end satellites
		rn = rg.endnode;
		int startsatid = 0, endsatid = -1;
		id = -4;
		while (true) {
			if (rn == rg.startnode) {
				startsatid = id;
				break;
			}
			id = rn.Id;
			path = path + id.ToString () + " ";

			if (endsatid == -1 && id >= 0) {
				endsatid = id;
			}
			rn = rn.Parent;
			if (rn == null) {
				Debug.Log ("No route " + pathnum.ToString());
				if (log_choice == LogChoice.RTT) {
					logfile.WriteLine (elapsed_time.ToString () + " rtt 0 change 0 path " + pathnum.ToString());
				}
                highlight_reachable();
				return Node.INFINITY;
			}
		}
		int pathchange = 0;
		if (path != prevpath) {
			prevpath = path;
			pathchange = 1;
		}

		float enddist = km_per_unit * rg.endnode.Dist;
		float radiodist1 = km_per_unit * Vector3.Distance (satlist[startsatid].gameobject.transform.position, city1.gameObject.transform.position);
		float radiodist2 = km_per_unit * Vector3.Distance (satlist[endsatid].gameobject.transform.position, city2.gameObject.transform.position);
		float satdist = enddist - (radiodist1 + radiodist2);
		string s = "Sat: " + ((int)satdist).ToString() + " Dist: " + ((int)enddist).ToString () + " R1 " + ((int)radiodist1).ToString() + " R2 " + ((int)radiodist2).ToString()
			+ " Up " + uplinks.ToString() + " Down " + downlinks.ToString();
		float ms = 2 * enddist / 299.792f;
		if (log_choice == LogChoice.RTT) {
			logfile.WriteLine (elapsed_time.ToString () + " rtt " + ms.ToString () + " change " + pathchange
				+ " path " + pathnum.ToString());
			logfile.Flush ();
		}
		float fibre_rtt = 1.5f * 2f * actualdist / 299.792f;
		string s2 = name + "\nSat dist:" + ((int)enddist).ToString() + "\nSat RTT: " + ((int)ms).ToString () + "ms" 
			+ "\nGreat circle fibre RTT: " + ((int)fibre_rtt).ToString()  + "ms"
			+ "\nCurrent Internet RTT: " + ((int)rtt).ToString() + "ms";
		txt.text = s2;
		rn = rg.endnode;
		Node prevnode = null;
		sat = null;
		prevsat = null;
		string ds = "";
		int previd = -4;
		id = -4;

		while (true) {
			previd = id;
			id = rn.Id;
			ds += "s" + id.ToString () + "," + ((int)(km_per_unit*rn.Dist)).ToString () + " ";
			if (previd != -4) {
				if (previd >= 0 && id >= 0) {
					/* it's an ISL */
					sat = satlist [id];
					prevsat = satlist [previd];
					sat.ColourLink (prevsat, yellowMaterial);
					prevsat.ColourLink (sat, yellowMaterial);
					used_isl_links.Add (new ActiveISL (sat, rn, prevsat, prevnode));
				} else {
					/* it's an RF link */
					if (id >= 0) {  // sats have IDs >= 0, cities, < 0;
						sat = satlist [id];
						if (previd == -2) {
							sat.LinkOn (city2);
							used_rf_links.Add (new ActiveRF (city2, prevnode, sat, rn));
						} else {
							GameObject city = get_relay (previd);
							sat.LinkOn (city);
							used_rf_links.Add (new ActiveRF (city, prevnode, sat, rn));
						}
					} else {
						sat = satlist [previd];
						if (id == -1) {
							sat.LinkOn (city1);
							used_rf_links.Add (new ActiveRF (city1, rn, sat, prevnode));
						} else {
							GameObject city = get_relay (id);
							sat.LinkOn (city);
							used_rf_links.Add (new ActiveRF (city, rn, sat, prevnode));
							ChangeCityMaterial (city, cityMaterial);
						}
					}
				}
			}
			if (rn == rg.startnode) {
				break;
			}
			prevnode = rn;
			rn = rn.Parent;
			if (rn == null) {
                highlight_reachable();
                return Node.INFINITY;
			}
		}
		DateTime enddate = DateTime.Now;
		long elapsed = enddate.Ticks - startdate.Ticks;
		elapsed_sum += elapsed;
		elapsed_count++;
		CreateSatBeams (satlist [startsatid], city1, satlist [endsatid], city2);
		return ms;
	}

	void MultiRoute(int numroutes, GameObject city1, GameObject city2, string name, float rtt, float actualdist) {
		string s2 = name + "\nSat path RTTs: ";
		for (int i = 0; i < numroutes; i++) {
			float satrtt = Route (i, city1, city2, name, rtt, actualdist);
			if (i != 0) {
				s2 += ", ";
				if (i % 5 == 0)
					s2 += "\n";
			}
			if (satrtt < 2000f) {
				s2 += ((int)satrtt).ToString () + "ms";
			} else {
				s2 += "Fail!";
			}
		}
		float fibre_rtt = 1.5f * 2f * actualdist / 299.792f;
		s2 += "\nGreat circle fibre RTT: " + ((int)fibre_rtt).ToString()  + "ms"
			+ "\nCurrent Internet RTT: " + ((int)rtt).ToString() + "ms";
		txt.text = s2;
	}

	float FindNearest(int sat_id, float now) {
        float nearest_dist = Node.INFINITY;
		SatelliteSP0031 sat = satlist [sat_id];
        float[] dists = new float[4];
        if (now - last_dist_calc > 2f || nearest_sats[0] == null) {
            for (int i = 0; i < 4; i++) {
                dists[i] = Node.INFINITY;
                nearest_sats[i] = null;
            }
            for (int satnum = 0; satnum < maxsats; satnum++) {
                if (satnum == sat_id) {
                    continue;
                }
                SatelliteSP0031 othersat = satlist[satnum];
                float dist = Vector3.Distance(sat.position(), othersat.position());
                if (dist < dists[3]) {
                    dists[3] = dist;
                    nearest_sats[3] = othersat;
                    // bubble the new entry up the list
                    for (int i = 3; i >= 1; i--) {
                        if (dists[i] < dists[i - 1]) {
                            dists[i] = dists[i - 1];
                            dists[i - 1] = dist;
                            nearest_sats[i] = nearest_sats[i - 1];
                            nearest_sats[i - 1] = othersat;
                        } else {
                            break;
                        }
                    }
                }
            }
            nearest_dist = dists[0];
            last_dist_calc = now;
		} else {
            // only update dists themselves, not list
            
            SatelliteSP0031 nearest_sat = null;
            for (int i = 0; i < 4; i++) {
                SatelliteSP0031 othersat = nearest_sats[i];
                dists[i] = Vector3.Distance(sat.position(), othersat.position());
                if (dists[i] < nearest_dist)
                {
                    nearest_dist = dists[i];
                    nearest_sat = othersat;
                }
			}
		}

        // slow down when we're close to minimum distance to improve accuracy
        bool speed_change = false;
        float prev_speed = speed;
        if (nearest_dist - mindist < 10f && nearest_dist < 100f && speed == orig_speed) {
            speed = 1f;
            simspeed = speed * 360f / 86400f; // Speed 1 is realtime
            rightbottom.text = speed.ToString() + "x realtime";
            speed_change = true;

        } else if (nearest_dist - mindist >= 10f && speed != orig_speed) {
            speed = orig_speed;
            simspeed = speed * 360f / 86400f; // Speed 1 is realtime
            rightbottom.text = speed.ToString() + "x realtime";
            speed_change = true;
        }

        if (speed_change) {
            float time_now = Time.time;
            elapsed_time = last_elapsed_time + (time_now - last_speed_change) * prev_speed;
            last_elapsed_time = elapsed_time;
            last_speed_change = time_now;
        }

        if (nearest_dist < mindist) {
            mindist = nearest_dist;
        }
        /*print(((int)(dists[0])).ToString() + " " +
              ((int)(dists[1])).ToString() + " " +
              ((int)(dists[2])).ToString() + " " +
              ((int)(dists[3])).ToString() + " (" + mindist.ToString() + ")");*/
        txt.text = "Closest pass: " + mindist.ToString("0.00") + " km\n" +  
            "Current nearest: " +
              ((int)(dists[0])).ToString() + " " +
              ((int)(dists[1])).ToString() + " " +
              ((int)(dists[2])).ToString() + " " +
              ((int)(dists[3])).ToString();

        return nearest_dist;
	}

	// Update is called once per frame
	void Update() {
        elapsed_time = last_elapsed_time + (Time.time - last_speed_change) * speed;

		if (Input.GetKeyDown ("space") || Input.GetKeyDown (".")) {
			if (pause == false) {
				pause = true;
				pause_start_time = Time.time;
				countdown.text = "[Paused]";

			} else {
				pause = false;
				start_time += Time.time - pause_start_time;
			}
		}

		if (Input.GetKeyDown ("b") || Input.GetKeyDown (KeyCode.PageUp)) {
			SceneManager.LoadScene (prevscene);
			return;
		}
		if (Input.GetKeyDown ("n") || Input.GetKeyDown (KeyCode.PageDown)) {
			SceneManager.LoadScene (nextscene);
			return;
		}

		int i=0;
		if (direction < 1f) {
			direction += Time.deltaTime / (directionChangeSpeed / 2);
		}

		if (spin) {
			transform.Rotate (-Vector3.up, (simspeed * direction) * Time.deltaTime);
		}
		for (i = 0; i < maxorbits; i++) {
			orbits [i].transform.RotateAround (Vector3.zero, orbitaxes [i], (float)(-1f * earthperiod / orbitalperiod [i] * simspeed * direction) * Time.deltaTime);
		}

        /* 
	 	* 0 - transatlantic
	 	* 1 - london-joburg
	 	* 2 - transpacific
	 	* 3 - us, sparse
	 	* 4 - us, dense
	 	*/
        switch (route_choice)
        {
		case RouteChoice.TransAt:
			MultiRoute (no_of_paths, london, new_york, "London-New York", 76f, 5576f);
			break;
			
		case RouteChoice.LonJob:
			MultiRoute (no_of_paths, london, johannesburg, "London-Johannesburg", 164f, 9079.92f);
			break;

		case RouteChoice.TransPac:
			MultiRoute (no_of_paths, tokyo, chicago, "Chicago-Tokyo", 143f, 10159f);
			break;

		case RouteChoice.USdense:
		case RouteChoice.USsparse:
			MultiRoute (no_of_paths, new_york, redmond, "New York-Seattle", 78f, 3876f);
			break;

		case RouteChoice.TorMia:
			MultiRoute (no_of_paths, toronto, miami, "Toronto-Miami", 41f, 1985f);
			break;

        case RouteChoice.Sydney_SFO:
            MultiRoute(no_of_paths, sydney, san_francisco, "Sydney-San Francisco", 151f, 11940f);
                break;

        case RouteChoice.Sydney_Tokyo:
            MultiRoute(no_of_paths, sydney, tokyo, "Sydney-Tokyo", 0f, 0f);
            break;

        case RouteChoice.Sydney_Lima:
            MultiRoute(no_of_paths, sydney, lima, "Sydney-lima", 0f, 0f);
            break;

        case RouteChoice.Followsat:
			// no actual routing, just show the lasers of one satellite
			float dist = FindNearest(followsat_id, elapsed_time);
            if (log_choice == LogChoice.Distance) {
                logfile.WriteLine(elapsed_time.ToString() + " " + dist.ToString());
            }
			break;
        }
			
		if (!pause) countdown.text = elapsed_time.ToString("0.0");
			//Route (london, auckland, "London-Auckland", 294f, 18357f);
			//Route (london, san_francisco, "London-San Francisco", 146f, 8626f);
			//Route (london, singapore, "London-Singapore", 159f, 10860f);
			//Route (london, athens, "London-Athens", 48f, 2394f);
			//Route (new_york, san_francisco, "New York-San Francisco", 0f, 4133f);
			//Route (new_york, san_francisco, "New York-San Francisco", 0f, 4689f); // road route
		//}

		UpdateLasers ();

		framecount++;
	}
}


