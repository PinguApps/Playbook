I’ll inspect the project, implement `Compare`, then build and check the required edge cases using the pinned SDK.
The implementation is in place. The SDK command was blocked while trying to reach `builds.dotnet.microsoft.com`; I’m checking how to run it entirely from the installed SDK so I can complete verification.
The pinned SDK built the implementation, and all 112 checks passed. They cover empty manifests, zero and missing quantities, equal quantities, duplicate rejection, ordinal behavior across cultures, and randomized input order. I’m removing the temporary harness and running the original console example.
Implemented `Reconciliation.Compare` with the public interface unchanged. It preserves zero and equal quantities, uses null for missing values, rejects duplicate keys, and returns rows in ordinal key order.

Built with the pinned SDK; all 112 checks and the original console example passed.
