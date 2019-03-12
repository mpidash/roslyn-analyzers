﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis
{
    using ValueContentAnalysisResult = DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>;
    using CopyAnalysisResult = DataFlowAnalysisResult<CopyAnalysis.CopyBlockAnalysisResult, CopyAnalysis.CopyAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track value content of <see cref="AnalysisEntity"/>/<see cref="IOperation"/>.
    /// </summary>
    public partial class ValueContentAnalysis : ForwardDataFlowAnalysis<ValueContentAnalysisData, ValueContentAnalysisContext, ValueContentAnalysisResult, ValueContentBlockAnalysisResult, ValueContentAbstractValue>
    {
        private ValueContentAnalysis(ValueContentDataFlowOperationVisitor operationVisitor)
            : base(ValueContentAnalysisDomain.Instance, operationVisitor)
        {
        }

        public static ValueContentAnalysisResult GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            AnalyzerOptions analyzerOptions,
            DiagnosticDescriptor rule,
            CancellationToken cancellationToken,
            InterproceduralAnalysisKind interproceduralAnalysisKind = InterproceduralAnalysisKind.None,
            bool pessimisticAnalysis = true,
            bool performPointsToAnalysis = true)
        {
            return GetOrComputeResult(cfg, owningSymbol, wellKnownTypeProvider, analyzerOptions, rule,
                cancellationToken, out var _, out var _, interproceduralAnalysisKind,
                pessimisticAnalysis, performPointsToAnalysis);
        }

        public static ValueContentAnalysisResult GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            AnalyzerOptions analyzerOptions,
            DiagnosticDescriptor rule,
            CancellationToken cancellationToken,
            out CopyAnalysisResult copyAnalysisResultOpt,
            out PointsToAnalysisResult pointsToAnalysisResultOpt,
            InterproceduralAnalysisKind interproceduralAnalysisKind = InterproceduralAnalysisKind.None,
            bool pessimisticAnalysis = true,
            bool performPointsToAnalysis = true,
            bool performCopyAnalysisIfNotUserConfigured = true)
        {
            var interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                analyzerOptions, rule, interproceduralAnalysisKind, cancellationToken);
            return GetOrComputeResult(cfg, owningSymbol, wellKnownTypeProvider,
                interproceduralAnalysisConfig, out copyAnalysisResultOpt,
                out pointsToAnalysisResultOpt, pessimisticAnalysis, performPointsToAnalysis,
                performCopyAnalysis: analyzerOptions.GetCopyAnalysisOption(rule, defaultValue: performCopyAnalysisIfNotUserConfigured, cancellationToken));
        }

        internal static ValueContentAnalysisResult GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
            out CopyAnalysisResult copyAnalysisResultOpt,
            out PointsToAnalysisResult pointsToAnalysisResultOpt,
            bool pessimisticAnalysis = true,
            bool performPointsToAnalysis = true,
            bool performCopyAnalysis = true)
        {
            copyAnalysisResultOpt = null;
            pointsToAnalysisResultOpt = performPointsToAnalysis ?
                PointsToAnalysis.PointsToAnalysis.GetOrComputeResult(
                    cfg, owningSymbol, wellKnownTypeProvider, out copyAnalysisResultOpt, interproceduralAnalysisConfig, pessimisticAnalysis, performCopyAnalysis) :
                null;
            var analysisContext = ValueContentAnalysisContext.Create(
                ValueContentAbstractValueDomain.Default, wellKnownTypeProvider, cfg, owningSymbol,
                interproceduralAnalysisConfig, pessimisticAnalysis, copyAnalysisResultOpt,
                pointsToAnalysisResultOpt, GetOrComputeResultForAnalysisContext);
            return GetOrComputeResultForAnalysisContext(analysisContext);
        }

        private static ValueContentAnalysisResult GetOrComputeResultForAnalysisContext(ValueContentAnalysisContext analysisContext)
        {
            var operationVisitor = new ValueContentDataFlowOperationVisitor(analysisContext);
            var nullAnalysis = new ValueContentAnalysis(operationVisitor);
            return nullAnalysis.GetOrComputeResultCore(analysisContext, cacheResult: false);
        }

        protected override ValueContentAnalysisResult ToResult(ValueContentAnalysisContext analysisContext, ValueContentAnalysisResult dataFlowAnalysisResult)
            => dataFlowAnalysisResult;

        protected override ValueContentBlockAnalysisResult ToBlockResult(BasicBlock basicBlock, ValueContentAnalysisData blockAnalysisData)
            => new ValueContentBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
