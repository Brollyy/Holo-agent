using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public class Weapon : Component
    {
        private SoundEffect gunshotSound;
        [DataMember]
        private WeaponTypes weaponType;
        [DataMember]
        private int magazine;
        [DataMember]
        private int ammo;
        [DataMember]
        private int magazineCapacity;
        [DataMember]
        private int ammoCapacity;
        [DataMember]
        private bool isArmed;
        private bool isLocked, gunfire;
        [DataMember]
        private bool collision;
        [DataMember]
        private float range;
        [DataMember]
        private readonly Vector3 asChildPosition;
        public string info;
        float machineGunTimer = 100;
        float timer = 0;
        const float MACHINE_GUN_TIMER = 100;

        public bool IsArmed
        {
            get
            {
                return isArmed;
            }
            set
            {
                isArmed = value;
            }
        }
        public bool IsLocked
        {
            get { return isLocked; }
        }
        public bool Collision
        {
            get
            {
                return collision;
            }
            set
            {
                collision = value;
                Collider col = Owner.GetComponent<Collider>();
                if (col != null) col.Enabled = value;
            }
        }
        public Vector3 AsChildPosition
        {
            get
            {
                return asChildPosition;
            }
        }
        public SoundEffect GunshotSound
        {
            get { return gunshotSound; }
            set { gunshotSound = value; }
        }
        public Weapon(WeaponTypes weaponType, int magazine, int ammo, int magazineCapacity, int ammoCapacity, float range, Vector3 asChildPosition)
        {
            this.weaponType = weaponType;
            this.magazine = magazine;
            if (magazineCapacity < magazine)
                this.magazineCapacity = magazine;
            else
                this.magazineCapacity = magazineCapacity;
            this.ammo = ammo;
            if (ammoCapacity < ammo)
                this.ammoCapacity = ammo;
            else
                this.ammoCapacity = ammoCapacity;
            this.range = range;
            this.asChildPosition = asChildPosition;
            this.gunshotSound = null;
            isArmed = false;
            isLocked = false;
            gunfire = false;
            collision = true;
        }
        public void shoot()
        {
            GameObject gameObject = null;
            float? distance = null;
            if (Owner.Parent.GetComponent<CharacterController>() != null)
            {
                gameObject = Owner.Parent.GetComponent<CharacterController>().ClosestObject;
                distance = Owner.Parent.GetComponent<CharacterController>().ClosestObjectDistance;
            }
            if (weaponType == WeaponTypes.MachineGun && !isLocked && magazine > 0)
            {
                if (!isLocked)
                {
                    magazine--;
                    if (gunshotSound != null) gunshotSound.Play();
                    gunfire = true;
                    if (gameObject != null && distance != null && distance <= 1000.0f)
                    {
                        CharacterController contr = gameObject.GetComponent<CharacterController>();
                        if (contr != null) contr.DealDamage(10, this);
                        info = gameObject.Name + " " + distance;
                    }
                    machineGunTimer = MACHINE_GUN_TIMER;
                    isLocked = true;
                }
            }
            if (weaponType == WeaponTypes.Pistol && isLocked == false && magazine > 0)
            {
                magazine--;
                if (gunshotSound != null) gunshotSound.Play();
                gunfire = true;
                if (gameObject != null && distance <= 1000.0f)
                {
                    CharacterController contr = gameObject.GetComponent<CharacterController>();
                    if (contr != null) contr.DealDamage(20, this);
                    info = gameObject.Name + " " + distance;
                }
                isLocked = true;
            }
            if (magazine == 0)
            {
                reload();
            }
        }
        public void zoom()
        {

        }
        public void reload()
        {
            int bulletsToAdd = magazineCapacity - magazine;
            if (ammo < bulletsToAdd)
            {
                bulletsToAdd = ammo;
            }
            ammo -= bulletsToAdd;
            magazine += bulletsToAdd;
        }
        public GameObject getGunfireInstance()
        {
            if (timer <= 0) return null;
            int index = Owner.GetChildren().FindIndex(child => child.GetInactiveComponent<SpriteInstance>() != null);
            if (index != -1)
            {
                return Owner.GetChild(index);
            }
            else
            {
                return null;
            }
        }
        public WeaponTypes getWeaponType()
        {
            return weaponType;
        }
        public int getMagazine()
        {
            return magazine;
        }
        public int getAmmo()
        {
            return ammo;
        }
        public bool getGunfire()
        {
            return gunfire;
        }
        public void setGunfire(bool gunfire)
        {
            this.gunfire = gunfire;
        }
        public void unlockWeapon()
        {
            isLocked = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(timer > 0)
            {
                timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (gunfire)
            {
                timer = 0.01f;
                setGunfire(false);
            }

            if (weaponType == WeaponTypes.MachineGun && isLocked)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                machineGunTimer -= deltaTime;
                if (machineGunTimer < 0.0f) isLocked = false;
            }
        }
    }
    public enum WeaponTypes
    {
        Pistol = 0,
        MachineGun = 1
    }
}
