using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Helper;

namespace TetrisTests.Serializer
{
    internal class SerializerTest
    {
        private IList<Tuple<string, int, int, DateTime>> list;

        [SetUp]
        public void Setup()
        {
            list = new List<Tuple<string, int, int, DateTime>>();
            list.Add(new Tuple<string, int, int, DateTime>("string1", 34534, 5, DateTime.Now));
        }

        [TestCase, Description("Test whether Serialization of List of Tuples works")]
        public void TestSerializeListOfTuples()
        {
            byte[] d = ObjectSerialize.Serialize(list);
            object c = ObjectSerialize.DeSerialize(d);
            Assert.AreEqual(list, c);
        }
    }
}
