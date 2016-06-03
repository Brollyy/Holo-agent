﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utilities;
using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class AIController : CharacterController
    {
        protected DecisionTree decisionTree;
        protected List<object> attributes;

        /// <summary>
        /// Override this method with attributes update and then calling this version.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if(decisionTree != null) decisionTree.Update(gameTime, ref attributes);
        }

        public AIController() : base()
        {
            decisionTree = new DecisionTree();
            attributes = new List<object>();
        }

        public AIController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume) :
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            decisionTree = new DecisionTree();
            attributes = new List<object>();
        }
    }
}