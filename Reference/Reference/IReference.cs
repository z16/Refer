using System;

namespace Reference
{
    public interface IReference<TProp, TBase>
    {
        Boolean Valid { get; }
        TProp Value { get; set; }
        TProp ValueOrDefault { get; }
        TBase Model { get; set; }
    }

    public interface IReference<TProp>
    {
        Boolean Valid { get; }
        TProp Value { get; set; }
        TProp ValueOrDefault { get; }
    }

    public interface IReference
    {
        Boolean Valid { get; }
        Object Value { get; set; }
        Object ValueOrDefault { get; }
    }
}
