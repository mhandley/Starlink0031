using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RouteGraph {
	public Node[] nodes;
	GameObject[] objs;
	public Node startnode = null;
	public Node endnode = null;
	int nodecount = 0;
	int satcount = 0;
	public int rstcount = 0;
	float maxdist = 0f;
	float km_per_unit = 0f;
	BinaryHeap heap;

	public RouteGraph() {
	}

	public void Init(int maxsat, int maxrelay, float maxdist_, float km_per_unit_) {
		nodes = new Node[maxsat + 2 + maxrelay];
		objs = new GameObject[maxsat + 2 + maxrelay];
		nodecount = 0;
		maxdist = maxdist_;
		km_per_unit = km_per_unit_;
	}

	public void NewNode(int satid, GameObject obj) {
		nodes [nodecount] = new Node (satid, obj.transform.position);
		objs [nodecount] = obj;
		nodecount++;
	}

	public Node GetNode(int satid) {
		for (int i = 0; i < nodecount; i++) {
			if (nodes [i].Id == satid) {
				return(nodes [i]);
			}
		}
		return null;
	}

	// only call AddEndNodes after you've added all the satellites
	public void AddEndNodes() {
		satcount = nodecount;
		nodes [nodecount] = new Node (-1, Vector3.zero);
		startnode = nodes [nodecount];
		startnode.Dist = 0f;
		nodecount++;
		nodes [nodecount] = new Node (-2, Vector3.zero);
		endnode = nodes [nodecount];
		nodecount++;
	}

	public void ResetNodes(GameObject startobj, GameObject endobj) {
		objs[satcount] = startobj;
		objs[satcount+1] = endobj;
		for (int i = 0; i < nodecount; i++) {
			nodes[i].Reset(objs[i].transform.position);
		}
		startnode.Dist = 0f;
	}

	public void ResetNodesPos(GameObject startobj, GameObject endobj) {
		objs[satcount] = startobj;
		objs[satcount+1] = endobj;
		for (int i = 0; i < nodecount; i++) {
			nodes[i].ResetPos(objs[i].transform.position);
		}
		for (int i = 0; i < nodecount; i++) {
			nodes [i].UpdateDists (maxdist/km_per_unit);
		}
		startnode.Dist = 0f;
	}

	public void ResetNodeDistances() {
		for (int i = 0; i < nodecount; i++) {
			nodes [i].Dist = Node.INFINITY;
		}
		startnode.Dist = 0f;
	}

	public void AddNeighbour(int nodenum1, int nodenum2) {
		nodes [nodenum1].AddNeighbour (nodes [nodenum2]);
	}

	public void AddNeighbour(int nodenum1, int nodenum2, float dist) {
		nodes [nodenum1].AddNeighbour (nodes [nodenum2], dist);
	}
		

	public void ComputeRoutes() {
		heap = new BinaryHeap (nodecount);
		for (int i = 0; i < nodecount; i++) {
			heap.Add (nodes [i], (double)nodes [i].Dist);
		}

		//startnode.Reset ();
		startnode.Dist = 0f;
		while (heap.Count > 0) {
			Node u = heap.ExtractMin ();
			for (int i = 0; i < u.LinkCount; i++) {
				Link l = u.GetLink (i);
				Node n = u.GetNeighbour (l);
				float dist = n.Dist;
				float newdist = u.Dist + l.Dist;
				if (newdist < dist) {
					n.Dist = newdist;
					n.Parent = u;
					heap.DecreasePriority (n, (double)newdist);
				}
			}
		}
	}
    public Node[] GetReachableNodes() {
        int reachablecount = 0;
        for (int i = 0; i < nodecount; i++) {
            if (nodes[i].Dist < Node.INFINITY) {
                reachablecount++;
            }
        }
        Node[] reachable = new Node[reachablecount];
        int ix = 0;
        for (int i = 0; i < nodecount; i++) {
            if (nodes[i].Dist < Node.INFINITY) {
                reachable[ix] = nodes[i];
                ix++;
            }
        }
        return reachable;
    }
}

class City {
	public GameObject gameobject;
	public float lat;
	public float lng;
	public int relayid;
	public City(float lat_, float lng_, int relayid_, GameObject gameobj_) {
		lat = lat_;
		lng = lng_;
		relayid = relayid_;
		gameobject = gameobj_;
	}
}

class GroundRegion {
	int lat, lng, size;
	public List<City> cities;
	public int citycount = 0;
	public GameObject centre;
	// params are edge of region, lat, lng are centre
	public GroundRegion(int lat_, int lng_, int size_, GameObject regioncentre, Transform earth_transform) {
		size = size_;
		lat = lat_ + size/2;
		lng = lng_ + size/2;
		cities = new List<City>();
		centre = new GameObject("name");
		centre.transform.position = new Vector3 (0f, 0f, -6371f); //6371 = earth radius in km

		float long_offset = 20f;
		centre.transform.RotateAround (Vector3.zero, Vector3.up, lng - long_offset);
		Vector3 lat_axis = Quaternion.Euler(0f, -90f, 0f) * centre.transform.position;
		centre.transform.RotateAround (Vector3.zero, lat_axis, lat);
		centre.transform.SetParent (earth_transform, false);
	}


	public void AddCity(float lat, float lng, int relayid, GameObject city) {
		cities.Add (new City(lat, lng, relayid, city));
		citycount++;
	}
}



class GroundGrid {
	int size;
	GroundRegion[,] regions;
	List<List<City>> in_range;
	int numregions;
	float maxdist, margin, squaremargin, km_per_unit;
	public int citycount = 0;

	public GroundGrid(int size_, float maxdist_, float margin_, float km_per_unit_, GameObject regioncentre, Transform earth_transform) {
		size = size_;
		maxdist = maxdist_;
		margin = margin_;
		km_per_unit = km_per_unit_;
		// 111 km per degree. size is in degrees.  0.7 is half of diagonal of a square (worst case)
		squaremargin = 0.7f * 111f * size;  
		numregions = 360 / size;
		regions = new GroundRegion[numregions/2, numregions]; // only 180 degrees n to s
		for (int lat = 0; lat < numregions/2; lat++) { 
			for (int lng = 0; lng < numregions; lng++) {
				int reallat = lat * size;
				if (lat * size > 90) {
					reallat = lat * size - 180;
				}
				int reallng = lng * size;
				if (lng * size > 180) {
					reallng = lng * size - 360;
				}
				regions[lat,lng] = new GroundRegion(reallat, reallng, size, regioncentre, earth_transform);
			}
		}
		in_range = new List<List<City>> ();
	}

	public void AddCity(float lat, float lng, int relayid, GameObject city) {
		if (lat < 0)
			lat += 180f;
		if (lng < 0)
			lng += 360f;
		int ilat = (int)(lat/size);
		int ilng = (int)(lng/size);

		regions[ilat,ilng].AddCity(lat, lng, relayid, city);
		citycount++;
	}

	public List<List<City>> FindInRange(Vector3 pos) {
		in_range.Clear ();
		float maxrange =  (maxdist + margin + squaremargin) / km_per_unit;
		for (int lat = 0; lat < numregions/2; lat++) {
			for (int lng = 0; lng < numregions; lng++) {
				if (regions[lat,lng].citycount > 0 &&
					Vector3.Distance (pos, regions[lat,lng].centre.transform.position) < maxrange) {
					in_range.Add(regions[lat,lng].cities);
				}
			}
		}
		return in_range;
	}


}
