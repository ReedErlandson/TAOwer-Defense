using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(AbilityManager))]
	public class AbilityManagerEditor : TDEditorInspector {

		private static AbilityManager instance;
		
		private bool showAbilityList=true;
		
		
		void Awake(){
			instance = (AbilityManager)target;
			LoadDB();
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "AbilityManager");
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Energy Capacity:", "The maximum amount of energy available to player. Energy is used to cast ability and is recharged overtime");
				instance.fullEnergy=EditorGUILayout.FloatField(cont, instance.fullEnergy);
				
				cont=new GUIContent("Full Energy Start:", "Check to start the game with full energy");
				instance.startWithFullEnergy=EditorGUILayout.Toggle(cont, instance.startWithFullEnergy);
				
				cont=new GUIContent(" - Starting Energy:", "The amount of energy player possess at the start of the game");
				if(!instance.startWithFullEnergy) instance.energy=EditorGUILayout.FloatField(cont, instance.energy);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
				EditorGUILayout.Space();
			
				cont=new GUIContent("Energy Rate:", "The amount of energy to be recharged at each second");
				instance.energyRate=EditorGUILayout.FloatField(cont, instance.energyRate);
				
				//cont=new GUIContent("ChargeBeforeSpawn:", "Check to start energy charging before spawning");
				//instance.onlyChargeOnSpawn=EditorGUILayout.Toggle(cont, instance.onlyChargeOnSpawn);
			
			
			EditorGUILayout.Space();
				
				
				cont=new GUIContent("Default Indicator:", "The default cursor indicator to used for ability target selecting in case the selected ability doesnt have a designated indicator");
				instance.defaultIndicator=(Transform)EditorGUILayout.ObjectField(cont, instance.defaultIndicator, typeof(Transform), true);
			
				
			EditorGUILayout.Space();
			
			
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showAbilityList=EditorGUILayout.Foldout(showAbilityList, "Show Ability List");
				EditorGUILayout.EndHorizontal();
				
				if(showAbilityList){
					
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button("EnableAll") && !Application.isPlaying){
						instance.unavailableIDList=new List<int>();
					}
					if(GUILayout.Button("DisableAll") && !Application.isPlaying){
						instance.unavailableIDList=new List<int>();
						for(int i=0; i<abilityDB.abilityList.Count; i++){
							if(abilityDB.abilityList[i].disableInAbilityManager) continue;
							instance.unavailableIDList.Add(abilityDB.abilityList[i].ID);
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					for(int i=0; i<abilityDB.abilityList.Count; i++){
						Ability ability=abilityDB.abilityList[i];
						
						if(ability.disableInAbilityManager){
							if(instance.unavailableIDList.Contains(ability.ID)) instance.unavailableIDList.Remove(ability.ID);
							continue;
						}
						
						GUILayout.BeginHorizontal();
							
							GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
							Rect rect=GUILayoutUtility.GetLastRect();
							TDEditor.DrawSprite(rect, ability.icon, ability.desp, false);
							
							GUILayout.BeginVertical();
								EditorGUILayout.Space();
								GUILayout.Label(ability.name, GUILayout.ExpandWidth(false));
								
								GUILayout.BeginHorizontal();
									
									EditorGUI.BeginChangeCheck();
									bool flag=!instance.unavailableIDList.Contains(ability.ID) ? true : false;
									flag=EditorGUILayout.Toggle(new GUIContent(" - enabled: ", "check to enable the ability in this level"), flag);
									
									if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
										if(!flag && !instance.unavailableIDList.Contains(ability.ID))
											instance.unavailableIDList.Add(ability.ID);
										else if(flag) instance.unavailableIDList.Remove(ability.ID);
									}
								
								GUILayout.EndHorizontal();
									
							GUILayout.EndVertical();
						
						GUILayout.EndHorizontal();
					}
				
				}
				
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			
				if(GUILayout.Button("Open AbilityEditor")) AbilityEditorWindow.Init();
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
	}

}