using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using QuickLook.Common.Commands;
using QuickLook.Common.Controls;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using QuickLook.Common.Plugin.MoreMenu;

[assembly: AssemblyVersion("1.1.0.0")]

#if SOURCE
namespace QuickLook.Plugin.SixelSource
#else
namespace QuickLook.Plugin.SixelViewer
#endif
{
    public sealed class Plugin : IViewer, IMoreMenu
    {
#if SOURCE
        const string Host = "QuickLook.Plugin.TextViewer";
        const string Target = "QuickLook.Plugin.SixelViewer";
        const string Label = "MW_ReopenAsImagePreview";
        const string Icon = FontSymbols.Picture;
#else
        const string Host = "QuickLook.Plugin.ImageViewer";
        const string Target = "QuickLook.Plugin.SixelSource";
        const string Label = "MW_ReopenAsSourceCode";
        const string Icon = FontSymbols.Code;
#endif
        IViewer viewer;
        string file;

        public int Priority { get { return 10; } }
        public void Init() { }

        public bool CanHandle(string path)
        {
#if SOURCE
            return false; // Explicit reopen only.
#else
            return (path.EndsWith(".six", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".sixel", StringComparison.OrdinalIgnoreCase)) && File.Exists(path);
#endif
        }

        public void Prepare(string path, ContextObject context)
        {
            file = path;
            var type = Type.GetType(Host + ".Plugin, " + Host, true);
            viewer = (IViewer)Activator.CreateInstance(type);
            viewer.Prepare(path, context);
        }

        public void View(string path, ContextObject context)
        {
            viewer.View(path, context);
        }

        public IEnumerable<IMenuItem> MenuItems
        {
            get
            {
                var item = new MoreMenuItem();
                item.Icon = Icon;
                item.Header = TranslationHelper.Get(Label, domain: Host);
                item.Command = new RelayCommand(Reopen);
                return new IMenuItem[] { item };
            }
        }

        void Reopen() { PluginHelper.InvokePluginPreview(Target, file); }

        public void Cleanup()
        {
            if (viewer != null) viewer.Cleanup();
            viewer = null;
            file = null;
        }
    }
}
