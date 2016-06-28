using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public abstract class Interaction : Component
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="go"></param>
        public abstract void Interact(GameObject go);  
        //TODO possibility of informing another GameObjects about done interaction.      
    }
}
