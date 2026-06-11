using System.Collections;

public interface ISkillEffect
{
    IEnumerator Execute(Player caster, Enemy target, float damageMultiplier);
}
