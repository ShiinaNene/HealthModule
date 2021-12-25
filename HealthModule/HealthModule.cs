using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckGame;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HealthModule
{
    public class HealthModule : Mod
    {
        protected override void OnPostInitialize()
        {
            Injection.Init();
            Type network = typeof(Network);
            FieldInfo typeToMessageID = network.GetField("_typeToMessageID", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            dynamic _typeToMessageID = typeToMessageID.GetValue(Level.current) as Map<ushort, Type>;

            IEnumerable<Type> subclasses = Editor.GetSubclasses(typeof(NetMessage));
            _typeToMessageID.Clear();
            ushort key = 1;
            foreach (Type type in subclasses)
            {
                if (type.GetCustomAttributes(typeof(FixedNetworkID), false).Length != 0)
                {
                    FixedNetworkID customAttribute = (FixedNetworkID)type.GetCustomAttributes(typeof(FixedNetworkID), false)[0];
                    if (customAttribute != null)
                        _typeToMessageID.Add(type, customAttribute.FixedID);
                }
            }
            foreach (Type type in subclasses)
            {
                if (!_typeToMessageID.ContainsValue(type))
                {
                    while (_typeToMessageID.ContainsKey(key))
                        ++key;
                    _typeToMessageID.Add(type, key);
                    ++key;
                }
            }

        }

        public static void Add(Type type, int[] damage) => Global.Damages.Add(type, damage);

    }
}
