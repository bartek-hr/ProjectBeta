namespace ProjectBeta.CI.Components;

public sealed class Navigation : Component
{
    private readonly List<Button> _buttons = [];
    private Button? _activeButton;

    public Navigation()
    {
    }

    public Navigation(params Button[] buttons)
    {
        foreach (var button in buttons)
            Add(button);
    }

    public override bool IsFocusable => _buttons.Any(button => !button.IsHidden);

    public Navigation Add(Button button)
    {
        _buttons.Add(button);

        if (_activeButton == null && !button.IsHidden)
            _activeButton = button;

        if (ParentForm != null)
            button.AttachToForm(ParentForm);

        return this;
    }

    public Button Button(string label)
    {
        var button = new Button(label);
        Add(button);
        return button;
    }

    public Navigation SetActive(Button button)
    {
        if (_buttons.Contains(button))
            _activeButton = button;

        return this;
    }

    public override int Render(ComponentRenderContext context)
    {
        var rows = 0;
        var activeButton = GetActiveButton();

        foreach (var button in _buttons)
        {
            if (button.IsHidden)
                continue;

            rows += button.Render(
                new ComponentRenderContext(context.Buffer, context.Top + rows, context.Width),
                IsFocused && ReferenceEquals(button, activeButton));
        }

        return rows;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        var visibleButtons = GetVisibleButtons();
        if (visibleButtons.Count == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
            {
                var activeBtn = GetActiveButton();
                var idx = activeBtn != null ? visibleButtons.IndexOf(activeBtn) : 0;
                if (idx <= 0) return false; // at top — let parent move focus to previous component
                MoveActive(visibleButtons, -1);
                return true;
            }
            case ConsoleKey.DownArrow:
            {
                var activeBtn = GetActiveButton();
                var idx = activeBtn != null ? visibleButtons.IndexOf(activeBtn) : 0;
                if (idx >= visibleButtons.Count - 1) return false; // at bottom — let parent move focus to next component
                MoveActive(visibleButtons, 1);
                return true;
            }
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                GetActiveButton()?.Invoke();
                return true;
            default:
                return false;
        }
    }

    public override (int Start, int End)? GetFocusedRowRange()
    {
        var visibleButtons = GetVisibleButtons();
        var activeButton = GetActiveButton();
        if (activeButton == null)
            return null;

        var index = visibleButtons.IndexOf(activeButton);
        return index >= 0 ? (index, index + 1) : null;
    }

    protected override void OnAttachedToForm(Form form)
    {
        foreach (var button in _buttons)
            button.AttachToForm(form);
    }

    private Button? GetActiveButton()
    {
        var visibleButtons = GetVisibleButtons();
        if (visibleButtons.Count == 0)
        {
            _activeButton = null;
            return null;
        }

        if (_activeButton == null || _activeButton.IsHidden || !_buttons.Contains(_activeButton))
            _activeButton = visibleButtons[0];

        return _activeButton;
    }

    private List<Button> GetVisibleButtons()
    {
        return _buttons.Where(button => !button.IsHidden).ToList();
    }

    private void MoveActive(IReadOnlyList<Button> visibleButtons, int direction)
    {
        var activeButton = GetActiveButton() ?? visibleButtons[0];
        var currentIndex = -1;
        for (var index = 0; index < visibleButtons.Count; index++)
        {
            if (!ReferenceEquals(visibleButtons[index], activeButton))
                continue;

            currentIndex = index;
            break;
        }

        if (currentIndex < 0)
            currentIndex = 0;

        var nextIndex = (currentIndex + direction + visibleButtons.Count) % visibleButtons.Count;
        _activeButton = visibleButtons[nextIndex];
    }
}
