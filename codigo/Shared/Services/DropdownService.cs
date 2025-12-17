using System;

namespace TruequeTextil.Shared.Services;

public class DropdownService
{
    public event Action? OnDropdownOpened;

    public void NotifyDropdownOpened()
    {
        OnDropdownOpened?.Invoke();
    }
}
