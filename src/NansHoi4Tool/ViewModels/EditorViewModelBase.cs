using NansHoi4Tool.Core;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;

namespace NansHoi4Tool.ViewModels;

public abstract class EditorViewModelBase : ViewModelBase
{
    protected readonly INotificationService Notifications;
    protected readonly ILogService Log;
    protected readonly IProjectService Projects;

    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        protected set => SetProperty(ref _hasUnsavedChanges, value);
    }

    protected EditorViewModelBase(INotificationService notifications, ILogService log, IProjectService projects)
    {
        Notifications = notifications;
        Log = log;
        Projects = projects;

        projects.ProjectOpened += (_, _) =>
        {
            HasUnsavedChanges = false;
            OnProjectOpened(projects.CurrentData);
        };
        projects.ProjectClosed += (_, _) =>
        {
            HasUnsavedChanges = false;
            OnProjectClosed();
        };

        // If a project is already open when this VM is first created, load it now
        if (projects.IsProjectOpen)
            OnProjectOpened(projects.CurrentData);
    }

    protected virtual void OnProjectOpened(ProjectData data) { }
    protected virtual void OnProjectClosed() { }
    protected virtual void SyncToProjectData(ProjectData data) { }

    protected void MarkDirty()
    {
        HasUnsavedChanges = true;
        if (Projects.IsProjectOpen)
            SyncToProjectData(Projects.CurrentData);
    }
}
