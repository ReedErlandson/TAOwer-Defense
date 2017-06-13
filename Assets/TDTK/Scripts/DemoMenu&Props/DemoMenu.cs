using UnityEngine;
using UnityEngine.UI;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class DemoMenu : MonoBehaviour {
		
		public RectTransform frame;
		
		public List<string> displayedNameList=new List<string>();
		//~ public List<string> levelName=new List<string>();
		public List<string> levelNameList=new List<string>();
		//~ public List<string> levelDesp=new List<string>();
		public List<string> levelDespList=new List<string>();
		public List<UIButton> buttonList=new List<UIButton>();
		
		public Text labelTooltip;
		
		// Use this for initialization
		void Start (){
			
			levelNameList=new List<string>();
			levelDespList=new List<string>();
			
			//string genText="The level uses procedural wave generation so every play-through is different.\n";
			//string genText="All level in this demo uses procedural generation so the incoming waves will be different in each play-through.\n";
			string genText="";
			
			displayedNameList.Add("Demo FixedPath");
			levelNameList.Add("TDTK_Demo_FixedPath");
			levelDespList.Add("A simple level with 2 possible linear paths for incoming creeps.\n"+genText);
			
			displayedNameList.Add("Demo Maze");
			levelNameList.Add("TDTK_Demo_Maze");
			levelDespList.Add("A level that uses series of build platforms as path's waypoint. Player can build tower formation on the platforms to create maze for slowing down incoming creeps.\n"+genText);
			
			displayedNameList.Add("Demo Maze (Loop)");
			levelNameList.Add("TDTK_Demo_Maze_Loop");
			levelDespList.Add("Like previous level 'TDTK_Demo_Maze' except the path loops. Creep will carry on looping along the path until they are destroyed.\n"+genText);
			
			displayedNameList.Add("Demo Maze (Terrain)");
			levelNameList.Add("TDTK_Demo_Maze_Terrain");
			levelDespList.Add("A simple 'mazing' level showing how a natural terrain can be integrated with the toolkit.\n"+genText);
			
			
			for(int i=0; i<levelNameList.Count; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "Button"+(i+1)));
				
				//~ buttonList[i].label.text="Demo "+i;
				buttonList[i].label.text=displayedNameList[i];
				buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnButton, null);
			}
			
			OnExitButton(null);
		}
		
		
		
		public void OnButton(GameObject butObj, int pointerID=-1){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){
					#if UNITY_5_3_OR_NEWER
						SceneManager.LoadScene(levelNameList[i]);
					#else
						Application.LoadLevel(levelNameList[i]);
					#endif
				}
			}
		}
		
		public void OnHoverButton(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj){
					labelTooltip.text=levelDespList[i];
				}
			}
			
			labelTooltip.gameObject.SetActive(true);
		}
		
		public void OnExitButton(GameObject butObj){
			labelTooltip.text="";
			labelTooltip.gameObject.SetActive(false);
		}
		
	}

}