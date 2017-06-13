using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK{
	
	public class TDEditorInspector : Editor {
		
		protected static bool styleDefined=false;
		protected static GUIStyle headerStyle;
		protected static GUIStyle foldoutStyle;
		protected static GUIStyle conflictStyle;
		protected static GUIStyle toggleHeaderStyle;
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		
		public override void OnInspectorGUI(){
			DefineStyle();
		}
		
		
		protected static bool showDefaultEditor=false;
		protected void DefaultInspector(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showDefaultEditor=EditorGUILayout.Foldout(showDefaultEditor, "Show default editor", foldoutStyle);
			EditorGUILayout.EndHorizontal();
			if(showDefaultEditor) DrawDefaultInspector();
			
			EditorGUILayout.Space();
		}
		
		
		protected static void DefineStyle(){
			if(styleDefined) return;
			styleDefined=true;
			
			headerStyle=new GUIStyle("Label");
			headerStyle.fontStyle=FontStyle.Bold;
			headerStyle.normal.textColor = Color.black;
			
			toggleHeaderStyle=new GUIStyle("Toggle");
			toggleHeaderStyle.fontStyle=FontStyle.Bold;
			//toggleHeaderStyle.normal.textColor = Color.black;
			
			foldoutStyle=new GUIStyle("foldout");
			foldoutStyle.fontStyle=FontStyle.Bold;
			foldoutStyle.normal.textColor = Color.black;
			
			conflictStyle=new GUIStyle("Label");
			conflictStyle.normal.textColor = Color.red;
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
		public static void SetCreepDB(CreepDB db, List<int> IDList, string[] label){
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
		
		
		
		
		
		
		protected SerializedProperty srlPpt;
	
		protected const float labelWidth=125;
		protected const float fieldWidth=50;
		protected const float fieldWidthL=140;
		protected const float fieldWidthS=10;
		
		protected void PropertyFieldL(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthL); }
		protected void PropertyFieldS(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthS); }
		protected void PropertyField(SerializedProperty property, GUIContent gcon, float width=fieldWidth){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.PropertyField(property, contN, GUILayout.MaxWidth(width));
			EditorGUILayout.EndHorizontal();
		}
		protected void InvalidField(GUIContent gcon){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.LabelField("-", GUILayout.MaxWidth(fieldWidthS));
			EditorGUILayout.EndHorizontal();
		}

	}
	
	
	
	
	
	
}