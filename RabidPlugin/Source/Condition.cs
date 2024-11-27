using Dalamud.Game.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RabidPlugin
{
    public enum DataType
    {
        Unused,
        Category,
        UInt,
        Float,
        String,
        Count
    }

    public enum OperatorType
    {
        None,
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Count,
    }

    public unsafe class DataSource
    {
        public string Name;
        public DataType Type;
        public byte* Data;
    }

    public class Condition
    {
        public DataSource A;
        public OperatorType Comparitor;

    }
}
