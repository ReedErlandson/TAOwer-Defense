using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(PlatformTD))]
	[CanEditMultipleObjects]
	public class PlatformTDEditor : TDEditorInspector {

		private static PlatformTD instance;
		
		private static bool showTowerList=true;
		
		
		void Awake(){
			instance = (PlatformTD)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "PlatformTD");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Gizmo(Show Nodes):", "Check to enable gizmo to show path-finding nodes generated on the grid\nThe gizmo are only visible in runtime and are for debug purpose only");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("GizmoShowNodes"), cont);
				
			
			EditorGUILayout.Space();
			
				
				showTowerList=EditorGUILayout.Foldout(showTowerList, "Show Valid Tower List");
				if(showTowerList){
					
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button("EnableAll")){
						EnableAllTowerOnAll();
					}
					if(GUILayout.Button("DisableAll")){
						DisableAllTowerOnAll();
					}
					EditorGUILayout.EndHorizontal ();
					
					
					for(int i=0; i<towerDB.towerList.Count; i++){
						UnitTower tower=towerDB.towerList[i];
						
						//if(tower.disableInBuildManager) continue;
						
						GUILayout.BeginHorizontal();
							
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							Rect rect=GUILayoutUtility.GetLastRect();
							TDEditor.DrawSprite(rect, tower.iconSprite, tower.desp, false);
							
							GUILayout.BeginVertical();
						
								EditorGUILayout.Space();
								GUILayout.Label(tower.name, GUILayout.ExpandWidth(false));
								
								EditorGUI.showMixedValue=!UnavailableListSharesValue(tower.prefabID);
								
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailableTowerIDList.Contains(tower.prefabID) ? true : false;
								flag=EditorGUILayout.Toggle(" - enabled:", flag);
								
								if(EditorGUI.EndChangeCheck()){
									if(flag) RemoveIDFromList(tower.prefabID);
									else AddIDToList(tower.prefabID);
								}
								
								EditorGUI.showMixedValue=false;
								
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
					}
				}
			
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		private bool UnavailableListSharesValue(int ID){
			if(!serializedObject.isEditingMultipleObjects) return true;
			
			PlatformTD platformInstance=(PlatformTD)serializedObject.targetObjects[0];
			bool flag=platformInstance.unavailableTowerIDList.Contains(ID);
			
			for(int i=1; i<serializedObject.targetObjects.Length; i++){
				platformInstance=(PlatformTD)serializedObject.targetObjects[i];
				if(flag!=platformInstance.unavailableTowerIDList.Contains(ID)) return false;
			}
			
			return true;
		}
		
		
		public void RemoveIDFromList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlatformTD platformInstance=(PlatformTD)serializedObject.targetObjects[i];
				platformInstance.unavailableTowerIDList.Remove(ID);
			}
		}
		public void AddIDToList(int ID){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlatformTD platformInstance=(PlatformTD)serializedObject.targetObjects[i];
				if(!platformInstance.unavailableTowerIDList.Contains(ID))
					platformInstance.unavailableTowerIDList.Add(ID);
			}
		}
		
		
		
		public void EnableAllTowerOnAll(){
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlatformTD platformInstance=(PlatformTD)serializedObject.targetObjects[i];
				platformInstance.unavailableTowerIDList=new List<int>();
			}
		}
		public void DisableAllTowerOnAll(){
			List<int> allIDList=new List<int>();
			for(int i=0; i<towerDB.towerList.Count; i++) allIDList.Add(towerDB.towerList[i].prefabID);
			
			for(int i=0; i<serializedObject.targetObjects.Length; i++){
				PlatformTD platformInstance=(PlatformTD)serializedObject.targetObjects[i];
				platformInstance.unavailableTowerIDList=new List<int>( allIDList );
			}
		}
		
	}
	
	
}