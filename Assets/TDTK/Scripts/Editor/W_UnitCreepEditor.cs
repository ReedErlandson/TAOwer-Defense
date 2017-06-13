using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UnitCreepEditorWindow : TDUnitEditorWindow {
		
		private static UnitCreepEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (UnitCreepEditorWindow)EditorWindow.GetWindow(typeof (UnitCreepEditorWindow), false, "Creep Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			InitLabel();
			
			if(prefabID>=0) window.selectID=TDEditor.GetCreepIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		public void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
			
			SelectItem();
		}

		
		
		private static string[] creepTypeLabel;
		private static string[] creepTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_CreepType)).Length;
			creepTypeLabel=new string[enumLength];
			creepTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				creepTypeLabel[i]=((_CreepType)i).ToString();
				if((_CreepType)i==_CreepType.Default) 		creepTypeTooltip[i]="Typical TD creep, just moving from start to finish";
				if((_CreepType)i==_CreepType.Offense) 	creepTypeTooltip[i]="Offensive creep, creep will attack tower";
				if((_CreepType)i==_CreepType.Support) 	creepTypeTooltip[i]="Support creep, creep will buff other creep";
			}
		}
		
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<UnitCreep> creepList=creepDB.creepList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(towerDB, "creepDB");
			if(creepList.Count>0) Undo.RecordObject(creepList[selectID], "creep");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTD();
			
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Creep:");
			UnitCreep newCreep=null;
			newCreep=(UnitCreep)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newCreep, typeof(UnitCreep), false);
			if(newCreep!=null) Select(NewItem(newCreep));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawCreepList(startX, startY, creepList);	
			startX=v2.x+25;
			
			if(creepList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawUnitConfigurator(startX, startY, creepList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		
		private bool foldStats=true;
		Vector2 DrawUnitConfigurator(float startX, float startY, UnitCreep unit){
			float maxX=startX;
			
			startY=DrawUnitBasicStats(startX, startY, unit);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Basic Creep Setting", headerStyle);
			
				//~ startX+=15;
				
				
			
				//~ startX-=15;	startY+=spaceY;
			
			startY=DrawCreepSetting(startX, startY+spaceY, unit);
			
			startY=DrawUnitDefensiveStats(startX, startY+spaceY, unit);
			
			if(unit.type==_CreepType.Offense)
				startY=DrawUnitOffensiveStats(startX, startY+spaceY, unit, false);
			else{
				GUI.color=new Color(0.5f, 0.5f, 0.5f, .5f);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width*2, height), "- Offensive Setting (Invalid)", headerStyle);
				startY+=spaceY;
				GUI.color=Color.white;
			}
			
			
			if(unit.type!=_CreepType.Default){
				
				if(unit.stats.Count==0) unit.stats.Add(new UnitStat());
				
				//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, spaceX, height), "Unit Stats:", headerStyle);
				string text=!foldStats ? "Unit Stats (show)" : "Stats (hide)" ;
				foldStats=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), foldStats, text, foldoutStyle);
				if(foldStats){
					
					if(unit.type==_CreepType.Offense)
						startY=DrawUnitStats(startX+15, startY+=spaceY, unit.stats[0], unit, false);
					if(unit.type==_CreepType.Support) 
						startY=DrawUnitStatsSupport(startX+15, startY+=spaceY, unit.stats[0], unit, false);
					
				}
			}
			else{
				GUI.color=new Color(0.5f, 0.5f, 0.5f, .5f);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width*2, height), "- Unit Stats (Invalid)", headerStyle);
				GUI.color=Color.white;
			}
			
			startY+=25;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Unit description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			unit.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), unit.desp, style);
			
			
			return new Vector2(maxX, startY+170);
		}
		
		
		
		
		
		
		
		private bool foldBasicSetting=true;
		private bool rscGainFoldout=true;
		protected float DrawCreepSetting(float startX, float startY, UnitCreep unit){
			//EditorGUI.LabelField(new Rect(startX, startY, width, height), "Visual Effect", headerStyle);
			string text="Basic Creep Setting "+(!foldBasicSetting ? "(show)" : "(hide)");
			foldBasicSetting=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldBasicSetting, text, foldoutStyle);
			if(foldBasicSetting){
				startX+=15;
				
					int type=(int)unit.type;
					cont=new GUIContent("Creep Type:", "Type of the creep. Different type of creep has different capabilities");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, width, height), cont);
					contL=new GUIContent[creepTypeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(creepTypeLabel[i], creepTypeTooltip[i]);
					type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
					unit.type=(_CreepType)type;
				
				startY+=5;
				
					cont=new GUIContent("Move Speed:", "Moving speed of the creep");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.moveSpeed=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.moveSpeed);
				
					cont=new GUIContent("Life Cost:", "The amont of life taken from player when this creep reach it's destination");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.lifeCost=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.lifeCost);
				
				startY+=5;
				
					cont=new GUIContent("Flying:", "Check to set the creep as flying unit.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.flying=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.flying);
				
					cont=new GUIContent("Face Destination:", "Check to have the target's transform face the traveling direction.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.rotateTowardsDestination=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.rotateTowardsDestination);
				
					cont=new GUIContent("Match Slope:", "Check to have the target's transform rotate (in x-axis) to match the slope they are moving along.");
					EditorGUI.LabelField(new Rect(startX+spaceX+50, startY, width, height), cont);
					if(!unit.rotateTowardsDestination) EditorGUI.LabelField(new Rect(startX+spaceX*2+10, startY, widthS, height), "-");
					else unit.rotateTowardsDestinationX=EditorGUI.Toggle(new Rect(startX+spaceX*2+10, startY, widthS, height), unit.rotateTowardsDestinationX);
				
				startY+=5;
					
					cont=new GUIContent("Life Gain:", "Life awarded to the player when player successfully destroy this creep");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.lifeValue=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.lifeValue);
					
					cont=new GUIContent("Energy Gain:", "Energy awarded to the player when player successfully destroy this creep");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.valueEnergyGain=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.valueEnergyGain);
				
				startY+=5;
				
					while(unit.valueRscMin.Count<rscDB.rscList.Count) unit.valueRscMin.Add(0);
					while(unit.valueRscMax.Count<rscDB.rscList.Count) unit.valueRscMax.Add(0);
					while(unit.valueRscMin.Count>rscDB.rscList.Count) unit.valueRscMin.RemoveAt(unit.valueRscMin.Count-1);
					while(unit.valueRscMax.Count>rscDB.rscList.Count) unit.valueRscMax.RemoveAt(unit.valueRscMax.Count-1);
				
					cont=new GUIContent("Resource Gain Upon Destroyed:", "The amont of resource taken from player when this creep reach it's destination");
					//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					rscGainFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), rscGainFoldout, cont);
					if(rscGainFoldout){
						for(int i=0; i<rscDB.rscList.Count; i++){
							TDEditor.DrawSprite(new Rect(startX+25, startY+=spaceY-2, 20, 20), rscDB.rscList[i].icon);	startY+=2;
							EditorGUI.LabelField(new Rect(startX, startY, width, height), "    -       min/max");//+rscList[i].name);
							unit.valueRscMin[i]=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.valueRscMin[i]);
							unit.valueRscMax[i]=EditorGUI.IntField(new Rect(startX+spaceX+40, startY, widthS, height), unit.valueRscMax[i]);
						}
						startY+=5;
					}
			
				startY+=5;
				
					cont=new GUIContent("SpawnUponDestroyed:", "Creep prefab to be spawn when an instance of this unit is destroyed. Note that the HP of the spawned unit is inherit from the destroyed unit. Use HP-multiplier to specifiy how much of the HP should be carried forward");
					GUI.Label(new Rect(startX, startY+=spaceY, width, height), cont);
					int ID=unit.spawnUponDestroyed!=null ? TDEditor.GetCreepIndex(unit.spawnUponDestroyed.prefabID) : 0;
					ID = EditorGUI.Popup(new Rect(startX+spaceX+15, startY, width-15, height), ID, creepLabel);
					if(ID>0) unit.spawnUponDestroyed=creepDB.creepList[ID-1];
					else if(ID==0) unit.spawnUponDestroyed=null;
				
					cont=new GUIContent(" - Num to Spawn:", "The amount of creep to spawn when this unit is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.spawnUponDestroyed==null) GUI.Label(new Rect(startX+spaceX+15, startY, widthS, height), "-");
					else unit.spawnUponDestroyedCount=EditorGUI.IntField(new Rect(startX+spaceX+15, startY, widthS, height), unit.spawnUponDestroyedCount);
					
					cont=new GUIContent(" - HP Multiplier:", "The percentage of HP to pass to the next unit. 0.5 being 50% of parent unit's fullHP, 1 being 100% of parent unit's fullHP");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.spawnUponDestroyed==null) GUI.Label(new Rect(startX+spaceX+15, startY, widthS, height), "-");
					else unit.spawnUnitHPMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX+15, startY, widthS, height), unit.spawnUnitHPMultiplier);
			
				startY+=5;
				
					cont=new GUIContent("Destination Effect:", "The effect object to be spawned when the unit reaches its destination\nThis is entirely optional");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.destinationEffObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.destinationEffObj, typeof(GameObject), false);
					
					cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destinationEffObj!=null) unit.autoDestroydestinationEff=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.autoDestroydestinationEff);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destinationEffObj!=null && unit.autoDestroydestinationEff) 
						unit.destinationEffDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.destinationEffDuration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		protected Vector2 DrawCreepList(float startX, float startY, List<UnitCreep> creepList){
			List<Item> list=new List<Item>();
			for(int i=0; i<creepList.Count; i++){
				Item item=new Item(creepList[i].prefabID, creepList[i].unitName, creepList[i].iconSprite);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(UnitCreep creep){ return window._NewItem(creep); }
		int _NewItem(UnitCreep creep){
			if(creepDB.creepList.Contains(creep)) return selectID;
			
			creep.prefabID=GenerateNewID(creepIDList);
			creepIDList.Add(creep.prefabID);
			
			creepDB.creepList.Add(creep);
			
			UpdateLabel_Creep();
			
			return creepDB.creepList.Count-1;
		}
		void DeleteItem(){
			creepIDList.Remove(creepDB.creepList[deleteID].prefabID);
			creepDB.creepList.RemoveAt(deleteID);
			
			UpdateLabel_Creep();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<creepDB.creepList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			UnitCreep creep=creepDB.creepList[selectID];
			creepDB.creepList[selectID]=creepDB.creepList[selectID+dir];
			creepDB.creepList[selectID+dir]=creep;
			selectID+=dir;
		}
		
		void SelectItem(){ SelectItem(selectID); }
		void SelectItem(int newID){ 
			selectID=newID;
			if(creepDB.creepList.Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, creepDB.creepList.Count-1);
			UpdateObjectHierarchyList(creepDB.creepList[selectID].gameObject);
		}
		
		
	}

	
}