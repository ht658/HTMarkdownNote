# HT Markdown Note - 技术架构设计

## 项目整体架构

```
┌─────────────────────────────────────────────────────────┐
│                   HT Markdown Note                       │
├─────────────────────────────────────────────────────────┤
│                     UI 层 (WPF)                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │  MainWindow (主窗口/便签列表)                     │   │
│  │  ├─ ToolBar (新建、搜索、设置)                    │   │
│  │  ├─ NoteListView (便签列表)                      │   │
│  │  └─ ContextMenu (右键菜单)                       │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  EditorWindow (编辑窗口)                         │   │
│  │  ├─ FormatToolBar (格式工具栏)                   │   │
│  │  ├─ Editor (所见即所得编辑器)                    │   │
│  │  └─ StatusBar (状态栏)                           │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  SettingsWindow (设置窗口)                       │   │
│  │  RecycleBinWindow (回收站窗口)                   │   │
│  └──────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────┤
│                  业务逻辑层 (Core)                       │
│  ┌──────────────────────────────────────────────────┐   │
│  │  NoteService (便签服务)                          │   │
│  │  ├─ Create/Read/Update/Delete Note               │   │
│  │  ├─ Search Notes                                 │   │
│  │  └─ Manage Window State                          │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  MarkdownService (Markdown 处理)                 │   │
│  │  ├─ Parse Markdown                               │   │
│  │  ├─ Render to FlowDocument                       │   │
│  │  └─ Apply Format                                 │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  StorageService (存储服务)                       │   │
│  │  ├─ Load Metadata                                │   │
│  │  ├─ Save Note File (Atomic Write)                │   │
│  │  └─ Backup/Restore                               │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  SettingsService (设置服务)                      │   │
│  │  └─ Manage User Settings                         │   │
│  └──────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────┤
│                  数据持久化层                           │
│  ┌──────────────────────────────────────────────────┐   │
│  │  File System (本地文件系统)                      │   │
│  │  ├─ %LocalAppData%\HTMarkdownNote\                │   │
│  │  │  ├─ metadata.json                             │   │
│  │  │  ├─ notes/                                    │   │
│  │  │  ├─ Backups/                                  │   │
│  │  │  └─ Recycle/                                  │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

## 核心模块设计

### 1. NoteService（便签服务）

**职责：** 便签的生命周期管理

```csharp
public class NoteService
{
    // CRUD 操作
    public Note CreateNote();
    public Note GetNote(Guid id);
    public IEnumerable<Note> GetAllNotes();
    public void UpdateNote(Note note);
    public void DeleteNote(Guid id);  // 移至回收站
    
    // 查询操作
    public IEnumerable<Note> SearchNotes(string keyword);
    public IEnumerable<Note> GetSortedNotes(SortBy sortBy);
    
    // 窗口状态
    public void SaveWindowState(Guid noteId, WindowState state);
    public WindowState GetWindowState(Guid noteId);
    
    // 回收站操作
    public IEnumerable<Note> GetRecycledNotes();
    public void RestoreNote(Guid id);
    public void PermanentlyDeleteNote(Guid id);
    public void ClearRecycleBin();
}
```

### 2. MarkdownService（Markdown 处理）

**职责：** Markdown 解析、渲染、格式应用

```csharp
public class MarkdownService
{
    // 渲染
    public FlowDocument RenderMarkdown(string markdown);
    public string GetPlainText(string markdown);
    public string GetFirstLine(string markdown);  // 获取标题
    
    // 格式应用
    public string ApplyFormat(string text, MarkdownFormat format);
    public string RemoveFormat(string text, MarkdownFormat format);
    public bool IsFormatApplied(string text, MarkdownFormat format);
    
    // 预览
    public string GetPreview(string markdown, int maxLines = 6);
    
    // 语法高亮
    public void ApplySyntaxHighlight(CodeBlock codeBlock);
}

