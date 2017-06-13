using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UnitTowerEditorWindow : TDUnitEditorWindow {
		
		private static UnitTowerEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (UnitTowerEditorWindow)EditorWindow.GetWindow(typeof (UnitTowerEditorWindow), false, "Tower Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			InitLabel();
			
			if(prefabID>=0) window.selectID=TDEditor.GetTowerIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		public void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
			
			SelectItem();
		}

		
		
		private static string[] towerTypeLabel;
		private static string[] towerTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_TowerType)).Length;
			towerTypeLabel=new string[enumLength];
			towerTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				towerTypeLabel[i]=((_TowerType)i).ToString();
				if((_TowerType)i==_TowerType.Turret) 	towerTypeTooltip[i]="Typical tower, fire shootObject to damage creep";
				if((_TowerType)i==_TowerType.AOE) 	towerTypeTooltip[i]="A tower that apply its effect to all creep within it's area of effective";
				if((_TowerType)i==_TowerType.Support) towerTypeTooltip[i]="A tower that buff nearby friendly tower";
				if((_TowerType)i==_TowerType.Resource) towerTypeTooltip[i]="A tower that generate resource ovetime";
				if((_TowerType)i==_TowerType.Mine) 	towerTypeTooltip[i]="Explode and apply aoe effects when a creep wanders into it's range\nDoesn't block up path when built on a walkable platform";
				if((_TowerType)i==_TowerType.Block) 	towerTypeTooltip[i]="A tower with no any particular function other than as a structure to block up path.";
			}
			
			enumLength = Enum.GetValues(typeof(_TargetMode)).Length;
			targetModeLabel=new string[enumLength];
			targetModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetModeLabel[i]=((_TargetMode)i).ToString();
				if((_TargetMode)i==_TargetMode.Hybrid) 	targetModeTooltip[i]="Target both air and ground units";
				if((_TargetMode)i==_TargetMode.Air) 		targetModeTooltip[i]="Target air units only";
				if((_TargetMode)i==_TargetMode.Ground) 	targetModeTooltip[i]="Target ground units only";
			}
		}
		
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<UnitTower> towerList=towerDB.towerList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(towerDB, "towerDB");
			if(towerList.Count>0) Undo.RecordObject(towerList[selectID], "tower");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTD();
			
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Tower:");
			UnitTower newTower=null;
			newTower=(UnitTower)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newTower, typeof(UnitTower), false);
			if(newTower!=null) Select(NewItem(newTower));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawTowerList(startX, startY, towerList);	
			startX=v2.x+25;
			
			if(towerList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawUnitConfigurator(startX, startY, towerList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		
		private bool foldStats=true;
		Vector2 DrawUnitConfigurator(float startX, float startY, UnitTower unit){
			float maxX=startX;
			
			startY=DrawUnitBasicStats(startX, startY, unit);
			
			
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Basic Tower Setting", headerStyle);
			
				startX+=15;
				
				int type=(int)unit.type;
				cont=new GUIContent("Tower Type:", "Type of the tower. Each type of tower serve a different function");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[towerTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(towerTypeLabel[i], towerTypeTooltip[i]);
				type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
				unit.type=(_TowerType)type;
				
				cont=new GUIContent("Disable in BuildManager:", "When checked, tower won't appear on BuildManager list and thus can't be built\nThis is to mark towers that can only be upgrade from a built tower or unlock from perk");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.disableInBuildManager=EditorGUI.Toggle(new Rect(startX+spaceX+25, startY, widthS, height), unit.disableInBuildManager);
				
				cont=new GUIContent("Can be sold:", "Check to disable the tower from being sold when built");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.canBeSold=EditorGUI.Toggle(new Rect(startX+spaceX+25, startY, widthS, height), unit.canBeSold);
				
				startX-=15;	startY+=spaceY;
			
			
			startY=DrawUnitDefensiveStats(startX, startY+spaceY, unit);
			
			startY=DrawUnitOffensiveStats(startX, startY+spaceY, unit);
			
			startY=DrawTowerBuildVisual(startX, startY+spaceY, unit);
			
			startY=DrawTowerUpgradeSetting(startX, startY+spaceY, unit);
			
			
			if(unit.stats.Count==0) unit.stats.Add(new UnitStat());
			
			//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, spaceX, height), "Unit Stats:", headerStyle);
			string text=!foldStats ? "Unit Stats (show)" : "Stats (hide)" ;
			foldStats=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldStats, text, foldoutStyle);
			if(foldStats){
				float cachedX=(startX+=15);
				float cachedY=(startY+=spaceY);
				for(int i=0; i<unit.stats.Count; i++){
					EditorGUI.LabelField(new Rect(startX, cachedY, width, height), "Tier "+(i+1)+" stats", headerStyle);
					
					if(unit.type==_TowerType.Support) 
						startY=DrawUnitStatsSupport(startX, cachedY+spaceY, unit.stats[i], unit);
					else if(unit.type==_TowerType.Resource) 
						startY=DrawUnitStatsResource(startX, cachedY+spaceY, unit.stats[i], unit);
					else startY=DrawUnitStats(startX, cachedY+spaceY, unit.stats[i], unit);
					
					startX+=spaceX+2*widthS+20;
					
					maxX=startX;
				}
				startX=cachedX-15;
			}
			
			
			
			startY+=25;
			
			//cont=new GUIContent("Use Custom Description:", "Enable to add your own runtime description to this unit. Oth");
			//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			//unit.useCustomDesp=EditorGUI.Toggle(new Rect(startX+spaceX+25, startY, 40, height), unit.useCustomDesp);
			
			//if(unit.useCustomDesp){
				GUIStyle style=new GUIStyle("TextArea");
				style.wordWrap=true;
				cont=new GUIContent("Unit description (for runtime and editor): ", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
				unit.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			//}
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		
		private bool foldUpgrade=true;
		protected float DrawTowerUpgradeSetting(float startX, float startY, UnitTower tower){
			//EditorGUI.LabelField(new Rect(startX, startY, width, height), "Upgrade Setting", headerStyle);
			string text="Upgrade Setting "+(!foldUpgrade ? "(show)" : "(hide)");
			foldUpgrade=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldUpgrade, text, foldoutStyle);
			if(foldUpgrade){
				startX+=15;
				
				int ID=0;
				
				cont=new GUIContent("Prev lvl Tower:", "Tower prefab which this current selected tower is upgrade from. If blank then this is the base tower (level 1). ");
				GUI.Label(new Rect(startX, startY+=spaceY, 120, height), cont);
				if(tower.prevLevelTower!=null){
					GUI.Label(new Rect(startX+spaceX, startY, 105, height), tower.prevLevelTower.unitName);
					if(GUI.Button(new Rect(startX+spaceX+width, startY, 48, 15), "Select"))
						SelectItem(TDEditor.GetTowerIndex(tower.prevLevelTower.prefabID));
				}
				else GUI.Label(new Rect(startX+spaceX, startY, 105, height), "-");
				
				startY+=5;
				
				cont=new GUIContent("level within Prefab:", "How many level the prefab can be upgrade before switching to next level tower");
				GUI.Label(new Rect(startX, startY+=spaceY, width, height), cont);
				if(GUI.Button(new Rect(startX+spaceX, startY, widthS, 15), "-1")){
					if(tower.stats.Count>1) tower.stats.RemoveAt(tower.stats.Count-1);
				}
				if(GUI.Button(new Rect(startX+spaceX+widthS+5, startY, widthS, 15), "+1")){
					tower.stats.Add(tower.stats[tower.stats.Count-1].Clone());
				}
				
				startY+=5;
				
				//if(tower.nextLevelTowerList.Count==0) tower.nextLevelTowerList.Add(null);
				while(tower.nextLevelTowerList.Count<2) tower.nextLevelTowerList.Add(null);
				while(tower.nextLevelTowerList.Count>2) tower.nextLevelTowerList.RemoveAt(tower.nextLevelTowerList.Count-1);
				
				cont=new GUIContent("Next level Tower 1:", "Tower prefab to be used beyond the stats level specified for this prefab");
				GUI.Label(new Rect(startX, startY+=spaceY, 120, height), cont);
				ID=tower.nextLevelTowerList[0]!=null ? TDEditor.GetTowerIndex(tower.nextLevelTowerList[0].prefabID) : 0;
				ID=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), ID, towerLabel);
				if(ID>0 && towerDB.towerList[ID-1]!=tower && !tower.nextLevelTowerList.Contains(towerDB.towerList[ID-1])){
					tower.nextLevelTowerList[0]=towerDB.towerList[ID-1];
				}
				else if(ID==0){
					if(tower.nextLevelTowerList[1]!=null){
						tower.nextLevelTowerList[0]=tower.nextLevelTowerList[1];
						tower.nextLevelTowerList[1]=null;
					}
					else tower.nextLevelTowerList[0]=null;
				}
				
				if(tower.nextLevelTowerList[0]!=null){
					if(GUI.Button(new Rect(startX+spaceX+width+5, startY, widthS+10, 15), "Select")){ if(tower.nextLevelTowerList[0]!=null) SelectItem(ID-1); }
				}
				
				
				cont=new GUIContent("Next level Tower 2:", "Tower prefab to be used beyond the stats level specified for this prefab");
				GUI.Label(new Rect(startX, startY+=spaceY, 120, height), cont);
				
				if(tower.nextLevelTowerList[0]!=null){
					ID=tower.nextLevelTowerList[1]!=null ? TDEditor.GetTowerIndex(tower.nextLevelTowerList[1].prefabID) : 0;
					ID=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), ID, towerLabel);
					if(ID>0 && towerDB.towerList[ID-1]!=tower && !tower.nextLevelTowerList.Contains(towerDB.towerList[ID-1])){
						tower.nextLevelTowerList[1]=towerDB.towerList[ID-1];
					}
					else if(ID==0) tower.nextLevelTowerList[1]=null;
					
					if(tower.nextLevelTowerList[1]!=null){
						if(GUI.Button(new Rect(startX+spaceX+width+5, startY, widthS+10, 15), "Select")){ if(tower.nextLevelTowerList[1]!=null) SelectItem(ID-1); }
					}
				}
				else GUI.Label(new Rect(startX+spaceX, startY, width, height), "-");
				
			}
			
			return startY+spaceY;
		}
		
		
		
		
		private bool foldBuildVisual=true;
		protected float DrawTowerBuildVisual(float startX, float startY, UnitTower unit){
			//EditorGUI.LabelField(new Rect(startX, startY, width, height), "Visual Effect", headerStyle);
			string text="Build Visual Effect "+(!foldBuildVisual ? "(show)" : "(hide)");
			foldBuildVisual=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldBuildVisual, text, foldoutStyle);
			if(foldBuildVisual){
				startX+=15;
				
					cont=new GUIContent("HideWhenBuilding:", "Check to disable all renderer component of the prefab instance when it's being built/unbuilt, making it invisible");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.hideWhenBuilding=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.hideWhenBuilding);
				
				startY+=5;
				
					cont=new GUIContent("Building Effect:", "The effect object to be spawned when the tower starts building/upgrading(Optional)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.buildingEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.buildingEffect, typeof(GameObject), false);
					
					cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.buildingEffect!=null) unit.destroyBuildingEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.destroyBuildingEffect);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.buildingEffect!=null && unit.destroyBuildingEffect) 
						unit.destroyBuildingDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.destroyBuildingDuration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				startY+=5;
					
					cont=new GUIContent("built Effect:", "The effect object to be spawned when the tower completes building/upgrading(Optional)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.builtEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.builtEffect, typeof(GameObject), false);
					
					cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.builtEffect!=null) unit.destroyBuiltEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.destroyBuiltEffect);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.builtEffect!=null && unit.destroyBuiltEffect) 
						unit.destroyBuiltDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.destroyBuiltDuration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		protected Vector2 DrawTowerList(float startX, float startY, List<UnitTower> towerList){
			List<Item> list=new List<Item>();
			for(int i=0; i<towerList.Count; i++){
				Item item=new Item(towerList[i].prefabID, towerList[i].unitName, towerList[i].iconSprite);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(UnitTower tower){ return window._NewItem(tower); }
		int _NewItem(UnitTower tower){
			if(towerDB.towerList.Contains(tower)) return selectID;
			
			tower.prefabID=GenerateNewID(towerIDList);
			towerIDList.Add(tower.prefabID);
			
			towerDB.towerList.Add(tower);
			
			UpdateLabel_Tower();
			
			return towerDB.towerList.Count-1;
		}
		void DeleteItem(){
			towerIDList.Remove(towerDB.towerList[deleteID].prefabID);
			towerDB.towerList.RemoveAt(deleteID);
			
			UpdateLabel_Tower();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<towerDB.towerList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			UnitTower tower=towerDB.towerList[selectID];
			towerDB.towerList[selectID]=towerDB.towerList[selectID+dir];
			towerDB.towerList[selectID+dir]=tower;
			selectID+=dir;
		}
		
		void SelectItem(){ SelectItem(selectID); }
		void SelectItem(int newID){ 
			selectID=newID;
			if(towerDB.towerList.Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, towerDB.towerList.Count-1);
			UpdateObjectHierarchyList(towerDB.towerList[selectID].gameObject);
		}
		
		
		
	}

	
}