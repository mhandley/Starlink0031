#Starlink Simulator for Unity3d

This is a basic connection-level simulator for the SpaceX Starlink
network.  It simulates routing across a mesh of inter-satellite links
and via ground relays.  It does not simulator packet-level behaviour,
and also ignores the reachability constraints imposed by needing to
avoid transmissions too close to the geostationary arc.  As a result,
RF links are permitted that should not be. Most of the time this is
not an issue because shortest path routing ususally chooses low
elevation satellites, but for east-west paths near to the equator, the
results are unrealistically good.

## Running the Simulator

To run the simulator, use a recent version of Unity 3d.  Clone the github repo, and from UnityHub, add the repo directory as a new project.

Select the project from UnityHub and Unity should start.

In the Project tab near to the bottom of the screen, open the Assets/Orbits folder, and select Scene_SP_basic.

You should be able to run a simulation using the Play icon at the top of the screen.

To select simulation parameters, select the EarthHigh object on the left.