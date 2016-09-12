///////////////////////////////////////////////////////////////////////////////
// ENVIRONMENT VARIABLE NAMES
///////////////////////////////////////////////////////////////////////////////

private static string githubUserNameVariable = "GITHUB_USERNAME";
private static string githubPasswordVariable = "GITHUB_PASSWORD";
private static string myGetApiKeyVariable = "MYGET_API_KEY";
private static string myGetSourceUrlVariable = "MYGET_SOURCE";
private static string nuGetApiKeyVariable = "NUGET_API_KEY";
private static string nuGetSourceUrlVariable = "NUGET_SOURCE";
private static string chocolateyApiKeyVariable = "CHOCOLATEY_API_KEY";
private static string chocolateySourceUrlVariable = "CHOCOLATEY_SOURCE";
private static string gitterTokenVariable = "GITTER_TOKEN";
private static string gitterRoomIdVariable = "GITTER_ROOM_ID";
private static string slackTokenVariable = "SLACK_TOKEN";
private static string slackChannelVariable = "SLACK_CHANNEL";
private static string twitterConsumerKeyVariable = "TWITTER_CONSUMER_KEY";
private static string twitterConsumerSecretVariable = "TWITTER_CONSUMER_SECRET";
private static string twitterAccessTokenVariable = "TWITTER_ACCESS_TOKEN";
private static string twitterAccessTokenSecretVariable = "TWITTER_ACCESS_TOKEN_SECRET";
private static string appVeyorApiTokenVariable = "APPVEYOR_API_TOKEN";
private static string coverallsRepoTokenVariable = "COVERALLS_REPO_TOKEN";

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
