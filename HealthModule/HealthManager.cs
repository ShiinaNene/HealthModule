using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckGame;

namespace HealthModule
{
    public class HealthManager
    {
        public Duck duck = null;

        public static float MaxHealth = 100f;
        public static float MaxShield = 20f;

        public float health = 100f;
        public float healthCache = 0f;

        public float shield = 20f;
        public float shieldCache = 0f;

        private float _stunTime = 0f;
        public float stunTime
        {
            get => _stunTime;
            set => stunTimeCache = _stunTime = value;
        }
        public float stunTimeCache = 0f;

        public float recoveryHealth = 0f;
        public float recoveryMul = 0.3f;
        public int recoveryTime = 180;

        public float lastStunned = 0f;
        public float lastDamage = 0f;
        public int sinceLastHurt = 180;

        public HealthManager(Duck duck) => this.duck = duck;
        public void Draw()
        {
            if (duck == null || !duck.visible) return;
            if (health == 0 && healthCache == 0 && shieldCache == 0 && shield == 0) return;
            Vec2 TopLeft;
            Vec2 TopRight;
            Vec2 BottomLeft;
            if (duck.ragdoll != null)
            {
                TopLeft = duck.ragdoll.topLeft - new Vec2(12f, 10f);
                TopRight = duck.ragdoll.topRight + new Vec2(12f, -10f);
                BottomLeft = TopLeft + new Vec2(0, 4);
            }
            else if (!duck.dead)
            {
                TopLeft = duck.topLeft - new Vec2(7f, 5f);
                TopRight = duck.topRight + new Vec2(7f, -5f);
                BottomLeft = TopLeft + new Vec2(0, 4);
            }
            else return;
            float length = TopRight.x - TopLeft.x;
            Color color;
            float healthPercent = health / MaxHealth;
            if (healthPercent <= 0.34f) color = Color.Red;
            else if (healthPercent >= 1) color = Color.Gray;
            else color = Color.White;
            Graphics.DrawRect(TopLeft, BottomLeft + new Vec2(length * healthPercent, 0), color, duck.depth);
            Vec2 rightHealthPos = new Vec2(length * healthPercent, 0);
            Vec2 rightShieldPos = Vec2.Zero;
            if (recoveryHealth > 0)
            {
                float recoveryHealthPercent = recoveryHealth / MaxHealth;
                Graphics.DrawRect(TopLeft + rightHealthPos, BottomLeft + new Vec2(length * recoveryHealthPercent + rightHealthPos.x, 0), Color.OrangeRed, duck.depth);
                rightHealthPos.x += length * recoveryHealthPercent;
            }
            if ((healthCache - health - recoveryHealth) > 0)
            {
                float remainPercent = (healthCache - health - recoveryHealth) / MaxHealth;
                Graphics.DrawRect(TopLeft + rightHealthPos, BottomLeft + new Vec2(length * remainPercent + rightHealthPos.x, 0), Color.OrangeRed, duck.depth);
            }
            if (shield > 0)
            {
                float shieldPercent = shield / MaxShield;
                Graphics.DrawRect(TopLeft + new Vec2(0, -2), TopLeft + new Vec2(length * shieldPercent, 0), Color.SkyBlue, duck.depth);
                rightShieldPos.x = length * shieldPercent;
            }
            if (shieldCache > 0)
            {
                float shieldCachePercent = (shieldCache - shield) / MaxShield;
                Graphics.DrawRect(TopLeft + new Vec2(0, -2) + rightShieldPos, TopLeft + new Vec2(length * shieldCachePercent + rightShieldPos.x, 0), Color.LightSkyBlue, duck.depth);
            }
            if (stunTime > 0 && stunTimeCache > 0)
            {
                float stunPercent = stunTime / stunTimeCache;
                Graphics.DrawRect(TopLeft + new Vec2(0, -4), TopLeft + new Vec2(length * stunPercent, -2), Color.Yellow, duck.depth);
            }
        }

        public void Update()
        {
            if (duck == null) return;
            if (duck.dead && healthCache <= 0 && shieldCache <= 0 && health <= 0 && shield <= 0) return;
            sinceLastHurt++;
            if (sinceLastHurt >= recoveryTime)
            {
                if (recoveryHealth >= 1)
                {
                    health++;
                    recoveryHealth--;
                }
                else
                {
                    health += recoveryHealth;
                    recoveryHealth = 0;
                }

                if (recoveryHealth <= 0 && shield < MaxShield)
                {
                    if(shield <= 0)
                        SFX.Play("laserChargeShort", 1f, Rando.Float(-0.1f, 0.1f), 0f, false);
                    shield += 0.25f;
                }
            }
            if (!Network.isActive || duck.connection == DuckNetwork.localConnection)
                if (duck.onFire)
                {
                    if (Hit(0.3f, 0, 0, true))
                        duck.Kill(new DTIncinerate(duck.lastBurnedBy));
                    duck.burnt = 0f;
                }
            bool shielding = false;
            int speed = 2;
            if (duck.dead)
            {
                speed = 4;
                if (health > 0 || shield > 0)
                {
                    healthCache = health;
                    shieldCache = shield;
                    health = 0;
                    shield = 0;
                }
            }
            if (_stunTime > 0)
            {
                _stunTime--;
                duck.listening = true;
                duck.immobilized = true;
            }
            if (_stunTime <= 0 && stunTimeCache > 0)
            {
                stunTimeCache = 0;
                duck.listening = false;
                duck.immobilized = false;
            }
            if (shieldCache > shield) { shieldCache -= speed; shielding = true; }
            if (healthCache > health && !shielding) healthCache -= speed;
            if (shieldCache <= shield) shieldCache = 0;
            if (healthCache <= health) healthCache = 0;

        }

        public bool Hit(float damage, float stuned, float hSpeed, bool bypassShield = false)
        {

            if (duck == null) return false;
            sinceLastHurt = 0;
            lastDamage = damage;
            stunTime = stuned;
            if (!Network.isActive || duck.connection == DuckNetwork.localConnection)
            {
                if (hSpeed != 0)
                {
                    duck.Swear();
                    duck.hSpeed += hSpeed;
                }
            }
            float tempDamage = damage;
            healthCache = health;
            if (!bypassShield)
            {
                shieldCache = shield;
                if (shield >= tempDamage)
                {
                    shield -= tempDamage;
                    return false;
                }
                else if (shield > 0)
                {
                    tempDamage -= shield;
                    shield = 0;
                    SFX.Play("laserUnchargeShort", 1f, Rando.Float(-0.1f, 0.1f), 0f, false);
                }
            }

            if (health > tempDamage)
            {
                health -= tempDamage;
                recoveryHealth += tempDamage * recoveryMul;
                return false;
            }
            shield = 0;
            health = 0;
            return true;
        }
    }
}
