using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GitReleaseManager.Core.Provider;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Octokit;
using Shouldly;
using ApiException = GitReleaseManager.Core.Exceptions.ApiException;
using Issue = GitReleaseManager.Core.Model.Issue;
using IssueComment = GitReleaseManager.Core.Model.IssueComment;
using ItemState = GitReleaseManager.Core.Model.ItemState;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
using Label = GitReleaseManager.Core.Model.Label;
using Milestone = GitReleaseManager.Core.Model.Milestone;
using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
using RateLimit = GitReleaseManager.Core.Model.RateLimit;
using Release = GitReleaseManager.Core.Model.Release;
using ReleaseAssetUpload = GitReleaseManager.Core.Model.ReleaseAssetUpload;

namespace GitReleaseManager.Core.Tests.Provider
{
    [TestFixture]
    public class GitHubProviderTests
    {
        private const string OWNER = "owner";
        private const string REPOSITORY = "repository";
        private const string BASE = "0.1.0";
        private const string HEAD = "0.5.0";
        private const int MILESTONE_NUMBER = 1;
        private const string MILESTONE_NUMBER_STRING = "1";
        private const string MILESTONE_TITLE = "0.1.0";
        private const int ISSUE_NUMBER = 1;
        private const string ISSUE_COMMENT = "Issue Comment";
        private const string LABEL_NAME = "Label";
        private const string TAG_NAME = "0.1.0";
        private const int RELEASE_ID = 1;
        private const int ASSET_ID = 1;
        private const string NOT_FOUND_MESSAGE = "NotFound";
        private const bool SKIP_PRERELEASES = false;

        private readonly Release _release = new Release();
        private readonly ReleaseAssetUpload _releaseAssetUpload = new ReleaseAssetUpload();
        private readonly Octokit.NewLabel _newLabel = new Octokit.NewLabel(LABEL_NAME, "ffffff");
        private readonly Octokit.NewRelease _newRelease = new Octokit.NewRelease(TAG_NAME);
        private readonly Exception _exception = new Exception("API Error");
        private readonly Octokit.NotFoundException _notFoundException = new Octokit.NotFoundException(NOT_FOUND_MESSAGE, HttpStatusCode.NotFound);

        private IMapper _mapper;
        private IGitHubClient _gitHubClient;
        private GitHubProvider _gitHubProvider;

        [SetUp]
        public void Setup()
        {
            _mapper = Substitute.For<IMapper>();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _gitHubProvider = new GitHubProvider(_gitHubClient, _mapper);
        }

