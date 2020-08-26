namespace GitReleaseManager.Core.ReleaseNotes
{
    public static class ReleaseNotesTemplate
    {
        public const string Default = @"{{
if issues_count > 0
    if commits_count > 0
        ""As part of this release we had ["" + commits_text + ""]("" + commits_link + "") which resulted in ["" + issues_text + ""]("" + milestone_html_url + ""?closed=1) being closed.""
    else
        ""As part of this release we had ["" + issues_text + ""]("" + milestone_html_url + ""?closed=1) closed.""
    end

else if commits_count > 0
    ""As part of this release we had ["" + commits_text + ""]("" + commits_link + "").""
end
}}
{{ milestone_description }}

{{~ for issue_label in issue_labels ~}}
__{{ issue_label }}__

{{~ for issue in issues[issue_label] ~}}
- [__#{{ issue.number }}__]({{ issue.html_url }}) {{ issue.title }}
{{~ end ~}}

{{~ end ~}}
{{~ if include_footer ~}}
### {{ footer_heading }}
{{ footer_content }}
{{~ end ~}}";
    }
}