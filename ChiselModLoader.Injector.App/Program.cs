using ChiselModLoader.Shared;
using System.Reflection;
using System.Runtime.InteropServices;
using ChiselModLoader.Shared.Ini;
namespace ChiselModLoader.Injector.App
{
    internal class Program
    {
        static CMLCFG LiveCMLCFG = null!;
        static Assembly CMLAsm = null!;
        static Assembly GameAsm = null!;

        static void Main(string[] args)
        {
            Console.WriteLine("Looking for CMLInjector.cfg");
            LiveCMLCFG = ReadOrCreateCMLCFG();
            Console.WriteLine("done grabbing CMLInjector.cfg");
            CMLAsm = Assembly.LoadFrom(CMLGlobals.GetFullPath(LiveCMLCFG.InjectedDllPath));
            Console.WriteLine("loaded cml asm");

            
            GameAsm = Assembly.LoadFrom(CMLGlobals.GetFullPath(LiveCMLCFG.CMLGameDll));
            Console.WriteLine("Loaded GameAsm");
            Console.WriteLine("Loading native libs");
            //game asm needs some native dlls so we load them here
            LoadNativeLibs();

            
            Type? runtimeType = CMLAsm.GetType("ChiselModLoader.Runtime.CML_Bootstrapper");

            if (runtimeType == null)
            {
                throw new Exception("Could not find ChiselModLoader.Runtime.CML_Bootstrapper class!");
            }
            MethodInfo? loaderMain = runtimeType.GetMethod("Main",
                BindingFlags.Static | BindingFlags.NonPublic);

            if (loaderMain == null)
            {
                throw new Exception("Could not find the static Main method inside CML_Bootstrapper!");
            }
            object[] parameters = new object[] { GameAsm };
            loaderMain.Invoke(null, parameters);
            Console.WriteLine("program ended");
        }
        static void LoadNativeLibs()
        {
            if(!Directory.Exists(Path.Combine(CMLGlobals.basedir,"runtimes")))
            {
                Console.WriteLine("the folder runtimes does not exist, could not load runtimes");
                return;
            }
            if(!Directory.Exists(Path.Combine(CMLGlobals.basedir, "runtimes",RuntimeInformation.RuntimeIdentifier)))
            {
                Console.WriteLine("target runtime not found in runtime directory, could not load runtimes");
                return;
            }
            string nativepathdir = Path.Combine(CMLGlobals.basedir, "runtimes", RuntimeInformation.RuntimeIdentifier, "native");
            if (!Directory.Exists(nativepathdir))
            {
                Console.WriteLine("target runtime native libs could not be found in runtime directory, could not load runtimes");
                return;
            }
            Console.WriteLine("target runtimes found");
            foreach(var file in Directory.GetFiles(nativepathdir))
            {
                Console.WriteLine($"loading native lib {file}");
                NativeLibrary.Load(file);
            }
        }
        static CMLCFG ReadOrCreateCMLCFG()
        {
            if (File.Exists(Path.Combine(CMLGlobals.basedir, "CMLInjector.cfg")))
            {

                var cmlcfg = ReadCMLCFG();
                if (!IsCMLCFGValid(cmlcfg))
                {
                    return CreateCMLCFG();
                }
                return cmlcfg;
            }
            return CreateCMLCFG();
        }
        static CMLCFG ReadCMLCFG()
        {
            var cfg =  new CMLCFG();
            IniFileReader reader = new IniFileReader(Path.Combine(CMLGlobals.basedir,"CMLInjector.cfg"));
            cfg.CMLVersion = Int64.Parse(reader.Get("Injector", "CMLVersion", "-1000")); //this tells it to replace the file if it cant find it
            cfg.InjectedDllPath = reader.Get("Injector", "InjectedDllPath", "");
            cfg.CMLGameDll = reader.Get("Injector", "CMLGameDll", "");
            return cfg;
        }
        static bool IsCMLCFGValid(CMLCFG CMLCFG)
        {
            if(CMLCFG == null)
            {
                return false;
            }
            if(CMLCFG.CMLVersion != CMLGlobals.CMLVersion)
            {
                return false;
            }
            return true;
        }
        static CMLCFG CreateCMLCFG()
        {
            CMLCFG cmlcfg = new CMLCFG();
            cmlcfg.CMLVersion = CMLGlobals.CMLVersion;
            cmlcfg.InjectedDllPath = @"ChiselModLoader.Runtime.dll";
            //they way we find the exe name is by seeing what exes are there and finding the one that isnt us
            var files = Directory.GetFiles(CMLGlobals.basedir);
            string? ExePath = null;
            foreach (var fullfile in files)
            {
                string file = Path.GetFileName(fullfile);
                if (Path.GetExtension(file) != ".exe")
                {
                    continue;
                }
                if (file.StartsWith("ChiselModLoader"))
                {
                    continue;
                }
                ExePath = file;
            }
            if(ExePath == null)
            {
                Console.WriteLine("ERROR:Could not find Chisel game please select the chisel game exe");
                bool HasSelectedExe = false;
                while(!HasSelectedExe)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        Console.WriteLine($"{i}.{Path.GetFileName(files[i])}");
                    }
                    Console.Write("Please Select the number that is the games exe file:");
                    string? SelectedExeS = Console.ReadLine();
                    if(SelectedExeS == null)
                    {
                        continue;
                    }

                    if(!int.TryParse(SelectedExeS,out var SelectedExe))
                    {
                        continue;
                    }

                    if (SelectedExe >= 0 && SelectedExe < files.Length)
                    {
                        HasSelectedExe = true;
                        ExePath = files[SelectedExe];
                    }
                }


            }
            cmlcfg.CMLGameDll = Path.GetFileNameWithoutExtension(ExePath) + ".dll";
            //writing it to disk
            var writer = new IniFileWriter();
            writer.Add("Injector", "CMLVersion",cmlcfg.CMLVersion);
            writer.Add("Injector", "CMLGameDll", cmlcfg.CMLGameDll);
            writer.Add("Injector", "InjectedDllPath", cmlcfg.InjectedDllPath);
            writer.Save("CMLInjector.cfg");
            return cmlcfg;
        }
    }
}
