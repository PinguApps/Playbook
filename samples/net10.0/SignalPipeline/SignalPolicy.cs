using System.Numerics;

namespace Playbook.Samples;

// snippet:policy
public enum SignalState { Finite, DirectedInfinity, Undefined }

public static class SignalPolicy
{
    public static SignalState Classify(Complex result)
    {
        if (double.IsInfinity(result.Real) || double.IsInfinity(result.Imaginary))
            return SignalState.DirectedInfinity;
        if (double.IsNaN(result.Real) || double.IsNaN(result.Imaginary))
            return SignalState.Undefined;
        return SignalState.Finite;
    }
}
// endsnippet:policy
