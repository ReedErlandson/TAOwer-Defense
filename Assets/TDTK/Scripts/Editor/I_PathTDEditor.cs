using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(PathTD))]
	[CanEditMultipleObjects]
	public class PathTDEditor : TDEditorInspector {

		private static PathTD instance;
		
		private bool showPath=true;
		
		void Awake(){
			instance = (PathTD)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "PathTD");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Dynamic Offset:", "A random offset range which somewhat randomize the waypoint for each individual creep\nSet to 0 to disable and any value >0 to enable\nNot recommend for any value larger than BuildManager's grid-size\nNot recommend for path with varying height");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("dynamicOffset"), cont);
				
				cont=new GUIContent("Loop:", "Check to enable path-looping. On path that loops, creep will carry on to the looping point and repeat the path until they are destroyed");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"), cont);
				
				
				cont=new GUIContent("Loop Point:", "The ID of the waypoint which will act as the loop start point. Creep will move to this waypoint to start the path again after reaching destination\nStarts from 0.");
				if(instance.loop || serializedObject.FindProperty("loop").hasMultipleDifferentValues)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("loopPoint"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				
				
				cont=new GUIContent("Generate Path Line:", "Check to generate the a line which make the path visible in game during runtime");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("createPathLine"), cont);
			
			
			EditorGUILayout.Space();
			
				if(serializedObject.isEditingMultipleObjects){
					EditorGUILayout.HelpBox("editing waypoints on multiple instance is not supported", MessageType.Info);
				}
				else{
					showPath=EditorGUILayout.Foldout(showPath, "Show Waypoint List");
					if(showPath){
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Set Childs To Waypoints")){
								SetChildsToPath();
							}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Clear All Waypoints")){
								ClearAllWaypoints();
							}
						GUILayout.EndHorizontal();
						
						EditorGUILayout.Space();
						
						for(int i=0; i<instance.wpList.Count; i++){
							GUILayout.BeginHorizontal();
							
							GUILayout.Label("  Element "+(i+1));
							
							instance.wpList[i]=(Transform)EditorGUILayout.ObjectField(instance.wpList[i], typeof(Transform), true);
							
							if(GUILayout.Button("+", GUILayout.MaxWidth(20))){
								InsertWaypoints(i);
							}
							if(GUILayout.Button("-", GUILayout.MaxWidth(20))){
								i-=RemoveWaypoints(i);
							}
							GUILayout.EndHorizontal();
						}
						
						EditorGUILayout.Space();
						
						GUILayout.BeginHorizontal();
							GUILayout.Label("", GUILayout.MaxWidth(8));
							if(GUILayout.Button("Add Waypoint")){
								AddWaypoint();
							}
							if(GUILayout.Button("Reduce Waypoint")){
								RemoveWaypoint();
							}
						GUILayout.EndHorizontal();
					}
				}
			
				
			EditorGUILayout.Space();
			EditorGUILayout.Space();
				
				
				cont=new GUIContent("Show Gizmo:", "Check to enable gizmo to show the active path");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("showGizmo"), cont);
				
				cont=new GUIContent("Gizmo Color:", "Color of the gizmo\nSet different path's gizmo color to different color to help you differentiate them");
				if(instance.showGizmo || serializedObject.FindProperty("showGizmo").hasMultipleDifferentValues)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoColor"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
			
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		
		
		void SetChildsToPath(){
			instance.wpList=new List<Transform>();
			foreach(Transform child in instance.transform){
				instance.wpList.Add(child);
			}
		}
		void ClearAllWaypoints(){
			instance.wpList=new List<Transform>();
		}
		
		
		
		void InsertWaypoints(int ID){
			if(Application.isPlaying) return;
			instance.wpList.Insert(ID, instance.wpList[ID]);
		}
		int RemoveWaypoints(int ID){
			if(Application.isPlaying) return 0;
			instance.wpList.RemoveAt(ID);
			return 1;
		}
		void AddWaypoint(){
			if(Application.isPlaying) return;
			if(instance.wpList.Count==0) instance.wpList.Add(null);
			else instance.wpList.Add(instance.wpList[instance.wpList.Count-1]);
		}
		void RemoveWaypoint(){
			if(Application.isPlaying) return;
			if(instance.wpList.Count==0) return;
			instance.wpList.RemoveAt(instance.wpList.Count-1);
		}
		
		
	}

}