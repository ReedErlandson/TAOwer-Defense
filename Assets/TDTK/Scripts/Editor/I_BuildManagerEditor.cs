using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(BuildManager))]
	public class BuildManagerEditor : TDEditorInspector {
		
		private static BuildManager instance;
		
		private string[] buildModeLabel;
		private string[] buildModeTooltip;

		private static bool showTowerList=true;
		
		
		void Awake(){
			instance = (BuildManager)target;
			LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_BuildMode)).Length;
			buildModeLabel=new string[enumLength];
			buildModeTooltip=new string[enumLength];
			for(int n=0; n<enumLength; n++){
				buildModeLabel[n]=((_BuildMode)n).ToString();
				
				if((_BuildMode)n==_BuildMode.PointNBuild)  buildModeTooltip[n]="";
				if((_BuildMode)n==_BuildMode.DragNDrop) buildModeTooltip[n]="";
			}
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "BuildManager");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			
				srlPpt=serializedObject.FindProperty("buildMode");
				EditorGUI.BeginChangeCheck();
				
				cont=new GUIContent("Build Mode:", "");
				contL=new GUIContent[buildModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(buildModeLabel[i], buildModeTooltip[i]);
				int type = EditorGUILayout.Popup(cont, srlPpt.enumValueIndex, contL);
				
				if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=type;
			
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Grid Size:", "The grid size of the grid on the platform");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("gridSize"), cont);
				
				cont=new GUIContent("AutoAdjustTextureToGrid:", "Check to let the BuildManager reformat the texture tiling of the platform to fix the gridsize");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("autoAdjustTextureToGrid"), cont);
			
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("DisableBuildInPlay:", "When checked, the player cannot build tower when there are active creep in the game");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("disableBuildWhenInPlay"), cont);
			
			EditorGUILayout.Space();
			
			
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showTowerList=EditorGUILayout.Foldout(showTowerList, "Show Tower List");
				EditorGUILayout.EndHorizontal();
				if(showTowerList){
					
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button("EnableAll") && !Application.isPlaying){
						instance.unavailableTowerIDList=new List<int>();
					}
					if(GUILayout.Button("DisableAll") && !Application.isPlaying){
						instance.unavailableTowerIDList=new List<int>();
						for(int i=0; i<towerDB.towerList.Count; i++){
							if(towerDB.towerList[i].disableInBuildManager) continue;
							instance.unavailableTowerIDList.Add(towerDB.towerList[i].prefabID);
						}
					}
					EditorGUILayout.EndHorizontal();
				
					int disableCount=0;
					for(int i=0; i<towerDB.towerList.Count; i++){
						UnitTower tower=towerDB.towerList[i];
						
						if(tower.disableInBuildManager){
							if(instance.unavailableTowerIDList.Contains(tower.prefabID)) instance.unavailableTowerIDList.Remove(tower.prefabID);
							disableCount+=1;
							continue;
						}
						
						GUILayout.BeginHorizontal();
							
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							Rect rect=GUILayoutUtility.GetLastRect();
							TDEditor.DrawSprite(rect, tower.iconSprite, tower.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(tower.unitName, GUILayout.ExpandWidth(false));
								
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailableTowerIDList.Contains(tower.prefabID) ? true : false;
								flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the tower in this level"), flag);
								
								if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
									if(!flag && !instance.unavailableTowerIDList.Contains(tower.prefabID))
										instance.unavailableTowerIDList.Add(tower.prefabID);
									else if(flag) instance.unavailableTowerIDList.Remove(tower.prefabID);
								}
								
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
					}
					
					if(disableCount>0){
						EditorGUILayout.Space();
						EditorGUILayout.LabelField(" - "+disableCount+" Towers are disabled in BuildManager");
					}
					
				}
				
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			
				if(GUILayout.Button("Open TowerEditor")) UnitTowerEditorWindow.Init();
			
			
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

	
}

