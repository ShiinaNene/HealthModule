using DuckGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthModule
{
    public class Patches
    {
        [HarmonyPatch(typeof(Duck), "Hit", new Type[] { typeof(Bullet), typeof(Vec2) })]
        public static class Duck_Hit_Prefix
        {
            [HarmonyPrefix]
            public static bool Prefix(Duck __instance, Bullet bullet, Vec2 hitPos, ref bool __result)
            {
                Duck duck = __instance;
                if (duck._trapped != null || (duck._trappedInstance != null && duck._trappedInstance.visible))
                {
                    __result = false;
                    return false;
                }
                if (duck.ragdoll != null || (duck._ragdollInstance != null && duck._ragdollInstance.visible))
                {
                    __result = false;
                    return false;
                }
                if (bullet.isLocal)
                {
                    int hitPart = 1;
                    if (bullet.y < duck.y) hitPart = 0;
                    float damage = 0f;
                    float stunTime = 0f;
                    if ((bullet.ammo is ATShrapnel && (bullet.firedFrom is Grenade || bullet.firedFrom is Mine))
                        || (bullet.ammo is ATRCShrapnel && bullet.firedFrom is RCCar))
                    {
                        Vec2 delta = bullet.travelStart - duck.position;
                        float distance = delta.length / bullet.ammo.range;
                        if (distance <= 0.7f)
                        {
                            damage = 100;
                            stunTime = 60;
                        }
                        else
                        {
                            damage = 90 * (1.7f - distance);
                            stunTime = 40 * (1.7f - distance);
                        }
                    }
                    else if (bullet.ammo is ATPhaser phaserAmmo)
                    {
                        switch (phaserAmmo.penetration)
                        {
                            case 1:
                                if (hitPart == 0)
                                    damage = 25;
                                else damage = 17;
                                break;
                            case 2:
                                if (hitPart == 0)
                                    damage = 62;
                                else damage = 47;
                                stunTime = 5;
                                break;
                            case 3:
                                damage = 87;
                                stunTime = 10;
                                break;
                        }
                    }
                    else if (bullet.firedFrom is Gun gun && gun != null)
                    {

                        damage = Global.GetDamage(gun.GetType(), hitPart);
                        stunTime = Global.GetStunTime(gun.GetType());
                    }
                    else
                    {
                        damage = Global.GetDamage(bullet.ammo.GetType(), hitPart);
                        stunTime = Global.GetStunTime(bullet.ammo.GetType());
                    }
                    var healthManager = duck.GetManager();
                    if (healthManager == null)
                    {
                        duck.Kill(new DTShot(bullet));
                        __result = (duck.thickness > bullet.ammo.penetration);
                        return false;
                    }
                    int dir = 0;
                    if (bullet.travelDirNormalized.x > 0) dir = 1;
                    if (bullet.travelDirNormalized.x < 0) dir = -1;

                    if (Network.isActive)
                        Send.Message(new NMHealthModuleHit(duck.netProfileIndex, damage, stunTime, dir));
                    if (healthManager.Hit(damage, stunTime, dir))
                        duck.Kill(new DTShot(bullet));
                }
                __result = (duck.thickness > bullet.ammo.penetration);
                return false;
            }
        }

        [HarmonyPatch(typeof(Duck), "Draw", new Type[] { })]
        public static class Duck_Draw_Prefix
        {
            [HarmonyPrefix]
            public static void Prefix(Duck __instance) => __instance?.GetManager()?.Draw();
        }

        [HarmonyPatch(typeof(Duck), "Update", new Type[] { })]
        public static class Duck_Update_Prefix
        {
            [HarmonyPrefix]
            public static void Prefix(Duck __instance) => __instance?.GetManager()?.Update();
        }
    }
}