        // Assets
        [Test]
        public async Task Should_Delete_Asset()
        {
            _gitHubClient.Repository.Release.DeleteAsset(OWNER, REPOSITORY, ASSET_ID)
                .Returns(Task.FromResult);

            await _gitHubProvider.DeleteAssetAsync(OWNER, REPOSITORY, ASSET_ID).ConfigureAwait(false);

            await _gitHubClient.Repository.Release.Received(1).DeleteAsset(OWNER, REPOSITORY, ASSET_ID).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Asset_For_Non_Existing_Id()
        {
            _gitHubClient.Repository.Release.DeleteAsset(OWNER, REPOSITORY, ASSET_ID)
                .Returns(Task.FromException(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.DeleteAssetAsync(OWNER, REPOSITORY, ASSET_ID)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Asset()
        {
            _gitHubClient.Repository.Release.DeleteAsset(OWNER, REPOSITORY, ASSET_ID)
                .Returns(Task.FromException(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.DeleteAssetAsync(OWNER, REPOSITORY, ASSET_ID)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Upload_Asset()
        {
            var octokitRelease = new Octokit.Release();
            var octokitReleaseAssetUpload = new Octokit.ReleaseAssetUpload();

            _mapper.Map<Octokit.Release>(_release)
                .Returns(octokitRelease);

            _mapper.Map<Octokit.ReleaseAssetUpload>(_releaseAssetUpload)
                .Returns(octokitReleaseAssetUpload);

            _gitHubClient.Repository.Release.UploadAsset(octokitRelease, octokitReleaseAssetUpload)
                .Returns(Task.FromResult(new Octokit.ReleaseAsset()));

            await _gitHubProvider.UploadAssetAsync(_release, _releaseAssetUpload).ConfigureAwait(false);

            _mapper.Received(1).Map<Octokit.Release>(_release);
            _mapper.Received(1).Map<Octokit.ReleaseAssetUpload>(_releaseAssetUpload);
            await _gitHubClient.Repository.Release.Received(1).UploadAsset(octokitRelease, octokitReleaseAssetUpload).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Uploading_Asset_For_Non_Existing_Release()
        {
            _mapper.Map<Octokit.Release>(_release)
                .Returns(new Octokit.Release());

            _mapper.Map<Octokit.ReleaseAssetUpload>(_releaseAssetUpload)
                .Returns(new Octokit.ReleaseAssetUpload());

            _gitHubClient.Repository.Release.UploadAsset(Arg.Any<Octokit.Release>(), Arg.Any<Octokit.ReleaseAssetUpload>())
                .Returns(Task.FromException<Octokit.ReleaseAsset>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.UploadAssetAsync(_release, _releaseAssetUpload)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Uploading_Asset()
        {
            _mapper.Map<Octokit.Release>(_release)
                .Returns(new Octokit.Release());

            _mapper.Map<Octokit.ReleaseAssetUpload>(_releaseAssetUpload)
                .Returns(new Octokit.ReleaseAssetUpload());

            _gitHubClient.Repository.Release.UploadAsset(Arg.Any<Octokit.Release>(), Arg.Any<Octokit.ReleaseAssetUpload>())
                .Returns(Task.FromException<Octokit.ReleaseAsset>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.UploadAssetAsync(_release, _releaseAssetUpload)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        // Commits
        [Test]
        public async Task Should_Get_Commits_Count()
        {
            var commitsCount = 12;

            _gitHubClient.Repository.Commit.Compare(OWNER, REPOSITORY, BASE, HEAD)
                .Returns(Task.FromResult(new CompareResult(null, null, null, null, null, null, null, null, commitsCount, 0, 0, null, null)));

            var result = await _gitHubProvider.GetCommitsCount(OWNER, REPOSITORY, BASE, HEAD).ConfigureAwait(false);
            result.ShouldBe(commitsCount);

            await _gitHubClient.Repository.Commit.Received(1).Compare(OWNER, REPOSITORY, BASE, HEAD).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Get_Commits_Count_Zero_If_No_Commits_Found()
        {
            _gitHubClient.Repository.Commit.Compare(OWNER, REPOSITORY, BASE, HEAD)
                .Returns(Task.FromException<CompareResult>(_notFoundException));

            var result = await _gitHubProvider.GetCommitsCount(OWNER, REPOSITORY, BASE, HEAD).ConfigureAwait(false);
            result.ShouldBe(0);

            await _gitHubClient.Repository.Commit.Received(1).Compare(OWNER, REPOSITORY, BASE, HEAD).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Commits_Count()
        {
            _gitHubClient.Repository.Commit.Compare(OWNER, REPOSITORY, BASE, HEAD)
                .Returns(Task.FromException<CompareResult>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetCommitsCount(OWNER, REPOSITORY, BASE, HEAD)).ConfigureAwait(false);
            ex.Message.ShouldContain(_exception.Message);
            ex.InnerException.ShouldBeSameAs(_exception);
        }

        [TestCase("0.1.0", null, "https://github.com/owner/repository/commits/0.1.0")]
        [TestCase("0.5.0", "0.1.0", "https://github.com/owner/repository/compare/0.1.0...0.5.0")]
        public void Should_Get_Commits_Url(string head, string @base, string expectedResult)
        {
            var result = _gitHubProvider.GetCommitsUrl(OWNER, REPOSITORY, head, @base);
            result.ShouldBe(expectedResult);
        }

        [TestCaseSource(nameof(GetCommitsUrl_TestCases))]
        public void Should_Throw_An_Exception_If_Parameter_Is_Invalid(string owner, string repository, string head, string paramName, Type expectedException)
        {
            var ex = Should.Throw(() => _gitHubProvider.GetCommitsUrl(owner, repository, head), expectedException);
            ex.Message.ShouldContain(paramName);
        }

        public static IEnumerable GetCommitsUrl_TestCases()
        {
            var typeArgumentException = typeof(ArgumentException);
            var typeArgumentNullException = typeof(ArgumentNullException);

            yield return new TestCaseData(null, null, null, "owner", typeArgumentNullException);
            yield return new TestCaseData(string.Empty, null, null, "owner", typeArgumentException);
            yield return new TestCaseData(" ", null, null, "owner", typeArgumentException);

            yield return new TestCaseData("owner", null, null, "repository", typeArgumentNullException);
            yield return new TestCaseData("owner", string.Empty, null, "repository", typeArgumentException);
            yield return new TestCaseData("owner", " ", null, "repository", typeArgumentException);

            yield return new TestCaseData("owner", "repository", null, "head", typeArgumentNullException);
            yield return new TestCaseData("owner", "repository", string.Empty, "head", typeArgumentException);
            yield return new TestCaseData("owner", "repository", " ", "head", typeArgumentException);
        }

        // Issues
        [Test]
        public async Task Should_Create_Issue_Comment()
        {
            _gitHubClient.Issue.Comment.Create(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT)
                .Returns(Task.FromResult(new Octokit.IssueComment()));

            await _gitHubProvider.CreateIssueCommentAsync(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT).ConfigureAwait(false);

            await _gitHubClient.Issue.Comment.Received(1).Create(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Creating_Issue_Comment_For_Non_Existing_Issue_Number()
        {
            _gitHubClient.Issue.Comment.Create(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT)
                .Returns(Task.FromException<Octokit.IssueComment>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.CreateIssueCommentAsync(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Creating_Issue_Comment()
        {
            _gitHubClient.Issue.Comment.Create(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT)
                .Returns(Task.FromException<Octokit.IssueComment>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.CreateIssueCommentAsync(OWNER, REPOSITORY, ISSUE_NUMBER, ISSUE_COMMENT)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [TestCase(ItemStateFilter.Open)]
        [TestCase(ItemStateFilter.Closed)]
        [TestCase(ItemStateFilter.All)]
        public async Task Should_Get_Issues_For_Milestone(ItemStateFilter itemStateFilter)
        {
            var issues = new List<Issue>();

            _gitHubClient.Issue.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<RepositoryIssueRequest>(), Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Issue>)new List<Octokit.Issue>()));

            _mapper.Map<IEnumerable<Issue>>(Arg.Any<object>())
                .Returns(issues);

            var result = await _gitHubProvider.GetIssuesAsync(OWNER, REPOSITORY, MILESTONE_NUMBER, itemStateFilter).ConfigureAwait(false);
            result.ShouldBeSameAs(issues);

            await _gitHubClient.Issue.Received(1).GetAllForRepository(
                OWNER,
                REPOSITORY,
                Arg.Is<RepositoryIssueRequest>(o => o.Milestone == MILESTONE_NUMBER_STRING && o.State == (Octokit.ItemStateFilter)itemStateFilter),
                Arg.Any<ApiOptions>()).ConfigureAwait(false);

            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Issue>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Issues_For_Non_Existent_Milestone()
        {
            _gitHubClient.Issue.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<RepositoryIssueRequest>(), Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Issue>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetIssuesAsync(OWNER, REPOSITORY, 1)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Get_Issue_Comments()
        {
            var comments = new List<IssueComment>();

            _gitHubClient.Issue.Comment.GetAllForIssue(OWNER, REPOSITORY, ISSUE_NUMBER, Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.IssueComment>)new List<Octokit.IssueComment>()));

            _mapper.Map<IEnumerable<IssueComment>>(Arg.Any<object>())
                .Returns(comments);

            var result = await _gitHubProvider.GetIssueCommentsAsync(OWNER, REPOSITORY, ISSUE_NUMBER).ConfigureAwait(false);
            result.ShouldBeSameAs(comments);

            await _gitHubClient.Issue.Comment.Received(1).GetAllForIssue(OWNER, REPOSITORY, ISSUE_NUMBER, Arg.Any<ApiOptions>()).ConfigureAwait(false);
            _mapper.Received(1).Map<IEnumerable<IssueComment>>(Arg.Any<object>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Issue_Comments_For_Non_Existing_Issue_Number()
        {
            _gitHubClient.Issue.Comment.GetAllForIssue(OWNER, REPOSITORY, ISSUE_NUMBER, Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.IssueComment>>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.GetIssueCommentsAsync(OWNER, REPOSITORY, ISSUE_NUMBER)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Issue_Comments()
        {
            _gitHubClient.Issue.Comment.GetAllForIssue(OWNER, REPOSITORY, ISSUE_NUMBER, Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.IssueComment>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetIssueCommentsAsync(OWNER, REPOSITORY, ISSUE_NUMBER)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        // Labels
        [Test]
        public async Task Should_Create_Label()
        {
            _mapper.Map<Octokit.NewLabel>(Arg.Any<Label>())
                .Returns(_newLabel);

            _gitHubClient.Issue.Labels.Create(OWNER, REPOSITORY, _newLabel)
                .Returns(Task.FromResult(new Octokit.Label()));

            await _gitHubProvider.CreateLabelAsync(OWNER, REPOSITORY, new Label()).ConfigureAwait(false);

            await _gitHubClient.Issue.Labels.Received(1).Create(OWNER, REPOSITORY, _newLabel).ConfigureAwait(false);
            _mapper.Received(1).Map<Octokit.NewLabel>(Arg.Any<Label>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Creating_Label()
        {
            var label = new Label();

            _mapper.Map<Octokit.NewLabel>(label)
                .Returns(_newLabel);

            _gitHubClient.Issue.Labels.Create(OWNER, REPOSITORY, _newLabel)
                .Returns(Task.FromException<Octokit.Label>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.CreateLabelAsync(OWNER, REPOSITORY, label)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Delete_Label()
        {
            _gitHubClient.Issue.Labels.Delete(OWNER, REPOSITORY, LABEL_NAME)
                .Returns(Task.FromResult);

            await _gitHubProvider.DeleteLabelAsync(OWNER, REPOSITORY, LABEL_NAME).ConfigureAwait(false);

            await _gitHubClient.Issue.Labels.Received(1).Delete(OWNER, REPOSITORY, LABEL_NAME).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Label_For_Non_Existing_Label()
        {
            _gitHubClient.Issue.Labels.Delete(OWNER, REPOSITORY, LABEL_NAME)
                .Returns(Task.FromException<IReadOnlyList<Octokit.Label>>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.DeleteLabelAsync(OWNER, REPOSITORY, LABEL_NAME)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Label()
        {
            _gitHubClient.Issue.Labels.Delete(OWNER, REPOSITORY, LABEL_NAME)
                .Returns(Task.FromException<IReadOnlyList<Octokit.Label>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.DeleteLabelAsync(OWNER, REPOSITORY, LABEL_NAME)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Get_Labels()
        {
            var labels = new List<Label>();

            _gitHubClient.Issue.Labels.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Label>)new List<Octokit.Label>()));

            _mapper.Map<IEnumerable<Label>>(Arg.Any<object>())
                .Returns(labels);

            var result = await _gitHubProvider.GetLabelsAsync(OWNER, REPOSITORY).ConfigureAwait(false);
            result.ShouldBeSameAs(labels);

            await _gitHubClient.Issue.Labels.Received(1).GetAllForRepository(OWNER, REPOSITORY, Arg.Any<ApiOptions>()).ConfigureAwait(false);
            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Label>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Labels()
        {
            _gitHubClient.Issue.Labels.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Label>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetLabelsAsync(OWNER, REPOSITORY)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        // Milestones
        [TestCase(ItemStateFilter.Open)]
        [TestCase(ItemStateFilter.Closed)]
        [TestCase(ItemStateFilter.All)]
        public async Task Should_Get_Milestone(ItemStateFilter itemStateFilter)
        {
            var milestone = new Milestone { Title = MILESTONE_TITLE };
            var milestones = new List<Milestone> { milestone };

            _gitHubClient.Issue.Milestone.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<MilestoneRequest>(), Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Milestone>)new List<Octokit.Milestone>()));

            _mapper.Map<IEnumerable<Milestone>>(Arg.Any<object>())
                .Returns(milestones);

            var result = await _gitHubProvider.GetMilestoneAsync(OWNER, REPOSITORY, MILESTONE_TITLE, itemStateFilter).ConfigureAwait(false);
            result.ShouldBeSameAs(milestone);

            await _gitHubClient.Issue.Milestone.Received(1).GetAllForRepository(
                OWNER,
                REPOSITORY,
                Arg.Is<MilestoneRequest>(o => o.State == (Octokit.ItemStateFilter)itemStateFilter),
                Arg.Any<ApiOptions>()).ConfigureAwait(false);

            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Milestone>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Milestone_For_Non_Existing_Title()
        {
            _gitHubClient.Issue.Milestone.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<MilestoneRequest>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Milestone>)new List<Octokit.Milestone>()));

            _mapper.Map<IEnumerable<Milestone>>(Arg.Any<object>())
                .Returns(new List<Milestone>());

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.GetMilestoneAsync(OWNER, REPOSITORY, MILESTONE_TITLE)).ConfigureAwait(false);
            ex.Message.ShouldBe(NOT_FOUND_MESSAGE);
            ex.InnerException.ShouldBeNull();
        }

        [TestCase(ItemStateFilter.Open)]
        [TestCase(ItemStateFilter.Closed)]
        [TestCase(ItemStateFilter.All)]
        public async Task Should_Get_Milestones(ItemStateFilter itemStateFilter)
        {
            var milestones = new List<Milestone>();

            _gitHubClient.Issue.Milestone.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<MilestoneRequest>(), Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Milestone>)new List<Octokit.Milestone>()));

            _mapper.Map<IEnumerable<Milestone>>(Arg.Any<object>())
                .Returns(milestones);

            var result = await _gitHubProvider.GetMilestonesAsync(OWNER, REPOSITORY, itemStateFilter).ConfigureAwait(false);
            result.ShouldBeSameAs(milestones);

            await _gitHubClient.Issue.Milestone.Received(1).GetAllForRepository(
                OWNER,
                REPOSITORY,
                Arg.Is<MilestoneRequest>(o => o.State == (Octokit.ItemStateFilter)itemStateFilter),
                Arg.Any<ApiOptions>()).ConfigureAwait(false);

            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Milestone>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Milestones()
        {
            _gitHubClient.Issue.Milestone.GetAllForRepository(OWNER, REPOSITORY, Arg.Any<MilestoneRequest>(), Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Milestone>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetMilestonesAsync(OWNER, REPOSITORY)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [TestCase(ItemState.Closed)]
        [TestCase(ItemState.Open)]
        public async Task Should_Set_Milestone_State(ItemState itemState)
        {
            _gitHubClient.Issue.Milestone.Update(OWNER, REPOSITORY, MILESTONE_NUMBER, Arg.Any<MilestoneUpdate>())
                .Returns(Task.FromResult(new Octokit.Milestone()));

            await _gitHubProvider.SetMilestoneStateAsync(OWNER, REPOSITORY, MILESTONE_NUMBER, itemState).ConfigureAwait(false);

            await _gitHubClient.Issue.Milestone.Received(1).Update(OWNER, REPOSITORY, MILESTONE_NUMBER, Arg.Is<MilestoneUpdate>(o => o.State == (Octokit.ItemState)itemState)).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Setting_Milestone_State_For_Non_Existent_Number()
        {
            _gitHubClient.Issue.Milestone.Update(OWNER, REPOSITORY, MILESTONE_NUMBER, Arg.Any<MilestoneUpdate>())
                .Returns(Task.FromException<Octokit.Milestone>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.SetMilestoneStateAsync(OWNER, REPOSITORY, MILESTONE_NUMBER, ItemState.Closed)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBeSameAs(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Setting_Milestone_State()
        {
            _gitHubClient.Issue.Milestone.Update(OWNER, REPOSITORY, MILESTONE_NUMBER, Arg.Any<MilestoneUpdate>())
                .Returns(Task.FromException<Octokit.Milestone>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.SetMilestoneStateAsync(OWNER, REPOSITORY, MILESTONE_NUMBER, ItemState.Closed)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBeSameAs(_exception);
        }

        // Releases
        [Test]
        public async Task Should_Create_Release()
        {
            var createRelease = new Release();
            var resultRelease = new Release();

            _mapper.Map<Octokit.NewRelease>(createRelease)
                .Returns(_newRelease);

            _gitHubClient.Repository.Release.Create(OWNER, REPOSITORY, _newRelease)
                .Returns(Task.FromResult(new Octokit.Release()));

            _mapper.Map<Release>(Arg.Any<Octokit.Release>())
                .Returns(resultRelease);

            var result = await _gitHubProvider.CreateReleaseAsync(OWNER, REPOSITORY, createRelease).ConfigureAwait(false);
            result.ShouldBeSameAs(resultRelease);

            await _gitHubClient.Repository.Release.Received(1).Create(OWNER, REPOSITORY, _newRelease).ConfigureAwait(false);
            _mapper.Received(1).Map<Octokit.NewRelease>(Arg.Any<Release>());
            _mapper.Received(1).Map<Release>(Arg.Any<Octokit.Release>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Creating_Release()
        {
            var label = new Release();

            _mapper.Map<Octokit.NewRelease>(label)
                .Returns(_newRelease);

            _gitHubClient.Repository.Release.Create(OWNER, REPOSITORY, _newRelease)
                .Returns(Task.FromException<Octokit.Release>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.CreateReleaseAsync(OWNER, REPOSITORY, label)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Delete_Release()
        {
            var id = 1;

            _gitHubClient.Repository.Release.Delete(OWNER, REPOSITORY, id)
                .Returns(Task.CompletedTask);

            await _gitHubProvider.DeleteReleaseAsync(OWNER, REPOSITORY, id).ConfigureAwait(false);

            await _gitHubClient.Repository.Release.Received(1).Delete(OWNER, REPOSITORY, id).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Release_For_Non_Existent_Id()
        {
            var id = 1;

            _gitHubClient.Repository.Release.Delete(OWNER, REPOSITORY, id)
                .Returns(Task.FromException(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.DeleteReleaseAsync(OWNER, REPOSITORY, id)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBeSameAs(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Deleting_Release()
        {
            var id = 1;

            _gitHubClient.Repository.Release.Delete(OWNER, REPOSITORY, id)
                .Returns(Task.FromException(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.DeleteReleaseAsync(OWNER, REPOSITORY, id)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBeSameAs(_exception);
        }

        [Test]
        public async Task Should_Get_Release()
        {
            var release = new Release();

            _gitHubClient.Repository.Release.Get(OWNER, REPOSITORY, TAG_NAME)
                .Returns(Task.FromResult(new Octokit.Release()));

            _mapper.Map<Release>(Arg.Any<object>())
                .Returns(release);

            var result = await _gitHubProvider.GetReleaseAsync(OWNER, REPOSITORY, TAG_NAME).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _gitHubClient.Repository.Release.Received(1).Get(OWNER, REPOSITORY, TAG_NAME).ConfigureAwait(false);
            _mapper.Received(1).Map<Release>(Arg.Any<object>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Release_For_Non_Existent_Tag()
        {
            _gitHubClient.Repository.Release.Get(OWNER, REPOSITORY, TAG_NAME)
                .Returns(Task.FromException<Octokit.Release>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.GetReleaseAsync(OWNER, REPOSITORY, TAG_NAME)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Release()
        {
            _gitHubClient.Repository.Release.Get(OWNER, REPOSITORY, TAG_NAME)
                .Returns(Task.FromException<Octokit.Release>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetReleaseAsync(OWNER, REPOSITORY, TAG_NAME)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Get_Releases()
        {
            var releases = new List<Release>();

            _gitHubClient.Repository.Release.GetAll(OWNER, REPOSITORY, Arg.Any<ApiOptions>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Release>)new List<Octokit.Release>()));

            _mapper.Map<IEnumerable<Release>>(Arg.Any<object>())
                .Returns(releases);

            var result = await _gitHubProvider.GetReleasesAsync(OWNER, REPOSITORY, SKIP_PRERELEASES).ConfigureAwait(false);
            result.ShouldBeSameAs(releases);

            await _gitHubClient.Repository.Release.Received(1).GetAll(OWNER, REPOSITORY, Arg.Any<ApiOptions>()).ConfigureAwait(false);
            _mapper.Received(1).Map<IEnumerable<Release>>(Arg.Any<object>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Releases()
        {
            _gitHubClient.Repository.Release.GetAll(OWNER, REPOSITORY, Arg.Any<ApiOptions>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Release>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetReleasesAsync(OWNER, REPOSITORY, SKIP_PRERELEASES)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Publish_Release()
        {
            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, RELEASE_ID, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromResult(new Octokit.Release()));

            await _gitHubProvider.PublishReleaseAsync(OWNER, REPOSITORY, TAG_NAME, RELEASE_ID).ConfigureAwait(false);

            await _gitHubClient.Repository.Release.Received(1).Edit(OWNER, REPOSITORY, RELEASE_ID, Arg.Is<ReleaseUpdate>(o =>
                o.Draft == false &&
                o.TagName == TAG_NAME)).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Publishing_Release_For_Non_Existent_Id()
        {
            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, RELEASE_ID, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromException<Octokit.Release>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.PublishReleaseAsync(OWNER, REPOSITORY, TAG_NAME, RELEASE_ID)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Publishing_Release()
        {
            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, RELEASE_ID, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromException<Octokit.Release>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.PublishReleaseAsync(OWNER, REPOSITORY, TAG_NAME, RELEASE_ID)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Update_Release()
        {
            var release = new Release
            {
                Id = RELEASE_ID,
                Body = "Body",
                Draft = true,
                Name = "Name",
                Prerelease = true,
                TagName = "TagName",
                TargetCommitish = "TargetCommitish",
            };

            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, release.Id, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromResult(new Octokit.Release()));

            await _gitHubProvider.UpdateReleaseAsync(OWNER, REPOSITORY, release).ConfigureAwait(false);

            await _gitHubClient.Repository.Release.Received(1).Edit(OWNER, REPOSITORY, release.Id, Arg.Is<ReleaseUpdate>(o =>
                o.Body == release.Body &&
                o.Draft == release.Draft &&
                o.Name == release.Name &&
                o.Prerelease == release.Prerelease &&
                o.TagName == release.TagName &&
                o.TargetCommitish == release.TargetCommitish)).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Updating_Release_For_Non_Existent_Id()
        {
            var release = new Release { Id = RELEASE_ID };

            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, release.Id, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromException<Octokit.Release>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.UpdateReleaseAsync(OWNER, REPOSITORY, release)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Updating_Release()
        {
            var release = new Release { Id = RELEASE_ID };

            _gitHubClient.Repository.Release.Edit(OWNER, REPOSITORY, release.Id, Arg.Any<ReleaseUpdate>())
                .Returns(Task.FromException<Octokit.Release>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.UpdateReleaseAsync(OWNER, REPOSITORY, release)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public void Should_Get_Rate_Limit()
        {
            var rateLimit = new RateLimit();
            var apiInfo = new Octokit.ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), string.Empty, new Octokit.RateLimit());

            _gitHubClient.GetLastApiInfo()
                .Returns(apiInfo);

            _mapper.Map<RateLimit>(apiInfo.RateLimit)
                .Returns(rateLimit);

            var result = _gitHubProvider.GetRateLimit();
            result.ShouldBeSameAs(rateLimit);

            _gitHubClient.Received(1).GetLastApiInfo();
            _mapper.Received(1).Map<RateLimit>(apiInfo.RateLimit);
        }

        [Test]
        public void Should_Throw_An_Exception_On_Getting_Rate_Limit()
        {
            _gitHubClient.GetLastApiInfo()
                .Throws(_exception);

            var ex = Should.Throw<ApiException>(() => _gitHubProvider.GetRateLimit());
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }
    }
}