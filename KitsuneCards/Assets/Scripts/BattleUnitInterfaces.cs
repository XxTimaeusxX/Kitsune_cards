using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IBlockable
{
    void ApplyBlock(int amount);
    void ApplyReflect(float percentage);
}

public interface IBuffable
{
    void BuffDoT(int turns, int BonusDamage);
    void BuffAllEffects(int turns, float multiplier);
    
    void ExtendDebuff(int turns);

    void BuffBlock(int turns, float BlockAmount);
}

public interface IDebuffable
{
    void ApplyDoT(int turns, int damageAmount);
    void TripleDoT();
   
    void ApplyDamageDebuff(int turns, float multiplier);
    void LoseEnergy(int amount);
    void ApplyStun(int turns);
}
