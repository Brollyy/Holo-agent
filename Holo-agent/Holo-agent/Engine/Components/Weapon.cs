using Engine.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.Components
{
    class Weapon : Component
    {
        private WeaponTypes weaponType;
        private int magazine, ammo, magazineCapacity, ammoCapacity;
        private bool isArmed = false, isLocked = false, gunfire = false;
        private float range;
        public string info;
        float machineGunTimer = 100;
        const float MACHINE_GUN_TIMER = 100;
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
        }
        public void shoot(GameTime gameTime)
        {
            float? closest = null;
            GameObject closestGameObject = null;
            Owner.Parent.GetComponent<PlayerController>().Ray(ref closestGameObject, ref closest, range);
            if (weaponType == WeaponTypes.MachineGun && magazine > 0)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                machineGunTimer -= deltaTime;
                if (machineGunTimer < 0)
                {
                    magazine--;
                    gunfire = true;
                    if (closestGameObject != null)
                    {
                        info = closestGameObject.Name + " " + closest;
                    }
                    machineGunTimer = MACHINE_GUN_TIMER;
                }
            }
            if (weaponType == WeaponTypes.Pistol && isLocked == false && magazine > 0)
            {
                magazine--;
                gunfire = true;
                if (closestGameObject != null)
                {
                    info = closestGameObject.Name + " " + closest;
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
