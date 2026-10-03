using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiselModLoader.Runtime
{
    public class CMLMod
    {
        public Logging logger;
        /// <summary>
        /// ran before chisel main start its recomended to do harmony patches here
        /// </summary>
        public virtual void Preload()
        {

        }

        /// <summary>
        /// called every frame while in a level does not care if the game is paused or not
        /// </summary>
        public virtual void Update()
        {
          
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class CMLPluginInfoAttribute : System.Attribute
    {
        public string Name;
        public string Version;

        public CMLPluginInfoAttribute(string name,string version)
        {
            Name = name;
            Version = version;
        }
    }
}
