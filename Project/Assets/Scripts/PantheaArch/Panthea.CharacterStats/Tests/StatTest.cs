using NUnit.Framework;
using R3;

namespace Panthea.CharacterStats
{
    public class StatTest
    {
        public enum STAT_TYPE { POWER, HEALTH, INTELIGENCE, WISDOM, AGILITY, LUCKY }

        [Test]
        public void _00_创建测试()
        {
            var stat = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 100);

            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(100, stat.Value);
        }

        [Test]
        public void _01_加法测试()
        {
            var stat = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 100);

            stat.Add(new Modifier(ModifierType.Add, 5));

            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(105, stat.Value);

            stat.Add(new Modifier(ModifierType.Add, 10));

            Assert.AreEqual(115, stat.Value);
        }

        [Test]
        public void _01_百分比测试()
        {
            var stat = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 100);

            stat.Add(new Modifier(ModifierType.Percent, 0.15f));

            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(115, stat.Value);

            stat.Add(new Modifier(ModifierType.Percent, 0.25f));

            Assert.AreEqual(140, stat.Value);
        }

        [Test]
        public void _02_乘法测试()
        {
            var stat = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 125);

            stat.Add(new Modifier(ModifierType.Multiply, 0.1f));

            Assert.AreEqual(125, stat.BaseValue);
            Assert.AreEqual(137.5f, stat.Value);

            stat.Add(new Modifier(ModifierType.Multiply, 0.1f));
            Assert.AreEqual(151.25f, stat.Value);
        }

        [Test]
        public void _03_综合测试()
        {
            var stat = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 100);

            Assert.AreEqual(100, stat.BaseValue);

            stat.Add(new Modifier(ModifierType.Add, 25));

            Assert.AreEqual(125, stat.Value);

            stat.Add(new Modifier(ModifierType.Percent, 0.15f));

            Assert.AreEqual(143.75f, stat.Value);

            stat.Add(new Modifier(ModifierType.Percent, 0.05f));

            Assert.AreEqual(150f, stat.Value);

            stat.Add(new Modifier(ModifierType.Multiply, 0.05f));

            Assert.AreEqual(157.5f, stat.Value);

            stat.Add(new Modifier(ModifierType.Multiply, 0.15f));

            Assert.AreEqual(181.125f, stat.Value);
        }

        [Test]
        public void _04_继承测试()
        {
            var parent = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 85);
            var stat = new Stat<STAT_TYPE>(parent);

            Assert.AreEqual(85, stat.BaseValue);
            Assert.AreEqual(85, stat.Value);

            parent.Add(new Modifier(ModifierType.Add, 15));

            Assert.AreEqual(100, parent.Value);
            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(100, stat.Value);

            stat.Add(new Modifier(ModifierType.Add, 25));

            Assert.AreEqual(125, stat.Value);

            stat.Add(new Modifier(ModifierType.Percent, 0.15f));

            Assert.AreEqual(143.75f, stat.Value);

            stat.Add(new Modifier(ModifierType.Percent, 0.05f));

            Assert.AreEqual(150f, stat.Value);

            stat.Add(new Modifier(ModifierType.Multiply, 0.05f));

            Assert.AreEqual(157.5f, stat.Value);

            stat.Add(new Modifier(ModifierType.Multiply, 0.15f));

            Assert.AreEqual(181.125f, stat.Value);

            Assert.AreEqual(85, parent.BaseValue);
            Assert.AreEqual(100, parent.Value);
        }

        [Test]
        public void _05_后修正测试()
        {
            var parent = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 0);
            var stat = new Stat<STAT_TYPE>(parent, 100);

            Assert.AreEqual(0, parent.BaseValue);
            Assert.AreEqual(0, parent.Value);
            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(100, stat.Value);

            parent.Add(new Modifier(ModifierType.Percent, 0.25f).SetPost(true));

            Assert.AreEqual(0, parent.BaseValue);
            Assert.AreEqual(0, parent.Value);
            Assert.AreEqual(100, stat.BaseValue);
            Assert.AreEqual(125, stat.Value);

            var parent2 = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 100);
            var stat2 = new Stat<STAT_TYPE>(parent2);

            Assert.AreEqual(100, parent2.BaseValue);
            Assert.AreEqual(100, parent2.Value);
            Assert.AreEqual(100, stat2.BaseValue);
            Assert.AreEqual(100, stat2.Value);

            parent2.Add(new Modifier(ModifierType.Percent, 0.25f).SetPost(true));

            Assert.AreEqual(100, parent2.BaseValue);
            Assert.AreEqual(125, parent2.Value);
            Assert.AreEqual(100, stat2.BaseValue);
            Assert.AreEqual(125, stat2.Value);

            var stat2Changed = false;

            stat2.OnChangeValue.Subscribe(_ => 
            { 
                stat2Changed = true;
            });

            parent2.Add(new Modifier(ModifierType.Percent, 0.25f).SetPost(true));

            Assert.IsTrue(stat2Changed);

            Assert.AreEqual(100, parent2.BaseValue);
            Assert.AreEqual(150, parent2.Value);
            Assert.AreEqual(100, stat2.BaseValue);
            Assert.AreEqual(150, stat2.Value);
        }

        [Test]
        public void _06_事件测试()
        {
            var parent = new Stat<STAT_TYPE>(STAT_TYPE.POWER, 0);
            var stat = new Stat<STAT_TYPE>(parent);

            Assert.AreEqual(0, parent.BaseValue);
            Assert.AreEqual(0, parent.Value);
            Assert.AreEqual(0, stat.BaseValue);
            Assert.AreEqual(0, stat.Value);

            stat.Add(new Modifier(ModifierType.Add, 100f));

            Assert.AreEqual(0, parent.BaseValue);
            Assert.AreEqual(0, parent.Value);
            Assert.AreEqual(0, stat.BaseValue);
            Assert.AreEqual(100, stat.Value);

            var statChanged = false;

            stat.OnChangeValue.Subscribe(s => statChanged = true);

            parent.Add(new Modifier(ModifierType.Percent, 0.25f).SetPost(true));

            Assert.IsTrue(statChanged);
            Assert.AreEqual(0, parent.BaseValue);
            Assert.AreEqual(0, parent.Value);
            Assert.AreEqual(0, stat.BaseValue);
            Assert.AreEqual(125, stat.Value);
        }
    }
}