using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading.Tasks;

namespace HealthModule
{
    public static class Global
    {
        internal static Level level = null;
        internal static HealthManager[] managers = new HealthManager[16];
        public static Dictionary<Type, int[]> Damages = new Dictionary<Type, int[]>()
        {
            {typeof(AT9mm), new int[2]{51, 25} },
            {typeof(ATSniper), new int[2]{100, 75} },
            {typeof(ATShrapnel), new int[2]{23, 17} },
            {typeof(Sharpshot), new int[3]{120, 120, 60} },
            {typeof(Sniper), new int[3]{100, 100, 30} },
            {typeof(Pistol), new int[2]{51, 25} },
            {typeof(SnubbyPistol), new int[2]{51, 25} },
            {typeof(PewPewLaser), new int[2]{34, 25} },
            {typeof(LaserRifle), new int[2]{45, 31} },
            {typeof(Chaingun), new int[2]{34, 25} },
            {typeof(SMG), new int[2]{26, 19} },
            {typeof(PlasmaBlaster), new int[2]{26, 19} },
            {typeof(Shotgun), new int[2]{21, 18} },
            {typeof(Blunderbuss), new int[2]{32, 24} },
            {typeof(CombatShotgun), new int[2]{21, 18} },
            {typeof(VirtualShotgun), new int[2]{21, 18} },
            {typeof(CowboyPistol), new int[2]{94, 65} },
            {typeof(Magnum), new int[2]{94, 65} },
            {typeof(DuelingPistol), new int[2]{110, 110} },
            {typeof(G18), new int[2]{12, 7} },
            {typeof(MagBlaster), new int[2]{100, 100} },
            {typeof(GrenadeLauncher), new int[2]{120, 120} },
            {typeof(Musket), new int[2]{74, 61} },
            {typeof(OldPistol), new int[2]{74, 61} },
            {typeof(SuicidePistol), new int[2]{94, 65} },
            {typeof(Bazooka), new int[2]{120, 120} },
        };

        internal static Dictionary<Type, int[,]> SpecialDamages = new Dictionary<Type, int[,]>()
        {
            {typeof(ATPhaser), new int[4, 3]{
                {0, 0, 0},
                {25, 17, 0},
                {71, 53, 20},
                {120, 100, 40},
            } },

        };

        internal static int GetDamage(Type type, int part)
        {
            if (Damages.ContainsKey(type))
            {
                if (Damages[type].Length > part)
                    return Damages[type][part];
            }
            return -34 * (part - 2);
        }
        internal static int GetDamage(Type type, int part, int charge)
        {
            if (SpecialDamages.ContainsKey(type))
            {
                if (SpecialDamages[type].GetLength(0) > charge)
                    if (SpecialDamages[type].GetLength(1) > part)
                        return SpecialDamages[type][charge, part];
            }
            return -34 * (part - 2);
        }
        internal static int GetDamage(Type type, int part, float charge)
        {
            int chargeInt = (int)Math.Floor(charge);
            if (chargeInt < 0) chargeInt = 0;
            if (SpecialDamages.ContainsKey(type))
            {
                if (SpecialDamages[type].GetLength(0) > chargeInt)
                    if (SpecialDamages[type].GetLength(1) > part)
                        return SpecialDamages[type][chargeInt, part];
            }
            return -34 * (part - 2);
        }

        internal static int GetStunTime(Type type)
        {

            if (Damages.ContainsKey(type))
            {
                if (Damages[type].Count() >= 3)
                    return Damages[type][2];
            }
            return 0;

        }
        internal static int GetStunTime(Type type, int charge)
        {
            if (SpecialDamages.ContainsKey(type))
            {
                if (SpecialDamages[type].GetLength(0) > charge)
                    if (SpecialDamages[type].GetLength(1) > 2)
                        return SpecialDamages[type][charge, 2];
            }
            return 0;

        }
        internal static int GetStunTime(Type type, float charge)
        {
            int chargeInt = 0;
            if (charge >= 1f && charge < 2f)
                chargeInt = 1;
            else if (charge >= 2f)
                chargeInt = 2;
            if (SpecialDamages.ContainsKey(type))
            {
                if (SpecialDamages[type].GetLength(0) > chargeInt)
                    if (SpecialDamages[type].GetLength(1) > 2)
                        return SpecialDamages[type][chargeInt, 2];
            }
            return 0;

        }

        internal static void Update()
        {
            if (level != Level.current)
            {
                level = Level.current;
                managers = new HealthManager[16];
            }

            for (byte i = 0; i < DuckNetwork.profiles.Count(); i++)
            {
                var obj = managers[i];
                if (obj == null || (obj.duck != null && obj.duck.level != Level.current))
                {
                    var duck = Network.isActive ? DuckNetwork.profiles[i]?.duck : Duck.Get(i);
                    managers[i] = (duck != null) ? new HealthManager(duck) : null;
                }
            }
        }

        public static HealthManager GetManager(this Duck duck) => Network.isActive ? managers[duck.netProfileIndex] : managers[duck.GetIndex()];

        public static int GetIndex(this Duck duck) => Persona.Number(duck?.profile?.persona);
    }
}
