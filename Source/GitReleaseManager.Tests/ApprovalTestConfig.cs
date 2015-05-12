//-----------------------------------------------------------------------
// <copyright file="ApprovalTestConfig.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(AllFailingTestsClipboardReporter), typeof(DiffReporter))]