using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FPTMart.ViewModels;

/// <summary>
/// Base ViewModel using CommunityToolkit.Mvvm
/// All ViewModels should inherit from this
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    protected void ClearError() => ErrorMessage = null;
    
    protected void SetError(string message) => ErrorMessage = message;
}
