using Godot;
using System;

public partial class ClickableCounter : Area2D
{
    [Export] public int Counter = 10;  // 改成10，与Label初始值匹配
    
    private Label _counterLabel;  // 引用手动添加的Label
    private Sprite2D _sprite;
    private Vector2 _originalScale;
    
    public override void _Ready()
    {
        InputPickable = true;
        
        // 获取Sprite2D
        _sprite = GetNode<Sprite2D>("Sprite2D");
        if (_sprite != null)
        {
            _originalScale = _sprite.Scale;
        }
        
        // 获取手动添加的Label（不创建新的）
        SetupCounterLabel();
        
        // 更新显示
        UpdateDisplay();
        
        GD.Print($"初始化完成，剩余次数: {Counter}");
    }
    
    // 设置显示数字的标签 - 只获取手动添加的Label，不创建
    private void SetupCounterLabel()
    {
        // 获取场景中手动添加的Label节点
        _counterLabel = GetNodeOrNull<Label>("../CounterLabel");
        
        if (_counterLabel == null)
        {
            // 如果没找到，报错提示
            GD.PrintErr("错误：请在场景中添加一个名为 CounterLabel 的 Label 节点！");
            GD.PrintErr("位置：与 ClickableCookie 同级的节点下");
            return;
        }
        
        GD.Print("成功获取手动添加的Label节点");
        
        // 可选：将Label上手动设置的初始文字同步到Counter变量
        if (int.TryParse(_counterLabel.Text, out int initialValue))
        {
            Counter = initialValue;
            GD.Print($"从Label读取初始值: {Counter}");
        }
    }
    
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.Pressed && 
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            // 检查是否还可以减少
            if (Counter <= 0)
            {
                GD.Print("已经没有剩余次数了！");
                return;
            }
            
            // 数字减1
            Counter--;
            
            // 输出到控制台
            GD.Print($"点击！剩余次数: {Counter}");
            
            // 更新屏幕显示
            UpdateDisplay();
            
            // 播放点击动画
            AnimateClick();
            
            // 显示漂浮数字
            ShowFloatingNumber(mouseEvent.GlobalPosition);
            
            // 数字归零时的处理
            if (Counter == 0)
            {
                OnCounterZero();
            }
        }
    }
    
    // 更新屏幕显示的数字
    private void UpdateDisplay()
    {
        if (_counterLabel != null)
        {
            // ✅ 关键：更新Label显示的文字
            _counterLabel.Text = $"剩余资产: {Counter}元";
            
            // 根据剩余次数改变颜色
            if (Counter <= 3 && Counter > 0)
            {
                _counterLabel.AddThemeColorOverride("font_color", Colors.Red);
            }
            else if (Counter == 0)
            {
                _counterLabel.AddThemeColorOverride("font_color", Colors.Gray);
            }
            else
            {
                _counterLabel.AddThemeColorOverride("font_color", Colors.White);
            }
        }
    }
    
    // 漂浮数字效果
    private void ShowFloatingNumber(Vector2 clickPosition)
    {
        var floatingLabel = new Label();
        floatingLabel.Text = "-1";
        floatingLabel.Position = clickPosition - new Vector2(15, 15);
        floatingLabel.AddThemeFontSizeOverride("font_size", 60);
        floatingLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        floatingLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
        floatingLabel.AddThemeConstantOverride("outline_size", 2);
        GetTree().Root.AddChild(floatingLabel);
        
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(floatingLabel, "position", 
            floatingLabel.Position + new Vector2(0, -60), 1.0f);
        tween.TweenProperty(floatingLabel, "modulate:a", 0.0f, 1.0f);
        tween.Finished += () => floatingLabel.QueueFree();
    }
    
    // 饼干点击动画
    private void AnimateClick()
    {
        if (_sprite != null)
        {
            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.Out);
            tween.SetTrans(Tween.TransitionType.Back);
            
            tween.TweenProperty(_sprite, "scale", _originalScale * 0.9f, 0.05f);
            tween.TweenProperty(_sprite, "scale", _originalScale, 0.1f);
        }
        
        // 数字标签动画
        AnimateCounterText();
    }
    
    // 数字标签的点击动画
    private void AnimateCounterText()
    {
        if (_counterLabel != null)
        {
            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.Out);
            tween.SetTrans(Tween.TransitionType.Back);
            
            tween.TweenProperty(_counterLabel, "scale", new Vector2(1.2f, 1.2f), 0.05f);
            tween.TweenProperty(_counterLabel, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        }
    }
    
    // 游戏结束处理
    private void OnCounterZero()
    {
        GD.Print("计数器已归零！游戏结束！");
        
        if (_counterLabel != null)
        {
            _counterLabel.AddThemeColorOverride("font_color", Colors.Gray);
        }
        
        // 禁用点击
        InputPickable = false;
        
        // 显示游戏结束文字
        ShowGameOver();
    }
    
    // 显示游戏结束文字
    private void ShowGameOver()
    {
        var gameOverLabel = new Label();
        gameOverLabel.Text = "游戏结束！";
        gameOverLabel.Position = new Vector2(20, 80);
        gameOverLabel.AddThemeFontSizeOverride("font_size", 32);
        gameOverLabel.AddThemeColorOverride("font_color", Colors.Red);
        GetTree().Root.GetChild(0).AddChild(gameOverLabel);
        
        var timer = new Timer();
        timer.WaitTime = 3;
        timer.OneShot = true;
        timer.Timeout += () => gameOverLabel.QueueFree();
        AddChild(timer);
        timer.Start();
    }
}