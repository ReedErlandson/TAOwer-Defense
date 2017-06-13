using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(ResourceManager))]
	public class ResourceManagerEditor : TDEditorInspector{

		
		private static ResourceManager instance;
		
		
		void Awake(){
			instance = (ResourceManager)target;
			LoadDB();
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "GameControl");
			
			EditorGUILayout.Space();
			
				int rscCount=rscDB.rscList.Count;
			
				while(instance.startingValueList.Count<rscCount) instance.startingValueList.Add(0);
				while(instance.startingValueList.Count>rscCount) instance.startingValueList.RemoveAt(instance.startingValueList.Count-1);
				
				while(instance.rscGenRateList.Count<rscCount) instance.rscGenRateList.Add(0);
				while(instance.rscGenRateList.Count>rscCount) instance.rscGenRateList.RemoveAt(instance.rscGenRateList.Count-1);
			
			
				/*
				GUILayout.BeginHorizontal();
					cont=new GUIContent("UseValueFromPreviousLevel:", "check to enable value from previous level, current setting will be overriden"); 
					EditorGUILayout.LabelField(cont, GUILayout.Width(170));
					instance.useValueFromPrevLevel=EditorGUILayout.Toggle(instance.useValueFromPrevLevel);
				GUILayout.EndHorizontal();
				
				if(instance.useValueFromPrevLevel){
					GUILayout.BeginHorizontal();
						cont=new GUIContent("AccumulateValue:", "check to add the value from previous level to the starting value of the current one"); 
						EditorGUILayout.LabelField(cont, GUILayout.Width(170));
						instance.accumulateValueFromPrevLevel=EditorGUILayout.Toggle(instance.accumulateValueFromPrevLevel);
					GUILayout.EndHorizontal();
				}
				*/
			
			
				cont=new GUIContent("CarryFromLastScene:", "Check to carry the resource value from last scene. If this is the first scene, the specified value is used");
				instance.carryFromLastScene=EditorGUILayout.Toggle(cont, instance.carryFromLastScene);
				
				cont=new GUIContent("Generate Overtime:", "Check to have the resource generate overtime. Value specified is value per second");
				instance.enableRscGen=EditorGUILayout.Toggle(cont, instance.enableRscGen);
				
				
			EditorGUILayout.Space();
				
				
				for(int i=0; i<rscCount; i++){
					GUILayout.BeginHorizontal();
						GUILayout.Label(rscDB.rscList[i].icon.texture, GUILayout.Width(40),  GUILayout.Height(40));
					
						GUILayout.BeginVertical();
							
							GUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Start Value: ", GUILayout.Width(70));
							instance.startingValueList[i]=EditorGUILayout.IntField(instance.startingValueList[i]);
							GUILayout.EndHorizontal();
				
							if(instance.enableRscGen){
								GUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("Regen Rate: ", GUILayout.Width(70));
								instance.rscGenRateList[i]=EditorGUILayout.FloatField(instance.rscGenRateList[i]);
								GUILayout.EndHorizontal();
							}
							else EditorGUILayout.LabelField("Regen Rate: -");
							
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
					
					if(i<rscCount-1) EditorGUILayout.Space();
				}
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}