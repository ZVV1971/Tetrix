using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Helper;

namespace TetrisTests.Serializer
{
    class SerializerTest
    {
        List<Tuple<string, int, int, DateTime>> list;

        [SetUp]
        public void Setup()
        {
            list = new List<Tuple<string, int, int, DateTime>>();
            list.Add(new Tuple<string, int, int, DateTime>("string1", 34534, 5, DateTime.Now));
        }

        [TestCase, Description("Test whether Serialization of List of Tuples works")]
        public void TestSerializeListOfTuples()
        {
            var d = ObjectSerialize.Serialize(list);
            var c = ObjectSerialize.DeSerialize(d);
            Assert.AreEqual(list, c);
        }
    }
}
