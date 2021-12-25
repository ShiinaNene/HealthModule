using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthModule
{
    [FixedNetworkID(32761)]
    public class NMHealthModuleHit : NMEvent
    {
        public byte index;
        public float damage;
        public float stuned;
        public float hSpeed;
        public bool bypassShield;
        public NMHealthModuleHit() { }

        public NMHealthModuleHit(byte index, float damage, float stuned, float hSpeed, bool bypassShield = false)
        {
            this.index = index;
            this.damage = damage;
            this.stuned = stuned;
            this.hSpeed = hSpeed;
            this.bypassShield = bypassShield;
        }

        protected override void OnSerialize()
        {
            base.OnSerialize();
            _serializedData.Write(index);
            _serializedData.Write(damage);
            _serializedData.Write(stuned);
            _serializedData.Write(hSpeed);
            _serializedData.Write(bypassShield);

        }
        public override void OnDeserialize(BitBuffer msg)
        {
            base.OnDeserialize(msg);
            index = msg.ReadByte();
            damage = msg.ReadFloat();
            stuned = msg.ReadFloat();
            hSpeed = msg.ReadFloat();
            bypassShield = msg.ReadBool();
        }
        public override void Activate()
        {
            base.Activate();
            DuckNetwork.profiles[index]?.duck?.GetManager()?.Hit(damage, stuned, hSpeed, bypassShield);
        }
    }
}

