using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class Weapon : Component
    {
        private WeaponTypes weaponType;
        private int magazine, ammo, magazineCapacity, ammoCapacity;
        private bool isArmed, isLocked, gunfire, collision;
        private float range;
        private readonly Vector3 asChildPosition;
        public string info;
        float machineGunTimer = 100;
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
            isArmed = false;
            isLocked = false;
            gunfire = false;
            collision = true;
        }
        public void shoot(GameTime gameTime)
        {
            GameObject gameObject = null;
            float? distance = null;
            if (Owner.Parent.GetComponent<PlayerController>() != null)
            {
                gameObject = Owner.Parent.GetComponent<PlayerController>().ClosestObject;
                distance = Owner.Parent.GetComponent<PlayerController>().ClosestObjectDistance;
            }
            if (weaponType == WeaponTypes.MachineGun && magazine > 0)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                machineGunTimer -= deltaTime;
                if (machineGunTimer < 0)
                {
                    magazine--;
                    gunfire = true;
                    if (gameObject != null && distance != null && distance <= 1000.0f)
                    {
                        CharacterController contr = gameObject.GetComponent<CharacterController>();
                        if (contr != null) contr.DealDamage(15, this);
                        info = gameObject.Name + " " + distance;
                    }
                    machineGunTimer = MACHINE_GUN_TIMER;
                }
            }
            if (weaponType == WeaponTypes.Pistol && isLocked == false && magazine > 0)
            {
                magazine--;
                gunfire = true;
                if (gameObject != null && distance <= 1000.0f)
                {
                    CharacterController contr = gameObject.GetComponent<CharacterController>();
                    if (contr != null) contr.DealDamage(40, this);
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
            int index = Owner.GetChildren().FindIndex(child => child.GetComponent<SpriteInstance>() != null);
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
    }
    public enum WeaponTypes
    {
        Pistol = 0,
        MachineGun = 1
    }
}
