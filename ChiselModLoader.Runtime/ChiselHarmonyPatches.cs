using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChiselModLoader.Runtime
{
    public class ChiselHarmonyPatches
    {
        private static Harmony _harmony = null!;

        public static void PatchEngineAsm(Assembly engineAsm)
        {
            Harmony.DEBUG = true;

            _harmony = new Harmony("com.cml.engine.patches");
            Type? gameStartupType = engineAsm.GetType("Engine.Utils.GameStartup");
            if (gameStartupType != null)
            {
                MethodInfo? original = gameStartupType.GetMethod(
                    "CompileEntityData",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(Assembly), typeof(string) },
                    null
                );

                if (original != null)
                {
                    _harmony.Patch(
                        original: original,
                        prefix: new HarmonyMethod(typeof(ChiselHarmonyPatches).GetMethod(nameof(CompileEntityDataPatch), BindingFlags.Public | BindingFlags.Static))
                    );
                    CML_Bootstrapper.logger.LogDebug("Patched GameStartup.CompileEntityData");
                }
                else
                {
                    CML_Bootstrapper.logger.LogError("GetMethod returned NULL for CompileEntityData!");
                }
            }
            else
            {
                CML_Bootstrapper.logger.LogError("GetType returned NULL for Engine.Utils.GameStartup!");
            }

            MakeUpdatePatch(engineAsm);
        }

        public static void MakeUpdatePatch(Assembly engineAsm)
        {
            Type? baseEngineType = engineAsm.GetType("Engine.MainEngine")
                                  ?? engineAsm.GetTypes().FirstOrDefault(t => t.Name == "MainEngine");

            if (baseEngineType == null)
            {
                CML_Bootstrapper.logger.LogError($"Could not find Engine.MainEngine base type in {engineAsm.GetName().Name}!");
                return;
            }

            MethodInfo updatePrefix = typeof(ChiselHarmonyPatches).GetMethod(nameof(MainEngineUpdatePrefix), BindingFlags.Public | BindingFlags.Static)!;

            var targetTypes = engineAsm.GetTypes()
                .Where(t => baseEngineType.IsAssignableFrom(t) && !t.IsAbstract);

            int patchedCount = 0;
            foreach (var type in targetTypes)
            {
                MethodInfo? updateMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == "Update" && m.GetParameters().Length == 1 && m.DeclaringType == type);

                if (updateMethod != null)
                {
                    _harmony.Patch(
                        original: updateMethod,
                        prefix: new HarmonyMethod(updatePrefix)
                    );
                    patchedCount++;
                }
            }

            CML_Bootstrapper.logger.LogDebug($"Patched Update across {patchedCount} MainEngine class(es) in {engineAsm.GetName().Name}.");
        }

        public static bool MainEngineUpdatePrefix(object __instance)
        {
            foreach (var mod in CML_Bootstrapper.Mods)
            {
                try
                {
                    mod.Update();
                }
                catch (Exception ex)
                {
                    CML_Bootstrapper.logger.LogError($"Error in mod {mod.GetType().Name}.Update(): {ex.Message}");
                }
            }

            return true;
        }

        //runs before compileentitydata and adds my asms not just the game asm
        public static void CompileEntityDataPatch(Assembly gameAssembly, string path)
        {
            CML_Bootstrapper.logger.LogDebug("Patching in mods");
            MethodInfo compileMethod = AccessTools.Method("Engine.Compilation.EntityCompiler:CompileAllEntities");

            if (compileMethod == null)
            {
                CML_Bootstrapper.logger.LogError("failed to find compiler method");
                return;
            }
            CML_Bootstrapper.logger.LogDebug("found compiler");
            foreach (var asm in CML_Bootstrapper.ModAsms)
            {
                CML_Bootstrapper.logger.LogDebug($"adding asm {asm.FullName}");
                compileMethod.Invoke(null, [asm, asm.Location, false]);
            }
        }
    }
}