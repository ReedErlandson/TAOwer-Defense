using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TDTK {

	[System.Serializable]
	public class Ability : TDTKItem{
		
		public bool disableInAbilityManager=false;
		
		public int cost=10;
		public float cooldown=10;	//cd duration
		[HideInInspector] public float currentCD=0;	//variable for counting cd during runtime, ability only available when this is <0
		
		public bool requireTargetSelection=true;
		public bool singleUnitTargeting=false;
		
		public enum _TargetType{Hostile, Friendly, Hybrid}
		public _TargetType targetType=_TargetType.Hostile;
		
		public int maxUseCount=-1;
		[HideInInspector] public int usedCount=0;		//variable for counting usage during runtime
		[HideInInspector] public int usedRemained=0;	//variable for counting usage during runtime
		
		public bool useDefaultEffect=true;
		public AbilityEffect effect=new AbilityEffect();
		
		public float aoeRadius=2;
		public float effectDelay=0.25f;
		
		
		public Transform indicator;
		
		public GameObject effectObj;
		public bool destroyEffectObj=true;
		public float destroyEffectDuration=1.5f;
		
		public bool useCustomDesp=false;
		public string desp="";
		
		
		public void Init(){
			if(maxUseCount>0) usedRemained=maxUseCount;
			else usedRemained=-1;
			
			if(indicator!=null){
				indicator=(Transform)MonoBehaviour.Instantiate(indicator);
				indicator.parent=AbilityManager.GetInstance().transform;
			}
		}
		
		public void Activate(Vector3 pos){
			usedCount+=1;
			usedRemained-=1;
			
			AbilityManager.GetInstance().StartCoroutine(CooldownRoutine());
			
			if(effectObj!=null){
				if(!destroyEffectObj) ObjectPoolManager.Spawn(effectObj, pos, Quaternion.identity);
				else ObjectPoolManager.Spawn(effectObj, pos, Quaternion.identity, destroyEffectDuration);
			}
		}
		
		
		public IEnumerator CooldownRoutine(){
			currentCD=GetCooldown();
			while(currentCD>0){
				currentCD-=Time.deltaTime;
				yield return null;
			}
			
			TDTK.OnAbilityReady(this);
		}
		
		
		public string IsAvailable(){
			if(GetCost()>AbilityManager.GetEnergy()) return "Insufficient Energy";
			if(currentCD>0) return "Ability is on cooldown";
			if(maxUseCount>0 && usedCount>=maxUseCount) return "Usage limit exceed";
			return "";
		}
		
		
		
		public Ability Clone(){
			Ability ab=new Ability();
			ab.ID=ID;
			ab.name=name;
			ab.icon=icon;
			
			ab.disableInAbilityManager=disableInAbilityManager;
			
			ab.cost=cost;
			ab.cooldown=cooldown;
			ab.currentCD=currentCD;
			
			ab.requireTargetSelection=requireTargetSelection;
			ab.singleUnitTargeting=singleUnitTargeting;
			ab.targetType=targetType;
			
			ab.maxUseCount=maxUseCount;
			ab.usedCount=usedCount;
			ab.usedRemained=usedRemained;
			
			ab.useDefaultEffect=useDefaultEffect;
			ab.effect=effect.Clone();
			ab.aoeRadius=aoeRadius;
			ab.effectDelay=effectDelay;
			
			ab.indicator=indicator;
			
			ab.effectObj=effectObj;
			ab.destroyEffectObj=destroyEffectObj;
			ab.destroyEffectDuration=destroyEffectDuration;
			
			ab.useCustomDesp=useCustomDesp;
			ab.desp=desp;
			return ab;
		}
		
		
		public float GetCost(){ return cost * (1-PerkManager.GetAbilityCost(ID)); }
		public float GetCooldown(){ return cooldown * (1-PerkManager.GetAbilityCooldown(ID)); }
		public float GetAOERadius(){ return aoeRadius * (1+PerkManager.GetAbilityAOERadius(ID)); }
		
		
		public AbilityEffect GetActiveEffect(){
			AbilityEffect eff=new AbilityEffect();
			
			/*
			PerkAbilityModifier gModifier=PerkManager.GetGlobalAbilityModifier();
			PerkAbilityModifier modifier=PerkManager.GetAbilityModifier(ID);
			
			eff.duration=effect.duration * (1+gModifier.effects.duration+modifier.effects.duration);
			eff.damageMin=effect.damageMin * (1+gModifier.effects.damageMin+modifier.effects.damageMin);
			eff.damageMax=effect.damageMax * (1+gModifier.effects.damageMax+modifier.effects.damageMax);
			eff.stunChance=effect.stunChance * (1+gModifier.effects.stunChance+modifier.effects.stunChance);
			
			eff.slow=effect.slow.Clone();
			eff.slow.slowMultiplier*=(1-(gModifier.effects.slow.slowMultiplier+modifier.effects.slow.slowMultiplier));
			eff.slow.duration*=(1+(gModifier.effects.slow.duration+modifier.effects.slow.duration));
			
			eff.damageBuff=effect.damageBuff * (1+gModifier.effects.damageBuff+modifier.effects.damageBuff);
			eff.rangeBuff=effect.rangeBuff * (1+gModifier.effects.rangeBuff+modifier.effects.rangeBuff);
			eff.cooldownBuff=effect.cooldownBuff * (1+gModifier.effects.cooldownBuff+modifier.effects.cooldownBuff);
			eff.HPGainMin=effect.HPGainMin * (1+gModifier.effects.HPGainMin+modifier.effects.HPGainMin);
			eff.HPGainMax=effect.HPGainMax * (1+gModifier.effects.HPGainMax+modifier.effects.HPGainMax);
			*/
			
			
			eff.duration=effect.duration * (1+PerkManager.GetAbilityDuration(ID));
			eff.damageMin=effect.damageMin * (1+PerkManager.GetAbilityDamage(ID));
			eff.damageMax=effect.damageMax * (1+PerkManager.GetAbilityDamage(ID));
			eff.stunChance=effect.stunChance * (1+PerkManager.GetAbilityStunChance(ID));
			
			eff.slow=PerkManager.ModifySlowWithPerkBonus(effect.slow.Clone(), ID, 2);	//pass 2 to indicate this is for ability
			eff.dot=PerkManager.ModifyDotWithPerkBonus(effect.dot.Clone(), ID, 2);
			
			eff.damageBuff=effect.damageBuff * (1+PerkManager.GetAbilityDamageBuff(ID));
			eff.rangeBuff=effect.rangeBuff * (1+PerkManager.GetAbilityRangeBuff(ID));
			eff.cooldownBuff=effect.cooldownBuff * (1+PerkManager.GetAbilityCooldownBuff(ID));
			eff.HPGainMin=effect.HPGainMin * (1+PerkManager.GetAbilityHPGain(ID));
			eff.HPGainMax=effect.HPGainMax * (1+PerkManager.GetAbilityHPGain(ID));
			
			return eff;
		}
		
		
		public string GetDesp(){
			if(useCustomDesp) return desp;
			
			string text="";
			
			AbilityEffect eff=GetActiveEffect();
			
			if(eff.damageMax>0){
				if(requireTargetSelection) text+="Deals "+eff.damageMin+"-"+eff.damageMax+" to hostile target in range\n";
				else text+="Deals "+eff.damageMin+"-"+eff.damageMax+" to all hostile on the map\n";
			}
			if(eff.stunChance>0 && eff.duration>0){
				if(requireTargetSelection) text+=(eff.stunChance*100).ToString("f0")+"% chance to stun hostile target for "+eff.duration+"s\n";
				else text+=(eff.stunChance*100).ToString("f0")+"% chance to stun all hostile on the map for "+eff.duration+"s\n";
			}
			if(eff.slow.IsValid()){
				if(requireTargetSelection) text+="Slows hostile target down for "+eff.duration+"s\n";
				else text+="Slows all hostile on the map down for "+eff.duration+"s\n";
			}
			if(eff.dot.GetTotalDamage()>0){
				if(requireTargetSelection) text+="Deals "+eff.dot.GetTotalDamage().ToString("f0")+" to hostile target over "+eff.duration+"s\n";
				else text+="Deals "+eff.dot.GetTotalDamage().ToString("f0")+" to all hostile on the map over "+eff.duration+"s\n";
			}
			
			
			if(eff.HPGainMax>0){
				if(requireTargetSelection) text+="Restore "+eff.HPGainMin+"-"+eff.HPGainMax+" of friendly target HP\n";
				else text+="Restore "+eff.HPGainMin+"-"+eff.HPGainMax+" of all tower HP\n";
			}
			if(eff.duration>0){
				if(eff.damageBuff>0){
					if(requireTargetSelection) text+="Increase friendly target damage by "+(eff.damageBuff*100).ToString("f0")+"%\n";
					else text+="Increase all towers damage by "+(eff.damageBuff*100).ToString("f0")+"%\n";
				}
				if(eff.rangeBuff>0){
					if(requireTargetSelection) text+="Increase friendly target range by "+(eff.rangeBuff*100).ToString("f0")+"%\n";
					else text+="Increase all towers range by "+(eff.rangeBuff*100).ToString("f0")+"%\n";
				}
				if(eff.cooldownBuff>0){
					if(requireTargetSelection) text+="Decrease friendly target cooldown by "+(eff.cooldownBuff*100).ToString("f0")+"%\n";
					else text+="Decrease all towers cooldown by "+(eff.cooldownBuff*100).ToString("f0")+"%\n";
				}
			}
			
			
			return text;
		}
	}
	
	
	[System.Serializable]
	public class AbilityEffect{
		
		public float duration=0;	//duration of the effect, shared by all (stun & buff)
		
		//offsense stat
		public float damageMin=0;
		public float damageMax=0;
		public float stunChance=0;
		public Slow slow=new Slow();
		public Dot dot=new Dot();
		
		//defense stat
		public float damageBuff=0;
		public float rangeBuff=0;
		public float cooldownBuff=0;
		public float HPGainMin=0;
		public float HPGainMax=0;
		
		
		public AbilityEffect Clone(){
			AbilityEffect eff=new AbilityEffect();
			
			eff.duration=duration;
			
			eff.damageMin=damageMin;
			eff.damageMax=damageMax;
			eff.stunChance=stunChance;
			eff.slow=slow.Clone();
			eff.dot=dot.Clone();
			
			eff.slow.duration=eff.duration;
			eff.dot.duration=eff.duration;
			
			eff.damageBuff=damageBuff;
			eff.rangeBuff=rangeBuff;
			eff.cooldownBuff=cooldownBuff;
			eff.HPGainMin=HPGainMin;
			eff.HPGainMax=HPGainMax;
			
			return eff;
		}
	}
	
	
	
}
