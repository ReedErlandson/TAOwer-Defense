using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class AudioManager : MonoBehaviour {
		
		[Tooltip("Check to keep using the same AudioManager gameObject when loading the new scene\nOtherwise the music will get cut off as soon as a new scene loads")]
		public bool dontDestroyOnLoad=true;

		private List<AudioSource> audioSourceList=new List<AudioSource>();
		
		private static float musicVolume=.75f;
		private static float sfxVolume=.75f;
		
		[Header("Music")]
		public List<AudioClip> musicList;
		public bool playMusic=true;
		public bool shuffle=false;
		private int currentTrackID=0;
		private AudioSource musicSource;
		
		
		private static AudioManager instance;
		private GameObject thisObj;
		private Transform thisT;
		
		
		public static void Init(){
			if(instance!=null) return;
			GameObject obj=new GameObject();
			obj.name="AudioManager";
			obj.AddComponent<AudioManager>();
		}
		
		
		void Awake(){
			if(instance!=null){
				Destroy(gameObject);
				return;
			}
			
			instance=this;
			
			thisObj=gameObject;
			thisT=transform;
			
			if(dontDestroyOnLoad) DontDestroyOnLoad(thisObj);
			
			if(playMusic && musicList!=null && musicList.Count>0){
				musicSource=thisObj.AddComponent<AudioSource>();
				musicSource.loop=false;
				//musicSource.playOnAwake=false;
				musicSource.volume=musicVolume;
				
				musicSource.ignoreListenerVolume=true;
				
				if(shuffle) currentTrackID=Random.Range(0, musicList.Count);
				musicSource.clip=musicList[currentTrackID];
				musicSource.Play();
			}
			
			audioSourceList=new List<AudioSource>();
			for(int i=0; i<10; i++){
				GameObject obj=new GameObject();
				obj.name="AudioSource"+(i+1);
				
				AudioSource src=obj.AddComponent<AudioSource>();
				src.playOnAwake=false;
				src.loop=false;
				
				obj.transform.parent=thisT;
				obj.transform.localPosition=Vector3.zero;
				
				audioSourceList.Add(src);
			}
			
			AudioListener.volume=sfxVolume;
		}
		
		
		void Update(){
			if(musicSource!=null && !musicSource.isPlaying){
				if(shuffle) musicSource.clip=musicList[Random.Range(0, musicList.Count)];
				else{
					musicSource.clip=musicList[currentTrackID];
					currentTrackID+=1;
					if(currentTrackID==musicList.Count) currentTrackID=0;
				}
				
				musicSource.Play();
			}
		}
		
		
		void OnEnable(){
			TDTK.onLifeE += OnLostLife;
			TDTK.onGameOverE += OnGameOver;
			
			TDTK.onNewWaveE += OnNewWave;
			TDTK.onWaveClearedE += OnWaveCleared;
			
			TDTK.onCreepDestinationE += OnCreepDestination;
			
			TDTK.onTowerDestroyedE += OnTowerDestroyed;
			TDTK.onTowerConstructingE += OnTowerConstructing;
			TDTK.onTowerConstructedE += OnTowerConstructed;
			TDTK.onTowerUpgradedE += OnTowerUpgraded;
			TDTK.onTowerSoldE += OnTowerSold;
			
			TDTK.onAbilityActivatedE += OnAbilityActivated;
			TDTK.onEnergyFullE += OnEnergyFull;
			
			TDTK.onFPSModeE += OnFPSMode;
			TDTK.onFPSReloadE += OnFPSReload;
			TDTK.onFPSSwitchWeaponE += OnFPSSwitchWeapon;
			
			TDTK.onPerkPurchasedE += OnPerkPurchased;
		}
		
		void OnDisable(){
			TDTK.onLifeE -= OnLostLife;
			TDTK.onGameOverE -= OnGameOver;
			
			TDTK.onNewWaveE -= OnNewWave;
			TDTK.onWaveClearedE -= OnWaveCleared;
			
			TDTK.onCreepDestinationE -= OnCreepDestination;
			
			TDTK.onTowerDestroyedE -= OnTowerDestroyed;
			TDTK.onTowerConstructingE -= OnTowerConstructing;
			TDTK.onTowerConstructedE -= OnTowerConstructed;
			TDTK.onTowerUpgradedE -= OnTowerUpgraded;
			TDTK.onTowerSoldE -= OnTowerSold;
			
			TDTK.onAbilityActivatedE -= OnAbilityActivated;
			TDTK.onEnergyFullE -= OnEnergyFull;
			
			TDTK.onFPSModeE -= OnFPSMode;
			TDTK.onFPSReloadE -= OnFPSReload;
			TDTK.onFPSSwitchWeaponE -= OnFPSSwitchWeapon;
			
			TDTK.onPerkPurchasedE -= OnPerkPurchased;
		}
		
		
		[Header("Sound Effect")]
		public AudioClip gameWonSound;
		public AudioClip gameLostSound;
		void OnGameOver(bool playerWon){ 
			if(playerWon){ if(gameWonSound!=null) _PlaySound(gameWonSound);  }
			else{ if(gameLostSound!=null) _PlaySound(gameLostSound);  }
		}
		
		public AudioClip lostLifeSound;
		void OnLostLife(int lostLife){ if(lostLife<0 && lostLifeSound!=null) _PlaySound(lostLifeSound); }
		
		
		public AudioClip newWaveSound;
		void OnNewWave(int waveID){ if(newWaveSound!=null) _PlaySound(newWaveSound); }
		
		public AudioClip waveClearedSound;
		void OnWaveCleared(int waveID){ if(waveClearedSound!=null) _PlaySound(waveClearedSound); }
		
		
		public AudioClip creepReachDestinationSound;
		void OnCreepDestination(UnitCreep creep){ if(creepReachDestinationSound!=null) _PlaySound(creepReachDestinationSound); }
		
		
		public AudioClip towerDestroyedSound;
		void OnTowerDestroyed(UnitTower tower){ if(towerDestroyedSound!=null) _PlaySound(towerDestroyedSound); }
		public AudioClip towerConstructedSound;
		void OnTowerConstructed(UnitTower tower){ if(towerConstructedSound!=null) _PlaySound(towerConstructedSound); }
		public AudioClip towerConstructingSound;
		void OnTowerConstructing(UnitTower tower){ if(towerConstructingSound!=null) _PlaySound(towerConstructingSound); }
		
		
		public AudioClip towerSoldSound;
		void OnTowerSold(UnitTower tower){ if(towerSoldSound!=null) _PlaySound(towerSoldSound); }
		public AudioClip towerUpgradedSound;
		void OnTowerUpgraded(UnitTower tower){ if(towerUpgradedSound!=null) _PlaySound(towerUpgradedSound); }
		
		
		public AudioClip abilityActivatedSound;
		void OnAbilityActivated(Ability ab){ if(abilityActivatedSound!=null) _PlaySound(abilityActivatedSound); }
		public AudioClip energyFullSound;
		void OnEnergyFull(){ if(energyFullSound!=null) _PlaySound(energyFullSound); }
		
		
		
		
		
		void OnFPSMode(bool flag){ if(fpsModeSound!=null) _PlaySound(fpsModeSound); }
		void OnFPSReload(bool flag){ if(flag && fpsReloadSound!=null) _PlaySound(fpsReloadSound); }
		void OnFPSSwitchWeapon(){ if(fpsSwitchWeaponSound!=null) _PlaySound(fpsSwitchWeaponSound); }
			
		void OnPerkPurchased(Perk perk){ if(perkPurchasedSound!=null) _PlaySound(perkPurchasedSound); }
		
		
		
		
		
		
		
		
		
		
		public AudioClip fpsModeSound;
		public AudioClip fpsReloadSound;
		public AudioClip fpsSwitchWeaponSound;
		
		public AudioClip perkPurchasedSound;
		
		
		
		
		
		
		//check for the next free, unused audioObject
		private int GetUnusedAudioSourceID(){
			for(int i=0; i<audioSourceList.Count; i++){
				if(!audioSourceList[i].isPlaying) return i;
			}
			return 0;	//if everything is used up, use item number zero
		}
		
		
		//call to play a specific clip
		public static void PlaySound(AudioClip clip){ 
			if(instance==null) Init();
			instance._PlaySound(clip);
		}
		public void _PlaySound(AudioClip clip){ 
			int ID=GetUnusedAudioSourceID();
			
			audioSourceList[ID].clip=clip;
			audioSourceList[ID].Play();
		}
		
		
		
		
		
		
		
		
		public static void SetSFXVolume(float val){
			sfxVolume=val;
			AudioListener.volume=val;
		}
		
		public static void SetMusicVolume(float val){
			musicVolume=val;
			if(instance && instance.musicSource) instance.musicSource.volume=val;
		}
		
		public static float GetMusicVolume(){ return musicVolume; }
		public static float GetSFXVolume(){ return sfxVolume; }
	}




}