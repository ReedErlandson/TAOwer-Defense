using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(SpawnManager))]
	public class SpawnManagerEditor : TDEditorInspector {

		private static SpawnManager instance;
		
		private static string[] spawnLimitLabel=new string[0];
		private static string[] spawnLimitTooltip=new string[0];
		
		private static string[] spawnModeLabel=new string[0];
		private static string[] spawnModeTooltip=new string[0];
		
		
		void Awake(){
			instance = (SpawnManager)target;
			LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(SpawnManager._SpawnLimit)).Length;
			spawnLimitLabel=new string[enumLength];
			spawnLimitTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnLimitLabel[i]=((SpawnManager._SpawnLimit)i).ToString();
				if((SpawnManager._SpawnLimit)i==SpawnManager._SpawnLimit.Finite) spawnLimitTooltip[i]="Finite number of waves";
				if((SpawnManager._SpawnLimit)i==SpawnManager._SpawnLimit.Infinite) spawnLimitTooltip[i]="Infinite number of waves (for survival or endless mode)";
			}
			
			enumLength = Enum.GetValues(typeof(SpawnManager._SpawnMode)).Length;
			spawnModeLabel=new string[enumLength];
			spawnModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnModeLabel[i]=((SpawnManager._SpawnMode)i).ToString();
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.Continous) 
					spawnModeTooltip[i]="A new wave is spawn upon every wave duration countdown (with option to skip the timer)";
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.WaveCleared) 
					spawnModeTooltip[i]="A new wave is spawned when the current wave is cleared (with option to spawn next wave in advance)";
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.Round) 
					spawnModeTooltip[i]="Each wave is treated like a round. a new wave can only take place when the previous wave is cleared. Each round require initiation from user";
			}
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "SpawnManager");
			
			EditorGUILayout.Space();
			
			
				int spawnMode=(int)instance.spawnMode;
				cont=new GUIContent("Spawn Mode:", "Spawn mode in this level");
				contL=new GUIContent[spawnModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(spawnModeLabel[i], spawnModeTooltip[i]);
				spawnMode = EditorGUILayout.Popup(cont, spawnMode, contL);
				instance.spawnMode=(SpawnManager._SpawnMode)spawnMode;
				
				int spawnLimit=(int)instance.spawnLimit;
				cont=new GUIContent("Spawn Count:", "Spawn count in this level. Infinite (endless mode) must use procedural wave generation");
				contL=new GUIContent[spawnLimitLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(spawnLimitLabel[i], spawnLimitTooltip[i]);
				spawnLimit = EditorGUILayout.Popup(cont, spawnLimit, contL);
				instance.spawnLimit=(SpawnManager._SpawnLimit)spawnLimit;
			
				cont=new GUIContent("Allow Skip:", "Allow player to skip ahead and spawn the next wave");
				instance.allowSkip=EditorGUILayout.Toggle(cont, instance.allowSkip);
			
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Auto Start: ", "Check to have the spawning start on a fixed timer. Rather than waiting for player initiation");
				instance.autoStart=EditorGUILayout.Toggle(cont, instance.autoStart);
				
				cont=new GUIContent(" - Start Delay: ", "The duration to wait in second before the spawning start");
				if(instance.autoStart) instance.autoStartDelay=EditorGUILayout.FloatField(cont, instance.autoStartDelay);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				
				
			
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Auto-Gen Wave:", "Check to have the SpawnManager automatically generate the wave in runtime as opposed to using preset data");
				if(instance.spawnLimit==SpawnManager._SpawnLimit.Finite) instance.procedurallyGenerateWave=EditorGUILayout.Toggle(cont, instance.procedurallyGenerateWave);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				
				cont=new GUIContent("Default Path:", "The primary path to be used. Every creep will follow this path unless an alternate path is specified in a sub-wave");
				instance.defaultPath=(PathTD)EditorGUILayout.ObjectField(cont, instance.defaultPath, typeof(PathTD), true);
			
			
			EditorGUILayout.Space();
			
			
				EditorGUILayout.HelpBox("Editing Spawn Information using Inspector is not recommended.\nPlease use SpawnEditorWindow instead", MessageType.Info);
				if(GUILayout.Button("Open SpawnEditorWindow")) SpawnEditorWindow.Init();
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
	}

}