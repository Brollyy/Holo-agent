using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Audio;

namespace Engine.Components
{
    [DataContract]
    public class KeypadInteraction : Interaction
    {
        [DataMember]
        private string keyCode;
        [DataMember]
        private DoorInteraction door;

        [DataMember]
        public string Passcode { get; set; } = "";

        [IgnoreDataMember]
        public SoundEffect PassSound { get; set; } = null;
        public SoundEffect FailSound { get; set; } = null;

        public KeypadInteraction(string keyCode, DoorInteraction connectedDoor)
        {
            this.keyCode = keyCode;
            this.door = connectedDoor;
        }

        public override void Interact(GameObject go)
        {
            GameMenu.ShowKeypadScreen(this);
        }

        public bool Check()
        {
            if(Passcode.Equals(keyCode))
            {
                if (PassSound != null) PassSound.Play();
                if (door != null) door.IsLocked = false;
                return true;
            }
            else
            {
                if (FailSound != null) FailSound.Play();
                return false;
            }
        }
    }
}
