///////////////////////////////////////////////////////////////////////////////
// ENVIRONMENT VARIABLE NAMES
///////////////////////////////////////////////////////////////////////////////

private static string githubUserNameVariable = "GITTOOLS_GITHUB_USERNAME";
private static string githubPasswordVariable = "GITTOOLS_GITHUB_PASSWORD";
private static string myGetApiKeyVariable = "GITTOOLS_MYGET_API_KEY";
private static string myGetSourceUrlVariable = "GITTOOLS_MYGET_SOURCE";
private static string nuGetApiKeyVariable = "GITTOOLS_NUGET_API_KEY";
private static string nuGetSourceUrlVariable = "GITTOOLS_NUGET_SOURCE";
private static string chocolateyApiKeyVariable = "GITTOOLS_CHOCOLATEY_API_KEY";
private static string chocolateySourceUrlVariable = "GITTOOLS_CHOCOLATEY_SOURCE";
private static string gitterTokenVariable = "GITTOOLS_GITTER_TOKEN";
private static string gitterRoomIdVariable = "GITTOOLS_GITTER_ROOM_ID";
private static string slackTokenVariable = "GITTOOLS_SLACK_TOKEN";
private static string slackChannelVariable = "GITTOOLS_SLACK_CHANNEL";
private static string twitterConsumerKeyVariable = "GITTOOLS_TWITTER_CONSUMER_KEY";
private static string twitterConsumerSecretVariable = "GITTOOLS_TWITTER_CONSUMER_SECRET";
private static string twitterAccessTokenVariable = "GITTOOLS_TWITTER_ACCESS_TOKEN";
private static string twitterAccessTokenSecretVariable = "GITTOOLS_TWITTER_ACCESS_TOKEN_SECRET";
private static string appVeyorApiTokenVariable = "GITTOOLS_APPVEYOR_API_TOKEN";
private static string coverallsRepoTokenVariable = "GITTOOLS_COVERALLS_REPO_TOKEN";

///////////////////////////////////////////////////////////////////////////////
// BUILD ACTIONS
///////////////////////////////////////////////////////////////////////////////

var sendMessageToGitterRoom = false;
var sendMessageToSlackChannel = false;
var sendMessageToTwitter = false;

///////////////////////////////////////////////////////////////////////////////
// PROJECT SPECIFIC VARIABLES
///////////////////////////////////////////////////////////////////////////////

var rootDirectoryPath         = MakeAbsolute(Context.Environment.WorkingDirectory);
var solutionFilePath          = "./Source/GitReleaseManager.sln";
var solutionDirectoryPath     = "./Source/GitReleaseManager";
var title                     = "GitReleaseManager";
var resharperSettingsFileName = "GitReleaseManager.sln.DotSettings";
var repositoryOwner           = "GitTools";
var repositoryName            = "GitReleaseManager";
var appVeyorAccountName       = "GitTools";
var appVeyorProjectSlug       = "gitreleasemanager";

// NOTE: Only populate this, if required, but leave as is otherwise.
var dupFinderExcludePattern   = new string[] { rootDirectoryPath + "/Source/GitReleaseManager.Tests/*.cs" };

var testCoverageFilter = "+[*]* -[xunit.*]* -[Cake.Core]* -[Cake.Testing]* -[*.Tests]* -[Octokit]* -[YamlDotNet]*";
var testCoverageExcludeByAttribute = "*.ExcludeFromCodeCoverage*";
var testCoverageExcludeByFile = "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs";

///////////////////////////////////////////////////////////////////////////////
// CAKE FILES TO LOAD IN
///////////////////////////////////////////////////////////////////////////////

#l .\Tools\gep13.DefaultBuild\Content\appveyor.cake
#l .\Tools\gep13.DefaultBuild\Content\chocolatey.cake
#l .\Tools\gep13.DefaultBuild\Content\coveralls.cake
#l .\Tools\gep13.DefaultBuild\Content\credentials.cake
#l .\Tools\gep13.DefaultBuild\Content\gitreleasemanager.cake
#l .\Tools\gep13.DefaultBuild\Content\gitter.cake
#l .\Tools\gep13.DefaultBuild\Content\gitversion.cake
#l .\Tools\gep13.DefaultBuild\Content\nuget.cake
#l .\Tools\gep13.DefaultBuild\Content\packages.cake
#l .\Tools\gep13.DefaultBuild\Content\parameters.cake
#l .\Tools\gep13.DefaultBuild\Content\paths.cake
#l .\Tools\gep13.DefaultBuild\Content\resharper.cake
#l .\Tools\gep13.DefaultBuild\Content\slack.cake
#l .\Tools\gep13.DefaultBuild\Content\testing.cake
#l .\Tools\gep13.DefaultBuild\Content\twitter.cake
#l .\Tools\gep13.DefaultBuild\Content\build.cake
