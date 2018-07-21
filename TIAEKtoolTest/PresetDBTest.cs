using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PLC.Types;
using TIAEKtool;

namespace TIAEKtoolTest
{
    [TestClass]
    public class PresetDBTest
    {
        [TestMethod]
        public void CreateDB()
        {
            PresetDB db = new PresetDB("Preset", new IntegerLiteral(7));
        }

        [TestMethod]
        public void AddPaths()
        {
            SINT mtype = SINT.Type;
            PresetDB db = new PresetDB("Preset", new IntegerLiteral(7));
            STRUCT type = new STRUCT();
            ARRAY subtype = new ARRAY() { MemberType = new STRING() { Capacity = new IntegerLiteral(6) } };
            subtype.Limits.Add(new ArrayLimits(new IntegerLiteral(-4), new IntegerLiteral(7)));
            type.Members.Add(new StructMember() { Name = "Int", MemberType = UINT.Type });
            type.Members.Add(new StructMember() { Name = "Array", MemberType = subtype });
            PathComponent path = new MemberComponent("Foo", type);
            db.AddPath(path);
           
        }
    }
}
