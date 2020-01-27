using UnityEngine;
using System.Collections;

public class SatelliteSP0031 {
	int maxsats;
	int phase1_satcount;
	int maxlasers;
	public int satid;
	int satnum;
	int orbitnum;
	double sat_phase_stagger; // 5/32 or 17/32 for phase 1
	int sats_per_orbit; // 50 for phase 1
	int orbital_planes; // 32 for phase 1
	float altitude;
	const int LASERS_PER_SAT = 4;
	public GameObject gameobject;
	GameObject[] lasers;

	SatelliteSP0031[] laserdsts;
	double[] lasertimes;
	bool[] laseron;

	SatelliteSP0031[] nearestsats;
	int nearestcount;

	public SatelliteSP0031[] assignedsats;
	public int assignedcount;


	SatelliteSP0031[] preassignedsats; // pre, not prev
	int preassignedcount;

	SatelliteSP0031[] prevassignedsats; // prev, not pre
	int prevassignedcount;

	SatelliteSP0031[] drawsats;
	int drawcount;

	bool _glow = true;
	bool _beam = false;
	int _linkson = 0;
	int beam_angle = 0;
	float beam_radius = 0;
	GameObject orbit;
	GameObject beam1, beam2;
	GameObject beam_prefab1, beam_prefab2;
	GameObject laser_prefab, thin_laser_prefab;
	GameObject[] links;
	GameObject[] graphlinks;
	bool graphon = false;
	int graphcount = 0;
	int maxgraph = -1;
	Transform earth_transform;

	public SatelliteSP0031(int satelliteid, int satellitenum, int orbitnumber, Transform earth_transform_, 
		GameObject orbit_, double orbitalangle, int maxlasercount, int maxsatcount, int phase1_satcount_,
		double sat_phase_stagger_, int sats_per_orbit_, int orbital_planes_, 
		float altitude_, int beam_angle_, float beam_radius_, GameObject sat_prefab, GameObject beam_prefab1_, 
		GameObject beam_prefab2_, GameObject laser_prefab_, GameObject thin_laser_prefab_) {
		orbit = orbit_;
		satid = satelliteid; /* globally unique satellite ID */
		satnum = satellitenum; /* satellite's position in its orbit */
		orbitnum = orbitnumber;
		altitude = altitude_;
		beam_angle = beam_angle_;
		beam_radius = beam_radius_;
		beam_prefab1 = beam_prefab1_;
		beam_prefab2 = beam_prefab2_;
		laser_prefab = laser_prefab_;
        thin_laser_prefab = thin_laser_prefab_;
        earth_transform = earth_transform_;

		maxsats = maxsatcount; /* total number of satellites */
		phase1_satcount = phase1_satcount_; // number of satellites in phase 1 
		                                    // (will equal maxsats if only simulating phase 1)
		maxlasers = maxlasercount;
		sat_phase_stagger = sat_phase_stagger_;
		sats_per_orbit = sats_per_orbit_;
		orbital_planes = orbital_planes_;

		nearestsats = new SatelliteSP0031[maxsats];
		nearestcount = 0;

		assignedsats = new SatelliteSP0031[LASERS_PER_SAT];
		assignedcount = 0;

		prevassignedsats = new SatelliteSP0031[LASERS_PER_SAT];
		prevassignedcount = 0;

		preassignedsats = new SatelliteSP0031[LASERS_PER_SAT];
		preassignedcount = 0;

		Vector3 pos = earth_transform.position;
		pos.x += 1f;
		gameobject = GameObject.Instantiate (sat_prefab, pos, earth_transform.rotation);
		gameobject.transform.RotateAround (Vector3.zero, Vector3.up, (float)orbitalangle);
		gameobject.transform.SetParent (orbit.transform, false);

		links = new GameObject[2];

		lasers = new GameObject[LASERS_PER_SAT];
		laserdsts = new SatelliteSP0031[LASERS_PER_SAT];
		lasertimes = new double[LASERS_PER_SAT];
		laseron = new bool[LASERS_PER_SAT];
		for (int lc = 0; lc < maxlasers; lc++) {
			lasers [lc] = GameObject.Instantiate (laser_prefab, position(), 
				gameobject.transform.rotation);
			lasers [lc].transform.SetParent (gameobject.transform, true);
			lasertimes [lc] = Time.time;
			laseron[lc] = false;
		} 

		for (int linknum = 0; linknum < 2; linknum++) {
			links [linknum] = GameObject.Instantiate (laser_prefab, position (), 
				gameobject.transform.rotation);
			links [linknum].transform.SetParent (gameobject.transform, true);
		}
	}

