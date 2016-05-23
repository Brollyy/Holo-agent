using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Animation;
using Engine.Components;

namespace Engine.Utilities
{
    /// <summary>
    /// An encloser for an XNA model that we will use that includes support for
    /// bones, animation, and some manipulations.
    /// </summary>
    public class AnimatedModel
    {
        #region Fields

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        private Model model = null;

        /// <summary>
        /// Extra data associated with the XNA model
        /// </summary>
        private ModelExtra modelExtra = null;

        /// <summary>
        /// The model bones
        /// </summary>
        private List<Bone> bones = new List<Bone>();

        #endregion

        #region Properties

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        public Model Model
        {
            get { return model; }
        }

        /// <summary>
        /// The underlying bones for the model
        /// </summary>
        public List<Bone> Bones { get { return bones; } }

        /// <summary>
        /// The model animation clips
        /// </summary>
        public List<AnimationClip> Clips { get { return modelExtra.Clips; } }

        #endregion

        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates the model from an XNA model
        /// </summary>
        /// <param name="model">The underlying model</param>
        public AnimatedModel(Model model)
        {
            this.model = model;
            modelExtra = this.model.Tag as ModelExtra;

            ObtainBones();
        }

        #endregion

        #region Bones Management

        /// <summary>
        /// Get the bones from the model and create a bone class object for
        /// each bone. We use our bone class to do the real animated bone work.
        /// </summary>
        private void ObtainBones()
        {
            bones.Clear();
            foreach (ModelBone bone in model.Bones)
            {
                // Create the bone object and add to the heirarchy
                Bone newBone = new Bone(bone.Name, bone.Transform, bone.Parent != null ? bones[bone.Parent.Index] : null);

                // Add to the bones for this model
                bones.Add(newBone);
            }
        }

        /// <summary>
        /// Find a bone in this model by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Bone FindBone(string name)
        {
            foreach(Bone bone in Bones)
            {
                if (bone.Name == name)
                    return bone;
            }

            return null;
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw the model
        /// </summary>
        /// <param name="gameTime">The snapshot of time during drawing.</param>
        public void Draw(GameTime gameTime, GameObject owner, Vector3 offset)
        {
            if (model == null)
                return;

            //
            // Compute all of the bone absolute transforms
            //

            Matrix[] boneTransforms = new Matrix[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                Bone bone = bones[i];
                bone.ComputeAbsoluteTransform();

                boneTransforms[i] = bone.AbsoluteTransform;
            }

            //
            // Determine the skin transforms from the skeleton
            //

            Matrix[] skeleton = null;
            if (modelExtra != null)
            {
                skeleton = new Matrix[modelExtra.Skeleton.Count];
                for (int s = 0; s < modelExtra.Skeleton.Count; s++)
                {
                    Bone bone = bones[modelExtra.Skeleton[s]];
                    skeleton[s] = bone.SkinTransform * bone.AbsoluteTransform;
                }
            }

            Camera camera = owner.Scene.Camera.GetComponent<Camera>();
            // Draw the model.
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (Effect effect in modelMesh.Effects)
                {
                    if (effect is BasicEffect)
                    {
                        BasicEffect beffect = effect as BasicEffect;
                        beffect.World = /*boneTransforms[modelMesh.ParentBone.Index] */ owner.LocalToWorldMatrix;
                        beffect.View = camera.ViewMatrix;
                        beffect.Projection = camera.ProjectionMatrix;
                        beffect.EnableDefaultLighting();
                        beffect.PreferPerPixelLighting = true;
                    }

                    if (effect is SkinnedEffect)
                    {
                        SkinnedEffect seffect = effect as SkinnedEffect;
                        seffect.World = Matrix.CreateTranslation(offset) * boneTransforms[modelMesh.ParentBone.Index] * owner.LocalToWorldMatrix;
                        seffect.View = camera.ViewMatrix;
                        seffect.Projection = camera.ProjectionMatrix;
                        seffect.EnableDefaultLighting();
                        seffect.PreferPerPixelLighting = true;
                        if(skeleton != null) seffect.SetBoneTransforms(skeleton);
                    }
                }

                modelMesh.Draw();
            }
        }


        #endregion

    }
}
