using System.Collections;

public interface ISkillEffect
{
    bool NeedsTargetSelection { get; }
    IEnumerator Execute(Player caster, Enemy target, float damageMultiplier);
}
