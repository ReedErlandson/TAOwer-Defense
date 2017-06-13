//this is just to adjust the texture scale of the various grid object in the environment to show the correct grid size in runtime
//adjust the texture scale of the material to match the scale of the transform

using UnityEngine;
using System.Collections;

namespace TDTK {

	public class AutoAdjustGrid : MonoBehaviour {
		
		public float gridSize=2f;
		
		// Use this for initialization
		void Start () {
			Renderer rend=transform.GetComponent<Renderer>();
			if(rend==null) return;
			
			Material mat=rend.material;
			
			Vector3 worldScale=Utility.GetWorldScale(transform);
			mat.mainTextureScale=new Vector2(worldScale.x/gridSize, worldScale.z/gridSize);
		}
		
	}

}