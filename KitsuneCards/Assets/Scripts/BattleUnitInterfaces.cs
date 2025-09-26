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
    void BuffBlock(int turns, float percentage);
}

public interface IBuffable
{
    void BuffDoT(int turns, int bonusDoT);
    void BuffAllEffects(int turns, float multiplier);
}

public interface IDebuffable
{
    void ApplyDoT(int amount);
    void TripleDoT();
    void ExtendDebuff(int turns);
    void ApplyDamageDebuff(int turns, float multiplier);
    void LoseEnergy(int amount);
    void ApplyStun(int turns);
}
