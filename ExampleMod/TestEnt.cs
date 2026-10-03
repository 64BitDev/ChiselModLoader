//ents are handles as if you are codding in the engine for example you could just import the engine libs and everything work for example this test ent



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Engine;
using Engine.Compilation;
using Engine.Entities;
using Microsoft.Xna.Framework;

[EntityDescriptor]
public class CustomTestEntity : WorldEntity
{
    internal class CustomTestController : EntityController
    {
        public override void OnSpawn()
        {
            //ExampleMod.ExampleMod._inst.logger.LogDebug("OnSpawn");
        }
        public override void OnDespawn()
        {
            //ExampleMod.ExampleMod._inst.logger.LogDebug("OnDespawn");
        }
        public override void OnUpdate(GameTime gameTime)
        {
            //ExampleMod.ExampleMod._inst.logger.LogDebug("OnUpdate");
        }
        public override void OnRender(GameTime gameTime)
        {
            //ExampleMod.ExampleMod._inst.logger.LogDebug("OnRender");
        }
    }

    public CustomTestEntity()
    {
        base.Controller = new CustomTestController();
    }
}