public enum MarkdownFormat
{
    H1, H2, H3,
    Bold, Italic, Strikethrough,
    Quote, UnorderedList, OrderedList,
    InlineCode, CodeBlock,
    Table, Hyperlink, TaskCheckbox
}
```

### 3. StorageService（存储服务）

**职责：** 数据持久化、备份恢复、原子写入

```csharp
public class StorageService
{
    // 初始化
    public void Initialize();  // 检查目录、加载元数据
    
    // 元数据操作
    public Metadata LoadMetadata();
    public void SaveMetadata(Metadata metadata);
    
    // 便签文件操作
    public string ReadNoteFile(Guid noteId);
    public void WriteNoteFile(Guid noteId, string content);  // 原子写入
    
    // 备份操作
    public void CreateBackup();  // 每日自动调用
    public IEnumerable<BackupInfo> GetBackupHistory();
    public void RestoreFromBackup(DateTime backupDate);
    public void CleanupOldBackups();  // 仅保留7天
    
    // 回收站
    public void MoveToRecycle(Guid noteId);
    public void RestoreFromRecycle(Guid noteId);
    public void CleanupExpiredRecycledNotes();
    
    // 异常恢复
    public void DetectAndRecoverFromCrash();
    public void SaveSessionState(List<Guid> openWindowIds);
}
```

### 4. SettingsService（设置服务）

**职责：** 用户设置管理

```csharp
public class SettingsService
{
    public AppSettings LoadSettings();
    public void SaveSettings(AppSettings settings);
    public void ResetToDefaults();
}

public class AppSettings
{
    public string FontFamily { get; set; } = "Segoe UI";
    public int FontSize { get; set; } = 12;
    public SortBy SortBy { get; set; } = SortBy.Modified;
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
    public ThemeType Theme { get; set; } = ThemeType.System;
    public bool ShowModifiedTime { get; set; } = true;
}
```

## 关键设计决策

### 1. 所见即所得编辑器实现

**方案选择：**
- ✅ 使用 Markdig.Wpf 渲染为 FlowDocument
- 原因：
  - 原生 WPF 渲染，无额外进程开销
  - 性能优秀，符合启动速度要求
  - 易于集成格式工具栏
  - 支持现有 WPF 生态

**工作流程：**

```
用户输入
  ↓
300ms 防抖
  ↓
Markdig 解析
  ↓
FlowDocument 渲染
  ↓
UI 更新
  ↓
自动保存（原子写入）
```

### 2. 防抖与自动保存

**目标：** 平衡用户体验与磁盘写入频率

```csharp
private DispatcherTimer _autoSaveTimer;

public void OnTextChanged()
{
    _autoSaveTimer.Stop();
    _autoSaveTimer.Start();  // 重置 300ms 计时器
}

