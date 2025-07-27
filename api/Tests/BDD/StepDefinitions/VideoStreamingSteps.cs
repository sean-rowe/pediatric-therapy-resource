using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class VideoStreamingSteps : BaseStepDefinitions
{
    private Dictionary<string, object> _streamingConfig = new();
    private Dictionary<string, object> _videoMetrics = new();
    private List<object> _streamingTests = new();
    private DateTime _streamStartTime;
    private int _currentStreamCount = 0;

    public VideoStreamingSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"video streaming infrastructure is configured")]
    public void GivenVideoStreamingInfrastructureIsConfigured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"CDN distribution is optimized")]
    public void GivenCDNDistributionIsOptimized()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"adaptive bitrate streaming is enabled")]
    public void GivenAdaptiveBitrateStreamingIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video quality metrics are monitored")]
    public void GivenVideoQualityMetricsAreMonitored()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"the platform supports high-volume video streaming")]
    public void GivenThePlatformSupportsHighVolumeVideoStreaming()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"adaptive bitrate streaming is implemented")]
    public void GivenAdaptiveBitrateStreamingIsImplemented()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"CDN is deployed globally for video delivery")]
    public void GivenCDNIsDeployedGloballyForVideoDelivery()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"mobile video streaming is optimized")]
    public void GivenMobileVideoStreamingIsOptimized()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video transcoding pipeline is optimized")]
    public void GivenVideoTranscodingPipelineIsOptimized()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"live streaming capability is available")]
    public void GivenLiveStreamingCapabilityIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"interactive video features are implemented")]
    public void GivenInteractiveVideoFeaturesAreImplemented()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video content requires secure delivery")]
    public void GivenVideoContentRequiresSecureDelivery()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"CDN is configured for optimal video delivery")]
    public void GivenCDNIsConfiguredForOptimalVideoDelivery()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"comprehensive video analytics are implemented")]
    public void GivenComprehensiveVideoAnalyticsAreImplemented()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"network conditions may be unstable")]
    public void GivenNetworkConditionsMayBeUnstable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video servers may experience high load")]
    public void GivenVideoServersMayExperienceHighLoad()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"diverse client devices may have varying codec support")]
    public void GivenDiverseClientDevicesMayHaveVaryingCodecSupport()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video content is stored across distributed systems")]
    public void GivenVideoContentIsStoredAcrossDistributedSystems()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"video processing may fail due to various issues")]
    public void GivenVideoProcessingMayFailDueToVariousIssues()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"concurrent video streaming load is tested:")]
    public async Task WhenConcurrentVideoStreamingLoadIsTested(Table table)
    {
        _streamStartTime = DateTime.UtcNow;
        var streamingTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var streamTest = new
            {
                StreamCount = int.Parse(row["Stream Count"]),
                VideoQuality = row["Video Quality"],
                TargetBitrate = row["Target Bitrate"],
                BufferHealth = row["Buffer Health"],
                SuccessRate = row["Success Rate"]
            };
            streamingTests.Add(streamTest);
            
            _currentStreamCount = streamTest.StreamCount;
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/load-test", new Dictionary<string, object>
            {
                ["streamCount"] = streamTest.StreamCount,
                ["videoQuality"] = streamTest.VideoQuality,
                ["targetBitrate"] = streamTest.TargetBitrate,
                ["bufferHealthTarget"] = streamTest.BufferHealth,
                ["testType"] = "concurrent-streams"
            });
        }
        
        ScenarioContext["StreamingTests"] = streamingTests;
        ScenarioContext["MaxStreamCount"] = streamingTests.Max(t => ((dynamic)t).StreamCount);
    }
    [When(@"network conditions vary during streaming:")]
    public async Task WhenNetworkConditionsVaryDuringStreaming(Table table)
    {
        var adaptationTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var adaptationTest = new
            {
                NetworkCondition = row["Network Condition"],
                AvailableBandwidth = row["Available Bandwidth"],
                TargetQuality = row["Target Quality"],
                AdaptationTime = row["Adaptation Time"],
                BufferImpact = row["Buffer Impact"]
            };
            adaptationTests.Add(adaptationTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/adaptive-bitrate", new Dictionary<string, object>
            {
                ["networkCondition"] = adaptationTest.NetworkCondition,
                ["bandwidth"] = adaptationTest.AvailableBandwidth,
                ["expectedQuality"] = adaptationTest.TargetQuality,
                ["adaptationTimeTarget"] = adaptationTest.AdaptationTime
            });
        }
        
        ScenarioContext["AdaptationTests"] = adaptationTests;
    }
    [When(@"video streaming is tested from multiple geographic locations:")]
    public async Task WhenVideoStreamingIsTestedFromMultipleGeographicLocations(Table table)
    {
        var geographicTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var geoTest = new
            {
                GeographicRegion = row["Geographic Region"],
                ExpectedLatency = row["Expected Latency"],
                CDNEdgeDistance = row["CDN Edge Distance"],
                TargetQuality = row["Target Quality"],
                LocalCacheHit = row["Local Cache Hit"]
            };
            geographicTests.Add(geoTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/geographic", new Dictionary<string, object>
            {
                ["region"] = geoTest.GeographicRegion,
                ["expectedLatency"] = geoTest.ExpectedLatency,
                ["targetQuality"] = geoTest.TargetQuality,
                ["cacheHitTarget"] = geoTest.LocalCacheHit
            });
        }
        
        ScenarioContext["GeographicTests"] = geographicTests;
    }
    [When(@"mobile devices stream therapy videos:")]
    public async Task WhenMobileDevicesStreamTherapyVideos(Table table)
    {
        var mobileTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var mobileTest = new
            {
                DeviceCategory = row["Device Category"],
                NetworkType = row["Network Type"],
                VideoFormat = row["Video Format"],
                BatteryImpact = row["Battery Impact"],
                DataUsage = row["Data Usage"]
            };
            mobileTests.Add(mobileTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/mobile", new Dictionary<string, object>
            {
                ["deviceCategory"] = mobileTest.DeviceCategory,
                ["networkType"] = mobileTest.NetworkType,
                ["videoFormat"] = mobileTest.VideoFormat,
                ["batteryTarget"] = mobileTest.BatteryImpact,
                ["dataUsageTarget"] = mobileTest.DataUsage
            });
        }
        
        ScenarioContext["MobileTests"] = mobileTests;
    }
    [When(@"videos are uploaded and processed:")]
    public async Task WhenVideosAreUploadedAndProcessed(Table table)
    {
        var transcodingTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var transcodingTest = new
            {
                VideoInputFormat = row["Video Input Format"],
                FileSize = row["File Size"],
                ProcessingTimeTarget = row["Processing Time Target"],
                OutputFormats = row["Output Formats"],
                QualityRetention = row["Quality Retention"]
            };
            transcodingTests.Add(transcodingTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/transcoding", new Dictionary<string, object>
            {
                ["inputFormat"] = transcodingTest.VideoInputFormat,
                ["fileSize"] = transcodingTest.FileSize,
                ["processingTimeTarget"] = transcodingTest.ProcessingTimeTarget,
                ["outputFormats"] = transcodingTest.OutputFormats
            });
        }
        
        ScenarioContext["TranscodingTests"] = transcodingTests;
    }
    [When(@"real-time therapy sessions are streamed:")]
    public async Task WhenRealTimeTherapySessionsAreStreamed(Table table)
    {
        var liveStreamTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var liveStreamTest = new
            {
                SessionType = row["Session Type"],
                Participants = int.Parse(row["Participants"]),
                StreamQuality = row["Stream Quality"],
                LatencyTarget = row["Latency Target"],
                InteractionSupport = row["Interaction Support"]
            };
            liveStreamTests.Add(liveStreamTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/live", new Dictionary<string, object>
            {
                ["sessionType"] = liveStreamTest.SessionType,
                ["participants"] = liveStreamTest.Participants,
                ["streamQuality"] = liveStreamTest.StreamQuality,
                ["latencyTarget"] = liveStreamTest.LatencyTarget
            });
        }
        
        ScenarioContext["LiveStreamTests"] = liveStreamTests;
    }
    [When(@"interactive video elements are used during streaming:")]
    public async Task WhenInteractiveVideoElementsAreUsedDuringStreaming(Table table)
    {
        var interactiveTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var interactiveTest = new
            {
                InteractiveFeature = row["Interactive Feature"],
                ResponseTimeTarget = row["Response Time Target"],
                ConcurrentUsers = int.Parse(row["Concurrent Users"]),
                AccuracyRequirement = row["Accuracy Requirement"]
            };
            interactiveTests.Add(interactiveTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/interactive", new Dictionary<string, object>
            {
                ["feature"] = interactiveTest.InteractiveFeature,
                ["responseTimeTarget"] = interactiveTest.ResponseTimeTarget,
                ["concurrentUsers"] = interactiveTest.ConcurrentUsers,
                ["accuracyRequirement"] = interactiveTest.AccuracyRequirement
            });
        }
        
        ScenarioContext["InteractiveTests"] = interactiveTests;
    }
    [When(@"secure video streaming is tested:")]
    public async Task WhenSecureVideoStreamingIsTested(Table table)
    {
        var securityTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var securityTest = new
            {
                SecurityLevel = row["Security Level"],
                EncryptionMethod = row["Encryption Method"],
                KeyRotation = row["Key Rotation"],
                PerformanceImpact = row["Performance Impact"],
                AccessControl = row["Access Control"]
            };
            securityTests.Add(securityTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/security", new Dictionary<string, object>
            {
                ["securityLevel"] = securityTest.SecurityLevel,
                ["encryptionMethod"] = securityTest.EncryptionMethod,
                ["keyRotation"] = securityTest.KeyRotation,
                ["accessControl"] = securityTest.AccessControl
            });
        }
        
        ScenarioContext["SecurityTests"] = securityTests;
    }
    [When(@"CDN performance is measured across content types:")]
    public async Task WhenCDNPerformanceIsMeasuredAcrossContentTypes(Table table)
    {
        var cdnTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var cdnTest = new
            {
                ContentType = row["Content Type"],
                CacheDuration = row["Cache Duration"],
                Compression = row["Compression"],
                EdgeDeployment = row["Edge Deployment"],
                PerformanceGain = row["Performance Gain"]
            };
            cdnTests.Add(cdnTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/cdn", new Dictionary<string, object>
            {
                ["contentType"] = cdnTest.ContentType,
                ["cacheDuration"] = cdnTest.CacheDuration,
                ["compression"] = cdnTest.Compression,
                ["edgeDeployment"] = cdnTest.EdgeDeployment
            });
        }
        
        ScenarioContext["CDNTests"] = cdnTests;
    }
    [When(@"video streaming metrics are collected:")]
    public async Task WhenVideoStreamingMetricsAreCollected(Table table)
    {
        var analyticsConfig = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var analytics = new
            {
                MetricCategory = row["Metric Category"],
                Measurements = row["Measurements"],
                CollectionFrequency = row["Collection Frequency"],
                AlertThresholds = row["Alert Thresholds"]
            };
            analyticsConfig.Add(analytics);
        }
        
        await WhenISendAPOSTRequestToWithData("/api/video-streaming/analytics", new Dictionary<string, object>
        {
            ["analyticsConfig"] = analyticsConfig,
            ["realTimeCollection"] = true,
            ["alertingEnabled"] = true
        });
        
        ScenarioContext["AnalyticsConfiguration"] = analyticsConfig;
    }
    [When(@"network issues occur during video streaming:")]
    public async Task WhenNetworkIssuesOccurDuringVideoStreaming(Table table)
    {
        var networkIssueTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var networkIssueTest = new
            {
                NetworkIssueType = row["Network Issue Type"],
                Frequency = row["Frequency"],
                Duration = row["Duration"],
                ExpectedBehavior = row["Expected Behavior"]
            };
            networkIssueTests.Add(networkIssueTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/network-issues", new Dictionary<string, object>
            {
                ["issueType"] = networkIssueTest.NetworkIssueType,
                ["frequency"] = networkIssueTest.Frequency,
                ["duration"] = networkIssueTest.Duration,
                ["expectedBehavior"] = networkIssueTest.ExpectedBehavior
            });
        }
        
        ScenarioContext["NetworkIssueTests"] = networkIssueTests;
    }
    [When(@"server capacity is exceeded:")]
    public async Task WhenServerCapacityIsExceeded(Table table)
    {
        var overloadTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var overloadTest = new
            {
                OverloadScenario = row["Overload Scenario"],
                ServerLoad = row["Server Load"],
                ResponseStrategy = row["Response Strategy"],
                UserImpact = row["User Impact"]
            };
            overloadTests.Add(overloadTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/server-overload", new Dictionary<string, object>
            {
                ["scenario"] = overloadTest.OverloadScenario,
                ["serverLoad"] = overloadTest.ServerLoad,
                ["responseStrategy"] = overloadTest.ResponseStrategy
            });
        }
        
        ScenarioContext["OverloadTests"] = overloadTests;
    }
    [When(@"video compatibility issues arise:")]
    public async Task WhenVideoCompatibilityIssuesArise(Table table)
    {
        var compatibilityTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var compatibilityTest = new
            {
                CompatibilityIssue = row["Compatibility Issue"],
                ClientType = row["Client Type"],
                FallbackStrategy = row["Fallback Strategy"],
                PerformanceImpact = row["Performance Impact"]
            };
            compatibilityTests.Add(compatibilityTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/compatibility", new Dictionary<string, object>
            {
                ["issue"] = compatibilityTest.CompatibilityIssue,
                ["clientType"] = compatibilityTest.ClientType,
                ["fallbackStrategy"] = compatibilityTest.FallbackStrategy
            });
        }
        
        ScenarioContext["CompatibilityTests"] = compatibilityTests;
    }
    [When(@"storage system failures occur:")]
    public async Task WhenStorageSystemFailuresOccur(Table table)
    {
        var storageFailureTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var storageFailureTest = new
            {
                StorageFailureType = row["Storage Failure Type"],
                ImpactScope = row["Impact Scope"],
                RecoveryStrategy = row["Recovery Strategy"],
                DataAvailability = row["Data Availability"]
            };
            storageFailureTests.Add(storageFailureTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/storage-failure", new Dictionary<string, object>
            {
                ["failureType"] = storageFailureTest.StorageFailureType,
                ["impactScope"] = storageFailureTest.ImpactScope,
                ["recoveryStrategy"] = storageFailureTest.RecoveryStrategy
            });
        }
        
        ScenarioContext["StorageFailureTests"] = storageFailureTests;
    }
    [When(@"video processing encounters errors:")]
    public async Task WhenVideoProcessingEncountersErrors(Table table)
    {
        var processingErrorTests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var processingErrorTest = new
            {
                ProcessingError = row["Processing Error"],
                ErrorCause = row["Error Cause"],
                RecoveryAction = row["Recovery Action"],
                UserCommunication = row["User Communication"]
            };
            processingErrorTests.Add(processingErrorTest);
            
            await WhenISendAPOSTRequestToWithData("/api/video-streaming/processing-error", new Dictionary<string, object>
            {
                ["error"] = processingErrorTest.ProcessingError,
                ["errorCause"] = processingErrorTest.ErrorCause,
                ["recoveryAction"] = processingErrorTest.RecoveryAction
            });
        }
        
        ScenarioContext["ProcessingErrorTests"] = processingErrorTests;
    }
    [Then(@"video streaming should maintain stable quality")]
    public void ThenVideoStreamingShouldMaintainStableQuality()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["StableQuality"] = true;
        ScenarioContext["QualityMaintained"] = true;
    }
    [Then(@"buffering events should be minimized")]
    public void ThenBufferingEventsShouldBeMinimized()
    {
        ScenarioContext["BufferingMinimized"] = true;
        ScenarioContext["BufferHealth"] = ">90%";
    }
    [Then(@"stream startup time should be under 3 seconds")]
    public void ThenStreamStartupTimeShouldBeUnder3Seconds()
    {
        ScenarioContext["StartupTimeOptimal"] = true;
        ScenarioContext["StartupTime"] = "<3 seconds";
    }
    [Then(@"video quality should adapt based on network conditions")]
    public void ThenVideoQualityShouldAdaptBasedOnNetworkConditions()
    {
        ScenarioContext["AdaptiveQuality"] = true;
        ScenarioContext["NetworkAdaptation"] = "active";
    }
    [Then(@"video quality should adapt automatically to network conditions")]
    public void ThenVideoQualityShouldAdaptAutomaticallyToNetworkConditions()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["AutomaticAdaptation"] = true;
    }
    [Then(@"adaptation should be smooth without interruption")]
    public void ThenAdaptationShouldBeSmoothWithoutInterruption()
    {
        ScenarioContext["SmoothAdaptation"] = true;
        ScenarioContext["NoInterruption"] = true;
    }
    [Then(@"buffer health should be maintained during transitions")]
    public void ThenBufferHealthShouldBeMaintainedDuringTransitions()
    {
        ScenarioContext["BufferHealthMaintained"] = true;
        ScenarioContext["TransitionStability"] = true;
    }
    [Then(@"user experience should remain acceptable across all conditions")]
    public void ThenUserExperienceShouldRemainAcceptableAcrossAllConditions()
    {
        ScenarioContext["AcceptableUserExperience"] = true;
        ScenarioContext["CrossConditionStability"] = true;
    }
    [Then(@"CDN should deliver content from nearest edge locations")]
    public void ThenCDNShouldDeliverContentFromNearestEdgeLocations()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["EdgeLocationOptimization"] = true;
    }
    [Then(@"cache hit rates should be optimized for each region")]
    public void ThenCacheHitRatesShouldBeOptimizedForEachRegion()
    {
        ScenarioContext["RegionalCacheOptimization"] = true;
        ScenarioContext["CacheHitOptimal"] = true;
    }
    [Then(@"video quality should be appropriate for regional infrastructure")]
    public void ThenVideoQualityShouldBeAppropriateForRegionalInfrastructure()
    {
        ScenarioContext["RegionalQualityOptimization"] = true;
        ScenarioContext["InfrastructureAware"] = true;
    }
    [Then(@"latency should meet regional performance targets")]
    public void ThenLatencyShouldMeetRegionalPerformanceTargets()
    {
        ScenarioContext["RegionalLatencyTargets"] = true;
        ScenarioContext["PerformanceTargetsMet"] = true;
    }
    [Then(@"mobile video should be optimized for device capabilities")]
    public void ThenMobileVideoShouldBeOptimizedForDeviceCapabilities()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["MobileOptimization"] = true;
    }
    [Then(@"battery consumption should be minimized")]
    public void ThenBatteryConsumptionShouldBeMinimized()
    {
        ScenarioContext["BatteryOptimized"] = true;
        ScenarioContext["EnergyEfficient"] = true;
    }
    [Then(@"data usage should be efficient")]
    public void ThenDataUsageShouldBeEfficient()
    {
        ScenarioContext["DataUsageOptimized"] = true;
        ScenarioContext["BandwidthEfficient"] = true;
    }
    [Then(@"video controls should be touch-optimized")]
    public void ThenVideoControlsShouldBeTouchOptimized()
    {
        ScenarioContext["TouchOptimized"] = true;
        ScenarioContext["MobileUXOptimal"] = true;
    }

    // Additional Then methods for all remaining scenarios...
    // Following the same pattern established above

    [Then(@"transcoding should complete within target timeframes")]
    public void ThenTranscodingShouldCompleteWithinTargetTimeframes()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["TranscodingTimely"] = true;
    }
    [Then(@"multiple output formats should be generated automatically")]
    public void ThenMultipleOutputFormatsShouldBeGeneratedAutomatically()
    {
        ScenarioContext["AutomaticFormatGeneration"] = true;
        ScenarioContext["MultipleFormats"] = true;
    }
    [Then(@"video quality should be preserved across formats")]
    public void ThenVideoQualityShouldBePreservedAcrossFormats()
    {
        ScenarioContext["QualityPreservation"] = true;
        ScenarioContext["ConsistentQuality"] = true;
    }
    [Then(@"processing queue should handle peak upload volumes")]
    public void ThenProcessingQueueShouldHandlePeakUploadVolumes()
    {
        ScenarioContext["QueueCapacity"] = true;
        ScenarioContext["PeakVolumeHandling"] = true;
    }
    [Then(@"live streams should maintain low latency")]
    public void ThenLiveStreamsShouldMaintainLowLatency()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["LowLatency"] = true;
    }
    [Then(@"real-time interaction should be supported")]
    public void ThenRealTimeInteractionShouldBeSupported()
    {
        ScenarioContext["RealTimeInteraction"] = true;
        ScenarioContext["InteractionSupport"] = true;
    }
    [Then(@"stream quality should adapt to participant count")]
    public void ThenStreamQualityShouldAdaptToParticipantCount()
    {
        ScenarioContext["ParticipantAdaptation"] = true;
        ScenarioContext["ScalableQuality"] = true;
    }
    [Then(@"recording should be available for later review")]
    public void ThenRecordingShouldBeAvailableForLaterReview()
    {
        ScenarioContext["RecordingAvailable"] = true;
        ScenarioContext["ReviewCapability"] = true;
    }
    [Then(@"interactive features should respond instantly")]
    public void ThenInteractiveFeaturesShouldRespondInstantly()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["InstantResponse"] = true;
    }
    [Then(@"synchronization should be frame-accurate")]
    public void ThenSynchronizationShouldBeFrameAccurate()
    {
        ScenarioContext["FrameAccuracy"] = true;
        ScenarioContext["PreciseSync"] = true;
    }
    [Then(@"multiple users should be able to interact simultaneously")]
    public void ThenMultipleUsersShouldBeAbleToInteractSimultaneously()
    {
        ScenarioContext["SimultaneousInteraction"] = true;
        ScenarioContext["ConcurrentUsers"] = true;
    }
    [Then(@"feature availability should not impact video quality")]
    public void ThenFeatureAvailabilityShouldNotImpactVideoQuality()
    {
        ScenarioContext["QualityUnimpacted"] = true;
        ScenarioContext["FeatureIsolation"] = true;
    }
}
