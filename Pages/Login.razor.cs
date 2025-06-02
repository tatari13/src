using LAPS_WebUI.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using LAPS_WebUI.Services;

namespace LAPS_WebUI.Pages
{
    public partial class Login
    {
        private readonly UserLoginRequest _loginRequest = new();
        private bool _processing;
        private string _errorMessage = string.Empty;
        private List<string> _domains = [];

        [Inject]
        private AuditService AuditService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _domains = await SessionManager.GetDomainsAsync();

            if (_domains.Count > 0)
            {
                _loginRequest.DomainName = _domains[0];
            }
        }

        private async Task OnValidSubmitAsync(EditContext context)
        {
            _errorMessage = string.Empty;
            _processing = true;

            try
            {
                bool success = await SessionManager.LoginAsync(
                    _loginRequest.DomainName ?? string.Empty,
                    _loginRequest.Username ?? string.Empty,
                    _loginRequest.Password ?? string.Empty);

                if (success)
                {
                    try
                    {
                        await AuditService.LogAsync(
                            _loginRequest.Username ?? "Невідомо",
                            "Вхід",
                            "Форма логіну");
                    }
                    catch
                    {
                        // Не блокуємо логін, якщо аудит не вдався
                    }

                    NavigationManager.NavigateTo("/laps");
                }
                else
                {
                    throw new Exception("Login failed!");
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            finally
            {
                _processing = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
