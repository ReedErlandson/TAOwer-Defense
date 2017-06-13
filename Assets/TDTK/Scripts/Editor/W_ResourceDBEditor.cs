using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class ResourceDBEditor : TDEditorWindow {
		
		static private ResourceDBEditor window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (ResourceDBEditor)EditorWindow.GetWindow(typeof (ResourceDBEditor));
			window.minSize=new Vector2(355, 455);
			
			//EditorDBManager.Init();
			LoadDB();
		}
		
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(rscDB, "rscDB");
			
			
			if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), "Save")) SetDirtyTD();
			
			if(GUI.Button(new Rect(10, 10, 100, 30), "New Resource")){
				//EditorDBManager.AddNewRsc();
				rscDB.rscList.Add(new Rsc());
			}
			
			List<Rsc> rscList=rscDB.rscList;
			
			if(rscList.Count>0){
				GUI.Box(new Rect(5, 50, 50, 20), "ID");
				GUI.Box(new Rect(5+50-1, 50, 70+1, 20), "Texture");
				GUI.Box(new Rect(5+120-1, 50, 150+2, 20), "Name");
				GUI.Box(new Rect(5+270, 50, window.position.width-280, 20), "");
			}
			
			int row=0;
			for(int i=0; i<rscDB.rscList.Count; i++){
				if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
				else GUI.color=Color.white;
				GUI.Box(new Rect(5, 75+i*49, window.position.width-10, 50), "");
				GUI.color=Color.white;
				
				GUI.Label(new Rect(22, 15+75+i*49, 50, 20), rscList[i].ID.ToString());
				
				
				TDEditor.DrawSprite(new Rect(12+50, 3+75+i*49, 44, 44), rscList[i].icon);
				
				
				rscList[i].name=EditorGUI.TextField(new Rect(5+120, 5+75+i*49, 150, 18), rscList[i].name);
				GUI.Label(new Rect(5+120, 25+75+i*49, 120, 18), "Icon: ");
				rscList[i].icon=(Sprite)EditorGUI.ObjectField(new Rect(45+120, 25+75+i*49, 110, 18), rscList[i].icon, typeof(Sprite), false);
				
				if(deleteID!=i){
					if(GUI.Button(new Rect(window.position.width-35, 12+75+i*49, 25, 25), "X")){
						deleteID=i;
					}
				}
				else{
					GUI.color = Color.red;
					if(GUI.Button(new Rect(window.position.width-65, 12+75+i*49, 25, 25), "X")){
						//EditorDBManager.RemoveRsc(i);
						rscList.RemoveAt(deleteID);	i-=1;
						deleteID=-1;
					}
					GUI.color = Color.green;
					if(GUI.Button(new Rect(window.position.width-35, 12+75+i*49, 25, 25), "-")){
						deleteID=-1;
					}
					GUI.color = Color.white;
				}
				
				row+=1;
			}
			
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		
		private int currentSwapID=-1;
		void SwapResource(int ID){
			List<Rsc> rscList=rscDB.rscList;
			
			Rsc rsc=rscList[currentSwapID];
			rscList[currentSwapID]=rscList[ID];
			rscList[ID]=rsc;
			
			currentSwapID=-1;
			
			SetDirtyTD();
		}
	}

}