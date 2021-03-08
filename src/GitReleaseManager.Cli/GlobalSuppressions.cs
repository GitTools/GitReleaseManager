// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Wrong Usage", "DF0037:Marks undisposed objects assinged to a property, originated from a method invocation.", Justification = "Cleanup happens at the end of the application.", Scope = "member", Target = "~M:GitReleaseManager.Cli.Program.ConfigureLogging(GitReleaseManager.Cli.Options.BaseSubOptions)")]