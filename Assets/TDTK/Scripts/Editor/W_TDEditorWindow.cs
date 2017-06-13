using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class TDEditorWindow : EditorWindow {
		
		//repaint the editor windows when undo is performed, an item could have been added/removed
		public virtual void Awake(){ 
			Undo.undoRedoPerformed += RepaintWindow;
		}
		void RepaintWindow(){
			Repaint();
			TDEditor.dirty=!TDEditor.dirty;
		}
		
		
		
		//for item list
		protected delegate void EditorCallback();		
		protected EditorCallback selectCallback;
		protected EditorCallback shiftItemUpCallback;
		protected EditorCallback shiftItemDownCallback;
		//protected EditorCallback NewItemCallback;
		protected EditorCallback deleteItemCallback;
		
		protected bool minimiseList=false;
		protected Rect visibleRectList;
		protected Rect contentRectList;
		protected Vector2 scrollPosList;
		
		public int deleteID=-1;
		public int selectID=0;
		protected void Select(int ID){
			if(selectID==ID) return;
			
			selectID=ID;
			GUI.FocusControl ("");
			
			if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
			if(selectID*35>scrollPosList.y+visibleRectList.height-40) scrollPosList.y=selectID*35-visibleRectList.height+40;
			
			if(selectCallback!=null) selectCallback();
		}
		
		protected Vector2 DrawList(float startX, float startY, float winWidth, float winHeight, List<Item> list, bool drawRemove=true, bool shiftItem=true, bool clampSelectID=true){
			float width=minimiseList ? 60 : 260;
			
			if(!minimiseList && shiftItem){
				if(GUI.Button(new Rect(startX+180, startY-20, 40, 18), "up")){
					if(shiftItemUpCallback!=null) shiftItemUpCallback();
					else Debug.Log("call back is null");
					if(selectID*35<scrollPosList.y) scrollPosList.y=selectID*35;
				}
				if(GUI.Button(new Rect(startX+222, startY-20, 40, 18), "down")){
					if(shiftItemDownCallback!=null) shiftItemDownCallback();	
					else Debug.Log("call back is null");
					if(visibleRectList.height-35<selectID*35) scrollPosList.y=(selectID+1)*35-visibleRectList.height+5;
				}
			}
			
			visibleRectList=new Rect(startX, startY, width+15, winHeight-startY-5);
			contentRectList=new Rect(startX, startY, width, list.Count*35+5);
			
			GUI.color=new Color(.8f, .8f, .8f, 1f);
			GUI.Box(visibleRectList, "");
			GUI.color=Color.white;
			
			scrollPosList = GUI.BeginScrollView(visibleRectList, scrollPosList, contentRectList);
			
				startY+=5;	startX+=5;
			
				for(int i=0; i<list.Count; i++){
					
					TDEditor.DrawSprite(new Rect(startX, startY+(i*35), 30, 30), list[i].icon);
					
					if(minimiseList){
						if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
						if(GUI.Button(new Rect(startX+35, startY+(i*35), 30, 30), "")) Select(i);
						GUI.color = Color.white;
						continue;
					}
					
					if(selectID==i) GUI.color = new Color(0, 1f, 1f, 1f);
					if(GUI.Button(new Rect(startX+35, startY+(i*35), 150+(!drawRemove ? 60 : 0), 30), list[i].name)) Select(i);
					GUI.color = Color.white;
					
					if(!drawRemove) continue;
					
					if(deleteID==i){
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "cancel")) deleteID=-1;
						
						GUI.color = Color.red;
						if(GUI.Button(new Rect(startX+190, startY+(i*35)+15, 60, 15), "confirm")){
							if(selectID>=deleteID) Select(Mathf.Max(0, selectID-1));
							if(deleteItemCallback!=null) deleteItemCallback();
							else Debug.Log("callback is null");
							deleteID=-1;
						}
						GUI.color = Color.white;
					}
					else{
						if(GUI.Button(new Rect(startX+190, startY+(i*35), 60, 15), "remove")) deleteID=i;
					}
				}
			
			GUI.EndScrollView();
			
			if(clampSelectID) selectID=Mathf.Clamp(selectID, 0, list.Count-1);
			
			return new Vector2(startX+width+10, startY);
		}
		
		
		
		
		
		
		
		//for info/stats configuration
		protected float contentHeight=0;
		protected float contentWidth=0;
		
		protected Vector2 scrollPos;
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		protected float spaceX=120;
		protected float spaceY=18;
		protected float width=150;
		protected float widthS=40;
		protected float height=16;
		
		protected static GUIStyle headerStyle;
		protected static GUIStyle foldoutStyle;
		protected static GUIStyle conflictStyle;
		
		protected bool shootPointFoldout=false;
		
		
		private bool state_Play=false;
		private bool state_Editor=false;
		public static bool TDS_Changed=false;
		public virtual void Update(){
			if(EditorApplication.isPlaying!=state_Play){
				state_Play=EditorApplication.isPlaying;
				Repaint();
			}
			
			if(TDEditor.dirty!=state_Editor){
				state_Editor=TDEditor.dirty;
				Repaint();
			}
		}
		
		public virtual bool OnGUI(){
			if(Application.isPlaying){
				EditorGUILayout.HelpBox("Cannot edit while game is playing", MessageType.Info);
				return false;
			}
			
			return true;
		}
		
		private static bool loaded=false;
		protected static void LoadDB(){
			if(loaded) return;
			loaded=true;
			
			LoadRsc();
			LoadDamageTable();
			
			LoadTower();
			LoadCreep();
			
			LoadAbility();
			LoadPerk();
			
			LoadFPSWeapon();
			
			
			headerStyle=new GUIStyle("Label");
			headerStyle.fontStyle=FontStyle.Bold;
			
			foldoutStyle=new GUIStyle("foldout");
			foldoutStyle.fontStyle=FontStyle.Bold;
			foldoutStyle.normal.textColor = Color.black;
			
			conflictStyle=new GUIStyle("Label");
			conflictStyle.normal.textColor = Color.red;
		}
		
		
		protected static ResourceDB rscDB;
		protected static void LoadRsc(){ TDEditor.LoadRsc(); }
		public static void SetResourceDB(ResourceDB db){ rscDB=db; }
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		protected static void LoadDamageTable(){ TDEditor.LoadDamageTable(); }
		protected static void UpdateLabel_DamageTable(){ TDEditor.UpdateLabel_DamageTable(); }
		public static void SetDamageDB(DamageTableDB db, string[] dmgLabel, string[] armLabel){
			damageTableDB=db;
			damageTypeLabel=dmgLabel;
			armorTypeLabel=armLabel;
		}
		
		
		protected static TowerDB towerDB;
		protected static List<int> towerIDList=new List<int>();
		protected static string[] towerLabel;
		protected static void LoadTower(){ TDEditor.LoadTower(); }
		protected static void UpdateLabel_Tower(){ TDEditor.UpdateLabel_Tower(); }
		public static void SetTowerDB(TowerDB db, List<int> IDList, string[] label){
			towerDB=db;
			towerIDList=IDList;
			towerLabel=label;
		}
		
		protected static CreepDB creepDB;
		protected static List<int> creepIDList=new List<int>();
		protected static string[] creepLabel;
		protected static void LoadCreep(){ TDEditor.LoadCreep(); }
		protected static void UpdateLabel_Creep(){ TDEditor.UpdateLabel_Creep(); }
		public static void SetCreepDB(CreepDB db, List<int> IDList, string[] label){	//override by SpawnManagerEditor
			creepDB=db;
			creepIDList=IDList;
			creepLabel=label;
		}
		
		protected static FPSWeaponDB fpsWeaponDB;
		protected static List<int> fpsWeaponIDList=new List<int>();
		protected static string[] fpsWeaponLabel;
		protected static void LoadFPSWeapon(){ TDEditor.LoadFPSWeapon(); }
		protected static void UpdateLabel_FPSWeapon(){ TDEditor.UpdateLabel_FPSWeapon(); }
		public static void SetFPSWeaponDB(FPSWeaponDB db, List<int> IDList, string[] label){
			fpsWeaponDB=db;
			fpsWeaponIDList=IDList;
			fpsWeaponLabel=label;
		}
		
		
		protected static AbilityDB abilityDB;
		protected static List<int> abilityIDList=new List<int>();
		protected static string[] abilityLabel;
		protected static void LoadAbility(){ TDEditor.LoadAbility(); }
		protected static void UpdateLabel_Ability(){ TDEditor.UpdateLabel_Ability(); }
		public static void SetAbilityDB(AbilityDB db, List<int> IDList, string[] label){
			abilityDB=db;
			abilityIDList=IDList;
			abilityLabel=label;
		}
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		protected static void LoadPerk(){ TDEditor.LoadPerk(); }
		protected static void UpdateLabel_Perk(){ TDEditor.UpdateLabel_Perk(); }
		public static void SetPerkDB(PerkDB db, List<int> IDList, string[] label){
			perkDB=db;
			perkIDList=IDList;
			perkLabel=label;
		}
		
		
		
		
		protected List<GameObject> objHList=new List<GameObject>();
		protected string[] objHLabelList=new string[0];
		protected void UpdateObjectHierarchyList(GameObject obj){
			TDEditor.GetObjectHierarchyList(obj, this.SetObjListCallback);
		}
		protected void SetObjListCallback(List<GameObject> objList, string[] labelList){
			objHList=objList;
			objHLabelList=labelList;
		}
		protected static int GetObjectIDFromHList(Transform objT, List<GameObject> objHList){
			if(objT==null) return 0;
			for(int i=1; i<objHList.Count; i++){
				if(objT==objHList[i].transform) return i;
			}
			return 0;
		}
		
		
		
		
		
		protected static int GenerateNewID(List<int> list){
			int ID=0;
			while(list.Contains(ID)) ID+=1;
			return ID;
		}
		
		
		protected void SetDirtyTD(){ 
			EditorUtility.SetDirty(rscDB);
			EditorUtility.SetDirty(damageTableDB);
			EditorUtility.SetDirty(towerDB);
			EditorUtility.SetDirty(creepDB);
			EditorUtility.SetDirty(fpsWeaponDB);
			EditorUtility.SetDirty(abilityDB);
			EditorUtility.SetDirty(perkDB);
			
			for(int i=0; i<towerDB.towerList.Count; i++) EditorUtility.SetDirty(towerDB.towerList[i]);
			for(int i=0; i<creepDB.creepList.Count; i++) EditorUtility.SetDirty(creepDB.creepList[i]);
			for(int i=0; i<fpsWeaponDB.weaponList.Count; i++) EditorUtility.SetDirty(fpsWeaponDB.weaponList[i]);
		}
	}
	
}