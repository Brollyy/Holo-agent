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
        private float keySize = 2;
        [DataMember]
        private string keyCode;
        [DataMember]
        private string guess = "";
        [DataMember]
        private DoorInteraction door;

        [IgnoreDataMember]
        public string DisplayState
        {
            get { return guess; }
        }

        [IgnoreDataMember]
        public SoundEffect FailSound { get; set; }
        [IgnoreDataMember]
        public SoundEffect SuccessSound { get; set; }

        public KeypadInteraction(string keyCode, DoorInteraction connectedDoor, float keySize = 2)
        {
            this.keyCode = keyCode;
            this.keySize = keySize;
            this.door = connectedDoor;
        }

        public override void Interact(GameObject go, Vector3 point)
        {
            float x = Vector3.Dot(Owner.LocalToWorldMatrix.Left, point);
            float y = Vector3.Dot(Owner.LocalToWorldMatrix.Up, point);

            int key = 3*(int)((y + 2 * keySize) / keySize) + (int)((x + 1.5f * keySize) / keySize);

            if (key >= 0 && key <= 8 && guess.Length < keyCode.Length) guess += ('1' + key);
            else if (key == 9) guess = "";
            else if (key == 10 && guess.Length < keyCode.Length) guess += '0';
            else if (key == 11)
            {
                if (guess.Equals(keyCode))
                {
                    door.IsLocked = false;
                    SuccessSound.Play();
                }
                else
                {
                    FailSound.Play();
                    guess = "";
                }
            }
        }
    }
}
