using ChiselModLoader.Shared;
using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.Loader;
namespace ChiselModLoader.Runtime
{
    public class CML_Bootstrapper
    {
        // Ran before the game's main process runs.
        // It is loaded here to apply patches to Chisel.
        static public Assembly GameASM = null!;
        static public Logging logger = new Logging("Chisel Mod Loader");
        
        static public List<Assembly> ModAsms = new();
        static public List<CMLMod> Mods = new();
        static void Main(Assembly GameAsm)
        {
            AssemblyLoadContext.Default.Resolving += (context, name) =>
ModAsms.FirstOrDefault(a => a.GetName().Name == name.Name);

            GameASM = GameAsm;
            logger.LogDebug("loading Engine"); //we need to load engine so we can patch engine
            string enginePath = Path.GetFullPath("engine.dll");
            //Assembly engineAsm = Assembly.LoadFrom(enginePath);

            logger.LogDebug("Loading Libs");
            LoadCMLLibs();
            logger.LogDebug("Loading Mods");

            //Load Mods and other things when the engine loads
            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                if (args.LoadedAssembly.GetName().Name.Equals("engine", StringComparison.OrdinalIgnoreCase))
                {
                    CML_Bootstrapper.logger.LogDebug("engine loaded LoadingMods");
                    CML_Bootstrapper.LoadCMLPlugins();
                    ChiselHarmonyPatches.PatchEngineAsm(args.LoadedAssembly);
                }
            };
            //now we can try to load the game
            StartGame();
        }
        //the first thing we need to do is load the libs like harmony or else literaly everything else will go up in flames
        static void LoadCMLLibs()
        {
            if (!Directory.Exists(Path.Combine(CMLGlobals.basedir, "CML")))
            {
                Console.WriteLine("if you are reading this you forgot to the copy the cml folder with the mod");
            }
            foreach(var file in Directory.GetFiles(Path.Combine(CMLGlobals.basedir, "CML","CMLLibs")))
            {
                logger.LogDebug($"loading mannaged lib {file}");
                Assembly.LoadFrom(file);
                logger.LogDebug($"loaded managed lib {file}");
            }
        }

        static public void LoadCMLPlugins()
        {
            Directory.CreateDirectory(Path.Combine(CMLGlobals.basedir,"CML","Mods")); //Some people might delete the mods directory to clear all of there mods this there for that we should not crash because of an empty folder not being there
            foreach (var file in Directory.EnumerateFiles(Path.Combine(CMLGlobals.basedir, "CML", "Mods"), "*.dll"))
            {
                logger.LogDebug($"loading mannaged Mod {file}");
                var asm = Assembly.LoadFrom(file);
                ModAsms.Add(asm);
            }
            foreach(var asm in ModAsms)
            {
                foreach (var typedef in asm.DefinedTypes)
                {
                    if (!Attribute.IsDefined(typedef, typeof(CMLPluginInfoAttribute)))
                    {
                        continue;
                    }
                    var attribute = typedef.GetCustomAttribute<CMLPluginInfoAttribute>()!;
                    logger.LogDebug($"Loading mod:{attribute.Name} - Version {attribute.Version}");
                    var Mod = Activator.CreateInstance(typedef) as CMLMod;
                    if (Mod == null)
                    {
                        continue;
                    }
                    Mod.logger = new Logging(attribute.Name);
                    Mod.Preload();
                    Mods.Add(Mod);
                }
            }
        }

        static void StartGame()
        {
            MethodInfo? entryPoint = GameASM.EntryPoint;
            if (entryPoint == null)
            {
                throw new InvalidOperationException("The loaded game assembly does not have a valid EntryPoint.");
            }
            try
            {
                string[] gameArgs = ["-compile"]; //we need to force it to rebuild so mods work, this does cause the bug of breaking the engine when it fines a modded type file,but comes at the plus of being able to use modded ents in rockwell 2
                entryPoint.Invoke(null, new object[] { gameArgs });
            }
            catch(Exception e)
            {
                logger.LogException(e);
            }
            
        }
    }
}
