using Microsoft.VisualStudio.TestTools.UnitTesting;
using TIAEktool.Plc.Types;

namespace TIAEKtoolTest
{
    [TestClass]
    public class PLCTypeTest
    {
        [TestMethod]
        public void TestDataType()
        {
            string left;
            Assert.AreEqual(DataTypeParser.Parse("Int", out left).GetType(), typeof(INT));
            Assert.AreEqual(left, "");

            Assert.AreEqual(DataTypeParser.Parse(" udint jsadl", out left).GetType(), typeof(UDINT));
            Assert.AreEqual(left, " jsadl");

            DataType type = DataTypeParser.Parse(" array [ #start.. \"end\" ] of ulint garbage", out left);
            Assert.AreEqual(type.GetType(), typeof(ARRAY));
            Assert.AreEqual(left, " garbage");
            Assert.AreEqual(((ARRAY)type).Limits[0].LowLimit, new LocalConstant("start"));
            Assert.AreEqual(((ARRAY)type).Limits[0].HighLimit, new GlobalConstant("end"));
            Assert.AreEqual(((ARRAY)type).MemberType.GetType(), typeof(ULINT));

            type = DataTypeParser.Parse(" array [ -3.. 72 ] of struct garbage", out left);
            Assert.AreEqual(type.GetType(), typeof(ARRAY));
            Assert.AreEqual(((ARRAY)type).Limits[0].LowLimit, new IntegerLiteral(-3));
            Assert.AreEqual(((ARRAY)type).Limits[0].HighLimit, new IntegerLiteral(72));
            Assert.AreEqual(((ARRAY)type).MemberType.GetType(), typeof(STRUCT));

            Assert.AreEqual(left, " garbage");


            type = DataTypeParser.Parse(" array [ #start.. \"end\",  -3.. 72 ] of ulint garbage", out left);
            Assert.AreEqual(type.GetType(), typeof(ARRAY));
            Assert.AreEqual(left, " garbage");
            Assert.AreEqual(((ARRAY)type).Limits[0].LowLimit, new LocalConstant("start"));
            Assert.AreEqual(((ARRAY)type).Limits[0].HighLimit, new GlobalConstant("end"));
            Assert.AreEqual(((ARRAY)type).MemberType.GetType(), typeof(ULINT));
            Assert.AreEqual(((ARRAY)type).Limits[1].LowLimit, new IntegerLiteral(-3));
            Assert.AreEqual(((ARRAY)type).Limits[1].HighLimit, new IntegerLiteral(72));

            type = DataTypeParser.Parse(" array [ #start.. \"end\",  -3.. 72 ] of string[10] garbage", out left);
            Assert.AreEqual(type.GetType(), typeof(ARRAY));
            Assert.AreEqual(left, " garbage");
            Assert.AreEqual( "Array[#start..\"end\",-3..72] of String[10]", type.ToString());
        }
    }
}
