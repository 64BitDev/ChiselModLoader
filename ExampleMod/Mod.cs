using ChiselModLoader.Runtime;

namespace ExampleMod
{
    [CMLPluginInfo("Example Mod", "1.0.0")]
    public class ExampleMod : CMLMod
    {
        public static ExampleMod _inst;
        public override void Preload()
        {
            logger.LogDebug("mod preload called");
            _inst = this;


        }

        public override void Update()
        {
          
        }
    }
}