	// clear out all references so GC can work, delete game objects, prepare for deletion
	public void clearrefs() {
		for (int lc = 0; lc < maxlasers; lc++) {
			MonoBehaviour.Destroy (lasers [lc]);
			lasers [lc] = null;
		}

		for (int linknum = 0; linknum < 2; linknum++) {
			MonoBehaviour.Destroy(links [linknum]);
			links [linknum] = null;
		}

		for (int satnum = 0; satnum < maxsats; satnum++) {
			nearestsats [satnum] = null;
		}

		for (int satnum = 0; satnum < LASERS_PER_SAT; satnum++) {
			assignedsats [satnum] = null;
			preassignedsats [satnum] = null;
			prevassignedsats [satnum] = null;
		}
	}

	public bool glow {
		set {
			_glow = value;
			for (int lc = 0; lc < maxlasers; lc++) {
				lasertimes [lc] = Time.time;
			}
		}
		get {
			return _glow;
		}
	}

	public void BeamOn() {
		if (_beam) {
			return;
		}
		_beam = true;

		Vector3 pos = earth_transform.position;

		beam1 = MonoBehaviour.Instantiate (beam_prefab1, pos, Quaternion.Euler(0f, -90f, 0f));
		beam1.transform.SetParent (gameobject.transform, false);

		// move the ring down to earth's surface.  0.99 is a spherical correction, as we need 
		// to center to be below ground and the edge on the ground
		Vector3 scale = orbit.transform.localScale;
		pos = gameobject.transform.position * 0.99f * 6371/scale.x;
		beam1.transform.position = pos;
		// radius of circle for 25 degree beam/550km alt is 940km - adjust in GUI
		// radius of circle for 40 degree beam/550km alt is 573.5km
		// radius of circle for 40 degree beam/1150km alt is 1060km

		/* this is a very ugly way of changing the radius of the ring, 
		 * but you cannot directly change a particle system from the API */
		ParticleSystem ps = (ParticleSystem)beam1.GetComponent (typeof(ParticleSystem));
		float radius = ps.shape.radius;
		if (radius != beam_radius) {
			float sf = beam_radius/radius;
			ps.transform.localScale = new Vector3 (sf, sf, sf);
		}

		pos = earth_transform.position;
		beam2 = MonoBehaviour.Instantiate (beam_prefab2, pos, Quaternion.Euler(0f, -90f, 0f));
		beam2.transform.SetParent (gameobject.transform, false);
		Light light = (Light)beam2.GetComponent (typeof(Light));
		if (beam_angle == 25) {
			light.spotAngle = 122f;
		} else if (beam_angle == 40) {
			light.spotAngle = 95f;
		}
	}

	public void BeamOff() {
		if (!_beam) {
			return;
		}
		MonoBehaviour.Destroy (beam1);
		MonoBehaviour.Destroy (beam2);
		_beam = false;
	}

	public void LinkOn(GameObject city) {
		LaserScript ls = (LaserScript)links[_linkson].GetComponent (typeof(LaserScript));
		ls.SetPos(gameobject.transform.position, city.transform.position);
		_linkson++;
	}

	public void LinkOff() {
		if (_linkson > 0) {
			for (int linknum = 0; linknum < 2; linknum++) {
				LaserScript ls = (LaserScript)links [linknum].GetComponent (typeof(LaserScript));
				ls.SetPos (position (), position ());
			}
			_linkson = 0;
		}
	}

	public void GraphOn(GameObject city, Material mat) {
		if (graphon == false) {
			graphlinks = new GameObject[1200];
			graphcount = 0;
			graphon = true;
		}
		bool newlink = false;
		if (graphcount > maxgraph) {
			graphlinks [graphcount] = GameObject.Instantiate (thin_laser_prefab, position (), gameobject.transform.rotation);
			graphlinks [graphcount].transform.SetParent (gameobject.transform, true);
			maxgraph = graphcount;
			newlink = true;
		}
		LaserScript ls = (LaserScript)graphlinks [graphcount].GetComponent (typeof(LaserScript));
		if (newlink) {
			ls.line = ls.GetComponent<LineRenderer> ();
		}
		ls.SetPos(gameobject.transform.position, city.transform.position);
		if (mat != null) {
			ls.ChangeMaterial (mat);
		}
		graphcount++;
	}

