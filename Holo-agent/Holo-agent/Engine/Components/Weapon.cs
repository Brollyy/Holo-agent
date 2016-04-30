using Microsoft.Xna.Framework;

namespace Engine.Components
{
    class Weapon : Component
    {
        private WeaponTypes weaponType;
        private int magazine, ammo, magazineCapacity, ammoCapacity;
        private bool isArmed, isLocked, gunfire;
        private float range;
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
        public Weapon(WeaponTypes weaponType, int magazine, int ammo, int magazineCapacity, int ammoCapacity, float range)
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
            isArmed = false;
            isLocked = false;
            gunfire = false;
        }
        public void shoot(GameTime gameTime)
        {
            GameObject gameObject = Owner.Parent.GetComponent<PlayerController>().ClosestObject;
            float? distance = Owner.Parent.GetComponent<PlayerController>().ClosestObjectDistance;
            if (weaponType == WeaponTypes.MachineGun && magazine > 0)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                machineGunTimer -= deltaTime;
                if (machineGunTimer < 0)
                {
                    magazine--;
                    gunfire = true;
                    if (gameObject != null && distance <= 1000.0f)
                    {
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
