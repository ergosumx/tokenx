```
// * Detailed results *
Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 7.979 ms, StdErr = 0.535 ms (6.71%), N = 19, StdDev = 2.334 ms
Min = 5.203 ms, Q1 = 6.099 ms, Median = 7.483 ms, Q3 = 9.063 ms, Max = 13.307 ms
IQR = 2.964 ms, LowerFence = 1.653 ms, UpperFence = 13.508 ms
ConfidenceInterval = [5.879 ms; 10.079 ms] (CI 99.9%), Margin = 2.100 ms (26.31% of Mean)
Skewness = 0.79, Kurtosis = 2.48, MValue = 2.6
-------------------- Histogram --------------------
[ 5.195 ms ;  7.491 ms) | @@@@@@@@@@
[ 7.491 ms ;  9.971 ms) | @@@@@@
[ 9.971 ms ; 11.306 ms) |
[11.306 ms ; 13.602 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 49.067 ms, StdErr = 2.539 ms (5.17%), N = 20, StdDev = 11.355 ms
Min = 36.086 ms, Q1 = 40.088 ms, Median = 47.675 ms, Q3 = 54.834 ms, Max = 74.271 ms
IQR = 14.745 ms, LowerFence = 17.971 ms, UpperFence = 76.951 ms
ConfidenceInterval = [39.206 ms; 58.927 ms] (CI 99.9%), Margin = 9.860 ms (20.10% of Mean)
Skewness = 0.75, Kurtosis = 2.49, MValue = 2.67
-------------------- Histogram --------------------
[34.173 ms ; 46.587 ms) | @@@@@@@@@
[46.587 ms ; 57.568 ms) | @@@@@@@@
[57.568 ms ; 65.049 ms) |
[65.049 ms ; 76.030 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 235.066 ms, StdErr = 6.947 ms (2.96%), N = 19, StdDev = 30.282 ms
Min = 197.909 ms, Q1 = 210.581 ms, Median = 229.085 ms, Q3 = 253.902 ms, Max = 293.083 ms
IQR = 43.321 ms, LowerFence = 145.600 ms, UpperFence = 318.883 ms
ConfidenceInterval = [207.821 ms; 262.311 ms] (CI 99.9%), Margin = 27.245 ms (11.59% of Mean)
Skewness = 0.48, Kurtosis = 1.76, MValue = 2.89
-------------------- Histogram --------------------
[194.793 ms ; 224.583 ms) | @@@@@@@@@
[224.583 ms ; 256.632 ms) | @@@@@@
[256.632 ms ; 269.212 ms) |
[269.212 ms ; 299.002 ms) | @@@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.352 s, StdErr = 0.026 s (1.91%), N = 18, StdDev = 0.109 s
Min = 1.201 s, Q1 = 1.261 s, Median = 1.356 s, Q3 = 1.409 s, Max = 1.585 s
IQR = 0.148 s, LowerFence = 1.039 s, UpperFence = 1.631 s
ConfidenceInterval = [1.249 s; 1.454 s] (CI 99.9%), Margin = 0.102 s (7.57% of Mean)
Skewness = 0.51, Kurtosis = 2.19, MValue = 4
-------------------- Histogram --------------------
[1.184 s ; 1.294 s) | @@@@@@@
[1.294 s ; 1.346 s) | @
[1.346 s ; 1.456 s) | @@@@@@@
[1.456 s ; 1.506 s) | @
[1.506 s ; 1.615 s) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 902.113 us, StdErr = 41.485 us (4.60%), N = 19, StdDev = 180.827 us
Min = 645.950 us, Q1 = 769.650 us, Median = 894.150 us, Q3 = 1,027.900 us, Max = 1,354.750 us
IQR = 258.250 us, LowerFence = 382.275 us, UpperFence = 1,415.275 us
ConfidenceInterval = [739.425 us; 1,064.801 us] (CI 99.9%), Margin = 162.688 us (18.03% of Mean)
Skewness = 0.56, Kurtosis = 2.78, MValue = 2
-------------------- Histogram --------------------
[  557.007 us ;   659.107 us) | @
[  659.107 us ;   879.807 us) | @@@@@@@@
[  879.807 us ; 1,057.693 us) | @@@@@@@@
[1,057.693 us ; 1,265.807 us) | @
[1,265.807 us ; 1,443.693 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 662.847 us, StdErr = 36.549 us (5.51%), N = 19, StdDev = 159.312 us
Min = 500.900 us, Q1 = 554.900 us, Median = 603.900 us, Q3 = 722.950 us, Max = 1,094.600 us
IQR = 168.050 us, LowerFence = 302.825 us, UpperFence = 975.025 us
ConfidenceInterval = [519.516 us; 806.178 us] (CI 99.9%), Margin = 143.331 us (21.62% of Mean)
Skewness = 1.21, Kurtosis = 3.49, MValue = 2
-------------------- Histogram --------------------
[  480.690 us ;   637.410 us) | @@@@@@@@@@@@
[  637.410 us ;   743.290 us) | @@
[  743.290 us ;   900.010 us) | @@@@
[  900.010 us ; 1,016.240 us) |
[1,016.240 us ; 1,172.960 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 16.542 ms, StdErr = 0.293 ms (1.77%), N = 17, StdDev = 1.209 ms
Min = 14.133 ms, Q1 = 15.851 ms, Median = 16.430 ms, Q3 = 17.197 ms, Max = 19.441 ms
IQR = 1.347 ms, LowerFence = 13.831 ms, UpperFence = 19.217 ms
ConfidenceInterval = [15.365 ms; 17.719 ms] (CI 99.9%), Margin = 1.177 ms (7.12% of Mean)
Skewness = 0.45, Kurtosis = 3.21, MValue = 2.22
-------------------- Histogram --------------------
[14.123 ms ; 15.571 ms) | @@
[15.571 ms ; 16.805 ms) | @@@@@@@@@@
[16.805 ms ; 18.229 ms) | @@@@
[18.229 ms ; 18.824 ms) |
[18.824 ms ; 20.058 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Extra(...)kens) [25]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 9.226 ms, StdErr = 0.099 ms (1.08%), N = 18, StdDev = 0.422 ms
Min = 8.539 ms, Q1 = 8.892 ms, Median = 9.221 ms, Q3 = 9.433 ms, Max = 10.041 ms
IQR = 0.541 ms, LowerFence = 8.081 ms, UpperFence = 10.244 ms
ConfidenceInterval = [8.832 ms; 9.621 ms] (CI 99.9%), Margin = 0.395 ms (4.28% of Mean)
Skewness = 0.26, Kurtosis = 2.16, MValue = 2.89
-------------------- Histogram --------------------
[8.481 ms ;  8.904 ms) | @@@@@
[8.904 ms ;  9.108 ms) | @
[9.108 ms ;  9.531 ms) | @@@@@@@@@
[9.531 ms ; 10.102 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 2.819 ms, StdErr = 0.121 ms (4.30%), N = 18, StdDev = 0.514 ms
Min = 2.236 ms, Q1 = 2.421 ms, Median = 2.652 ms, Q3 = 3.297 ms, Max = 3.844 ms
IQR = 0.875 ms, LowerFence = 1.109 ms, UpperFence = 4.609 ms
ConfidenceInterval = [2.339 ms; 3.300 ms] (CI 99.9%), Margin = 0.481 ms (17.05% of Mean)
Skewness = 0.59, Kurtosis = 1.81, MValue = 2.73
-------------------- Histogram --------------------
[2.211 ms ; 2.727 ms) | @@@@@@@@@@@
[2.727 ms ; 3.117 ms) | @
[3.117 ms ; 3.632 ms) | @@@@@
[3.632 ms ; 4.101 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 9.272 ms, StdErr = 0.144 ms (1.55%), N = 19, StdDev = 0.628 ms
Min = 8.272 ms, Q1 = 8.944 ms, Median = 9.196 ms, Q3 = 9.574 ms, Max = 10.805 ms
IQR = 0.630 ms, LowerFence = 7.999 ms, UpperFence = 10.519 ms
ConfidenceInterval = [8.707 ms; 9.837 ms] (CI 99.9%), Margin = 0.565 ms (6.09% of Mean)
Skewness = 0.69, Kurtosis = 2.98, MValue = 2
-------------------- Histogram --------------------
[ 8.098 ms ;  8.813 ms) | @@@
[ 8.813 ms ;  9.430 ms) | @@@@@@@@@@@
[ 9.430 ms ;  9.733 ms) |
[ 9.733 ms ; 10.350 ms) | @@@@
[10.350 ms ; 11.114 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 105.762 ms, StdErr = 3.207 ms (3.03%), N = 18, StdDev = 13.606 ms
Min = 83.315 ms, Q1 = 96.524 ms, Median = 104.150 ms, Q3 = 114.884 ms, Max = 140.575 ms
IQR = 18.360 ms, LowerFence = 68.984 ms, UpperFence = 142.423 ms
ConfidenceInterval = [93.046 ms; 118.478 ms] (CI 99.9%), Margin = 12.716 ms (12.02% of Mean)
Skewness = 0.58, Kurtosis = 3.15, MValue = 2.22
-------------------- Histogram --------------------
[ 76.501 ms ;  92.482 ms) | @@
[ 92.482 ms ; 106.111 ms) | @@@@@@@@@
[106.111 ms ; 123.391 ms) | @@@@@@
[123.391 ms ; 133.761 ms) |
[133.761 ms ; 147.390 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 413.665 ms, StdErr = 10.306 ms (2.49%), N = 18, StdDev = 43.725 ms
Min = 352.354 ms, Q1 = 375.036 ms, Median = 416.354 ms, Q3 = 450.777 ms, Max = 503.632 ms
IQR = 75.741 ms, LowerFence = 261.425 ms, UpperFence = 564.388 ms
ConfidenceInterval = [372.800 ms; 454.531 ms] (CI 99.9%), Margin = 40.865 ms (9.88% of Mean)
Skewness = 0.17, Kurtosis = 1.9, MValue = 3.5
-------------------- Histogram --------------------
[349.746 ms ; 393.542 ms) | @@@@@@@
[393.542 ms ; 418.492 ms) | @@
[418.492 ms ; 462.289 ms) | @@@@@@@@
[462.289 ms ; 481.733 ms) |
[481.733 ms ; 525.530 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 565.267 us, StdErr = 31.316 us (5.54%), N = 18, StdDev = 132.864 us
Min = 413.300 us, Q1 = 446.300 us, Median = 551.650 us, Q3 = 666.425 us, Max = 810.200 us
IQR = 220.125 us, LowerFence = 116.112 us, UpperFence = 996.612 us
ConfidenceInterval = [441.093 us; 689.440 us] (CI 99.9%), Margin = 124.173 us (21.97% of Mean)
Skewness = 0.39, Kurtosis = 1.61, MValue = 2
-------------------- Histogram --------------------
[400.960 us ; 534.040 us) | @@@@@@@@@
[534.040 us ; 702.840 us) | @@@@@@
[702.840 us ; 842.440 us) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 359.519 us, StdErr = 8.336 us (2.32%), N = 16, StdDev = 33.344 us
Min = 308.900 us, Q1 = 328.075 us, Median = 363.300 us, Q3 = 376.475 us, Max = 421.200 us
IQR = 48.400 us, LowerFence = 255.475 us, UpperFence = 449.075 us
ConfidenceInterval = [325.568 us; 393.470 us] (CI 99.9%), Margin = 33.951 us (9.44% of Mean)
Skewness = 0.1, Kurtosis = 1.82, MValue = 3.14
-------------------- Histogram --------------------
[301.382 us ; 340.932 us) | @@@@@
[340.932 us ; 375.668 us) | @@@@@@@
[375.668 us ; 391.032 us) |
[391.032 us ; 425.768 us) | @@@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 13.063 ms, StdErr = 0.944 ms (7.22%), N = 19, StdDev = 4.113 ms
Min = 8.676 ms, Q1 = 9.978 ms, Median = 11.610 ms, Q3 = 14.942 ms, Max = 21.970 ms
IQR = 4.964 ms, LowerFence = 2.532 ms, UpperFence = 22.388 ms
ConfidenceInterval = [9.362 ms; 16.763 ms] (CI 99.9%), Margin = 3.700 ms (28.33% of Mean)
Skewness = 0.97, Kurtosis = 2.53, MValue = 2.46
-------------------- Histogram --------------------
[ 8.560 ms ; 12.605 ms) | @@@@@@@@@@@@@
[12.605 ms ; 14.283 ms) |
[14.283 ms ; 18.329 ms) | @@@
[18.329 ms ; 22.467 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Long (≈512 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 6.550 ms, StdErr = 0.234 ms (3.58%), N = 18, StdDev = 0.995 ms
Min = 5.361 ms, Q1 = 5.908 ms, Median = 6.263 ms, Q3 = 6.610 ms, Max = 9.401 ms
IQR = 0.702 ms, LowerFence = 4.855 ms, UpperFence = 7.662 ms
ConfidenceInterval = [5.620 ms; 7.480 ms] (CI 99.9%), Margin = 0.930 ms (14.19% of Mean)
Skewness = 1.37, Kurtosis = 4.29, MValue = 2.25
-------------------- Histogram --------------------
[4.863 ms ; 5.714 ms) | @
[5.714 ms ; 6.710 ms) | @@@@@@@@@@@@@
[6.710 ms ; 7.120 ms) |
[7.120 ms ; 8.117 ms) | @@@
[8.117 ms ; 8.903 ms) |
[8.903 ms ; 9.899 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 15.356 ms, StdErr = 0.955 ms (6.22%), N = 19, StdDev = 4.163 ms
Min = 9.635 ms, Q1 = 12.419 ms, Median = 14.310 ms, Q3 = 17.314 ms, Max = 26.241 ms
IQR = 4.895 ms, LowerFence = 5.077 ms, UpperFence = 24.656 ms
ConfidenceInterval = [11.611 ms; 19.102 ms] (CI 99.9%), Margin = 3.745 ms (24.39% of Mean)
Skewness = 0.93, Kurtosis = 3.17, MValue = 2
-------------------- Histogram --------------------
[ 7.588 ms ; 11.341 ms) | @
[11.341 ms ; 15.437 ms) | @@@@@@@@@@@
[15.437 ms ; 20.226 ms) | @@@@@
[20.226 ms ; 24.193 ms) | @
[24.193 ms ; 28.289 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 146.744 ms, StdErr = 2.538 ms (1.73%), N = 18, StdDev = 10.768 ms
Min = 129.748 ms, Q1 = 138.814 ms, Median = 144.513 ms, Q3 = 152.209 ms, Max = 172.414 ms
IQR = 13.395 ms, LowerFence = 118.722 ms, UpperFence = 172.302 ms
ConfidenceInterval = [136.681 ms; 156.807 ms] (CI 99.9%), Margin = 10.063 ms (6.86% of Mean)
Skewness = 0.7, Kurtosis = 2.79, MValue = 4
-------------------- Histogram --------------------
[124.355 ms ; 133.535 ms) | @
[133.535 ms ; 144.320 ms) | @@@@@@@@
[144.320 ms ; 157.073 ms) | @@@@@@@
[157.073 ms ; 163.338 ms) |
[163.338 ms ; 174.123 ms) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 264.245 ms, StdErr = 7.965 ms (3.01%), N = 19, StdDev = 34.719 ms
Min = 221.962 ms, Q1 = 235.887 ms, Median = 253.140 ms, Q3 = 292.196 ms, Max = 329.078 ms
IQR = 56.309 ms, LowerFence = 151.423 ms, UpperFence = 376.660 ms
ConfidenceInterval = [233.009 ms; 295.482 ms] (CI 99.9%), Margin = 31.236 ms (11.82% of Mean)
Skewness = 0.43, Kurtosis = 1.65, MValue = 2.8
-------------------- Histogram --------------------
[220.474 ms ; 254.628 ms) | @@@@@@@@@@
[254.628 ms ; 281.733 ms) | @@
[281.733 ms ; 315.887 ms) | @@@@@@
[315.887 ms ; 346.155 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 2.478 s, StdErr = 0.087 s (3.53%), N = 20, StdDev = 0.391 s
Min = 1.974 s, Q1 = 2.162 s, Median = 2.529 s, Q3 = 2.671 s, Max = 3.365 s
IQR = 0.509 s, LowerFence = 1.398 s, UpperFence = 3.435 s
ConfidenceInterval = [2.138 s; 2.818 s] (CI 99.9%), Margin = 0.340 s (13.71% of Mean)
Skewness = 0.53, Kurtosis = 2.44, MValue = 4.44
-------------------- Histogram --------------------
[1.945 s ; 2.323 s) | @@@@@@@@@
[2.323 s ; 2.462 s) |
[2.462 s ; 2.840 s) | @@@@@@@@@
[2.840 s ; 3.091 s) |
[3.091 s ; 3.470 s) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.233 ms, StdErr = 0.027 ms (2.17%), N = 20, StdDev = 0.120 ms
Min = 1.071 ms, Q1 = 1.148 ms, Median = 1.223 ms, Q3 = 1.335 ms, Max = 1.498 ms
IQR = 0.187 ms, LowerFence = 0.868 ms, UpperFence = 1.615 ms
ConfidenceInterval = [1.129 ms; 1.337 ms] (CI 99.9%), Margin = 0.104 ms (8.42% of Mean)
Skewness = 0.37, Kurtosis = 2.05, MValue = 3.11
-------------------- Histogram --------------------
[1.064 ms ; 1.180 ms) | @@@@@@@@@
[1.180 ms ; 1.264 ms) | @@@
[1.264 ms ; 1.380 ms) | @@@@@@@
[1.380 ms ; 1.441 ms) |
[1.441 ms ; 1.556 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 850.121 us, StdErr = 23.671 us (2.78%), N = 19, StdDev = 103.178 us
Min = 735.900 us, Q1 = 788.750 us, Median = 806.000 us, Q3 = 880.800 us, Max = 1,085.100 us
IQR = 92.050 us, LowerFence = 650.675 us, UpperFence = 1,018.875 us
ConfidenceInterval = [757.293 us; 942.949 us] (CI 99.9%), Margin = 92.828 us (10.92% of Mean)
Skewness = 1.13, Kurtosis = 2.98, MValue = 2.36
-------------------- Histogram --------------------
[  685.150 us ;   762.050 us) | @
[  762.050 us ;   863.550 us) | @@@@@@@@@@@@@
[  863.550 us ;   994.250 us) | @@@
[  994.250 us ; 1,028.900 us) |
[1,028.900 us ; 1,135.850 us) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 16.076 ms, StdErr = 0.200 ms (1.24%), N = 16, StdDev = 0.799 ms
Min = 15.016 ms, Q1 = 15.410 ms, Median = 16.106 ms, Q3 = 16.653 ms, Max = 17.703 ms
IQR = 1.243 ms, LowerFence = 13.544 ms, UpperFence = 18.518 ms
ConfidenceInterval = [15.263 ms; 16.890 ms] (CI 99.9%), Margin = 0.813 ms (5.06% of Mean)
Skewness = 0.27, Kurtosis = 1.88, MValue = 3.71
-------------------- Histogram --------------------
[15.002 ms ; 15.834 ms) | @@@@@@@
[15.834 ms ; 16.243 ms) | @
[16.243 ms ; 17.287 ms) | @@@@@@@
[17.287 ms ; 18.119 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Massi(...)kens) [22]]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 9.882 ms, StdErr = 0.257 ms (2.60%), N = 19, StdDev = 1.119 ms
Min = 8.215 ms, Q1 = 9.243 ms, Median = 9.639 ms, Q3 = 10.311 ms, Max = 12.117 ms
IQR = 1.068 ms, LowerFence = 7.642 ms, UpperFence = 11.912 ms
ConfidenceInterval = [8.875 ms; 10.888 ms] (CI 99.9%), Margin = 1.007 ms (10.19% of Mean)
Skewness = 0.54, Kurtosis = 2.42, MValue = 2
-------------------- Histogram --------------------
[ 7.928 ms ;  9.140 ms) | @@@@
[ 9.140 ms ; 10.241 ms) | @@@@@@@@@@
[10.241 ms ; 11.244 ms) | @@
[11.244 ms ; 12.345 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 737.419 us, StdErr = 9.765 us (1.32%), N = 16, StdDev = 39.062 us
Min = 684.050 us, Q1 = 712.675 us, Median = 731.450 us, Q3 = 760.300 us, Max = 840.750 us
IQR = 47.625 us, LowerFence = 641.237 us, UpperFence = 831.737 us
ConfidenceInterval = [697.647 us; 777.191 us] (CI 99.9%), Margin = 39.772 us (5.39% of Mean)
Skewness = 0.88, Kurtosis = 3.59, MValue = 2
-------------------- Histogram --------------------
[680.054 us ; 720.746 us) | @@@@@@@
[720.746 us ; 770.746 us) | @@@@@@@@
[770.746 us ; 820.404 us) |
[820.404 us ; 861.096 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.424 ms, StdErr = 0.039 ms (2.77%), N = 18, StdDev = 0.168 ms
Min = 1.263 ms, Q1 = 1.323 ms, Median = 1.350 ms, Q3 = 1.442 ms, Max = 1.833 ms
IQR = 0.119 ms, LowerFence = 1.145 ms, UpperFence = 1.621 ms
ConfidenceInterval = [1.267 ms; 1.581 ms] (CI 99.9%), Margin = 0.157 ms (11.00% of Mean)
Skewness = 1.3, Kurtosis = 3.28, MValue = 2.6
-------------------- Histogram --------------------
[1.249 ms ; 1.417 ms) | @@@@@@@@@@@@@
[1.417 ms ; 1.574 ms) | @@
[1.574 ms ; 1.682 ms) |
[1.682 ms ; 1.849 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 18.919 ms, StdErr = 0.301 ms (1.59%), N = 19, StdDev = 1.314 ms
Min = 17.256 ms, Q1 = 17.887 ms, Median = 18.566 ms, Q3 = 19.907 ms, Max = 21.766 ms
IQR = 2.021 ms, LowerFence = 14.855 ms, UpperFence = 22.939 ms
ConfidenceInterval = [17.737 ms; 20.101 ms] (CI 99.9%), Margin = 1.182 ms (6.25% of Mean)
Skewness = 0.53, Kurtosis = 2, MValue = 2.67
-------------------- Histogram --------------------
[16.610 ms ; 17.367 ms) | @
[17.367 ms ; 18.660 ms) | @@@@@@@@@
[18.660 ms ; 19.366 ms) | @@
[19.366 ms ; 20.659 ms) | @@@@@
[20.659 ms ; 21.969 ms) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 38.381 ms, StdErr = 0.590 ms (1.54%), N = 18, StdDev = 2.502 ms
Min = 34.928 ms, Q1 = 36.328 ms, Median = 38.737 ms, Q3 = 39.560 ms, Max = 44.721 ms
IQR = 3.232 ms, LowerFence = 31.479 ms, UpperFence = 44.409 ms
ConfidenceInterval = [36.043 ms; 40.719 ms] (CI 99.9%), Margin = 2.338 ms (6.09% of Mean)
Skewness = 0.65, Kurtosis = 2.96, MValue = 2
-------------------- Histogram --------------------
[34.903 ms ; 37.408 ms) | @@@@@@@@
[37.408 ms ; 38.471 ms) |
[38.471 ms ; 40.977 ms) | @@@@@@@@
[40.977 ms ; 43.468 ms) | @
[43.468 ms ; 45.974 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 150.344 us, StdErr = 1.782 us (1.19%), N = 17, StdDev = 7.347 us
Min = 137.250 us, Q1 = 146.750 us, Median = 150.350 us, Q3 = 152.450 us, Max = 170.250 us
IQR = 5.700 us, LowerFence = 138.200 us, UpperFence = 161.000 us
ConfidenceInterval = [143.190 us; 157.499 us] (CI 99.9%), Margin = 7.154 us (4.76% of Mean)
Skewness = 0.76, Kurtosis = 4.12, MValue = 2
-------------------- Histogram --------------------
[135.000 us ; 142.500 us) | @@
[142.500 us ; 146.350 us) | @
[146.350 us ; 153.850 us) | @@@@@@@@@@@
[153.850 us ; 161.150 us) | @@
[161.150 us ; 166.500 us) |
[166.500 us ; 174.000 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 145.078 us, StdErr = 8.871 us (6.11%), N = 18, StdDev = 37.638 us
Min = 111.700 us, Q1 = 117.550 us, Median = 124.500 us, Q3 = 167.525 us, Max = 222.800 us
IQR = 49.975 us, LowerFence = 42.587 us, UpperFence = 242.488 us
ConfidenceInterval = [109.902 us; 180.253 us] (CI 99.9%), Margin = 35.176 us (24.25% of Mean)
Skewness = 0.85, Kurtosis = 2.14, MValue = 2
-------------------- Histogram --------------------
[102.101 us ; 139.799 us) | @@@@@@@@@@@
[139.799 us ; 185.399 us) | @@@@
[185.399 us ; 233.099 us) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 2.329 ms, StdErr = 0.056 ms (2.40%), N = 19, StdDev = 0.243 ms
Min = 2.101 ms, Q1 = 2.149 ms, Median = 2.225 ms, Q3 = 2.407 ms, Max = 2.918 ms
IQR = 0.258 ms, LowerFence = 1.763 ms, UpperFence = 2.794 ms
ConfidenceInterval = [2.110 ms; 2.548 ms] (CI 99.9%), Margin = 0.219 ms (9.40% of Mean)
Skewness = 1.13, Kurtosis = 2.89, MValue = 2.46
-------------------- Histogram --------------------
[2.100 ms ; 2.339 ms) | @@@@@@@@@@@@@@
[2.339 ms ; 2.581 ms) | @
[2.581 ms ; 2.821 ms) | @@@
[2.821 ms ; 3.037 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Medium (≈128 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.189 ms, StdErr = 0.012 ms (0.99%), N = 16, StdDev = 0.047 ms
Min = 1.121 ms, Q1 = 1.150 ms, Median = 1.183 ms, Q3 = 1.226 ms, Max = 1.283 ms
IQR = 0.076 ms, LowerFence = 1.036 ms, UpperFence = 1.339 ms
ConfidenceInterval = [1.141 ms; 1.237 ms] (CI 99.9%), Margin = 0.048 ms (4.05% of Mean)
Skewness = 0.29, Kurtosis = 1.94, MValue = 2
-------------------- Histogram --------------------
[1.112 ms ; 1.164 ms) | @@@@@
[1.164 ms ; 1.214 ms) | @@@@@@
[1.214 ms ; 1.264 ms) | @@@@
[1.264 ms ; 1.308 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 373.874 us, StdErr = 11.478 us (3.07%), N = 17, StdDev = 47.326 us
Min = 306.050 us, Q1 = 348.450 us, Median = 367.850 us, Q3 = 375.050 us, Max = 498.950 us
IQR = 26.600 us, LowerFence = 308.550 us, UpperFence = 414.950 us
ConfidenceInterval = [327.788 us; 419.959 us] (CI 99.9%), Margin = 46.086 us (12.33% of Mean)
Skewness = 1.38, Kurtosis = 4.31, MValue = 3.14
-------------------- Histogram --------------------
[281.892 us ; 334.492 us) | @
[334.492 us ; 382.808 us) | @@@@@@@@@@@@@
[382.808 us ; 423.708 us) | @
[423.708 us ; 461.792 us) |
[461.792 us ; 510.108 us) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.035 ms, StdErr = 0.079 ms (7.65%), N = 19, StdDev = 0.345 ms
Min = 0.487 ms, Q1 = 0.840 ms, Median = 0.957 ms, Q3 = 1.292 ms, Max = 1.597 ms
IQR = 0.452 ms, LowerFence = 0.162 ms, UpperFence = 1.970 ms
ConfidenceInterval = [0.724 ms; 1.345 ms] (CI 99.9%), Margin = 0.311 ms (30.01% of Mean)
Skewness = -0.18, Kurtosis = 1.64, MValue = 3.5
-------------------- Histogram --------------------
[0.317 ms ; 0.715 ms) | @@@@
[0.715 ms ; 1.054 ms) | @@@@@@
[1.054 ms ; 1.175 ms) |
[1.175 ms ; 1.514 ms) | @@@@@@@@
[1.514 ms ; 1.767 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 5.988 ms, StdErr = 0.136 ms (2.28%), N = 18, StdDev = 0.578 ms
Min = 5.063 ms, Q1 = 5.551 ms, Median = 6.068 ms, Q3 = 6.402 ms, Max = 7.058 ms
IQR = 0.850 ms, LowerFence = 4.276 ms, UpperFence = 7.678 ms
ConfidenceInterval = [5.447 ms; 6.528 ms] (CI 99.9%), Margin = 0.540 ms (9.03% of Mean)
Skewness = 0.02, Kurtosis = 1.81, MValue = 3.25
-------------------- Histogram --------------------
[4.774 ms ; 5.174 ms) | @
[5.174 ms ; 5.753 ms) | @@@@@@
[5.753 ms ; 6.022 ms) | @
[6.022 ms ; 6.601 ms) | @@@@@@@@
[6.601 ms ; 7.212 ms) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 7.307 ms, StdErr = 0.106 ms (1.45%), N = 19, StdDev = 0.461 ms
Min = 6.481 ms, Q1 = 6.956 ms, Median = 7.240 ms, Q3 = 7.494 ms, Max = 8.303 ms
IQR = 0.538 ms, LowerFence = 6.149 ms, UpperFence = 8.301 ms
ConfidenceInterval = [6.892 ms; 7.722 ms] (CI 99.9%), Margin = 0.415 ms (5.67% of Mean)
Skewness = 0.5, Kurtosis = 2.54, MValue = 2.67
-------------------- Histogram --------------------
[6.254 ms ; 6.708 ms) | @
[6.708 ms ; 7.284 ms) | @@@@@@@@@
[7.284 ms ; 7.698 ms) | @@@@@@
[7.698 ms ; 7.918 ms) |
[7.918 ms ; 8.371 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 89.378 us, StdErr = 1.292 us (1.45%), N = 18, StdDev = 5.480 us
Min = 80.450 us, Q1 = 86.225 us, Median = 89.500 us, Q3 = 92.175 us, Max = 98.750 us
IQR = 5.950 us, LowerFence = 77.300 us, UpperFence = 101.100 us
ConfidenceInterval = [84.257 us; 94.499 us] (CI 99.9%), Margin = 5.121 us (5.73% of Mean)
Skewness = 0.04, Kurtosis = 1.96, MValue = 2
-------------------- Histogram --------------------
[77.706 us ;  81.406 us) | @
[81.406 us ;  86.894 us) | @@@@@
[86.894 us ;  93.694 us) | @@@@@@@@@
[93.694 us ; 101.494 us) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 99.889 us, StdErr = 10.533 us (10.54%), N = 18, StdDev = 44.687 us
Min = 42.550 us, Q1 = 55.100 us, Median = 109.300 us, Q3 = 133.075 us, Max = 173.850 us
IQR = 77.975 us, LowerFence = -61.862 us, UpperFence = 250.037 us
ConfidenceInterval = [58.125 us; 141.653 us] (CI 99.9%), Margin = 41.764 us (41.81% of Mean)
Skewness = -0.03, Kurtosis = 1.37, MValue = 3.25
-------------------- Histogram --------------------
[ 30.920 us ;  75.680 us) | @@@@@@@
[ 75.680 us ; 112.070 us) | @@
[112.070 us ; 156.830 us) | @@@@@@@@
[156.830 us ; 196.230 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 926.180 us, StdErr = 35.532 us (3.84%), N = 20, StdDev = 158.902 us
Min = 751.700 us, Q1 = 786.400 us, Median = 875.000 us, Q3 = 1,027.675 us, Max = 1,328.500 us
IQR = 241.275 us, LowerFence = 424.488 us, UpperFence = 1,389.588 us
ConfidenceInterval = [788.196 us; 1,064.164 us] (CI 99.9%), Margin = 137.984 us (14.90% of Mean)
Skewness = 0.82, Kurtosis = 2.75, MValue = 2
-------------------- Histogram --------------------
[  739.766 us ;   932.016 us) | @@@@@@@@@@@
[  932.016 us ; 1,085.684 us) | @@@@@@
[1,085.684 us ; 1,251.666 us) | @@
[1,251.666 us ; 1,405.334 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Short (≈32 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 2.318 ms, StdErr = 0.342 ms (14.74%), N = 20, StdDev = 1.528 ms
Min = 0.506 ms, Q1 = 0.797 ms, Median = 2.364 ms, Q3 = 3.283 ms, Max = 5.839 ms
IQR = 2.486 ms, LowerFence = -2.932 ms, UpperFence = 7.011 ms
ConfidenceInterval = [0.991 ms; 3.645 ms] (CI 99.9%), Margin = 1.327 ms (57.25% of Mean)
Skewness = 0.38, Kurtosis = 2.15, MValue = 3.5
-------------------- Histogram --------------------
[0.110 ms ; 1.588 ms) | @@@@@@@@
[1.588 ms ; 2.721 ms) | @@@
[2.721 ms ; 4.199 ms) | @@@@@@@@
[4.199 ms ; 5.101 ms) |
[5.101 ms ; 6.578 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 399.783 us, StdErr = 10.070 us (2.52%), N = 18, StdDev = 42.722 us
Min = 347.300 us, Q1 = 376.450 us, Median = 387.650 us, Q3 = 414.325 us, Max = 498.500 us
IQR = 37.875 us, LowerFence = 319.637 us, UpperFence = 471.137 us
ConfidenceInterval = [359.855 us; 439.711 us] (CI 99.9%), Margin = 39.928 us (9.99% of Mean)
Skewness = 1.06, Kurtosis = 3.27, MValue = 3
-------------------- Histogram --------------------
[325.904 us ; 348.854 us) | @
[348.854 us ; 400.554 us) | @@@@@@@@@@
[400.554 us ; 443.346 us) | @@@@@
[443.346 us ; 475.154 us) |
[475.154 us ; 519.896 us) | @@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 836.167 us, StdErr = 48.027 us (5.74%), N = 18, StdDev = 203.763 us
Min = 440.850 us, Q1 = 737.925 us, Median = 885.300 us, Q3 = 956.525 us, Max = 1,134.050 us
IQR = 218.600 us, LowerFence = 410.025 us, UpperFence = 1,284.425 us
ConfidenceInterval = [645.732 us; 1,026.601 us] (CI 99.9%), Margin = 190.435 us (22.77% of Mean)
Skewness = -0.73, Kurtosis = 2.23, MValue = 2.5
-------------------- Histogram --------------------
[  427.453 us ;   631.547 us) | @@@@
[  631.547 us ;   847.803 us) | @
[  847.803 us ; 1,051.897 us) | @@@@@@@@@@@@
[1,051.897 us ; 1,236.097 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 3.403 ms, StdErr = 0.096 ms (2.83%), N = 18, StdDev = 0.408 ms
Min = 2.637 ms, Q1 = 3.193 ms, Median = 3.427 ms, Q3 = 3.598 ms, Max = 4.169 ms
IQR = 0.404 ms, LowerFence = 2.587 ms, UpperFence = 4.204 ms
ConfidenceInterval = [3.021 ms; 3.784 ms] (CI 99.9%), Margin = 0.382 ms (11.21% of Mean)
Skewness = -0.1, Kurtosis = 2.38, MValue = 2.6
-------------------- Histogram --------------------
[2.608 ms ; 3.017 ms) | @@@@
[3.017 ms ; 3.300 ms) | @
[3.300 ms ; 3.709 ms) | @@@@@@@@@@
[3.709 ms ; 4.170 ms) | @@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_EncodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]   
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 5.613 ms, StdErr = 0.531 ms (9.47%), N = 20, StdDev = 2.376 ms
Min = 3.579 ms, Q1 = 3.688 ms, Median = 4.217 ms, Q3 = 7.395 ms, Max = 11.505 ms
IQR = 3.707 ms, LowerFence = -1.872 ms, UpperFence = 12.956 ms
ConfidenceInterval = [3.550 ms; 7.676 ms] (CI 99.9%), Margin = 2.063 ms (36.76% of Mean)
Skewness = 0.83, Kurtosis = 2.45, MValue = 3.33
-------------------- Histogram --------------------
[ 2.962 ms ;  5.259 ms) | @@@@@@@@@@@@
[ 5.259 ms ;  6.683 ms) |
[ 6.683 ms ;  8.980 ms) | @@@@@@@
[ 8.980 ms ; 10.357 ms) |
[10.357 ms ; 12.654 ms) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 96.344 us, StdErr = 2.743 us (2.85%), N = 17, StdDev = 11.311 us
Min = 80.750 us, Q1 = 88.650 us, Median = 94.850 us, Q3 = 101.250 us, Max = 125.950 us
IQR = 12.600 us, LowerFence = 69.750 us, UpperFence = 120.150 us
ConfidenceInterval = [85.330 us; 107.358 us] (CI 99.9%), Margin = 11.014 us (11.43% of Mean)
Skewness = 0.81, Kurtosis = 3.46, MValue = 3
-------------------- Histogram --------------------
[ 78.927 us ;  91.477 us) | @@@@@
[ 91.477 us ; 103.023 us) | @@@@@@@@@
[103.023 us ; 114.073 us) | @@
[114.073 us ; 120.177 us) |
[120.177 us ; 131.723 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeSingle: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]  
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 109.565 us, StdErr = 4.976 us (4.54%), N = 17, StdDev = 20.517 us
Min = 68.400 us, Q1 = 95.400 us, Median = 111.300 us, Q3 = 126.000 us, Max = 139.400 us
IQR = 30.600 us, LowerFence = 49.500 us, UpperFence = 171.900 us
ConfidenceInterval = [89.585 us; 129.544 us] (CI 99.9%), Margin = 19.979 us (18.24% of Mean)
Skewness = -0.28, Kurtosis = 1.94, MValue = 2.29
-------------------- Histogram --------------------
[ 63.027 us ;  83.973 us) | @@
[ 83.973 us ;  94.377 us) | @
[ 94.377 us ; 115.323 us) | @@@@@@@
[115.323 us ; 142.273 us) | @@@@@@@
---------------------------------------------------

Gpt2TokenizerBenchmarks.HuggingFace_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)] 
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 443.706 us, StdErr = 10.460 us (2.36%), N = 18, StdDev = 44.378 us
Min = 400.400 us, Q1 = 416.725 us, Median = 424.750 us, Q3 = 463.700 us, Max = 571.500 us
IQR = 46.975 us, LowerFence = 346.262 us, UpperFence = 534.163 us
ConfidenceInterval = [402.230 us; 485.181 us] (CI 99.9%), Margin = 41.475 us (9.35% of Mean)
Skewness = 1.44, Kurtosis = 4.29, MValue = 2.5
-------------------- Histogram --------------------
[397.225 us ; 441.675 us) | @@@@@@@@@@@@@
[441.675 us ; 466.125 us) |
[466.125 us ; 510.575 us) | @@@@
[510.575 us ; 549.275 us) |
[549.275 us ; 593.725 us) | @
---------------------------------------------------

Gpt2TokenizerBenchmarks.Microsoft_DecodeBatch: ExtendedShortRun(MinIterationTime=200.0000 ms, InvocationCount=1, IterationCount=20, LaunchCount=1, MinIterationCount=10, UnrollFactor=1, WarmupCount=5) [Case=Tiny (≈16 tokens)]   
Runtime = .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX2; GC = Concurrent Workstation
Mean = 1.187 ms, StdErr = 0.079 ms (6.68%), N = 19, StdDev = 0.346 ms
Min = 0.377 ms, Q1 = 1.038 ms, Median = 1.214 ms, Q3 = 1.396 ms, Max = 1.738 ms
IQR = 0.358 ms, LowerFence = 0.500 ms, UpperFence = 1.933 ms
ConfidenceInterval = [0.876 ms; 1.498 ms] (CI 99.9%), Margin = 0.311 ms (26.20% of Mean)
Skewness = -0.79, Kurtosis = 3.28, MValue = 2
-------------------- Histogram --------------------
[0.242 ms ; 0.582 ms) | @@
[0.582 ms ; 0.973 ms) |
[0.973 ms ; 1.313 ms) | @@@@@@@@@@@
[1.313 ms ; 1.661 ms) | @@@@@
[1.661 ms ; 1.908 ms) | @
---------------------------------------------------
```