	public void GraphReset() {
		graphcount = 0;
	}

	public void GraphDone() {
		for (int i = graphcount; i <= maxgraph; i++) {
			MonoBehaviour.Destroy (graphlinks [i]);
			graphlinks [i] = null;
		}
		maxgraph = graphcount - 1;
	}

	public Vector3 position() {
		return gameobject.transform.position;
	}

	public void AddSat(SatelliteSP0031 newsat) {
		nearestsats[nearestcount] = newsat;
		nearestcount++;
	}

	public void ChangeMaterial(Material newMat) {
		Renderer[] renderers = gameobject.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.material = newMat;
		}
	}

	public float Dist(SatelliteSP0031 s) {
		return Vector3.Distance(position(), s.position());
	}


	bool IsAssigned(SatelliteSP0031 s) {
		for (int i = 0; i < assignedcount; i++) {
			if (assignedsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	public void ClearAssignment() {
		for (int i = 0; i < assignedcount; i++) {
			prevassignedsats [i] = assignedsats [i];
		}
		prevassignedcount = assignedcount;
		assignedcount = 0;
	}

	bool WasAssigned(SatelliteSP0031 s) {
		for (int i = 0; i < prevassignedcount; i++) {
			if (prevassignedsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	public void SimpleAssign(SatelliteSP0031 s) {
		assignedsats [assignedcount] = s;
		assignedcount++;
	}

	bool Assign(SatelliteSP0031 s) {
		if (assignedcount == LASERS_PER_SAT || s.assignedcount == LASERS_PER_SAT || IsAssigned(s) ) {
			return false;
		}
		Debug.Assert (s.IsAssigned (this) == false);
		SimpleAssign (s);
		s.SimpleAssign (this);
		return true;
	}

	public void SimplePreAssign(SatelliteSP0031 s) {
		preassignedsats [preassignedcount] = s;
		preassignedcount++;
	}

	bool IsPreAssigned(SatelliteSP0031 s) {
		for (int i = 0; i < preassignedcount; i++) {
			if (preassignedsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	bool PreAssign(SatelliteSP0031 s) {
		if (preassignedcount == LASERS_PER_SAT || s.preassignedcount == LASERS_PER_SAT || IsPreAssigned(s) ) {
			return false;
		}
		Debug.Assert (s.IsPreAssigned (this) == false);
		SimplePreAssign (s);
		s.SimplePreAssign (this);
		return true;
	}

	public void ClearPreAssignedLasers() {
		preassignedcount = 0;
	}
		
	public void PreAssignLasersOrbitalPlane() {
		int count = 0;
		int satbase = (satid / sats_per_orbit) * sats_per_orbit;
		int nextsat = (((satid - satbase) + 1) % sats_per_orbit) + satbase;
		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == nextsat) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}

	// code intended for polar satellites
	public void PreAssignLasers1b() {
		int count = 0;
		int satbase = (satid / 75) * 75;
		int nextsat = (((satid - satbase) + 1) % 75) + satbase;
		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == nextsat) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}

	public void PreAssignLasersBetweenPlanes(int plane_shift, int plane_step) {
		if (satid < phase1_satcount) {
			PreAssignLasersBetweenPlanes1 (plane_shift, plane_step);
		} else {
			PreAssignLasersBetweenPlanes2 (plane_shift);
		}
	}

	public void PreAssignLasersBetweenPlanes1(int plane_shift, int plane_step) {
		int tmpsatid = satid;
		if (satid >= phase1_satcount) {
			return;
		}
		int count = 0;
		int sideways;

		int modsatid = satid % sats_per_orbit; // id of sat in its plane
		int offset = plane_step * sats_per_orbit + plane_shift;  // default offset, ignoring wrapping

		// ensure we connect to the correct plane
		while ((modsatid + offset) / sats_per_orbit < plane_step) {
			offset += sats_per_orbit;
		}
		while ((modsatid + offset) / sats_per_orbit > plane_step) {
			offset -= sats_per_orbit;
		}
		sideways = satid + offset;

		if (sideways >= phase1_satcount) {
			// wrap around end of constellation
			int stagger = ((int)Mathf.Round ((float)(orbital_planes * sat_phase_stagger)));
			offset = plane_step * sats_per_orbit + plane_shift + stagger;
			while ((modsatid + offset) / sats_per_orbit < plane_step) {
				offset += sats_per_orbit;
			}
			while ((modsatid + offset) / sats_per_orbit > plane_step) {
				offset -= sats_per_orbit;
			}
			sideways = (satid + offset) % phase1_satcount;
		}

		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == sideways) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}

	// old code for phase 2
	public void PreAssignLasersBetweenPlanes2(int shift) {
		int tmpsatid = satid;
		int offset = 0;
		if (satid >= phase1_satcount) {
			offset = phase1_satcount;
			tmpsatid -= offset;
		}
		int count = 0;
		int sideways;
		if ((tmpsatid % 50) + 50 + shift < 50) {
			sideways = tmpsatid + 100 + shift; // don't connect to your own orbital place with negative shifts
		} else if ((tmpsatid % 50) + 50 + shift >= 100) {
			sideways = tmpsatid + shift; // don't connect to two orbital planes over
		} else {
			sideways = tmpsatid + 50 + shift;
		}

		if (sideways >= 1600) {
			int stagger = (int)Mathf.Round ((float)(orbital_planes * sat_phase_stagger));
			sideways = ((sideways % 1600) + stagger) % 50; // 8 comes from the phase offset per plane 32/4 = 8
		}
		sideways += offset;
		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == sideways) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}


	public void UsePreAssigned() {
		for (int i = 0; i < preassignedcount; i++) {
			Assign (preassignedsats [i]);
		}
	}

	int FindFreeLaser() {
		for (int lc = 0; lc < maxlasers; lc++) {
			if (!laseron [lc]) {
				return lc;
			}
		}
		return -1;
	}

	public void FinalizeLasers(float speed, Material isl_material) {
		// Turn off lasers that are no longer assigned to the same target
		for (int lc = 0; lc < maxlasers; lc++) {
			if (laseron [lc]) {
				if (!IsAssigned (laserdsts [lc])) {
					// laser will need to be reassigned
					laseron[lc] = false;
				}
			}
		}

		for (int i = 0; i < assignedcount; i++) {
			SatelliteSP0031 sat = assignedsats [i];
			if (!WasAssigned (sat)) {
				/* destination is a new one - find a laser */
				int lasernum = FindFreeLaser ();
				laseron [lasernum] = true;
				lasertimes [lasernum] = Time.time;
				laserdsts [lasernum] = sat;
				LaserScript ls = (LaserScript)lasers [lasernum].GetComponent (typeof(LaserScript));
				ls.SetMaterial (isl_material);
			}
		}

		int oncount = 0;
		for (int lc = 0; lc < maxlasers; lc++) {
			if (laseron [lc]) {
				oncount++;
				LaserScript ls = (LaserScript)lasers [lc].GetComponent (typeof(LaserScript));
				Debug.Assert (this != laserdsts [lc]);
				if (position () == laserdsts [lc].position ()) {
					Debug.Log ("Same pos");
					Debug.Log ("Me: " + satid.ToString() + " Him: " + laserdsts [lc].satid.ToString());
					Debug.Log ("My pos: " + position ().ToString() + " His pos: " + laserdsts [lc].position ().ToString ());
				}
				Debug.Assert (position () != laserdsts [lc].position());

				ls.SetPos (position (), laserdsts [lc].position ());
				//ls.SetMaterial (isl_material);
			} else {
				LaserScript ls = (LaserScript)lasers [lc].GetComponent (typeof(LaserScript));
				ls.SetPos (position(), position());
			}
		}
	}

	public void ColourLink(SatelliteSP0031 nextsat, Material mat) {
		for (int lc = 0; lc < maxlasers; lc++) {
			if (laseron [lc]) {
				if (laserdsts [lc] == nextsat) {
					LaserScript ls = (LaserScript)lasers [lc].GetComponent (typeof(LaserScript));
					ls.SetMaterial (mat);
				}
			}
		}
	}
}


