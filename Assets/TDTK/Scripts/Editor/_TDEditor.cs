using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class TDEditor {
		
		public static bool IsPrefabOrPrefabInstance(GameObject obj){
			PrefabType type=PrefabUtility.GetPrefabType(obj);
			return type==PrefabType.Prefab || type==PrefabType.PrefabInstance;
		}
		public static bool IsPrefab(GameObject obj){
			return obj==null ? false : PrefabUtility.GetPrefabType(obj)==PrefabType.Prefab;
		}
		
		
		public static bool dirty=false; 	//a cache which the value is changed whenever something in the editor changed (name, icon, etc)
													//to let other custom editor not in focus to repaint
		
		
		
		
		public static int GetTowerIndex(int prefabID){
			for(int i=0; i<towerDB.towerList.Count; i++){
				if(towerDB.towerList[i].prefabID==prefabID) return (i+1);
			}
			return 0;
		}
		public static int GetCreepIndex(int prefabID){
			for(int i=0; i<creepDB.creepList.Count; i++){
				if(creepDB.creepList[i].prefabID==prefabID) return (i+1);
			}
			return 0;
		}
		public static int GetFPSWeaponIndex(int prefabID){
			for(int i=0; i<fpsWeaponDB.weaponList.Count; i++){
				if(fpsWeaponDB.weaponList[i].prefabID==prefabID) return (i+1);
			}
			return 0;
		}
		public static int GetAbilityIndex(int ID){
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				if(abilityDB.abilityList[i].ID==ID) return (i+1);
			}
			return 0;
		}
		public static int GetPerkIndex(int ID){
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i].ID==ID) return (i+1);
			}
			return 0;
		}
		
		
		public static bool ExistInDB(UnitTower unit){ return towerDB.towerList.Contains(unit); }
		public static bool ExistInDB(UnitCreep unit){ return creepDB.creepList.Contains(unit); }
		public static bool ExistInDB(FPSWeapon weapon){ return fpsWeaponDB.weaponList.Contains(weapon); }
		
		
		private static ResourceDB rscDB;
		public static void LoadRsc(){ 
			rscDB=ResourceDB.LoadDB();
			
			TDEditorWindow.SetResourceDB(rscDB);
			TDEditorInspector.SetResourceDB(rscDB);
		}
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		public static void LoadDamageTable(){ 
			damageTableDB=DamageTableDB.LoadDB();
			UpdateLabel_DamageTable();
			
			TDEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TDEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
		}
		public static void UpdateLabel_DamageTable(){
			damageTypeLabel=new string[damageTableDB.damageTypeList.Count];
			for(int i=0; i<damageTableDB.damageTypeList.Count; i++){ 
				damageTypeLabel[i]=damageTableDB.damageTypeList[i].name;
				if(damageTypeLabel[i]=="") damageTypeLabel[i]="unnamed";
			}
			
			armorTypeLabel=new string[damageTableDB.armorTypeList.Count];
			for(int i=0; i<damageTableDB.armorTypeList.Count; i++){
				armorTypeLabel[i]=damageTableDB.armorTypeList[i].name;
				if(armorTypeLabel[i]=="") armorTypeLabel[i]="unnamed";
			}
			
			TDEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TDEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		protected static TowerDB towerDB;
		protected static List<int> towerIDList=new List<int>();
		protected static string[] towerLabel;
		public static void LoadTower(){
			towerDB=TowerDB.LoadDB();
			
			for(int i=0; i<towerDB.towerList.Count; i++){
				if(towerDB.towerList[i]!=null) towerIDList.Add(towerDB.towerList[i].prefabID);
				else{ towerDB.towerList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Tower();
			
			TDEditorWindow.SetTowerDB(towerDB, towerIDList, towerLabel);
			TDEditorInspector.SetTowerDB(towerDB, towerIDList, towerLabel);
		}
		public static void UpdateLabel_Tower(){
			towerLabel=new string[towerDB.towerList.Count+1];
			towerLabel[0]="Unassigned";
			for(int i=0; i<towerDB.towerList.Count; i++){
				string name=towerDB.towerList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(towerLabel, name)>=0) name+="_";
				towerLabel[i+1]=name;
			}
			
			TDEditorWindow.SetTowerDB(towerDB, towerIDList, towerLabel);
			TDEditorInspector.SetTowerDB(towerDB, towerIDList, towerLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static CreepDB creepDB;
		protected static List<int> creepIDList=new List<int>();
		protected static string[] creepLabel;
		public static void LoadCreep(){
			creepDB=CreepDB.LoadDB();
			
			for(int i=0; i<creepDB.creepList.Count; i++){
				if(creepDB.creepList[i]!=null) creepIDList.Add(creepDB.creepList[i].prefabID);
				else{ creepDB.creepList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Creep();
			
			TDEditorWindow.SetCreepDB(creepDB, creepIDList, creepLabel);
			TDEditorInspector.SetCreepDB(creepDB, creepIDList, creepLabel);
		}
		public static void UpdateLabel_Creep(){
			creepLabel=new string[creepDB.creepList.Count+1];
			creepLabel[0]="Unassigned";
			for(int i=0; i<creepDB.creepList.Count; i++){
				string name=creepDB.creepList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(creepLabel, name)>=0) name+="_";
				creepLabel[i+1]=name;
			}
			
			TDEditorWindow.SetCreepDB(creepDB, creepIDList, creepLabel);
			TDEditorInspector.SetCreepDB(creepDB, creepIDList, creepLabel);
			
			dirty=!dirty;
		}
		
		
		protected static FPSWeaponDB fpsWeaponDB;
		protected static List<int> fpsWeaponIDList=new List<int>();
		protected static string[] fpsWeaponLabel;
		public static void LoadFPSWeapon(){
			fpsWeaponDB=FPSWeaponDB.LoadDB();
			
			for(int i=0; i<fpsWeaponDB.weaponList.Count; i++){
				if(fpsWeaponDB.weaponList[i]!=null) fpsWeaponIDList.Add(fpsWeaponDB.weaponList[i].prefabID);
				else{ fpsWeaponDB.weaponList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_FPSWeapon();
			
			TDEditorWindow.SetFPSWeaponDB(fpsWeaponDB, fpsWeaponIDList, fpsWeaponLabel);
			TDEditorInspector.SetFPSWeaponDB(fpsWeaponDB, fpsWeaponIDList, fpsWeaponLabel);
		}
		public static void UpdateLabel_FPSWeapon(){
			fpsWeaponLabel=new string[fpsWeaponDB.weaponList.Count+1];
			fpsWeaponLabel[0]="Unassigned";
			for(int i=0; i<fpsWeaponDB.weaponList.Count; i++){
				string name=fpsWeaponDB.weaponList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(fpsWeaponLabel, name)>=0) name+="_";
				fpsWeaponLabel[i+1]=name;
			}
			
			TDEditorWindow.SetFPSWeaponDB(fpsWeaponDB, fpsWeaponIDList, fpsWeaponLabel);
			TDEditorInspector.SetFPSWeaponDB(fpsWeaponDB, fpsWeaponIDList, fpsWeaponLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static AbilityDB abilityDB;
		protected static List<int> abilityIDList=new List<int>();
		protected static string[] abilityLabel;
		public static void LoadAbility(){
			abilityDB=AbilityDB.LoadDB();
			
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				if(abilityDB.abilityList[i]!=null) abilityIDList.Add(abilityDB.abilityList[i].ID);
				else{ abilityDB.abilityList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Ability();
			
			TDEditorWindow.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			TDEditorInspector.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
		}
		public static void UpdateLabel_Ability(){
			abilityLabel=new string[abilityDB.abilityList.Count+1];
			abilityLabel[0]="Unassigned";
			for(int i=0; i<abilityDB.abilityList.Count; i++){
				string name=abilityDB.abilityList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(abilityLabel, name)>=0) name+="_";
				abilityLabel[i+1]=name;
			}
			
			TDEditorWindow.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			TDEditorInspector.SetAbilityDB(abilityDB, abilityIDList, abilityLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		public static void LoadPerk(){
			perkDB=PerkDB.LoadDB();
			
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i]!=null) perkIDList.Add(perkDB.perkList[i].ID);
				else{ perkDB.perkList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Perk();
			
			TDEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TDEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
		}
		public static void UpdateLabel_Perk(){
			perkLabel=new string[perkDB.perkList.Count+1];
			perkLabel[0]="Unassigned";
			for(int i=0; i<perkDB.perkList.Count; i++){
				string name=perkDB.perkList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(perkLabel, name)>=0) name+="_";
				perkLabel[i+1]=name;
			}
			
			TDEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TDEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		
		public static bool DrawSprite(Rect rect, Sprite sprite, string tooltip="", bool drawBox=true){
			if(drawBox) GUI.Box(rect, new GUIContent("", tooltip));
			
			if(sprite!=null){
				Texture t = sprite.texture;
				Rect tr = sprite.textureRect;
				Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height );
				
				rect.x+=2;
				rect.y+=2;
				rect.width-=4;
				rect.height-=4;
				GUI.DrawTextureWithTexCoords(rect, t, r);
			}
			
			//if(addXButton){
			//	rect.width=12;	rect.height=12;
			//	bool flag=GUI.Button(rect, "X", GetXButtonStyle());
			//	return flag;
			//}
			
			return false;
		}
		
		//a guiStyle used to draw the button to delete sprite icon on TowerDB, CreepDB and ResourceDB Editor
		//private static GUIStyle xButtonStyle;
		//public static GUIStyle GetXButtonStyle(){
		//	if(xButtonStyle==null){
		//		xButtonStyle=new GUIStyle("Button");
		//		xButtonStyle.alignment=TextAnchor.MiddleCenter;
		//		xButtonStyle.padding=new RectOffset(0, 0, 0, 0);
		//	}
		//	return xButtonStyle;
		//}
		
		
		
		public delegate void SetObjListCallback(List<GameObject> objHList, string[] objHLabelList);
	
		public static void GetObjectHierarchyList(GameObject obj, SetObjListCallback callback){
			List<GameObject> objHList=new List<GameObject>();
			List<string> tempLabelList=new List<string>();
			
			HierarchyList hList=GetTransformInHierarchy(obj.transform, 0);
			
			objHList.Add(null);
			tempLabelList.Add(" - ");
			
			for(int i=0; i<hList.ListT.Count; i++){
				objHList.Add(hList.ListT[i].gameObject);
			}
			for(int i=0; i<hList.ListName.Count; i++){
				while(tempLabelList.Contains(hList.ListName[i])) hList.ListName[i]+=".";
				tempLabelList.Add(hList.ListName[i]);
			}
			
			string[] objHLabelList=new string[tempLabelList.Count];
			for(int i=0; i<tempLabelList.Count; i++) objHLabelList[i]=tempLabelList[i];
			
			callback(objHList, objHLabelList);
		}
		
		
		private static HierarchyList GetTransformInHierarchy(Transform transform, int depth){
			HierarchyList hl=new HierarchyList();
			
			hl=GetTransformInHierarchyRecursively(transform, depth);
			
			hl.ListT.Insert(0, transform);
			hl.ListName.Insert(0, "-"+transform.name);
			
			return hl;
		}
		private static HierarchyList GetTransformInHierarchyRecursively(Transform transform, int depth){
			HierarchyList hList=new HierarchyList();
			depth+=1;
			foreach(Transform t in transform){
				string label="";
				for(int i=0; i<depth; i++) label+="   ";
				
				hList.ListT.Add(t);
				hList.ListName.Add(label+"-"+t.name);
				
				HierarchyList tempHL=GetTransformInHierarchyRecursively(t, depth);
				foreach(Transform tt in tempHL.ListT){
					hList.ListT.Add(tt);
				}
				foreach(string ll in tempHL.ListName){
					hList.ListName.Add(ll);
				}
			}
			return hList;
		}
		
		private class HierarchyList{
			public List<Transform> ListT=new List<Transform>();
			public List<string> ListName=new List<string>();
		}
	}
	
}

