using System;

namespace ChiselModLoader.Shared
{
    public class CMLCFG
    {
        public Int64 CMLVersion { get; set; } //version of CML this json was created with
        public string CMLGameDll { get; set; } //we need the game dll sense we dont know the name of the game
        public string InjectedDllPath { get; set; } //what ever the modloader passes the dll to this is used to seperate the injector and main dll

        
    }
}