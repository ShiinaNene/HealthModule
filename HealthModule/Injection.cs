using DuckGame;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace HealthModule
{
    public static class Injection
    {
        public static void Init()
        {
            var reflectionFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
            (typeof(Game).GetField("updateableComponents", reflectionFlags).GetValue(MonoMain.instance) as List<IUpdateable>).Add(new ModUpdate());
            new Harmony("cc.tama.healthmodule").PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class ModUpdate : IUpdateable
    {
        public bool Enabled { get { return true; } }
        public int UpdateOrder { get { return 1; } }

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;


        public void Update(GameTime gameTime)
        {
            Global.Update();
        }
    }
}
