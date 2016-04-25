using Engine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private float timer;
        private const float TIMER = 1;
        private GameObject gunfireObject;
        public SoundEffect pistolShot;
        float machineGunTimer = 100;
        const float MACHINE_GUN_TIMER = 100;
        public Weapon(WeaponTypes weaponType, int magazine, int ammo, int magazineCapacity, int ammoCapacity, float range, GameObject gunfireObject)
        {
            timer = 1;
            this.gunfireObject = gunfireObject;
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

        public override void Update(GameTime gameTime)
        {
            timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(GameTime gameTime)
        {
            if (getGunfire())
            {
                timer = TIMER;
                if (timer >= 0 && gunfireObject != null)
                    gunfireObject.GetComponent<SpriteInstance>().Draw(gameTime);
                setGunfire(false);
                pistolShot.Play();
            }
        }
    }
    public enum WeaponTypes
    {
        Pistol = 0,
        MachineGun = 1
    }
}
