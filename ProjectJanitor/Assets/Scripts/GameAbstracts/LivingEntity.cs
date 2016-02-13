/*
    Optional DIRECTIVES : (not required to work well)  
       - Commented as SOUND for every line calling an optional audiosource.
       - Commented as UI or GUI for every line about UI/GUI
       - Commented as ANIM for animator variable trigger/param update.
    No Required Directive.
*/
using UnityEngine;
using System.Collections;
using GalacticJanitor.UI;
using System;

namespace GalacticJanitor.Game
{
    public abstract class LivingEntity : Entity
    {

        [System.Serializable]
        public class EntityBook
        {
            public bool alive = true;

            [Tooltip("Current Health point")]
            public int health = 20;

            [Tooltip("The maximum healh point")]
            public int maxHealth = 20;

            [Tooltip("Current armor point")]
            public int armor = 20;

            [Tooltip("The maximum armor Points")]
            public int maxArmor = 20;

            [Tooltip("Damages are divided by this number if the entity has at least one armor point")]
            public int armorDamageReduction = 2;
        }

        [Header("Living Behavior")]
        [SerializeField]
        public EntityBook m_entity;

        public bool destroyOnDeath = false;
        public bool saveOnDeath = true;
        public bool invincible = false;

        [Header("Animation")]
        [Tooltip("Animator passed as reference should have a trigger called 'hitted' to fire damage animation")]
        public Animator optionalAnimator;

        [Header("GUI")]
        public EntityResourceDisplay optionalDisplay;

        [Header("Sounds")]
        public AudioSource onDamageSound;
        public AudioSource onDieSound;
        public AudioSource onArmorBreakSound;
        public AudioSource onHealSound;
        public AudioSource onRepairSound;

        protected virtual void Start()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Inflict the specified amount of damage to this entity. If the entity health falls to 0, the entity die.
        /// <br>
        /// Damage are divided by armorDamageReduction
        /// </br>
        /// </summary>
        /// <param name="damage">health point to loose</param>
        public void TakeDirectDamage(int damage)
        {
            if (invincible) return;

            if (m_entity.alive)
            {
                if (m_entity.armor > 0)
                {
                    if (damage <= m_entity.armor)
                    {
                        m_entity.armor -= damage;
                        damage /= m_entity.armorDamageReduction;
                    }
                    else
                    {
                        //the damage is greater than armor points so we can take this value to determine how much damage need to be reduced 
                        int reduced = m_entity.armor / m_entity.armorDamageReduction;

                        //The rest represent the full damage to take...
                        int fullDamage = damage - m_entity.armor;

                        m_entity.armor = 0;

                        //Now we can calculate the damage.
                        damage = reduced + fullDamage;

                        /*SOUND*/
                        if (onArmorBreakSound)
                            onArmorBreakSound.Play();
                    }
                }

                m_entity.health -= damage;

                /*SOUND*/
                if (onDamageSound)
                    onDamageSound.Play();

                /*ANIM*/
                if (optionalAnimator)
                    optionalAnimator.SetTrigger("hitted");

                if (m_entity.health <= 0)
                {
                    m_entity.health = 0;
                    Die();
                }

                /*GUI*/
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Heal the entity with the given amount. (result cannot exceed max Health value)
        /// </summary>
        /// <param name="amount">Health point to gain</param>
        /// <returns>false if the current health value is already at it's maximum</returns>
        public bool Heal(int amount)
        {
            if (m_entity.health >= m_entity.maxHealth)
                return false;

            m_entity.health += amount;
            if (m_entity.health > m_entity.maxHealth)
                m_entity.health = m_entity.maxHealth;

            /*SOUND*/
            if (onHealSound)
                onHealSound.Play();

            /*GUI*/
            UpdateDisplay();

            return true;
        }

        /// <summary>
        /// Heal this entity completely.
        /// </summary>
        /// <returns>false if the current health value is already at it's maximum</returns>
        public bool Heal()
        {
            return Heal(m_entity.maxHealth);
        }

        /// <summary>
        /// Repair the armor points of this entity by the given amount. (result cannot exceed max ArmorPoint value)
        /// </summary>
        /// <returns>false if the current AP value is already at it's maximum</returns>
        public bool RepairArmor(int amount)
        {
            if (m_entity.armor >= m_entity.maxArmor)
                return false;

            m_entity.armor += amount;

            if (m_entity.armor > m_entity.maxArmor)
                m_entity.armor = m_entity.maxArmor;

            /*SOUND*/
            if (onRepairSound)
                onRepairSound.Play();

            /*GUI*/
            UpdateDisplay();
            return true;
        }

        /// <summary>
        /// Repair the armor points to maximum.
        /// </summary>
        /// <returns>false if the current AP value is already at it's maximum</returns>
        public bool RepairArmor()
        {
            return RepairArmor(m_entity.maxArmor);
        }

        /// <summary>
        /// Kill the entity. Destroy the gameoject if destroyOnDeath is set to true
        /// </summary>
        void Die()
        {
            m_entity.alive = false;

            if (saveOnDeath) Save();

            /*SOUND*/
            if (onDieSound)
                onDieSound.Play();

            /*GUI*/
            UpdateDisplay();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Instant kill this entity. Don't overpass destroyOnDeath behavior
        /// </summary>
        public void Kill()
        {
            Kill(false);
        }

        /// <summary>
        /// /// Instant kill this entity. 
        /// </summary>
        /// <param name="forceDestroyOnDeath">force to destroy on death if true</param>
        public void Kill(bool forceDestroyOnDeath)
        {
            if (forceDestroyOnDeath) destroyOnDeath = true;
            Die();
        }

        /// <summary>
        /// Kill this entity over the given time in seconds. Don't overpass destroyOnDeath behavior
        /// </summary>
        /// <param name="delay"></param>
        public void Kill(float delay)
        {
            Invoke("Kill", delay);
        }

        /// <summary>
        /// Kill this entity over the given time in seconds.
        /// </summary>
        /// <param name="forceDestroyOnDeath">force to destroy on death if true</param>
        /// <param name="delay">Delay in seconds</param>
        public void Kill(bool forceDestroyOnDeath, float delay)
        {
            if (forceDestroyOnDeath)
                StartCoroutine(KillAfterDelay(delay));
            else Kill(delay);
        }

        /// <summary>
        /// Kill couroutine for force destroy + delay...
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        IEnumerator KillAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Kill(true);
        }

        /*GUI*/
        /// <summary>
        /// To call when a GUI update is needed...
        /// </summary>
        public void UpdateDisplay()
        {
            if (optionalDisplay) optionalDisplay.UpdateState(this);
        }
    } 
}