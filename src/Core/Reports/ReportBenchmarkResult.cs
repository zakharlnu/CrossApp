namespace Core.Reports;

public sealed record ReportBenchmarkResult(
    int SourceCount,
    int ResultCount,
    TimeSpan WithoutIntermediateToList,
    TimeSpan WithIntermediateToList);
