//-----------------------------------------------------------------------
// <copyright file="ApprovalTestConfig.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(AllFailingTestsClipboardReporter), typeof(DiffReporter))]