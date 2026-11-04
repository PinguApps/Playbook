# Compare the Beacon depot manifests

Implement Reconciliation.Compare in this prepared console project. Keep its public
Entry, Row and Compare interface so callers can use it unchanged. You may change
its implementation and add helpers. No services or packages are needed.

Produce one Row for every key present in either manifest. A key on both sides
retains both quantities, even when equal. Missing quantities must be null; zero
is a real quantity. Compare keys using ordinal, case-sensitive equality without
trimming or normalization. Return rows sorted by key using ordinal order,
independent of input order. Empty manifests are valid.

Each manifest must have unique keys. Reject duplicates on either side with
ArgumentException before returning a report, even if the repeated entries are
identical or have no counterpart. Keys are non-null, nonempty strings; quantities
are nonnegative integers. Other invalid inputs are outside this task.

Build and exercise the result using the pinned SDK. Choose any correct approach;
there is no required API. Finish within ten wall-clock minutes including checks.
