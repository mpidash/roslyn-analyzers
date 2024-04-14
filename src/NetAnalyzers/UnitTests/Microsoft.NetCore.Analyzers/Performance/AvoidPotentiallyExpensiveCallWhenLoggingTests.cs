﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.AvoidPotentiallyExpensiveCallWhenLoggingAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.AvoidPotentiallyExpensiveCallWhenLoggingAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class AvoidPotentiallyExpensiveCallWhenLoggingTests
    {
        public static readonly TheoryData<string> LogLevels = new()
        {
            "Trace",
            "Debug",
            "Information",
            "Warning",
            "Error",
            "Critical"
        };

        [Fact]
        public async Task LiteralInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, "literal", exception, formatter);

                        logger.Log(LogLevel.Debug, "literal");
                        logger.Log(LogLevel.Information, eventId, "literal");
                        logger.Log(LogLevel.Warning, exception, "literal");
                        logger.Log(LogLevel.Error, eventId, exception, "literal");
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task LiteralInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        logger.Log{{logLevel}}("literal");
                        logger.Log{{logLevel}}(eventId, "literal");
                        logger.Log{{logLevel}}(exception, "literal");
                        logger.Log{{logLevel}}(eventId, exception, "literal");
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LiteralInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        logger.StaticLogLevel("literal");
                        logger.DynamicLogLevel(LogLevel.Debug, "literal");
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LocalInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        string local = "local";

                        logger.Log(LogLevel.Trace, eventId, local, exception, formatter);

                        logger.Log(LogLevel.Debug, local);
                        logger.Log(LogLevel.Information, eventId, local);
                        logger.Log(LogLevel.Warning, exception, local);
                        logger.Log(LogLevel.Error, eventId, exception, local);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task LocalInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        string local = "local";

                        logger.Log{{logLevel}}(local);
                        logger.Log{{logLevel}}(eventId, local);
                        logger.Log{{logLevel}}(exception, local);
                        logger.Log{{logLevel}}(eventId, exception, local);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LocalInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        string local = "local";

                        logger.StaticLogLevel(local);
                        logger.DynamicLogLevel(LogLevel.Debug, local);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task FieldInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private string _field;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, _field, exception, formatter);

                        logger.Log(LogLevel.Debug, _field);
                        logger.Log(LogLevel.Information, eventId, _field);
                        logger.Log(LogLevel.Warning, exception, _field);
                        logger.Log(LogLevel.Error, eventId, exception, _field);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task FieldInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private string _field;

                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        logger.Log{{logLevel}}(_field);
                        logger.Log{{logLevel}}(eventId, _field);
                        logger.Log{{logLevel}}(exception, _field);
                        logger.Log{{logLevel}}(eventId, exception, _field);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task FieldInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    private static string _field;

                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        logger.StaticLogLevel(_field);
                        logger.DynamicLogLevel(LogLevel.Debug, _field);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PropertyInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    public string Property { get; set; }

                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, Property, exception, formatter);

                        logger.Log(LogLevel.Debug, Property);
                        logger.Log(LogLevel.Information, eventId, Property);
                        logger.Log(LogLevel.Warning, exception, Property);
                        logger.Log(LogLevel.Error, eventId, exception, Property);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task PropertyInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    public string Property { get; set; }

                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        logger.Log{{logLevel}}(Property);
                        logger.Log{{logLevel}}(eventId, Property);
                        logger.Log{{logLevel}}(exception, Property);
                        logger.Log{{logLevel}}(eventId, exception, Property);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PropertyInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    public static string Property { get; set; }

                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        logger.StaticLogLevel(Property);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IndexerInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private Dictionary<LogLevel, string> _messages;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, _messages[LogLevel.Trace], exception, formatter);

                        logger.Log(LogLevel.Debug, _messages[LogLevel.Debug]);
                        logger.Log(LogLevel.Information, eventId, _messages[LogLevel.Information]);
                        logger.Log(LogLevel.Warning, exception, _messages[LogLevel.Warning]);
                        logger.Log(LogLevel.Error, eventId, exception, _messages[LogLevel.Error]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task IndexerInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using System.Collections.Generic;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private Dictionary<LogLevel, string> _messages;

                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        logger.Log{{logLevel}}(_messages[LogLevel.{{logLevel}}]);
                        logger.Log{{logLevel}}(eventId, _messages[LogLevel.{{logLevel}}]);
                        logger.Log{{logLevel}}(exception, _messages[LogLevel.{{logLevel}}]);
                        logger.Log{{logLevel}}(eventId, exception, _messages[LogLevel.{{logLevel}}]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IndexerInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    private static Dictionary<LogLevel, string> _messages;

                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        logger.StaticLogLevel(_messages[LogLevel.Information]);
                        logger.DynamicLogLevel(LogLevel.Debug, _messages[LogLevel.Debug]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayIndexerInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private string[] _messages;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, _messages[0], exception, formatter);

                        logger.Log(LogLevel.Debug, _messages[0]);
                        logger.Log(LogLevel.Information, eventId, _messages[0]);
                        logger.Log(LogLevel.Warning, exception, _messages[0]);
                        logger.Log(LogLevel.Error, eventId, exception, _messages[0]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task ArrayIndexerInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private string[] _messages;

                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        logger.Log{{logLevel}}(_messages[0]);
                        logger.Log{{logLevel}}(eventId, _messages[0]);
                        logger.Log{{logLevel}}(exception, _messages[0]);
                        logger.Log{{logLevel}}(eventId, exception, _messages[0]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayIndexerInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    private static string[] _messages;

                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        logger.StaticLogLevel(_messages[0]);
                        logger.DynamicLogLevel(LogLevel.Debug, _messages[0]);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ConditionalAccessInLog_NoDiagnostic_CS()
        {
            string source = """
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception? exception, Func<string?, Exception?, string> formatter)
                    {
                        logger.Log(LogLevel.Trace, eventId, exception?.Message, exception, formatter);

                        logger.Log(LogLevel.Debug, exception?.Message);
                        logger.Log(LogLevel.Information, eventId, exception?.Message);
                        logger.Log(LogLevel.Warning, exception, exception?.Message);
                        logger.Log(LogLevel.Error, eventId, exception, exception?.Message);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task ConditionalAccessInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception? exception)
                    {
                        logger.Log{{logLevel}}(exception?.Message);
                        logger.Log{{logLevel}}(eventId, exception?.Message);
                        logger.Log{{logLevel}}(exception, exception?.Message);
                        logger.Log{{logLevel}}(eventId, exception, exception?.Message);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ConditionalAccessInLoggerMessage_NoDiagnostic_CS()
        {
            string source = """
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string? message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string? message);

                    static void M(ILogger logger, Exception? exception)
                    {
                        logger.StaticLogLevel(exception?.Message);
                        logger.DynamicLogLevel(LogLevel.Debug, exception?.Message);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task OtherILoggerMethodCalled_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger)
                    {
                        logger.BeginScope(ExpensiveMethodCall());
                        logger.BeginScope("Processing calculation result {CalculationResult}", ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        // Tests for operations that get flagged.

        [Fact]
        public async Task AnonymousObjectCreationOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|new { Test = "42" }|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayCreationOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|new int[10]|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task AwaitOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using System.Threading.Tasks;
                using Microsoft.Extensions.Logging;

                class C
                {
                    async void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter, Task<string> task)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|await task|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task BinaryOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|4 + 2|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CoalesceOperation_ReportsDiagnostic_CS()
        {
            string source = """
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception? exception, Func<object, Exception?, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|exception ?? new Exception()|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CollectionExpressionOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<int[], Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|[4, 2]|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source, CodeAnalysis.CSharp.LanguageVersion.CSharp12);
        }

        [Fact]
        public async Task DefaultValueOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|default|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IncrementOrDecrementOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter, int input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|input++|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task InterpolatedStringOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter, int input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|$"{input}"|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task InvocationOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|exception.ToString()|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IsPatternOperation_ReportsDiagnostic_CS()
        {
            string source = """
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception? exception, Func<bool, Exception?, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|exception is not null|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IsTypeOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<bool, Exception, string> formatter, object input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|input is Exception|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NameOfOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|nameof(logger)|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ObjectCreationOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<Exception, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|new Exception()|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task SizeOfOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|sizeof(int)|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task TypeOfOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<Type, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|typeof(int)|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task UnaryOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<bool, Exception, string> formatter, bool input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|!input|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WithOperation_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    record Point(int X, int Y);

                    void M(ILogger logger, EventId eventId, Exception exception, Func<Point, Exception, string> formatter, Point input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|input with { Y = 42 }|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        // Tests for work done in indexers, array element references or instances of member references.

        [Fact]
        public async Task WorkInIndexerInstance_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall()[LogLevel.Debug]|], exception, formatter);
                    }

                    Dictionary<LogLevel, string> ExpensiveMethodCall()
                    {
                        return default;
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInIndexerArgument_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private Dictionary<LogLevel, string> _messages;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<object, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|_messages[ExpensiveMethodCall()]|], exception, formatter);
                    }

                    LogLevel ExpensiveMethodCall()
                    {
                        return LogLevel.Debug;
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInConditionalAccess_ReportsDiagnostic_CS()
        {
            string source = """
                #nullable enable

                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception? exception, Func<string?, Exception?, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|exception?.ToString()|], exception, formatter);
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInFieldInstance_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private int _field;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall()._field|], exception, formatter);
                    }

                    C ExpensiveMethodCall()
                    {
                        return new C();
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInPropertyInstance_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall().Length|], exception, formatter);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInArrayReference_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private int _field;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<int, Exception, string> formatter, int[] input)
                    {
                        logger.Log(LogLevel.Debug, eventId, [|input[ExpensiveMethodCall()]|], exception, formatter);
                    }

                    int ExpensiveMethodCall()
                    {
                        return 0;
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        // Tests when log call is guarded.

        [Fact]
        public async Task GuardedWorkInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        if (logger.IsEnabled(LogLevel.Trace))
                            logger.Log(LogLevel.Trace, eventId, ExpensiveMethodCall(), exception, formatter);

                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.Log(LogLevel.Debug, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.Log(LogLevel.Information, eventId, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.Log(LogLevel.Warning, exception, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.Log(LogLevel.Error, eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task GuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter);

                        if (logger.IsEnabled(level))
                            logger.Log(level, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, exception, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task GuardedWorkInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(ExpensiveMethodCall());
                        
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(exception, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task GuardedWorkInLoggerMessage_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            logger.StaticLogLevel(ExpensiveMethodCall());
                        }

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall());
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NestedGuardedWorkInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            if (exception is not null)
                            {
                                logger.Log(LogLevel.Debug, eventId, ExpensiveMethodCall(), exception, formatter);
                                logger.Log(LogLevel.Debug, ExpensiveMethodCall());
                                logger.Log(LogLevel.Debug, eventId, ExpensiveMethodCall());
                                logger.Log(LogLevel.Debug, exception, ExpensiveMethodCall());
                                logger.Log(LogLevel.Debug, eventId, exception, ExpensiveMethodCall());
                            }
                        }
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NestedGuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                        {
                            if (exception is not null)
                            {
                                logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter);
                                logger.Log(level, ExpensiveMethodCall());
                                logger.Log(level, eventId, ExpensiveMethodCall());
                                logger.Log(level, exception, ExpensiveMethodCall());
                                logger.Log(level, eventId, exception, ExpensiveMethodCall());
                            }
                        }
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task NestedGuardedWorkInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            if (exception is not null)
                                logger.Log{{logLevel}}(ExpensiveMethodCall());
                        
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            if (exception is not null)
                                logger.Log{{logLevel}}(eventId, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            if (exception is not null)
                                logger.Log{{logLevel}}(exception, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            if (exception is not null)
                                logger.Log{{logLevel}}(eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task NestedGuardedWorkInLoggerMessage_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    static bool IsExpensiveComputationEnabled { get; set; }

                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            if (IsExpensiveComputationEnabled)
                            {
                                logger.StaticLogLevel(ExpensiveMethodCall());
                            }
                        }

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            if (IsExpensiveComputationEnabled)
                            {
                                logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall());
                            }
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CustomLoggerGuardedWorkInLog_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class CustomLogger : ILogger
                {
                    public IDisposable BeginScope<TState>(TState state) { return default; }
                    public bool IsEnabled(LogLevel logLevel) { return true; }
                    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
                }

                class C
                {
                    void M(CustomLogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        if (logger.IsEnabled(LogLevel.Trace))
                            logger.Log(LogLevel.Trace, eventId, ExpensiveMethodCall(), exception, formatter);

                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.Log(LogLevel.Debug, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.Log(LogLevel.Information, eventId, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.Log(LogLevel.Warning, exception, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.Log(LogLevel.Error, eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CustomLoggerGuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class CustomLogger : ILogger
                {
                    public IDisposable BeginScope<TState>(TState state) { return default; }
                    public bool IsEnabled(LogLevel logLevel) { return true; }
                    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
                }

                class C
                {
                    void M(CustomLogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter);

                        if (logger.IsEnabled(level))
                            logger.Log(level, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, exception, ExpensiveMethodCall());
                
                        if (logger.IsEnabled(level))
                            logger.Log(level, eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task CustomLoggerGuardedWorkInLogNamed_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class CustomLogger : ILogger
                {
                    public IDisposable BeginScope<TState>(TState state) { return default; }
                    public bool IsEnabled(LogLevel logLevel) { return true; }
                    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
                }

                class C
                {
                    void M(CustomLogger logger, EventId eventId, Exception exception)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(ExpensiveMethodCall());
                        
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(exception, ExpensiveMethodCall());

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, exception, ExpensiveMethodCall());
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task CustomLoggerGuardedWorkInLoggerMessage_NoDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class CustomLogger : ILogger
                {
                    public IDisposable BeginScope<TState>(TState state) { return default; }
                    public bool IsEnabled(LogLevel logLevel) { return true; }
                    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
                }

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(CustomLogger logger)
                    {
                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            logger.StaticLogLevel(ExpensiveMethodCall());
                        }

                        if (logger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall());
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongLogLevelGuardedWorkInLog_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        if (logger.IsEnabled(LogLevel.Critical))
                            logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter);
                
                        if (logger.IsEnabled(LogLevel.Critical))
                            logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(LogLevel.Critical))
                            logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(LogLevel.Critical))
                            logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(LogLevel.Critical))
                            logger.Log(LogLevel.Error, eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongLogLevelGuardedWorkInLogNamed_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        if (logger.IsEnabled(LogLevel.None))
                            logger.Log{{logLevel}}([|ExpensiveMethodCall()|]);
                        
                        if (logger.IsEnabled(LogLevel.None))
                            logger.Log{{logLevel}}(eventId, [|ExpensiveMethodCall()|]);

                        if (logger.IsEnabled(LogLevel.None))
                            logger.Log{{logLevel}}(exception, [|ExpensiveMethodCall()|]);

                        if (logger.IsEnabled(LogLevel.None))
                            logger.Log{{logLevel}}(eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongLogLevelGuardedWorkInLoggerMessage_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        if (logger.IsEnabled(LogLevel.None))
                        {
                            logger.StaticLogLevel([|ExpensiveMethodCall()|]);
                        }

                        if (logger.IsEnabled(LogLevel.None))
                        {
                            logger.DynamicLogLevel(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|]);
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongDynamicLogLevelGuardedWorkInLog_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                            logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter);
                
                        if (logger.IsEnabled(level))
                            logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(level))
                            logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(level))
                            logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|]);
                
                        if (logger.IsEnabled(level))
                            logger.Log(LogLevel.Error, eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongDynamicLogLevelGuardedWorkInLogNamed_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    void M(ILogger logger, EventId eventId, Exception exception, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                            logger.Log{{logLevel}}([|ExpensiveMethodCall()|]);
                        
                        if (logger.IsEnabled(level))
                            logger.Log{{logLevel}}(eventId, [|ExpensiveMethodCall()|]);

                        if (logger.IsEnabled(level))
                            logger.Log{{logLevel}}(exception, [|ExpensiveMethodCall()|]);

                        if (logger.IsEnabled(level))
                            logger.Log{{logLevel}}(eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongDynamicLogLevelGuardedWorkInLoggerMessage_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger, LogLevel level)
                    {
                        if (logger.IsEnabled(level))
                        {
                            logger.StaticLogLevel([|ExpensiveMethodCall()|]);
                        }

                        if (logger.IsEnabled(level))
                        {
                            logger.DynamicLogLevel(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|]);
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongInstanceGuardedWorkInLog_ReportsDiagnostic_CS()
        {
            string source = """
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private ILogger _otherLogger;

                    void M(ILogger logger, EventId eventId, Exception exception, Func<string, Exception, string> formatter)
                    {
                        if (_otherLogger.IsEnabled(LogLevel.Trace))
                            logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter);
                
                        if (_otherLogger.IsEnabled(LogLevel.Debug))
                            logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|]);
                
                        if (_otherLogger.IsEnabled(LogLevel.Information))
                            logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|]);
                
                        if (_otherLogger.IsEnabled(LogLevel.Warning))
                            logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|]);
                
                        if (_otherLogger.IsEnabled(LogLevel.Error))
                            logger.Log(LogLevel.Error, eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongInstanceGuardedWorkInLogNamed_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                class C
                {
                    private ILogger _otherLogger;

                    void M(ILogger logger, EventId eventId, Exception exception)
                    {
                        if (_otherLogger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}([|ExpensiveMethodCall()|]);
                        
                        if (_otherLogger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, [|ExpensiveMethodCall()|]);

                        if (_otherLogger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(exception, [|ExpensiveMethodCall()|]);

                        if (_otherLogger.IsEnabled(LogLevel.{{logLevel}}))
                            logger.Log{{logLevel}}(eventId, exception, [|ExpensiveMethodCall()|]);
                    }

                    string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongInstanceGuardedWorkInLoggerMessage_ReportsDiagnostic_CS(string logLevel)
        {
            string source = $$"""
                using System;
                using Microsoft.Extensions.Logging;

                static partial class C
                {
                    private static ILogger _otherLogger;

                    [LoggerMessage(EventId = 0, Level = LogLevel.{{logLevel}}, Message = "Static log level `{message}`")]
                    static partial void StaticLogLevel(this ILogger logger, string message);

                    [LoggerMessage(EventId = 1, Message = "Dynamic log level `{message}`")]
                    static partial void DynamicLogLevel(this ILogger logger, LogLevel level, string message);

                    static void M(ILogger logger)
                    {
                        if (_otherLogger.IsEnabled(LogLevel.{{logLevel}}))
                        {
                            logger.StaticLogLevel([|ExpensiveMethodCall()|]);
                        }
                    }

                    static string ExpensiveMethodCall()
                    {
                        return "very expensive call";
                    }
                }
                """;

            await VerifyCSharpCodeFixAsync(source, source);
        }

        // VB tests

        [Fact]
        public async Task LiteralInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, "literal", exception, formatter)
                        logger.Log(LogLevel.Debug, "literal")
                        logger.Log(LogLevel.Information, eventId, "literal")
                        logger.Log(LogLevel.Warning, exception, "literal")
                        logger.Log(LogLevel.[Error], eventId, exception, "literal")
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task LiteralInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}("literal")
                        logger.Log{{logLevel}}(eventId, "literal")
                        logger.Log{{logLevel}}(exception, "literal")
                        logger.Log{{logLevel}}(eventId, exception, "literal")
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LiteralInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging

                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub

                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub

                	Sub M(logger As ILogger)
                		logger.StaticLogLevel("literal")
                		logger.DynamicLogLevel(LogLevel.Debug, "literal")
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LocalInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        Dim local As String = "local"

                        logger.Log(LogLevel.Trace, eventId, local, exception, formatter)
                        logger.Log(LogLevel.Debug, local)
                        logger.Log(LogLevel.Information, eventId, local)
                        logger.Log(LogLevel.Warning, exception, local)
                        logger.Log(LogLevel.[Error], eventId, exception, local)
                    End Sub
                End Class
                
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task LocalInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        Dim local As String = "local"

                        logger.Log{{logLevel}}("literal")
                        logger.Log{{logLevel}}(eventId, "literal")
                        logger.Log{{logLevel}}(exception, "literal")
                        logger.Log{{logLevel}}(eventId, exception, "literal")
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task LocalInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        Dim local As String = "local"

                        logger.StaticLogLevel(local)
                        logger.DynamicLogLevel(LogLevel.Debug, local)
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task FieldInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _field As String

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, _field, exception, formatter)
                        logger.Log(LogLevel.Debug, _field)
                        logger.Log(LogLevel.Information, eventId, _field)
                        logger.Log(LogLevel.Warning, exception, _field)
                        logger.Log(LogLevel.[Error], eventId, exception, _field)
                    End Sub
                End Class
                
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task FieldInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Private _field As String

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}(_field)
                        logger.Log{{logLevel}}(eventId, _field)
                        logger.Log{{logLevel}}(exception, _field)
                        logger.Log{{logLevel}}(eventId, exception, _field)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task FieldInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Private _field As String

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        logger.StaticLogLevel(_field)
                        logger.DynamicLogLevel(LogLevel.Debug, _field)
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PropertyInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Public Property [Property] As String

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, [Property], exception, formatter)
                        logger.Log(LogLevel.Debug, [Property])
                        logger.Log(LogLevel.Information, eventId, [Property])
                        logger.Log(LogLevel.Warning, exception, [Property])
                        logger.Log(LogLevel.[Error], eventId, exception, [Property])
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task PropertyInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Public Property [Property] As String

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}([Property])
                        logger.Log{{logLevel}}(eventId, [Property])
                        logger.Log{{logLevel}}(exception, [Property])
                        logger.Log{{logLevel}}(eventId, exception, [Property])
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task PropertyInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Public Property [Property] As String

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        logger.StaticLogLevel([Property])
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IndexerInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Collections.Generic
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _messages As Dictionary(Of LogLevel, String)

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, _messages(LogLevel.Trace), exception, formatter)
                        logger.Log(LogLevel.Debug, _messages(LogLevel.Debug))
                        logger.Log(LogLevel.Information, eventId, _messages(LogLevel.Information))
                        logger.Log(LogLevel.Warning, exception, _messages(LogLevel.Warning))
                        logger.Log(LogLevel.[Error], eventId, exception, _messages(LogLevel.[Error]))
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task IndexerInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Collections.Generic
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Private _messages As Dictionary(Of LogLevel, String)

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}(_messages(LogLevel.{{logLevel}}))
                        logger.Log{{logLevel}}(eventId, _messages(LogLevel.{{logLevel}}))
                        logger.Log{{logLevel}}(exception, _messages(LogLevel.{{logLevel}}))
                        logger.Log{{logLevel}}(eventId, exception, _messages(LogLevel.{{logLevel}}))
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IndexerInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Collections.Generic
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Private _messages As Dictionary(Of LogLevel, String)

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        logger.StaticLogLevel(_messages(LogLevel.Information))
                        logger.DynamicLogLevel(LogLevel.Debug, _messages(LogLevel.Debug))
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayIndexerInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _messages As String()

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, _messages(0), exception, formatter)
                        logger.Log(LogLevel.Debug, _messages(0))
                        logger.Log(LogLevel.Information, eventId, _messages(0))
                        logger.Log(LogLevel.Warning, exception, _messages(0))
                        logger.Log(LogLevel.[Error], eventId, exception, _messages(0))
                    End Sub
                End Class
                
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task ArrayIndexerInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Private _messages As String()

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}(_messages(0))
                        logger.Log{{logLevel}}(eventId, _messages(0))
                        logger.Log{{logLevel}}(exception, _messages(0))
                        logger.Log{{logLevel}}(eventId, exception, _messages(0))
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayIndexerInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Private _messages As String()

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        logger.StaticLogLevel(_messages(0))
                        logger.DynamicLogLevel(LogLevel.Debug, _messages(0))
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ConditionalAccessInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Trace, eventId, exception?.Message, exception, formatter)
                        logger.Log(LogLevel.Debug, exception?.Message)
                        logger.Log(LogLevel.Information, eventId, exception?.Message)
                        logger.Log(LogLevel.Warning, exception, exception?.Message)
                        logger.Log(LogLevel.[Error], eventId, exception, exception?.Message)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task ConditionalAccessInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log{{logLevel}}(exception?.Message)
                        logger.Log{{logLevel}}(eventId, exception?.Message)
                        logger.Log{{logLevel}}(exception, exception?.Message)
                        logger.Log{{logLevel}}(eventId, exception, exception?.Message)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ConditionalAccessInLoggerMessage_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.Information, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger, exception As Exception)
                        logger.StaticLogLevel(exception?.Message)
                        logger.DynamicLogLevel(LogLevel.Debug, exception?.Message)
                	End Sub
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task OtherILoggerMethodCalled_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger)
                        logger.BeginScope(ExpensiveMethodCall())
                        logger.BeginScope("Processing calculation result {CalculationResult}", ExpensiveMethodCall())
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        // Tests for operations that get flagged.

        [Fact]
        public async Task AnonymousObjectCreationOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|New With {.Test = "42"}|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ArrayCreationOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|New Integer(9) {}|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task AwaitOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Threading.Tasks
                Imports Microsoft.Extensions.Logging

                Class C
                    Async Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String), task As Task(Of String))
                        logger.Log(LogLevel.Debug, eventId, [|Await task|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task BinaryOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|4 + 2|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CoalesceOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|If(exception, New Exception())|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task InterpolatedStringOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), input As Integer)
                        logger.Log(LogLevel.Debug, eventId, [|$"{input}"|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task InvocationOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|exception.ToString()|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task IsTypeOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Boolean, Exception, String), input As Object)
                        logger.Log(LogLevel.Debug, eventId, [|TypeOf input Is Exception|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NameOfOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|NameOf(logger)|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task ObjectCreationOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Exception, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|New Exception()|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task TypeOfOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Type, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|GetType(Integer)|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task UnaryOperation_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Boolean, Exception, String), input As Boolean)
                        logger.Log(LogLevel.Debug, eventId, [|Not input|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        // Tests for work done in indexers, array element references or instances of member references.

        [Fact]
        public async Task WorkInIndexerInstance_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Collections.Generic
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall()(LogLevel.Debug)|], exception, formatter)
                    End Sub

                    Function ExpensiveMethodCall() As Dictionary(Of LogLevel, String)
                        Return Nothing
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInIndexerArgument_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports System.Collections.Generic
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _messages As Dictionary(Of LogLevel, String)

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Object, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|_messages(ExpensiveMethodCall())|], exception, formatter)
                    End Sub

                    Function ExpensiveMethodCall() As LogLevel
                        Return LogLevel.Debug
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInConditionalAccess_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|exception?.ToString()|], exception, formatter)
                    End Sub
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInFieldInstance_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _field As Integer

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Integer, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall()._field|], exception, formatter)
                    End Sub

                    Function ExpensiveMethodCall() As C
                        Return New C()
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInPropertyInstance_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Integer, Exception, String))
                        logger.Log(LogLevel.Debug, eventId, [|ExpensiveMethodCall().Length|], exception, formatter)
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WorkInArrayReference_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Private _field As Integer

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of Integer, Exception, String), input As Integer())
                        logger.Log(LogLevel.Debug, eventId, [|input(ExpensiveMethodCall())|], exception, formatter)
                    End Sub

                    Function ExpensiveMethodCall() As Integer
                        Return 0
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        // Tests when log call is guarded.

        [Fact]
        public async Task GuardedWorkInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.Trace) Then logger.Log(LogLevel.Trace, eventId, ExpensiveMethodCall(), exception, formatter)
                        If logger.IsEnabled(LogLevel.Debug) Then logger.Log(LogLevel.Debug, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.Information) Then logger.Log(LogLevel.Information, eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.Warning) Then logger.Log(LogLevel.Warning, exception, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.[Error]) Then logger.Log(LogLevel.[Error], eventId, exception, ExpensiveMethodCall())
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task GuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), level As LogLevel)
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter)
                        If logger.IsEnabled(level) Then logger.Log(level, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, exception, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, exception, ExpensiveMethodCall())
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task GuardedWorkInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log{{logLevel}}(ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log{{logLevel}}(eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log{{logLevel}}(exception, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log{{logLevel}}(eventId, exception, ExpensiveMethodCall())
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task GuardedWorkInLoggerMessage_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub

                	Sub M(logger As ILogger)
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.StaticLogLevel(ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall())
                	End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NestedGuardedWorkInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.Debug)
                            If exception IsNot Nothing
                                logger.Log(LogLevel.Debug, eventId, ExpensiveMethodCall(), exception, formatter)
                                logger.Log(LogLevel.Debug, ExpensiveMethodCall())
                                logger.Log(LogLevel.Debug, eventId, ExpensiveMethodCall())
                                logger.Log(LogLevel.Debug, exception, ExpensiveMethodCall())
                                logger.Log(LogLevel.Debug, eventId, exception, ExpensiveMethodCall())
                            End If
                        End If
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task NestedGuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), level As LogLevel)
                        If logger.IsEnabled(level) Then
                            If exception IsNot Nothing Then
                                logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter)
                                logger.Log(level, ExpensiveMethodCall())
                                logger.Log(level, eventId, ExpensiveMethodCall())
                                logger.Log(level, exception, ExpensiveMethodCall())
                                logger.Log(level, eventId, exception, ExpensiveMethodCall())
                            End If
                        End If
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task NestedGuardedWorkInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging

                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If exception IsNot Nothing
                                logger.Log{{logLevel}}(ExpensiveMethodCall())
                            End If
                        End If
                        
                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If exception IsNot Nothing
                                logger.Log{{logLevel}}(eventId, ExpensiveMethodCall())
                            End If
                        End If

                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If exception IsNot Nothing
                                logger.Log{{logLevel}}(exception, ExpensiveMethodCall())
                            End If
                        End If

                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If exception IsNot Nothing
                                logger.Log{{logLevel}}(eventId, exception, ExpensiveMethodCall())
                            End If
                        End If
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task NestedGuardedWorkInLoggerMessage_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Public Property IsExpensiveComputationEnabled As Boolean

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If IsExpensiveComputationEnabled
                                logger.StaticLogLevel(ExpensiveMethodCall())
                            End If
                        End If

                        If logger.IsEnabled(LogLevel.{{logLevel}})
                            If IsExpensiveComputationEnabled
                                logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall())
                            End If
                        End If
                	End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CustomLoggerGuardedWorkInLog_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class CustomLogger
                  Implements ILogger

                  Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log
                    Throw New NotImplementedException()
                  End Sub

                  Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled
                    Throw New NotImplementedException()
                  End Function

                  Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope
                    Throw New NotImplementedException()
                  End Function
                End Class

                Class C
                    Sub M(logger As CustomLogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.Trace) Then logger.Log(LogLevel.Trace, eventId, ExpensiveMethodCall(), exception, formatter)
                        If logger.IsEnabled(LogLevel.Debug) Then logger.Log(LogLevel.Debug, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.Information) Then logger.Log(LogLevel.Information, eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.Warning) Then logger.Log(LogLevel.Warning, exception, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.[Error]) Then logger.Log(LogLevel.[Error], eventId, exception, ExpensiveMethodCall())
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task CustomLoggerGuardedWorkInLogWithDynamicLogLevel_NoDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging

                Class CustomLogger
                  Implements ILogger
                
                  Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log
                    Throw New NotImplementedException()
                  End Sub
                
                  Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled
                    Throw New NotImplementedException()
                  End Function
                
                  Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope
                    Throw New NotImplementedException()
                  End Function
                End Class

                Class C
                    Sub M(logger As CustomLogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), level As LogLevel)
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, ExpensiveMethodCall(), exception, formatter)
                        If logger.IsEnabled(level) Then logger.Log(level, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, exception, ExpensiveMethodCall())
                        If logger.IsEnabled(level) Then logger.Log(level, eventId, exception, ExpensiveMethodCall())
                    End Sub

                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task CustomLoggerGuardedWorkInLogNamed_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class CustomLogger
                  Implements ILogger
                
                  Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log
                    Throw New NotImplementedException()
                  End Sub
                
                  Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled
                    Throw New NotImplementedException()
                  End Function
                
                  Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope
                    Throw New NotImplementedException()
                  End Function
                End Class

                Class C
                    Sub M(logger As CustomLogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log(LogLevel.{{logLevel}}, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log(LogLevel.{{logLevel}}, eventId, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log(LogLevel.{{logLevel}}, exception, ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.Log(LogLevel.{{logLevel}}, eventId, exception, ExpensiveMethodCall())
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task CustomLoggerGuardedWorkInLoggerMessage_NoDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Class CustomLogger
                  Implements ILogger
                
                  Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log
                    Throw New NotImplementedException()
                  End Sub
                
                  Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled
                    Throw New NotImplementedException()
                  End Function
                
                  Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope
                    Throw New NotImplementedException()
                  End Function
                End Class

                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As CustomLogger)
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.StaticLogLevel(ExpensiveMethodCall())
                        If logger.IsEnabled(LogLevel.{{logLevel}}) Then logger.DynamicLogLevel(LogLevel.{{logLevel}}, ExpensiveMethodCall())
                	End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongLogLevelGuardedWorkInLog_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If logger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|])                
                        If logger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.[Error], eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongLogLevelGuardedWorkInLogNamed_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If logger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If logger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, exception, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongLogLevelGuardedWorkInLoggerMessage_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        If logger.IsEnabled(LogLevel.None) Then logger.StaticLogLevel([|ExpensiveMethodCall()|])
                        If logger.IsEnabled(LogLevel.None) Then logger.DynamicLogLevel(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                	End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongDynamicLogLevelGuardedWorkInLog_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), level As LogLevel)
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|])                
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.[Error], eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongDynamicLogLevelGuardedWorkInLogNamed_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String), level As LogLevel)
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.{{logLevel}}, exception, [|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.Log(LogLevel.{{logLevel}}, eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongDynamicLogLevelGuardedWorkInLoggerMessage_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger, level As LogLevel)
                        If logger.IsEnabled(level) Then logger.StaticLogLevel([|ExpensiveMethodCall()|])
                        If logger.IsEnabled(level) Then logger.DynamicLogLevel(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                	End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Fact]
        public async Task WrongInstanceGuardedWorkInLog_ReportsDiagnostic_VB()
        {
            string source = """
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Private _otherLogger As ILogger

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If _otherLogger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Trace, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If _otherLogger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Debug, [|ExpensiveMethodCall()|])                
                        If _otherLogger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Information, eventId, [|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.Warning, exception, [|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.Critical) Then logger.Log(LogLevel.[Error], eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongInstanceGuardedWorkInLogNamed_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports Microsoft.Extensions.Logging
                
                Class C
                    Private _otherLogger As ILogger

                    Sub M(logger As ILogger, eventId As EventId, exception As Exception, formatter As Func(Of String, Exception, String))
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|], exception, formatter)
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, [|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, exception, [|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.Log(LogLevel.{{logLevel}}, eventId, exception, [|ExpensiveMethodCall()|])
                    End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Class
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public async Task WrongInstanceGuardedWorkInLoggerMessage_ReportsDiagnostic_VB(string logLevel)
        {
            string source = $$"""
                Imports System
                Imports System.Runtime.CompilerServices
                Imports Microsoft.Extensions.Logging
                
                Partial Module C
                    Private _otherLogger As ILogger

                	<Extension>
                	<LoggerMessage(EventId:=0, Level:=LogLevel.{{logLevel}}, Message:="Static log level `{message}`")>
                	Partial Private Sub StaticLogLevel(logger As ILogger, message As String)
                	End Sub
                
                	<Extension>
                	<LoggerMessage(EventId:=1, Message:="Dynamic log level `{message}`")>
                	Partial Private Sub DynamicLogLevel(logger As ILogger, level As LogLevel, message As String)
                	End Sub
                
                	Sub M(logger As ILogger)
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.StaticLogLevel([|ExpensiveMethodCall()|])
                        If _otherLogger.IsEnabled(LogLevel.None) Then logger.DynamicLogLevel(LogLevel.{{logLevel}}, [|ExpensiveMethodCall()|])
                	End Sub
                
                    Function ExpensiveMethodCall() As String
                        Return "very expensive call"
                    End Function
                End Module
                """;

            await VerifyBasicCodeFixAsync(source, source);
        }

        // Helpers

        private static async Task VerifyCSharpCodeFixAsync(string source, string fixedSource, CodeAnalysis.CSharp.LanguageVersion? languageVersion = null)
        {
            await new VerifyCS.Test
            {
                TestCode = source,
                FixedCode = fixedSource,
                ReferenceAssemblies = Net60WithMELogging,
                LanguageVersion = languageVersion ?? CodeAnalysis.CSharp.LanguageVersion.CSharp10
            }.RunAsync();
        }

        private static async Task VerifyBasicCodeFixAsync(string source, string fixedSource, CodeAnalysis.VisualBasic.LanguageVersion? languageVersion = null)
        {
            await new VerifyVB.Test
            {
                TestCode = source,
                FixedCode = fixedSource,
                ReferenceAssemblies = Net60WithMELogging,
                LanguageVersion = languageVersion ?? CodeAnalysis.VisualBasic.LanguageVersion.VisualBasic16_9
            }.RunAsync();
        }

        private static readonly ReferenceAssemblies Net60WithMELogging =
            ReferenceAssemblies.Net.Net60.AddPackages([new PackageIdentity("Microsoft.Extensions.Logging", "6.0.0")]);
    }
}