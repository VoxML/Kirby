# Kirby
Kirby is an integration of the VoxWorld platform into real-world robotics and a collaboration between the [Brandeis Lab for Linguistics and Computation](https://brandeis-llc.github.io) and the [Autonomous Robotics Lab](http://campusrover.org).  Our current project is a TurtleBot 2, Kirby, who responds to natural language and gestural commands to navigate, patrol and explore its environment, and to communicate about what it encounters and does with a human partner mediated through a virtual world.

## Setup Instructions

Install [Unity](https://unity3d.com/get-unity/download) (Current Unity version is **2018.4.16**)

### Clone the repository and set up the submodules

```$ git clone https://github.com/VoxML/Kirby.git```

```$ git submodule update --init --recursive``` (will take a while)

### Install 3rd party VoxSim assets

VoxSim requires the following Unity packages to run (can be found on the Asset Store):

* SimpleFileBrowser (recommended location: `submodules/VoxSim/`)
* Console Enhanced Free (recommended location: `submodules/VoxSim/Plugins`)
* RT-Voice Pro (recommended location: `submodules/VoxSim/Plugins`)
* Final IK (recommended location: `submodules/VoxSim/Plugins`)
