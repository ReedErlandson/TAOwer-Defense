using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK {

	//a list of position on a single waypoint transform
	//used for integration with Curvy where a lot of pos-point are needed on a single section
	public class WPSubPath : MonoBehaviour {	
		public List<Vector3> posList=new List<Vector3>();
	}

}