using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class SatelliteVLEO {
	int maxsats;
	int maxlasers;
	public int satid;
	int satnum;
	public int alt_in_km;
	int orbitnum;
	float sat_phase_stagger; // 5/32 or 17/32 for phase 1
	int sats_in_orbit; // 50 for phase 1
	int orbital_planes; // 32 for phase 1
	public GameObject gameobject;
	GameObject[] lasers;

	SatelliteVLEO prevsat;
	SatelliteVLEO nextsat;

	SatelliteVLEO[] laserdsts;
	double[] lasertimes;
	bool[] laseron;

	SatelliteVLEO[] nearestsats;
	int nearestcount;
	int nearcount;
	SatelliteVLEO closest;

	SatelliteVLEO[] provisionalsats;
	int provcount;

	public SatelliteVLEO[] assignedsats;
	public int assignedcount;

	SatelliteVLEO[] preassignedsats; // pre, not prev
	int preassignedcount;

	SatelliteVLEO[] cachedsats;
	int cachedcount;
	int cachenext;
	const int cachesize = 5;

	SatelliteVLEO[] prevassignedsats; // prev, not pre
	int prevassignedcount;

	SatelliteVLEO[] drawsats;
	int drawcount;

	//SP_basic script;
	int bubblecount = 0;

	bool _glow = true;

	public SatelliteVLEO(int satelliteid, int satellitenum, int orbitnumber, Transform earth_transform, 
		GameObject orbit, double orbitalangle, int maxlasercount, int maxsatcount,
		float sat_phase_stagger_, int sats_in_orbit_, int orbital_planes_, 
		float altitude, int alt_in_km_, GameObject sat_prefab, GameObject laser_prefab) {
		satid = satelliteid; /* globally unique satellite ID */
		satnum = satellitenum; /* satellite's position in its orbit */
		orbitnum = orbitnumber;
		//script = masterscript;

		maxsats = maxsatcount;
		maxlasers = maxlasercount;
		sat_phase_stagger = sat_phase_stagger_;
		sats_in_orbit = sats_in_orbit_;
		orbital_planes = orbital_planes_;
		alt_in_km = alt_in_km_;

		nearestsats = new SatelliteVLEO[maxsats];
		nearestcount = 0;
		nearcount = 0;

		provisionalsats = new SatelliteVLEO[10];
		provcount = 0;

		assignedsats = new SatelliteVLEO[5];
		assignedcount = 0;

		prevassignedsats = new SatelliteVLEO[5];
		prevassignedcount = 0;

		preassignedsats = new SatelliteVLEO[5];
		preassignedcount = 0;

		cachedsats = new SatelliteVLEO[cachesize]; // a crude cache to let us find sats by ID quickly
		cachedcount = 0;
		cachenext = 0;

		Vector3 pos = earth_transform.position;
		pos.x += 1f;
		gameobject = GameObject.Instantiate (sat_prefab, pos, earth_transform.rotation);
		gameobject.transform.RotateAround (Vector3.zero, Vector3.up, /*1.8f + orbitnum%2 * 7.2f/2f +*/ (float)orbitalangle);
		gameobject.transform.SetParent (orbit.transform, false);

		lasers = new GameObject[5];
		laserdsts = new SatelliteVLEO[5];
		lasertimes = new double[5];
		laseron = new bool[5];
		for (int lc = 0; lc < maxlasers; lc++) {
			lasers [lc] = GameObject.Instantiate (laser_prefab, position(), 
				gameobject.transform.rotation);
			lasers [lc].transform.SetParent (gameobject.transform, true);
			lasertimes [lc] = Time.time;
			laseron[lc] = false;
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

	// connect the sats in a list along an orbit
	public void SetPrev(SatelliteVLEO prev) {
		prevsat = prev;
		prev.nextsat = this;
	}

	public Vector3 position() {
		return gameobject.transform.position;
	}

	public void AddSat(SatelliteVLEO newsat) {
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

	public float Dist(SatelliteVLEO s) {
		return Vector3.Distance(position(), s.position());
	}

	public void GeneralRegionSort(double horizon) {
		SatelliteVLEO[] nearzone = new SatelliteVLEO[nearestcount];
		SatelliteVLEO[] farzone = new SatelliteVLEO[nearestcount];
		nearcount = 0;
		int farcount = 0;
		for (int i = 0; i < nearestcount; i++) {
			float dist = Dist (nearestsats [i]);
			if (dist < horizon) {
				nearzone [nearcount++] = nearestsats [i];
			} else {
				farzone [farcount++] = nearestsats [i];
			}
		}
		for (int i = 0; i < nearcount; i++) {
			nearestsats [i] = nearzone [i];
		}
		for (int i = 0; i < farcount; i++) {
			nearestsats[i+nearcount] = farzone[i];
		}
	}

	/* Run one pass of Bubblesort, so the nearest sat ends up at the front of the array */
	/* Bubblesort is inefficient in general, but we only care about getting the closest few, 
	not sorting the whole list, so this is actually better than Quicksort for this purpose. */
		public void RegionBubble() {
		/* one pass of all sats */
		QuickBubble (nearcount - 1);
		bubblecount++;
	}

	/* As the list is roughly sorted, we normally only need to re-sort the first few items */
	public void QuickBubble(int sortedrange) {
		float prevdist = Dist (nearestsats [sortedrange]);
		for (int i = sortedrange - 1; i >= 0; i--) {
			float dist = Dist (nearestsats [i]);
			if (prevdist < dist) {
				SatelliteVLEO tmp = nearestsats [i];
				nearestsats [i] = nearestsats [i + 1];
				nearestsats [i + 1] = tmp;
			} else {
				prevdist = dist;
			}
		}
	}

	public bool InNearest(SatelliteVLEO s) {
		if (bubblecount < 3)
			return false;

		for (int i = 0; i < 3; i++) {
			if (nearestsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	bool IsAssigned(SatelliteVLEO s) {
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
		provcount = 0;
	}

	public void ReuseAssignment() {
		for (int i = 0; i < assignedcount; i++) {
			prevassignedsats [i] = assignedsats [i];
		}
		prevassignedcount = assignedcount;
		//assignedcount = 0;
		//provcount = 0;
	}

	bool WasAssigned(SatelliteVLEO s) {
		for (int i = 0; i < prevassignedcount; i++) {
			if (prevassignedsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	public bool ProvAssign(SatelliteVLEO s) {
		if (provcount == 10 || assignedcount == 5 || IsAssigned (s) || orbitnum == s.orbitnum) {
			return false;
		}
		Debug.Assert (!IsAssigned (s));
		provisionalsats [provcount] = s;
		provcount++;
		return true;
	}

	public void SimpleAssign(SatelliteVLEO s) {
		assignedsats [assignedcount] = s;
		assignedcount++;
	}

	bool Assign(SatelliteVLEO s) {
		if (assignedcount == 5 || s.assignedcount == 5 || IsAssigned(s) ) {
			return false;
		}
		Debug.Assert (s.IsAssigned (this) == false);
		SimpleAssign (s);
		s.SimpleAssign (this);
		return true;
	}

	public void SimplePreAssign(SatelliteVLEO s) {
		preassignedsats [preassignedcount] = s;
		preassignedcount++;
	}

	bool IsPreAssigned(SatelliteVLEO s) {
		for (int i = 0; i < preassignedcount; i++) {
			if (preassignedsats [i] == s) {
				return true;
			}
		}
		return false;
	}

	bool PreAssign(SatelliteVLEO s) {
		if (preassignedcount == 5 || s.preassignedcount == 5 || IsPreAssigned(s) ) {
			return false;
		}
		Debug.Assert (s.IsPreAssigned (this) == false);
		SimplePreAssign (s);
		s.SimplePreAssign (this);
		return true;
	}

	void Cache(SatelliteVLEO s) {
		cachedsats [cachenext] = s;
		cachenext = (cachenext + 1) % cachesize;
		if (cachedcount < cachesize) {
			cachedcount++;
		}
	}

	public void ClearPreAssignedLasers() {
		preassignedcount = 0;
	}

	public void PreAssignLasers1() {
		if (satid < 1584) {
			PreAssignLasers1a1 ();
		} else {
			PreAssignLasers1a2 ();
		}
	}

	public void PreAssignLasers1a1() {
		int count = 0;
		int satbase = (satid / 66) * 66;
		int nextsat = (((satid - satbase) + 1) % 66) + satbase;
		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == nextsat) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}

	public void PreAssignLasers1a2() {
		int count = 0;
		int satbase = (satid / 50) * 50;
		int nextsat = (((satid - satbase) + 1) % 50) + satbase;
		for (int i = 0; i < nearestcount; i++) {
			if (nearestsats [i].satid == nextsat) {
				PreAssign (nearestsats [i]);
				if (count == 2)
					return;
				count++;
			}
		}
	}

	public void PreAssignLasers1b() {
		//if (satid >= 1600)
		//	return;
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

	public void PreAssignLasers2(int shift) {
		if (satid < 1584) {
			PreAssignLasers2a (shift);
		} else {
			PreAssignLasers2b (shift);
		}
	}

	public void PreAssignLasers2a(int shift) {
		int tmpsatid = satid;
		int offset = 0;
		if (satid >= 1584) {
			return;
		}
		int count = 0;
		int sideways;
		if ((tmpsatid % 66) + 66 + shift < 66) {
			sideways = tmpsatid + 132 + shift; // don't connect to your own orbital place with negative shifts
		} else if ((tmpsatid % 66) + 66 + shift >= 132) {
			sideways = tmpsatid + shift; // don't connect to two orbital planes over
		} else {
			sideways = tmpsatid + 66 + shift;
		}

		if (sideways >= 1584) {
			int stagger = (int)Mathf.Round (orbital_planes * sat_phase_stagger);
			//Debug.Log ("Stagger: " + stagger.ToString () + " " + sats_in_orbit.ToString() + " " + sat_phase_stagger.ToString());
			sideways = ((sideways % 1584) + stagger) % 66; // 8 comes from the phase offset per plane 32/4 = 8
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

	public void PreAssignLasers2b(int shift) {
		int tmpsatid = satid;
		int offset = 0;
		if (satid >= 1584) {
			offset = 1584;
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
			int stagger = (int)Mathf.Round (orbital_planes * sat_phase_stagger);
			//Debug.Log ("Stagger: " + stagger.ToString () + " " + sats_in_orbit.ToString() + " " + sat_phase_stagger.ToString());
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

	// find the closest sat we've not already got a laser pointed at
	public void FindClosest() {
		int i = 0;
		while (true) {
			SatelliteVLEO s = nearestsats [i];
			if (!IsAssigned (s)) {
				if (satid == 13240) {
					Debug.Log ("Closest: " + s.satid.ToString ());
				}
				closest = s;
				break;
			}
			i++;
		}
	}
	public void AssignClosest() {
		if (closest.closest == this) {
			// we're in agreement
			if (satid == 13240) {
				Debug.Log ("Agreed\n");
			}
			Assign (closest);
		} else {
			if (satid >= 3200 && closest.satid >= 3200) {
				Assign (closest);
			}
			if (satid == 13240) {
				Debug.Log ("Not Agreed\n");
			}
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

	public void FinalizeLasers(float speed, Material lineMaterial, Material newLineMaterial) {
		//if (satid != 158)
		//	return;
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
			SatelliteVLEO sat = assignedsats [i];
			if (!WasAssigned (sat)) {
				/* destination is a new one - find a laser */
				int lasernum = FindFreeLaser ();
				laseron [lasernum] = true;
				lasertimes [lasernum] = Time.time;
				laserdsts [lasernum] = sat;
			}
		}

		int oncount = 0;
		for (int lc = 0; lc < maxlasers; lc++) {
			if (laseron [lc]) {
				oncount++;
				LaserScript ls = (LaserScript)lasers [lc].GetComponent (typeof(LaserScript));
				Vector3 midpoint = (position () + laserdsts [lc].position ()) / 2;
				Debug.Assert (this != laserdsts [lc]);
				if (position () == laserdsts [lc].position ()) {
					Debug.Log ("Same pos");
					Debug.Log ("Me: " + satid.ToString() + " Him: " + laserdsts [lc].satid.ToString());
					Debug.Log ("My pos: " + position ().ToString() + " His pos: " + laserdsts [lc].position ().ToString ());
				}
				Debug.Assert (position () != laserdsts [lc].position());

				ls.SetPos (position (), laserdsts [lc].position ());
				if (Time.time - lasertimes [lc] > 5f/speed) {
					if (glow) {
						ls.SetMaterial (lineMaterial);
					} 
				} else {
					ls.SetMaterial (newLineMaterial);
				}
			} else {
				LaserScript ls = (LaserScript)lasers [lc].GetComponent (typeof(LaserScript));
				ls.SetPos (position(), position());
			}
		}
	}

	public void ColourLink(SatelliteVLEO nextsat, Material mat) {
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