private void AutoSaveTimer_Tick()
{
    _autoSaveTimer.Stop();
    SaveCurrentNote();
}
```

### 3. 搜索与索引

**方案选择：**
- ✅ 内存搜索（简单、快速、无数据库）
- 实现：
  - 启动时将所有便签加载到内存
  - 使用 LINQ 进行全文搜索（标题 + 正文）
  - 支持关键词高亮

```csharp
public IEnumerable<Note> SearchNotes(string keyword)
{
    var results = _notes
        .Where(n => n.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    n.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        .OrderBy(n => n.Modified);
    
    return results;
}
```

### 4. 多窗口管理

**设计原则：**
- 每个编辑窗口独立管理自己的状态
- 使用 MVVM 模式，每个窗口对应独立 ViewModel
- 通过 NoteService 共享便签数据

```csharp
public class EditorWindowViewModel : INotifyPropertyChanged
{
    private Note _currentNote;
    private string _editorContent;
    
    public void SaveWindowState()
    {
        _noteService.SaveWindowState(_currentNote.Id, 
            new WindowState 
            { 
                X = Window.Left, 
                Y = Window.Top, 
                Width = Window.Width, 
                Height = Window.Height, 
                IsTopmost = Window.Topmost 
            });
    }
}
```

### 5. 异常恢复

**流程：**

```
应用启动
  ↓
检查 SessionState.isNormalExit
  ↓
If false (异常退出):
  ├─ 记录日志
  ├─ 加载 SessionState.openWindowIds
  └─ 为每个 ID 打开编辑窗口
       (按保存的位置、尺寸、置顶状态还原)
```

## 性能优化策略

### 1. 启动优化

```csharp
// 异步初始化
public async Task InitializeAsync()
{
    // 1. 同步加载必需的元数据（< 50ms）
    await Task.Run(() => _storageService.LoadMetadata());
    
    // 2. 异步执行非关键操作
    _ = Task.Run(async () =>
    {
        await CleanupExpiredRecycleBin();
        await DetectAndRecoverFromCrash();
    });
}
```

### 2. 内存管理

- 虚拟化 ListBox（仅渲染可见项）
- 编辑窗口延迟加载（打开时才完整加载）
- 定期清理不必要的缓存

### 3. UI 响应性

- 长操作异步执行（备份、清理、搜索）
- 使用 Task.Run 避免阻塞 UI 线程

## 依赖注入与服务注册

```csharp
public static class ServiceConfiguration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<INoteService, NoteService>();
        services.AddSingleton<IMarkdownService, MarkdownService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        
        return services;
    }
}
```

## 文件组织结构

```
src/
├── HTMarkdownNote.Core/
│   ├── Models/
│   │   ├── Note.cs
│   │   ├── Metadata.cs
│   │   ├── AppSettings.cs
│   │   └── WindowState.cs
│   ├── Services/
│   │   ├── IStorageService.cs
│   │   ├── StorageService.cs
│   │   ├── INoteService.cs
│   │   ├── NoteService.cs
│   │   ├── IMarkdownService.cs
│   │   ├── MarkdownService.cs
│   │   ├── ISettingsService.cs
│   │   └── SettingsService.cs
│   ├── Utils/
│   │   ├── AtomicFileWriter.cs
│   │   ├── JsonSerializer.cs
│   │   └── PathHelper.cs
│   └── Constants/
│       └── AppConstants.cs
│
├── HTMarkdownNote.UI/
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   ├── EditorWindow.xaml
│   │   ├── SettingsWindow.xaml
│   │   └── RecycleBinWindow.xaml
│   ├── ViewModels/
│   │   ├── MainWindowViewModel.cs
│   │   ├── EditorWindowViewModel.cs
│   │   ├── SettingsWindowViewModel.cs
│   │   └── RecycleBinWindowViewModel.cs
│   ├── Controls/
│   │   ├── MarkdownEditor.xaml
│   │   ├── NoteListItem.xaml
│   │   └── FormatToolBar.xaml
│   ├── Converters/
│   │   ├── BoolToVisibilityConverter.cs
│   │   └── DateTimeFormatConverter.cs
│   ├── Behaviors/
│   │   └── AutoSaveBehavior.cs
│   └── Resources/
│       ├── Themes/
│       │   ├── Light.xaml
│       │   └── Dark.xaml
│       └── Strings/
│           └── Resources.resx
│
└── HTMarkdownNote.Tests/
    ├── Services/
    ├── Utils/
    └── Integration/
```

## 下一步实现优先级

1. **Phase 1: 核心基础设施**
   - StorageService + metadata.json 管理
   - NoteService 基本 CRUD
   - 文件系统操作与原子写入

2. **Phase 2: UI 框架**
   - MainWindow 列表界面
   - EditorWindow 编辑框架
   - 基本窗口状态管理

3. **Phase 3: Markdown 编辑**
   - MarkdownService 集成 Markdig.Wpf
   - 所见即所得编辑
   - 格式工具栏

4. **Phase 4: 高级功能**
   - 搜索、排序
   - 备份恢复
   - 回收站
   - 设置面板

5. **Phase 5: 优化与测试**
   - 性能测试与优化
   - 单元测试覆盖
   - 用户体验微调
