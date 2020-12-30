// -----------------------------------------------------------------------
// <copyright file="TemplateFactory.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Templates
{
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Extensions;
    using GitReleaseManager.Core.Helpers;
    using Scriban;
    using Scriban.Runtime;

    public class TemplateFactory
    {
        private readonly IFileSystem fileSystem;
        private readonly Config config;
        private readonly TemplateKind templateKind;

        public TemplateFactory(IFileSystem fileSystem, Config config, TemplateKind templateKind)
        {
            this.fileSystem = fileSystem;
            this.config = config;
            this.templateKind = templateKind;
        }

        public string RenderTemplate(string templateName, object model)
        {
            var loader = new TemplateLoader(config, fileSystem, templateKind);
            var sourcePath = loader.GetPath(null, default, templateName);
            var templateContent = loader.Load(null, default, sourcePath);

            var template = Template.Parse(templateContent);

            var context = CreateTemplateContext(model, loader, sourcePath);
            return template.Render(context);
        }

        public async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            var loader = new TemplateLoader(config, fileSystem, templateKind);
            var sourcePath = loader.GetPath(null, default, templateName);
            var templateContent = await loader.LoadAsync(null, default, sourcePath).ConfigureAwait(false);

            var template = Template.Parse(templateContent);

            var context = CreateTemplateContext(model, loader, sourcePath);
            return await template.RenderAsync(context).ConfigureAwait(false);
        }

        private TemplateContext CreateTemplateContext(object model, TemplateLoader loader, string sourcePath)
        {
            var sc = new ScriptObject();
            sc.Import(model);
            sc.Add("template_kind", templateKind.ToString().ToUpperInvariant());
            sc.Add("config", config);
            sc.ImportMember(typeof(StringExtensions), nameof(StringExtensions.ReplaceMilestoneTitle));
            var context = new TemplateContext
            {
                TemplateLoader = loader,
            };
            context.PushSourceFile(sourcePath);
            context.PushGlobal(sc);
            return context;
        }
    }
}