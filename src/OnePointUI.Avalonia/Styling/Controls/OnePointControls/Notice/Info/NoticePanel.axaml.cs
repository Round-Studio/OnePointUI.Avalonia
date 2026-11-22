using Avalonia.Controls;
using Avalonia.Threading;
using OnePointUI.Avalonia.Base.Entry;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Notice.Info;

public partial class NoticePanel : UserControl
{
    public static NoticePanel? InstancePanel;
    public static List<NoticeInfo> NoticeInfos = new();

    public NoticePanel()
    {
        InitializeComponent();

        InstancePanel = this;
    }

    public void AddNotice(NoticeInfo notice)
    {
        // 后台任务普遍直接调用本方法弹通知，而 NoticeBox 必须在 UI 线程构造，
        // 否则样式动画关键帧归属后台线程，绑定/布局每帧抛跨线程访问异常
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AddNotice(notice));
            return;
        }

        NoticeInfos.Add(notice);
        var noticeBox = new NoticeBox(notice);
        noticeBox.OnClose = box => NoticesPanel.Children.Remove(box);

        // 添加到开头
        NoticesPanel.Children.Insert(0, noticeBox);
        // NoticesPanel.Children.Add(noticeBox);

        // 如果超过5个，删除最后一个（带淡出动画）
        if (NoticesPanel.Children.Count > 5)
        {
            var oldestNotice = (NoticeBox)NoticesPanel.Children[NoticesPanel.Children.Count - 1];
            oldestNotice.CloseThis();
        }
    }
}