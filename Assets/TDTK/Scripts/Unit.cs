using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class Unit : MonoBehaviour {
		
		
		[Header("Base Info")]
		public int prefabID=-1;
		public int instanceID=-1;
		
		public string unitName="unit";
		public Sprite iconSprite;
		//public Texture icon;
		
		public bool useCustomDesp=false;
		[MultilineAttribute] public string desp="";
		
		protected bool isCreep=false;
		protected bool isTower=false;
		public bool IsCreep(){ return isCreep; }
		public bool IsTower(){ return isTower; }
		
		
		[Header("Basic Stats")]
		public float defaultHP=10;
		//public float fullHP=10;
		public float HP=10;
		public float HPRegenRate=0;
		public float HPStaggerDuration=10;
		private float currentHPStagger=0;
		
		[Space(10)]
		public float defaultShield=0;
		//public float fullShield=0;
		public float shield=0;
		public float shieldRegenRate=1;
		public float shieldStaggerDuration=1;
		private float currentShieldStagger=0;
		
		[Space(10)]
		public int damageType=0;
		public int armorType=0;
		
		public float dodgeChance=0.0f;
		
		[Space(10)]
		public bool immuneToCrit=false;
		public bool immuneToSlow=false;
		public bool immuneToStun=false;
		
		[Space(10)]
		public Transform targetPoint;
		public float hitThreshold=0.25f;		//hit distance from the targetPoint for the shootObj
		
		
		
		[Header("Attack And Aim Setting")]
		public List<UnitStat> stats=new List<UnitStat>();
		public UnitStat GetBaseStats(){ return stats[0]; }
		protected int currentActiveStat=0;
		
		//get from stats and store locally, just in case the upgraded stats doesnt have any shootObject assigned
		//[HideInInspector] public Transform localShootObjecT;
		[HideInInspector] public ShootObject localShootObject;		
		
		public enum _TargetPriority{Nearest, Weakest, Toughest, First, Random};
		public _TargetPriority targetPriority=_TargetPriority.Random;
		
		[Space(10)]
		public Transform turretObject;
		public Transform barrelObject;
		public List<Transform> shootPoints=new List<Transform>();
		
		public bool rotateTurretAimInXAxis=true;
		public float delayBetweenShootPoint=0;
		
		private float turretRotateSpeed=25;
		private float aimTolerance=5;
		private bool targetInLOS=false;
		
		[Space(10)]
		public bool directionalTargeting=false;
		public float dirScanAngle=0;
		public float dirScanFOV=60;
		protected Vector3 dirScanV;
		protected Quaternion dirScanRot;
		
		protected LayerMask maskTarget=0;
		public LayerMask GetTargetMask(){ return maskTarget; }
		
		public Transform scanDirT;	//what's this
		
		
		
		[Header("Unit Status")]
		protected Unit target;
		public Unit GetTarget(){ return target; }
		
		protected bool destroyed=false;
		public bool IsDestroyed(){ return destroyed; }
		
		protected bool stunned=false;
		private float stunDuration=0;
		
		protected float slowMultiplier=1;
		protected List<Slow> slowEffectList=new List<Slow>();
		
		protected List<Buff> buffEffect=new List<Buff>();
		
		
		
		[Header("Visual Effects")]
		public GameObject destroyedEffObj;
		public bool autoDestroyEff=true;
		public float destroyEffDuration=1;
		
		
		[HideInInspector] public GameObject thisObj;
		[HideInInspector] public Transform thisT;
		
		public virtual void Awake(){
			thisObj=gameObject;
			thisT=transform;
			
			if(shootPoints.Count==0) shootPoints.Add(thisT);
			
			ResetBuff();
			
			for(int i=0; i<stats.Count; i++){
				if(stats[i].shootObject!=null){
					if(localShootObject==null) localShootObject=stats[i].shootObject;
					//if(localShootObjectT==null) localShootObjectT=stats[i].shootObject.transform;
				}
			}
		}
		
		public void Init(){
			destroyed=false;
			stunned=false;
			
			//fullHP=GetFullHP();
			//HP=fullHP;
			//fullShield=GetFullShield();
			//shield=fullShield;
			HP=GetFullHP();
			shield=GetFullShield();
			
			currentHPStagger=0;
			currentShieldStagger=0;
			
			ResetBuff();
			ResetSlow();
		}
		

		
		public virtual void Start() {
		
		}
		
		public virtual void OnEnable() {
		
		}
		public virtual void OnDisable() {
		
		}
		
		public virtual void Update() {
			
		}
		public virtual void FixedUpdate() {
			if(regenHPBuff!=0){
				HP+=regenHPBuff*Time.fixedDeltaTime;
				HP=Mathf.Clamp(HP, 0, GetFullHP());
			}
			
			if(HPRegenRate>0 && currentHPStagger<=0){
				HP+=GetHPRegenRate()*Time.fixedDeltaTime;
				HP=Mathf.Clamp(HP, 0, GetFullHP());
			}
			if(defaultShield>0 && shieldRegenRate>0 && currentShieldStagger<=0){
				shield+=GetShieldRegenRate()*Time.fixedDeltaTime;
				shield=Mathf.Clamp(shield, 0, GetFullShield());
			}
			
			currentHPStagger-=Time.fixedDeltaTime;
			currentShieldStagger-=Time.fixedDeltaTime;
			
			
			if(target!=null && !IsInConstruction() && !stunned){
				if(turretObject!=null){
					if(rotateTurretAimInXAxis && barrelObject!=null){
						Vector3 targetPos=target.GetTargetT().position;
						Vector3 dummyPos=targetPos;
						dummyPos.y=turretObject.position.y;
						
						Quaternion wantedRot=Quaternion.LookRotation(dummyPos-turretObject.position);
						turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						float angle=Quaternion.LookRotation(targetPos-barrelObject.position).eulerAngles.x;
						float distFactor=Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos)/GetSOMaxRange());
						float offset=distFactor*GetSOMaxAngle();
						wantedRot=turretObject.rotation*Quaternion.Euler(angle-offset, 0, 0);
						
						barrelObject.rotation=Quaternion.Slerp(barrelObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						if(Quaternion.Angle(barrelObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
						else targetInLOS=false;
					}
					else{
						Vector3 targetPos=target.GetTargetT().position;
						if(!rotateTurretAimInXAxis) targetPos.y=turretObject.position.y;
						
						Quaternion wantedRot=Quaternion.LookRotation(targetPos-turretObject.position);
						if(rotateTurretAimInXAxis){
							float distFactor=Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos)/GetSOMaxRange());
							float offset=distFactor*GetSOMaxAngle();
							wantedRot*=Quaternion.Euler(-offset, 0, 0);
						}
						turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						if(Quaternion.Angle(turretObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
						else targetInLOS=false;
					}
				}
				else targetInLOS=true;
			}
			
			//rotate turret back to origin
			if(IsCreep() && target==null && turretObject!=null && !stunned){
				turretObject.localRotation=Quaternion.Slerp(turretObject.localRotation, Quaternion.identity, turretRotateSpeed*Time.deltaTime*0.25f);
			}
			
		}
		
		
		
		
		
		float GetSOMaxRange(){ 
			if(stats[currentActiveStat].shootObject==null) return localShootObject.GetMaxShootRange();
			return stats[currentActiveStat].shootObject.GetMaxShootRange();
		}
			
		float GetSOMaxAngle(){ 
			if(stats[currentActiveStat].shootObject==null) return localShootObject.GetMaxShootAngle();
			return stats[currentActiveStat].shootObject.GetMaxShootAngle();
		}
		
		
		
		public virtual void IterateTargetPriority(int i=1){
			int nextPrior=(int)targetPriority+i;
			if(nextPrior>=5) nextPrior=0;
			if(nextPrior<0) nextPrior=4;
			targetPriority=(_TargetPriority)nextPrior;
		}
		
		public virtual void ChangeScanAngle(int angle){
			dirScanAngle=angle;
			dirScanRot=thisT.rotation*Quaternion.Euler(0f, dirScanAngle, 0f);
			
			if(turretObject!=null && target==null)
				turretObject.localRotation=Quaternion.identity*Quaternion.Euler(0f, dirScanAngle, 0f);
		}
		
		public IEnumerator ScanForTargetRoutine(){
			while(true){
				ScanForTarget();
				yield return new WaitForSeconds(0.1f);
				//yield return new WaitForSeconds(Time.fixedDeltaTime);	//change this line to same outcome when fast-forward (might have performance issue if you have lots of unit)
				if(GameControl.ResetTargetAfterShoot()){
					while(turretOnCooldown) yield return null;
				}
			}
		}
		
		void ScanForTarget(){
			if(destroyed || IsInConstruction() || stunned) return;
				
			//creeps changes direction so the scan direction for creep needs to be update 
			if(directionalTargeting){
				if(IsCreep()) dirScanRot=thisT.rotation;
				else dirScanRot=thisT.rotation*Quaternion.Euler(0f, dirScanAngle, 0f);
			}
			
			if(directionalTargeting && scanDirT!=null) scanDirT.rotation=dirScanRot;
			
			if(target==null){
				List<Unit> tgtList=TDTK.GetUnitInRange(thisT.position, GetRange(), GetRangeMin(), maskTarget);
				
				if(tgtList.Count>0 && directionalTargeting){
					List<Unit> filtered=new List<Unit>();
					for(int i=0; i<tgtList.Count; i++){
						Quaternion currentRot=Quaternion.LookRotation(tgtList[i].thisT.position-thisT.position);
						if(Quaternion.Angle(dirScanRot, currentRot)<=dirScanFOV*0.5f) filtered.Add(tgtList[i]);
					}
					tgtList=filtered;
				}
				
				/*
				//to prevent unit target through wall/obstacle
				LayerMask maskUnit=1<<TDTK.GetLayerTower();
				//LayerMask maskUnit=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF() | 1<<LayerManager.LayerTower();
				for(int i=0; i<tgtList.Count; i++){
					if(Physics.Linecast(thisT.position, tgtList[i].thisT.position, maskUnit)){
						tgtList.RemoveAt (i);
						i-=1;
					}
				}
				*/
				
				if(tgtList.Count>0){
					if(targetPriority==_TargetPriority.Random) target=tgtList[Random.Range(0, tgtList.Count-1)];
					else if(targetPriority==_TargetPriority.Nearest){
						float nearest=Mathf.Infinity;
						for(int i=0; i<tgtList.Count; i++){
							float dist=Vector3.Distance(thisT.position, tgtList[i].thisT.position);
							if(dist<nearest){
								nearest=dist;
								target=tgtList[i];
							}
						}
					}
					else if(targetPriority==_TargetPriority.Weakest){
						float lowest=Mathf.Infinity;
						for(int i=0; i<tgtList.Count; i++){
							if(tgtList[i].HP<lowest){
								lowest=tgtList[i].HP;
								target=tgtList[i];
							}
						}
					}
					else if(targetPriority==_TargetPriority.Toughest){
						float highest=0;
						for(int i=0; i<tgtList.Count; i++){
							if(tgtList[i].HP>highest){
								highest=tgtList[i].HP;
								target=tgtList[i];
							}
						}
					}
					else if(targetPriority==_TargetPriority.First){
						target=tgtList[Random.Range(0, tgtList.Count-1)];
						float lowest=Mathf.Infinity;
						for(int i=0; i<tgtList.Count; i++){
							if(tgtList[i].GetDistFromDestination()<lowest){
								lowest=tgtList[i].GetDistFromDestination();
								target=tgtList[i];
							}
						}
					}
				}
				
				targetInLOS=false;
			}
			else{
				float dist=Vector3.Distance(thisT.position, target.thisT.position);
				if(target.IsDestroyed() || dist>GetRange()) target=null;
				
				//to prevent unit target through wall/obstacle
				//LayerMask maskUnit=1<<TDTK.GetLayerTower();
				//if(Physics.Linecast(thisT.position, target.thisT.position, maskUnit)) target=null;
				
				if(target!=null && directionalTargeting){
					Quaternion tgtRotation=Quaternion.LookRotation(target.thisT.position-thisT.position);
					if(Quaternion.Angle(dirScanRot, tgtRotation)>=dirScanFOV*0.6f) target=null;
				}
			}
			
		}
		
		
		
		
		
		
		private bool turretOnCooldown=false;
		public IEnumerator TurretRoutine(){
			turretOnCooldown=false;
			
			for(int i=0; i<shootPoints.Count; i++){
				if(shootPoints[i]==null){ shootPoints.RemoveAt(i);	i-=1;	}
			}
			
			if(shootPoints.Count==0){
				Debug.LogWarning("ShootPoint not assigned for unit - "+unitName+", auto assigned", this);
				shootPoints.Add(thisT);
			}
			
			for(int i=0; i<stats.Count; i++){
				if(stats[i].shootObject!=null) ObjectPoolManager.New(stats[i].shootObject.gameObject, 3);
			}
			
			yield return null;
			
			while(true){
				while(target==null || stunned || IsInConstruction() || !targetInLOS) yield return null;
				turretOnCooldown=true;
				
				Unit currentTarget=target;
				
				float animationDelay=PlayAnimAttack();
				if(animationDelay>0) yield return new WaitForSeconds(animationDelay);
				
				AttackInstance attInstance=new AttackInstance();
				attInstance.srcUnit=this;
				attInstance.tgtUnit=currentTarget;
				attInstance.Process();
				
				for(int i=0; i<shootPoints.Count; i++){
					Transform sp=shootPoints[i];
					//Transform objT=(Transform)Instantiate(GetShootObjectT(), sp.position, sp.rotation);
					GameObject sObj=ObjectPoolManager.Spawn(GetShootObject().gameObject, sp.position, sp.rotation);
					ShootObject shootObj=sObj.GetComponent<ShootObject>();
					shootObj.Shoot(attInstance, sp);
					
					if(delayBetweenShootPoint>0) yield return new WaitForSeconds(delayBetweenShootPoint);
				}
				
				yield return new WaitForSeconds(GetCooldown()-animationDelay-shootPoints.Count*delayBetweenShootPoint);
				
				if(GameControl.ResetTargetAfterShoot()) target=null;
				turretOnCooldown=false;
			}
		}
		
		public void ApplyEffect(AttackInstance attInstance){
			if(destroyed) return;
			
			if(attInstance.missed) return;
			
			shield-=attInstance.damageShield;
			HP-=attInstance.damageHP;
			new TextOverlay(GetTextOverlayPos(), attInstance.damage.ToString("f0"), new Color(1f, 1f, 1f, 1f));
			
			PlayAnimHit();
			
			TDTK.OnUnitDamaged(this);
			
			currentHPStagger=GetHPStaggerDuration(); 
			currentShieldStagger=GetShieldStaggerDuration();
			
			if(attInstance.destroy || HP<=0){
				Destroyed();
				return;
			}
			
			if(attInstance.breakShield){
				defaultShield=0;
				shield=0;
			}
			if(attInstance.stunned) ApplyStun(attInstance.stun.duration);
			if(attInstance.slowed) ApplySlow(attInstance.slow);
			if(attInstance.dotted) ApplyDot(attInstance.dot);
		}
		
		public void ApplyStun(float duration){
			stunDuration=duration;
			if(!stunned) StartCoroutine(StunRoutine());
		}
		IEnumerator StunRoutine(){
			stunned=true;
			while(stunDuration>0){
				stunDuration-=Time.deltaTime;
				yield return null;
			}
			stunned=false;
		}
		
		public void ApplySlow(Slow slow){ StartCoroutine(SlowRoutine(slow)); }
		IEnumerator SlowRoutine(Slow slow){
			slowEffectList.Add(slow);
			ResetSlowMultiplier();
			yield return new WaitForSeconds(slow.duration);
			slowEffectList.Remove(slow);
			ResetSlowMultiplier();
		}
		
		void ResetSlowMultiplier(){
			if(slowEffectList.Count==0){
				slowMultiplier=1;
				return;
			}
			
			for(int i=0; i<slowEffectList.Count; i++){
				if(slowEffectList[i].slowMultiplier<slowMultiplier){
					slowMultiplier=slowEffectList[i].slowMultiplier;
				}
			}
			
			slowMultiplier=Mathf.Max(0, slowMultiplier);
		}
		void ResetSlow(){
			slowEffectList=new List<Slow>();
			ResetSlowMultiplier();
		}
		
		public void ApplyDot(Dot dot){ StartCoroutine(DotRoutine(dot)); }
		IEnumerator DotRoutine(Dot dot){
			int count=(int)Mathf.Floor(dot.duration/dot.interval);
			for(int i=0; i<count; i++){
				if(dot.interval==0) yield return null;
				else yield return new WaitForSeconds(dot.interval);
				
				if(destroyed) break;
				DamageHP(dot.value);
				
				if(HP<=0){ Destroyed();	break; }
			}
		}
		
		
		//for ability and what not
		public void ApplyDamage(float dmg){
			DamageHP(dmg);
			if(HP<=0) Destroyed();
		}
		public void RestoreHP(float value){
			new TextOverlay(GetTextOverlayPos(), value.ToString("f0"), new Color(0f, 1f, .4f, 1f));
			HP=Mathf.Clamp(HP+value, 0, GetFullHP());
		}
		
		
		//called when unit take damage
		void DamageHP(float dmg){
			HP-=dmg;
			new TextOverlay(GetTextOverlayPos(), dmg.ToString("f0"), new Color(1f, 1f, 1f, 1f));
			
			TDTK.OnUnitDamaged(this);
			PlayAnimHit();
			
			currentHPStagger=HPStaggerDuration;
			currentShieldStagger=shieldStaggerDuration;
		}
		
		
		IEnumerator SupportEffectRoutine(){
			while(true){
				yield return new WaitForSeconds(GetCooldown());
				while(stunned || IsInConstruction()) yield return null;
				SpawnEffectObject();
			}
		}
		
		
		private List<Unit> buffedUnit=new List<Unit>();
		private bool supportRoutineRunning=false;
		protected IEnumerator SupportRoutine(){
			supportRoutineRunning=true;
			
			StartCoroutine(SupportEffectRoutine());
			
			while(true){
				yield return new WaitForSeconds(0.1f);
				
				if(!destroyed){
					List<Unit> tgtList=new List<Unit>();
					Collider[] cols=Physics.OverlapSphere(thisT.position, GetRange(), maskTarget);
					if(cols.Length>0){
						for(int i=0; i<cols.Length; i++){
							Unit unit=cols[i].gameObject.GetComponent<Unit>();
							if(unit!=null && !unit.IsDestroyed()) tgtList.Add(unit);
						}
					}
					
					for(int i=0; i<buffedUnit.Count; i++){
						Unit unit=buffedUnit[i];
						if(unit==null || unit.IsDestroyed()){
							buffedUnit.RemoveAt(i); i-=1;
						}
						else if(!tgtList.Contains(unit)){
							unit.UnBuff(GetBuff());
							buffedUnit.RemoveAt(i); i-=1;
						}
					}
					
					for(int i=0; i<tgtList.Count; i++){
						Unit unit=tgtList[i];
						if(!buffedUnit.Contains(unit)){
							unit.Buff(GetBuff());
							buffedUnit.Add(unit);
						}
					}
				}
			}
		}
		public void UnbuffAll(){
			for(int i=0; i<buffedUnit.Count; i++){
				buffedUnit[i].UnBuff(GetBuff());
			}
		}
		
		public void Buff(Buff buff){
			if(activeBuffList.Contains(buff)) return;
			
			activeBuffList.Add(buff);
			UpdateBuffStat();
		}
		public void UnBuff(Buff buff){
			if(!activeBuffList.Contains(buff)) return;
			
			activeBuffList.Remove(buff);
			UpdateBuffStat();
		}
		
		[Header("Buff multiplier")]
		[HideInInspector] public List<Buff> activeBuffList=new List<Buff>();
		[HideInInspector] public float damageBuffMul=0f;
		[HideInInspector] public float cooldownBuffMul=0f;
		[HideInInspector] public float rangeBuffMul=0f;
		[HideInInspector] public float criticalBuffMod=0.1f;
		[HideInInspector] public float hitBuffMod=0.1f;
		[HideInInspector] public float dodgeBuffMod=0.1f;
		[HideInInspector] public float regenHPBuff=0.0f;
		
		void UpdateBuffStat(){
			ClearBuffStats();
			for(int i=0; i<activeBuffList.Count; i++){
				Buff buff=activeBuffList[i];
				if(damageBuffMul<buff.damageBuff) damageBuffMul=buff.damageBuff;
				if(cooldownBuffMul<buff.cooldownBuff) cooldownBuffMul=buff.cooldownBuff;
				if(!supportRoutineRunning && rangeBuffMul<buff.rangeBuff) rangeBuffMul=buff.rangeBuff;
				if(criticalBuffMod<buff.criticalBuff) criticalBuffMod=buff.criticalBuff;
				if(hitBuffMod<buff.hitBuff) hitBuffMod=buff.hitBuff;
				if(dodgeBuffMod<buff.dodgeBuff) dodgeBuffMod=buff.dodgeBuff;
				if(regenHPBuff<buff.regenHP) regenHPBuff=buff.regenHP;
			}
		}
		void ResetBuff(){
			activeBuffList=new List<Buff>();
			ClearBuffStats();
		}
		void ClearBuffStats(){
			damageBuffMul=0.0f;
			cooldownBuffMul=0.0f;
			rangeBuffMul=0.0f;
			criticalBuffMod=0f;
			hitBuffMod=0f;
			dodgeBuffMod=0f;
			regenHPBuff=0f;
		}
		
		
		
		public virtual void Destroyed(float delay=0){
			destroyed=true;
			
			if(destroyedEffObj!=null){
				if(!autoDestroyEff) ObjectPoolManager.Spawn(destroyedEffObj, targetPoint.position, thisT.rotation);
				else ObjectPoolManager.Spawn(destroyedEffObj, targetPoint.position, thisT.rotation, destroyEffDuration);
			}
			
			if(supportRoutineRunning) UnbuffAll();
			
			StartCoroutine(_Destroyed(delay));
		}
		protected IEnumerator _Destroyed(float delay){
			yield return new WaitForSeconds(delay);
			ObjectPoolManager.Unspawn(thisObj);
		}
		
		
		public Transform GetTargetT(){
			return targetPoint!=null ? targetPoint : thisT; 
		}
		
		public Vector3 GetTextOverlayPos(){
			return GetTargetT().position+new Vector3(0, 0.05f, 0);
		}
		
		
		
		private float GetPerkMulHP(){					return IsTower() ? PerkManager.GetTowerHP(prefabID) : 0 ; } 
		private float GetPerkMulHPRegen(){ 			return IsTower() ? PerkManager.GetTowerHPRegen(prefabID) : 0 ; } 
		private float GetPerkMulHPStagger(){ 		return IsTower() ? PerkManager.GetTowerHPStagger(prefabID) : 0 ; } 
		private float GetPerkMulShield(){ 				return IsTower() ? PerkManager.GetTowerShield(prefabID) : 0 ; } 
		private float GetPerkMulShieldRegen(){	 	return IsTower() ? PerkManager.GetTowerShieldRegen(prefabID) : 0 ; } 
		private float GetPerkMulShieldStagger(){	return IsTower() ? PerkManager.GetTowerShieldStagger(prefabID) : 0 ; } 
		
		private float GetPerkMulDamage(){ 			return IsTower() ? PerkManager.GetTowerDamage(prefabID) : 0 ; } 
		private float GetPerkMulCooldown(){ 		return IsTower() ? PerkManager.GetTowerCD(prefabID) : 0 ; } 
		private float GetPerkMulClipSize(){ 			return IsTower() ? PerkManager.GetTowerClipSize(prefabID) : 0 ; } 
		private float GetPerkMulReloadDuration(){ 	return IsTower() ? PerkManager.GetTowerReloadDuration(prefabID) : 0 ; } 
		private float GetPerkMulRange(){ 				return IsTower() ? PerkManager.GetTowerRange(prefabID) : 0 ; } 
		private float GetPerkMulAOERadius(){ 		return IsTower() ? PerkManager.GetTowerAOERadius(prefabID) : 0 ; } 
		private float GetPerkModHit(){ 				return IsTower() ? PerkManager.GetTowerHit(prefabID) : 0 ; } 
		private float GetPerkModDodge(){ 			return IsTower() ? PerkManager.GetTowerDodge(prefabID) : 0 ; } 
		private float GetPerkModCritChance(){ 		return IsTower() ? PerkManager.GetTowerCritChance(prefabID) : 0 ; } 
		private float GetPerkModCritMul(){ 			return IsTower() ? PerkManager.GetTowerCritMultiplier(prefabID) : 0 ; } 
		
		private float GetPerkModShieldBreak(){ 		return IsTower() ? PerkManager.GetTowerShieldBreakMultiplier(prefabID) : 0 ; } 
		private float GetPerkModShieldPierce(){ 	return IsTower() ? PerkManager.GetTowerShieldPierceMultiplier(prefabID) : 0 ; } 
		
		private Stun ModifyStunWithPerkBonus(Stun stun){ return IsTower() ? PerkManager.ModifyStunWithPerkBonus(stun.Clone(), prefabID) : stun; }
		private Slow ModifySlowWithPerkBonus(Slow slow){ return IsTower() ? PerkManager.ModifySlowWithPerkBonus(slow.Clone(), prefabID) : slow; }
		private Dot ModifyDotWithPerkBonus(Dot dot){ 		return IsTower() ? PerkManager.ModifyDotWithPerkBonus(dot.Clone(), prefabID) : dot; }
		private InstantKill ModifyInstantKillWithPerkBonus(InstantKill instKill){ return IsTower() ? PerkManager.ModifyInstantKillWithPerkBonus(instKill.Clone(), prefabID) : instKill; }
		
		
		
		public float GetFullHP(){ return defaultHP * (1+GetPerkMulHP()); }
		public float GetFullShield(){ return defaultShield * (1+GetPerkMulShield()); }
		private float GetHPRegenRate(){ return HPRegenRate * (1+GetPerkMulHPRegen()); }
		private float GetShieldRegenRate(){ return shieldRegenRate * (1+GetPerkMulShieldRegen()); }
		private float GetHPStaggerDuration(){ return HPStaggerDuration * (1-GetPerkMulHPStagger()); }
		private float GetShieldStaggerDuration(){ return shieldStaggerDuration * (1-GetPerkMulShieldStagger()); }
		
		public float GetDamageMin(){ return Mathf.Max(0, stats[currentActiveStat].damageMin * (1+damageBuffMul+dmgABMul+GetPerkMulDamage())); }
		public float GetDamageMax(){ return Mathf.Max(0, stats[currentActiveStat].damageMax * (1+damageBuffMul+dmgABMul+GetPerkMulDamage())); }
		public float GetCooldown(){ return Mathf.Max(0.05f, stats[currentActiveStat].cooldown * (1-cooldownBuffMul-cdABMul-GetPerkMulCooldown())); }
		
		public float GetRangeMin(){ return stats[currentActiveStat].minRange; }
		public float GetRange(){ return Mathf.Max(0, stats[currentActiveStat].range * (1+rangeBuffMul+rangeABMul+GetPerkMulRange())); }
		public float GetAOERadius(){ return stats[currentActiveStat].aoeRadius * (1+GetPerkMulAOERadius()); }
		
		public float GetHit(){ return stats[currentActiveStat].hit + hitBuffMod + GetPerkModHit(); }
		//~ public float GetDodge(){ return stats.Count==0 ? 0 : stats[currentActiveStat].dodge + dodgeBuffMod + GetPerkModDodge(); }
		public float GetDodge(){ return dodgeChance + dodgeBuffMod + GetPerkModDodge(); }
		
		public float GetCritChance(){ return stats[currentActiveStat].crit.chance + criticalBuffMod + GetPerkModCritChance(); }
		public float GetCritMultiplier(){ return stats[currentActiveStat].crit.dmgMultiplier + GetPerkModCritMul(); }
		
		public float GetShieldBreak(){ return stats[currentActiveStat].shieldBreak + GetPerkModShieldBreak(); }
		public float GetShieldPierce(){ return stats[currentActiveStat].shieldPierce + GetPerkModShieldPierce(); }
		public bool DamageShieldOnly(){ return stats[currentActiveStat].damageShieldOnly; }
		
		public Stun GetStun(){ return ModifyStunWithPerkBonus(stats[currentActiveStat].stun); }
		public Slow GetSlow(){ return ModifySlowWithPerkBonus(stats[currentActiveStat].slow); }
		public Dot 	GetDot(){ return ModifyDotWithPerkBonus(stats[currentActiveStat].dot); }
		public InstantKill GetInstantKill(){ return ModifyInstantKillWithPerkBonus(stats[currentActiveStat].instantKill); }
		
		
		
		
		
		
		public int GetShootPointCount(){ return shootPoints.Count; }
		
		public ShootObject GetShootObject(){
			//if(stats[currentActiveStat].shootObjectT==null) return localShootObjectT;
			//return stats[currentActiveStat].shootObjectT;
			return stats[currentActiveStat].shootObject!=null ? stats[currentActiveStat].shootObject : localShootObject;
		}
		public GameObject GetEffectObject(){ return stats[currentActiveStat].effectObject; }
		
		protected void SpawnEffectObject(){
			GameObject effectObj=GetEffectObject();	//shoot-object is used as the visual indicator
			if(effectObj!=null){
				if(!stats[currentActiveStat].autoDestroyEffect) ObjectPoolManager.Spawn(effectObj, thisT.position, thisT.rotation);
				else ObjectPoolManager.Spawn(effectObj, thisT.position, thisT.rotation, stats[currentActiveStat].effectDuration);
			}
		}
		
		
		public List<int> GetResourceGain(){ return stats[currentActiveStat].rscGain; }
		
		public Buff GetBuff(){ return stats[currentActiveStat].buff; }
		
		
		
		//public string GetDespStats(){ return stats[currentActiveStat].desp; }
		public string GetDespGeneral(){ return desp; }
		
		
		
		
		//public float GetDistFromDestination(){ return unitC!=null ? unitC._GetDistFromDestination() : 0; }
		public virtual float GetDistFromDestination(){ return 0; }
		public virtual bool IsInConstruction(){ return false; }
		
		
		//used by abilities
		private float dmgABMul=0;
		public void ABBuffDamage(float value, float duration){ StartCoroutine(ABBuffDamageRoutine(value, duration)); }
		IEnumerator ABBuffDamageRoutine(float value, float duration){
			dmgABMul+=value;
			yield return new WaitForSeconds(duration);
			dmgABMul-=value;
		}
		private float rangeABMul=0;
		public void ABBuffRange(float value, float duration){ StartCoroutine(ABBuffDamageRoutine(value, duration)); }
		IEnumerator ABBuffRangeRoutine(float value, float duration){
			rangeABMul+=value;
			yield return new WaitForSeconds(duration);
			rangeABMul-=value;
		}
		private float cdABMul=0;
		public void ABBuffCooldown(float value, float duration){ StartCoroutine(ABBuffCooldownRoutine(value, duration)); }
		IEnumerator ABBuffCooldownRoutine(float value, float duration){
			cdABMul+=value;
			yield return new WaitForSeconds(duration);
			cdABMul-=value;
		}
		
		
		
		void OnDrawGizmos(){
			if(target!=null){
				if(IsCreep()) Gizmos.DrawLine(transform.position, target.transform.position);
			}
		}
		
		
		
		
		private UnitAnimation uAnimation;
		public void SetUnitAnimation(UnitAnimation uAnim){ uAnimation=uAnim; }
		
		public void PlayAnimMove(float speed){ if(uAnimation!=null) uAnimation.PlayMove(speed); }
		public void PlayAnimSpawn(){ if(uAnimation!=null) uAnimation.PlaySpawn(); }
		public void PlayAnimHit(){ if(uAnimation!=null) uAnimation.PlayHit(); }
		public float PlayAnimDestroyed(){ return uAnimation!=null ? uAnimation.PlayDestroyed() : 0 ; }
		public float PlayAnimDestination(){ return uAnimation!=null ? uAnimation.PlayDestination() : 0 ; }
			
		public void PlayAnimConstruct(){ if(uAnimation!=null) uAnimation.PlayConstruct(); }
		public void PlayAnimDeconstruct(){ if(uAnimation!=null) uAnimation.PlayDeconstruct(); }
		
		public float PlayAnimAttack(){ return uAnimation!=null ? uAnimation.PlayAttack() : 0 ; }
		
		
		
		//following function are used to replace ScanForTargetRoutine() and TurretRoutine() when the game are time sensitive, where the outcome must be same regardless of time scale
		/*
		private int targetFreqCount=0;
		void FixedTimeScanForTarget(){
			targetFreqCount+=1;
			if(targetFreqCount==10) targetFreqCount=0;
			else return;
			
			ScanForTarget();
		}
		
		private float attackCD=-1;
		private float reloadCD=-1;
		private int currentAmmo=-1;		//when enabled, make sure to set it reset it in init(), refer to turretRoutine
		void FixedTimeTurret(){
			if(reloadCD>0) reloadCD-=Time.fixedDeltaTime;
			if(attackCD>0) attackCD-=Time.fixedDeltaTime;
			
			if(attackCD>0 || reloadCD>0) return;
			
			if(target==null || stunned || IsInConstruction() || !targetInLOS) return;
			
			Unit currentTarget=target;
			
			AttackInstance attInstance=new AttackInstance();
			attInstance.srcUnit=this;
			attInstance.tgtUnit=currentTarget;
			
			for(int i=0; i<shootPoints.Count; i++){
				Transform sp=shootPoints[i];
				Transform objT=(Transform)Instantiate(GetShootObjectT(), sp.position, sp.rotation);
				ShootObject shootObj=objT.GetComponent<ShootObject>();
				shootObj.Shoot(attInstance, sp);
			}
			
			if(currentAmmo>-1){
				currentAmmo-=1;
				if(currentAmmo==0){
					reloadCD=GetReloadDuration();
					currentAmmo=GetClipSize();
				}
			}
			
			attackCD=GetCooldown();
		}
		*/
	}

}
