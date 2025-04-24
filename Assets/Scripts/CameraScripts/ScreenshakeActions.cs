using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeActions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RangedAttackAction.OnAnyRangedAttack += RangedAttackAction_OnAnyRangedAttack;
        ExplosiveProjectile.OnAnyExplosiveExploded += ExplosiveProjectile_OnAnyExplosiveExploded;
    }

    private void RangedAttackAction_OnAnyRangedAttack(object sender, RangedAttackAction.OnRangedAttackEventArgs e)
    {
        Screenshake.Instance.Shake(1.0f);
    }

    private void ExplosiveProjectile_OnAnyExplosiveExploded(object sender, EventArgs e)
    {
        Screenshake.Instance.Shake(2.5f);
    }
}
