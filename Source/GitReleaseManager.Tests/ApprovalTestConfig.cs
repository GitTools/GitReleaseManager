//-----------------------------------------------------------------------
// <copyright file="ApprovalTestConfig.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(AllFailingTestsClipboardReporter), typeof(DiffReporter))]