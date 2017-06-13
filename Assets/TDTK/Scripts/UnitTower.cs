using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	//~ public enum _TowerType{TurretTower, AOETower, DirectionalAOETower, SupportTower, ResourceTower, Mine, Block}
	public enum _TowerType{Turret, AOE, Support, Resource, Mine, Block}
	public enum _TargetMode{Hybrid, Air, Ground}
	
	
	public class UnitTower : Unit {
		
		[Header("Tower Setting")]
		public _TowerType type=_TowerType.Turret;
		public _TargetMode targetMode=_TargetMode.Hybrid;
		
		[Space(8)]
		public bool disableInBuildManager=false;	//when set to true, tower wont appear in BuildManager buildList
		public bool canBeSold=true;
		
		[Space(8)]
		public bool disableFPS=false;
		public int FPSWeaponID=-1;
		
		
		[Header("Upgrade Setting")]
		public List<UnitTower> nextLevelTowerList=new List<UnitTower>();
		[HideInInspector] public UnitTower prevLevelTower;
		[HideInInspector] public List<int> value=new List<int>();
		
		
		[Header("Visual Effect (Building)")]
		public bool hideWhenBuilding=false;
		
		[Space(8)]
		public GameObject buildingEffect;
		public bool destroyBuildingEffect=true;
		public float destroyBuildingDuration=1.5f;
		
		[Space(8)]
		public GameObject builtEffect;
		public bool destroyBuiltEffect=true;
		public float destroyBuiltDuration=1.5f;
		
		private enum _Construction{None, Constructing, Deconstructing}
		private _Construction construction=_Construction.None;
		public override bool IsInConstruction(){ return construction==_Construction.None ? false : true; }
		
		
		
		public override void Awake() {
			isTower=true;
			gameObject.layer=TDTK.GetLayerTower();
			
			base.Awake();
			
			for(int i=0; i<nextLevelTowerList.Count; i++){
				if(nextLevelTowerList[i]==null){ nextLevelTowerList.RemoveAt(i); i-=1; }
			}
			
			if(stats.Count==0) stats.Add(new UnitStat());
		}
		
		public override void Start() {
			base.Start();
		}
		
		public void InitTower(int ID){
			Init();
			
			instanceID=ID;
			
			value=stats[currentActiveStat].cost;
			
			int rscCount=ResourceManager.GetResourceCount();
			for(int i=0; i<stats.Count; i++){
				UnitStat stat=stats[i];
				stat.slow.effectID=instanceID;
				stat.dot.effectID=instanceID;
				stat.buff.effectID=instanceID;
				if(stat.rscGain.Count!=rscCount){
					while(stat.rscGain.Count<rscCount) stat.rscGain.Add(0);
					while(stat.rscGain.Count>rscCount) stat.rscGain.RemoveAt(stat.rscGain.Count-1);
				}
				
				if(stat.cost.Count!=rscCount){
					while(stat.cost.Count<rscCount) stat.cost.Add(0);
					while(stat.cost.Count>rscCount) stat.cost.RemoveAt(stat.cost.Count-1);
				}
			}
			
			if(type==_TowerType.Turret){
				SetupTargetLayerMask();
				StartCoroutine(ScanForTargetRoutine());
				StartCoroutine(TurretRoutine());
			}
			if(type==_TowerType.AOE){
				SetupTargetLayerMask();
				StartCoroutine(AOETowerRoutine());
			}
			if(type==_TowerType.Support){
				maskTarget=1<<TDTK.GetLayerTower();
				StartCoroutine(SupportRoutine());
			}
			if(type==_TowerType.Resource){
				StartCoroutine(ResourceTowerRoutine());
			}
			if(type==_TowerType.Mine){
				StartCoroutine(MineRoutine());
			}
		}
		
		void SetupTargetLayerMask(){
			if(targetMode==_TargetMode.Hybrid){
				LayerMask mask1=1<<TDTK.GetLayerCreep();
				LayerMask mask2=1<<TDTK.GetLayerCreepF();
				maskTarget=mask1 | mask2;
			}
			else if(targetMode==_TargetMode.Air){
				maskTarget=1<<TDTK.GetLayerCreepF();
			}
			else if(targetMode==_TargetMode.Ground){
				maskTarget=1<<TDTK.GetLayerCreep();
			}
		}
		
		
		[HideInInspector] public PlatformTD occupiedPlatform;
		[HideInInspector] public NodeTD occupiedNode;
		public void SetPlatform(PlatformTD platform, NodeTD node){
			occupiedPlatform=platform;
			occupiedNode=node;
		}
		
		
		public override void IterateTargetPriority(int i=1){
			base.IterateTargetPriority(i);
		}
		
		public override void ChangeScanAngle(int angle){
			base.ChangeScanAngle(angle);
			GameControl.TowerScanAngleChanged(this);
		}
		
		
		
		public void UnBuild(){ StartCoroutine(Building(false, stats[currentActiveStat].unBuildDuration, true));	}
		public void Build(bool isUpgrade=false){ StartCoroutine(Building(isUpgrade, stats[currentActiveStat].buildDuration));	}
		IEnumerator Building(bool isUpgrade, float duration, bool reverse=false){		//reverse flag is set to true when selling (thus unbuilding) the tower
			construction=!reverse ? _Construction.Constructing : _Construction.Deconstructing;
			float builtTime=0;	buildProgress=0;
			
			if(hideWhenBuilding) Utility.DisableAllChildRendererRecursively(thisT);
			
			if(!isUpgrade) TDTK.OnTowerConstructing(this);
			else TDTK.OnTowerUpgrading(this);

			if(buildingEffect!=null){
				if(!destroyBuildingEffect) ObjectPoolManager.Spawn(buildingEffect, thisT.position, thisT.rotation);
				else ObjectPoolManager.Spawn(buildingEffect, thisT.position, thisT.rotation, destroyBuildingDuration);
			}
			
			yield return null;
			if(!reverse) PlayAnimConstruct();
			else PlayAnimDeconstruct();
			//~ if(!reverse && playConstructAnimation!=null) playConstructAnimation();
			//~ else if(reverse && playDeconstructAnimation!=null) playDeconstructAnimation();
			
			while(true){
				yield return null;
				builtTime+=Time.deltaTime;
				
				if(!reverse) buildProgress=builtTime/duration;
				else buildProgress=(duration-builtTime)/duration;
				
				if(builtTime>duration) break;
			}
			
			construction=_Construction.None;	buildProgress=1;
			
			if(!reverse) {
				Utility.EnbleAllChildRendererRecursively(thisT);
				
				if(!isUpgrade) TDTK.OnTowerConstructed(this);
				else TDTK.OnTowerUpgraded(this);
				
				if(builtEffect!=null){
					if(!destroyBuiltEffect) ObjectPoolManager.Spawn(builtEffect, thisT.position, thisT.rotation);
					else ObjectPoolManager.Spawn(builtEffect, thisT.position, thisT.rotation, destroyBuiltDuration);
				}
			}
			
			if(reverse){
				ResourceManager.GainResource(GetValue());
				TDTK.OnTowerSold(this);
				RemoveFromGame();
			}
		}
		
		private float buildProgress=0;
		public float GetBuildProgress(){
			if(construction!=_Construction.None) return buildProgress;
			return 0;
		}
		
		
		public void Sell(){
			if(!canBeSold) return;
			UnBuild();
		}
		
		
		private bool isSampleTower;
		public bool IsSampleTower(){ return isSampleTower; }
		
		private UnitTower srcTower;
		public void SetAsSampleTower(UnitTower tower){
			isSampleTower=true;
			srcTower=tower;
			thisT.position=new Vector3(0, 99999, 0);
			
			int rscCount=ResourceManager.GetResourceCount();
			for(int i=0; i<stats.Count; i++){
				if(stats[i].cost.Count!=rscCount){
					while(stats[i].cost.Count<rscCount) stats[i].cost.Add(0);
					while(stats[i].cost.Count>rscCount) stats[i].cost.RemoveAt(stats[i].cost.Count-1);
				}
			}
		}
		
		
		
		private static bool inDragNDropRoutine=false;
		public IEnumerator DragNDropRoutine(int pointerID=-1){
			GameControl.SelectTower(this);
			yield return null;
			
			Vector3 cursorPos=Vector3.zero;
			TDTK.OnDragNDrop(true);
			inDragNDropRoutine=true;
			
			while(inDragNDropRoutine){
				if(Input.GetKeyDown(KeyCode.Escape)) break;
				
				bool invalidCursor=false;
				
				if(pointerID<0) cursorPos=Input.mousePosition;
				else cursorPos=TDTK.GetTouchPosition(pointerID);
				
				if(cursorPos.magnitude<0) invalidCursor=true;
				
				BuildInfo buildInfo=null;
				
				if(!invalidCursor){
					buildInfo=BuildManager.CheckBuildPoint(cursorPos, prefabID);
					
					if(buildInfo.status==_TileStatus.NoPlatform){
						Ray ray = Camera.main.ScreenPointToRay(cursorPos);
						RaycastHit hit;
						if(Physics.Raycast(ray, out hit, Mathf.Infinity)) thisT.position=hit.point;
						else thisT.position=ray.GetPoint(30);	//this there is no collier, randomly place it 30unit from camera
					}
					else{
						thisT.position=buildInfo.position;
						thisT.rotation=buildInfo.platform.thisT.rotation;
					}
					
					IndicatorControl.SetBuildTileIndicator(buildInfo);
				}
				
				bool cursorOnUI=UI.IsCursorOnUI(pointerID);
				
				if(pointerID<0){
					if(Input.GetMouseButtonDown(0)){
						if(cursorOnUI) break;
						string exception=BuildManager._BuildTower(srcTower, buildInfo);
						if(exception!="") TDTK.OnGameMessage(exception);
						break;
					}
					if(Input.GetMouseButtonDown(1)) break;
				}
				else{
					if(TDTK.IsTouchEnding(pointerID)){
						if(cursorOnUI) break;
						string exception=BuildManager._BuildTower(srcTower, buildInfo);
						if(exception!="") TDTK.OnGameMessage(exception);
						break;
					}
				}
				
				yield return null;
			}
			
			inDragNDropRoutine=false;
			
			TDTK.OnDragNDrop(false);
			IndicatorControl.SetDragNDropPhase(false);
			thisObj.SetActive(false);
		}
		public static void ExitDragNDrop(){ inDragNDropRoutine=false; }
		public static bool InDragNDrop(){ return inDragNDropRoutine; }
		
		
		
		public override void Update() {
			base.Update();
		}
		
		public override void FixedUpdate(){
			base.FixedUpdate();
		}
		
		
		
		IEnumerator AOETowerRoutine(){
			while(true){
				yield return new WaitForSeconds(GetCooldown());
				
				while(stunned || IsInConstruction()) yield return null;
				
				List<Unit> targetList=TDTK.GetUnitInRange(thisT.position, GetRange(), maskTarget);
				for(int i=0; i<targetList.Count; i++){
					AttackInstance attInstance=new AttackInstance(this, targetList[i]);
					attInstance.Process();
					targetList[i].ApplyEffect(attInstance);
				}
				
				SpawnEffectObject();
			}
		}
		
		IEnumerator ResourceTowerRoutine(){
			while(true){
				yield return new WaitForSeconds(GetCooldown());
				
				while(stunned || IsInConstruction()) yield return null;
				//while(stunned || IsInConstruction() || SpawnManager.GetActiveUnitCount()==0) yield return null;
				
				SpawnEffectObject();
				
				ResourceManager.GainResource(GetResourceGain(), PerkManager.GetRscTowerGain());
			}
		}
		
		IEnumerator MineRoutine(){
			LayerMask maskTarget=1<<TDTK.GetLayerCreep();
			while(true){
				if(!destroyed && !IsInConstruction()){
					Collider[] cols=Physics.OverlapSphere(thisT.position, GetRange(), maskTarget);
					if(cols.Length>0){
						List<Unit> targetList=TDTK.GetUnitInRange(thisT.position, GetAOERadius(), maskTarget);
						for(int i=0; i<targetList.Count; i++){
							AttackInstance attInstance=new AttackInstance(this, targetList[i]);
							attInstance.Process();
							targetList[i].ApplyEffect(attInstance);
						}
						
						SpawnEffectObject();
						
						Destroyed();
					}
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		
		
		
		
		
		private int level=1;
		public int GetLevel(){ return level; }
		public void SetLevel(int lvl){ level=lvl; }
		
		
		public int ReadyToBeUpgrade(){
			if(currentActiveStat<stats.Count-1) return 1;
			if(nextLevelTowerList.Count>0){
				if(nextLevelTowerList.Count>=2 && nextLevelTowerList[1]!=null) return 2;
				else if(nextLevelTowerList.Count>=1 && nextLevelTowerList[0]!=null) return 1;
			}
			return 0;
		}
		public string Upgrade(int ID=0){	//ID specify which nextTower to use
			if(nextLevelTowerList.Count==0 && currentActiveStat>=stats.Count-1) return "Tower is at maximum level!";
			
			List<int> cost=GetCost(ID);
			if(ResourceManager.HasSufficientResource(cost)>=0) return "Insufficient Resource";
			ResourceManager.SpendResource(cost);
			
			if(currentActiveStat<stats.Count-1) return UpgradeToNextStat();
			else if(nextLevelTowerList.Count>0) return UpgradeToNextTower(ID);
			return "";
		}
		public string UpgradeToNextStat(){
			level+=1;	currentActiveStat+=1;
			AddValue(stats[currentActiveStat].cost);
			Build(true);
			return "";
		}
		public string UpgradeToNextTower(int ID=0){
			UnitTower nextLevelTower=nextLevelTowerList[Mathf.Clamp(ID, 0, nextLevelTowerList.Count)];
			
			GameObject towerObj=(GameObject)Instantiate(nextLevelTower.gameObject, thisT.position, thisT.rotation);
			UnitTower towerInstance=towerObj.GetComponent<UnitTower>();
			towerInstance.InitTower(instanceID);
			towerInstance.SetPlatform(occupiedPlatform, occupiedNode);
			towerInstance.AddValue(value);
			towerInstance.SetLevel(level+1);
			towerInstance.Build(true);
			GameControl.SelectTower(towerInstance);
			
			Destroy(thisObj);
			return "";
		}
		
		
		//only use cost from sample towers or in game tower instance, not the prefab
		//ID is for upgrade path
		public List<int> GetCost(int ID=0){
			List<int> cost=new List<int>();
			float multiplier=1;
			if(isSampleTower){
				multiplier=GetBuildCostMultiplier();
				cost=new List<int>(stats[currentActiveStat].cost);
			}
			else{
				multiplier=GetUpgradeCostMultiplier();
				if(currentActiveStat<stats.Count-1) cost=new List<int>( stats[currentActiveStat+1].cost );
				else{
					if(ID<nextLevelTowerList.Count && nextLevelTowerList[ID]!=null) cost=new List<int>( nextLevelTowerList[ID].stats[0].cost );
					else Debug.Log("no next level tower?");
				}
			}
			for(int i=0; i<cost.Count; i++) cost[i]=(int)Mathf.Round(cost[i]*multiplier);
			return cost;
		}
		private float GetBuildCostMultiplier(){ return 1-PerkManager.GetTowerBuildCost(prefabID); }
		private float GetUpgradeCostMultiplier(){ return 1-PerkManager.GetTowerUpgradeCost(prefabID); }
		
		
		//apply the refund ratio from gamecontrol
		public List<int> GetValue(){
			List<int> newValue=new List<int>();
			for(int i=0; i<value.Count; i++) newValue.Add((int)(value[i]*GameControl.GetSellTowerRefundRatio()));
			return newValue;
		}
		//called when tower is upgraded to bring the value forward
		public void AddValue(List<int> list){
			for(int i=0; i<value.Count; i++){
				value[i]+=list[i];
			}
		}
		
		
		
		
		public bool DealDamage(){
			if(type==_TowerType.Turret || type==_TowerType.AOE || type==_TowerType.Mine) return true;
			return false;
		}
		
		
		public override void Destroyed(float delay=0){
			if(destroyed) return;
			
			destroyed=true;
			
			TDTK.OnUnitTowerDestroyed(this);
			RemoveFromGame();
		}
		
		
		public void RemoveFromGame(){
			IndicatorControl.TowerRemoved(this);
			
			destroyed=true;
			if(occupiedPlatform!=null) occupiedPlatform.UnbuildTower(occupiedNode);
			
			base.Destroyed(PlayAnimDestroyed());
		}
		
		
		
		
		
		
		public string GetDespStats(){
			//return "";
			
			if(stats[currentActiveStat].useCustomDesp) return stats[currentActiveStat].desp;
			
			string text="";
			
			if(type==_TowerType.Turret || type==_TowerType.AOE || type==_TowerType.Mine){
				float currentDmgMin=GetDamageMin();
				float currentDmgMax=GetDamageMax();
				if(currentDmgMax>0){
					if(currentDmgMin==currentDmgMax) text+="Damage:		 "+currentDmgMax.ToString("f0");
					else text+="Damage:		 "+currentDmgMin.ToString("f0")+"-"+currentDmgMax.ToString("f0");
				}
				
				float currentAOE=GetAOERadius();
				if(currentAOE>0) text+=" (AOE)";
				//if(currentAOE>0) text+="\nAOE Radius: "+currentAOE;
				
				if(type!=_TowerType.Mine){
					float currentCD=GetCooldown();
					if(currentCD>0) text+="\nCooldown:	 "+currentCD.ToString("f1")+"s";
				}
				
				//~ float critChance=GetCritChance();
				//~ if(critChance>0) text+="\nCritical:		 "+(critChance*100).ToString("f0")+"%";
				
				
				
				if(text!="") text+="\n";
				
				if(GetCritChance()>0) text+="\n"+(GetCritChance()*100).ToString("f0")+"% Chance to score critical hit";
				
				Stun stun=GetStun();
				if(stun.IsValid()) text+="\n"+(stun.chance*100).ToString("f0")+"% Chance to stuns target";
					
				Slow slow=GetSlow();
				if(slow.IsValid()) text+="\nSlows target for "+slow.duration.ToString("f1")+" seconds";
				
				Dot dot=GetDot();
				float dotDmg=dot.GetTotalDamage();
				if(dotDmg>0) text+="\nDamage target by "+dotDmg.ToString("f0")+" over "+dot.duration.ToString("f0")+"s";
				
				if(DamageShieldOnly()) text+="\nDamage target's shield only";
				if(GetShieldBreak()>0) text+="\n"+(GetShieldBreak()*100).ToString("f0")+"% Chance to break shield";
				if(GetShieldPierce()>0) text+="\n"+(GetShieldPierce()*100).ToString("f0")+"% Chance to pierce shield";
				
				InstantKill instKill=GetInstantKill();
				if(instKill.IsValid()) text+="\n"+(instKill.chance*100).ToString("f0")+"% Chance to kill target instantly";
			}
			else if(type==_TowerType.Support){
				Buff buff=GetBuff();
				
				if(buff.damageBuff>0) text+="Damage Buff: "+((buff.damageBuff)*100).ToString("f0")+"%";
				if(buff.cooldownBuff>0) text+="\nCooldown Buff: "+((buff.cooldownBuff)*100).ToString("f0")+"%";
				if(buff.rangeBuff>0) text+="\nRange Buff: "+((buff.rangeBuff)*100).ToString("f0")+"%";
				if(buff.criticalBuff>0) text+="\nRange Buff: "+((buff.criticalBuff)*100).ToString("f0")+"%";
				if(buff.hitBuff>0) text+="\nHit Buff: "+((buff.hitBuff)*100).ToString("f0")+"%";
				if(buff.dodgeBuff>0) text+="\nDodge Buff: "+((buff.dodgeBuff)*100).ToString("f0")+"%";
				
				if(text!="") text+="\n";
				
				if(buff.regenHP>0){
					float regenValue=buff.regenHP;
					float regenDuration=1;
					if(buff.regenHP<1){
						regenValue=1;
						regenDuration=1/buff.regenHP;
					}
					text+="\nRegen "+regenValue.ToString("f0")+ "HP every "+regenDuration.ToString("f0")+"s";
				}
			}
			else if(type==_TowerType.Resource){
				text+="Regenerate resource overtime";
			}
			
			return text;
		}
		
	}

}