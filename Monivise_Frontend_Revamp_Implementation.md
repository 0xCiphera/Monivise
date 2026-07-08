# Monivise Frontend Revamp — Implementation Guide

This is a revamp of the existing Blazor WASM frontend, not a rebuild from zero. Domain logic, DTO shapes, and API routes stay aligned with the backend contracts already in place. What changes: the CSS/design token layer, the layout shell, the component library, the onboarding/simulator flows, and — the addition on top of the original visual redesign — explicit error handling end to end (API layer, state effects, and UI).

Apply these files directly over the existing `Monivise.App` project. Where a file already exists (e.g. `Program.cs`), replace its content with the version shown here rather than merging by hand — the diffs are large enough that merging invites bugs.

---

## 1. `wwwroot/css/tokens.css`

```css
:root {
  /* Spacing */
  --mv-space-1: 4px;
  --mv-space-2: 8px;
  --mv-space-3: 12px;
  --mv-space-4: 16px;
  --mv-space-5: 20px;
  --mv-space-6: 24px;
  --mv-space-7: 32px;
  --mv-space-8: 40px;
  --mv-space-9: 48px;

  /* Radius */
  --mv-radius-sm: 6px;
  --mv-radius-md: 10px;
  --mv-radius-lg: 14px;
  --mv-radius-xl: 16px;
  --mv-radius-pill: 9999px;

  /* Motion */
  --mv-motion-fast: 150ms;
  --mv-motion-normal: 200ms;
  --mv-motion-slow: 300ms;
  --mv-ease-standard: cubic-bezier(0.2, 0, 0, 1);
  --mv-ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);

  /* Typography */
  --mv-font-sans: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  --mv-fs-xs: 12px;
  --mv-fs-sm: 13px;
  --mv-fs-md: 15px;
  --mv-fs-base: 16px;
  --mv-fs-lg: 19px;
  --mv-fs-xl: 24px;
  --mv-fs-2xl: 30px;

  /* Light mode (default) */
  --mv-bg: #f6f7f9;
  --mv-surface: #ffffff;
  --mv-surface-sunken: #f1f2f5;
  --mv-surface-inverted: #16181d;
  --mv-border: #e4e6ea;
  --mv-border-strong: #d3d6dc;
  --mv-text-primary: #16181d;
  --mv-text-secondary: #5b606b;
  --mv-text-tertiary: #8b9099;
  --mv-text-inverted: #ffffff;
  --mv-accent: #16181d;
  --mv-accent-text: #ffffff;
  --mv-positive: #1d9e75;
  --mv-positive-bg: #e1f5ee;
  --mv-positive-text: #085041;
  --mv-danger: #d3402f;
  --mv-danger-bg: #fcebeb;
  --mv-danger-text: #791f1f;
  --mv-warning: #b9791a;
  --mv-warning-bg: #faeeda;
  --mv-warning-text: #633806;
  --mv-info: #185fa5;
  --mv-info-bg: #e6f1fb;
  --mv-info-text: #0c447c;

  /* Bucket type tints */
  --mv-bucket-fixed-bg: #e6f1fb;
  --mv-bucket-fixed-text: #0c447c;
  --mv-bucket-flexible-bg: #e1f5ee;
  --mv-bucket-flexible-text: #085041;
  --mv-bucket-savings-bg: #eaf3de;
  --mv-bucket-savings-text: #27500a;
  --mv-bucket-reserve-bg: #f1efe8;
  --mv-bucket-reserve-text: #444441;

  --mv-shadow-fab: 0 4px 14px rgba(22, 24, 29, 0.18);
  --mv-shadow-toast: 0 6px 20px rgba(22, 24, 29, 0.14);
  --mv-safe-top: env(safe-area-inset-top, 0px);
  --mv-safe-bottom: env(safe-area-inset-bottom, 0px);
}

:root.dark {
  --mv-bg: #0d1117;
  --mv-surface: #161b22;
  --mv-surface-sunken: #10141a;
  --mv-surface-inverted: #f6f7f9;
  --mv-border: #262c36;
  --mv-border-strong: #333a46;
  --mv-text-primary: #e6e8eb;
  --mv-text-secondary: #9aa1ac;
  --mv-text-tertiary: #6b7280;
  --mv-text-inverted: #16181d;
  --mv-accent: #e6e8eb;
  --mv-accent-text: #16181d;
  --mv-positive: #3fb950;
  --mv-positive-bg: #0f2c22;
  --mv-positive-text: #6fdba0;
  --mv-danger: #f85149;
  --mv-danger-bg: #2d1416;
  --mv-danger-text: #ff8f88;
  --mv-warning: #d29922;
  --mv-warning-bg: #2b2210;
  --mv-warning-text: #e3b341;
  --mv-info: #58a6ff;
  --mv-info-bg: #101d2c;
  --mv-info-text: #79b8ff;

  --mv-bucket-fixed-bg: #10233a;
  --mv-bucket-fixed-text: #79b8ff;
  --mv-bucket-flexible-bg: #0f2c22;
  --mv-bucket-flexible-text: #6fdba0;
  --mv-bucket-savings-bg: #1c2a10;
  --mv-bucket-savings-text: #a3d977;
  --mv-bucket-reserve-bg: #1c1f26;
  --mv-bucket-reserve-text: #b4b8c2;
}

@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
    scroll-behavior: auto !important;
  }
}
```

---

## 2. `wwwroot/css/app.css`

```css
* { box-sizing: border-box; }
html, body { margin: 0; height: 100%; background: var(--mv-bg); }
body {
  font-family: var(--mv-font-sans);
  color: var(--mv-text-primary);
  font-size: var(--mv-fs-base);
  -webkit-font-smoothing: antialiased;
}

.mv-shell {
  height: 100dvh;
  width: 100vw;
  max-width: 430px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  position: relative;
  background: var(--mv-bg);
  overflow: hidden;
}
@media (min-width: 431px) {
  .mv-shell {
    height: min(100dvh, 900px);
    margin-top: max(20px, calc((100dvh - 900px) / 2));
    border: 1px solid var(--mv-border);
    border-radius: 28px;
    box-shadow: 0 12px 40px rgba(0,0,0,0.10);
  }
}

.mv-header {
  flex-shrink: 0;
  padding: calc(var(--mv-safe-top) + var(--mv-space-3)) var(--mv-space-4) var(--mv-space-3);
  background: color-mix(in srgb, var(--mv-bg) 82%, transparent);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--mv-border);
  z-index: 10;
}
.mv-header__brand { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 0; text-transform: uppercase; letter-spacing: 0.04em; }
.mv-header__title { font-size: var(--mv-fs-lg); font-weight: 600; margin: 2px 0 0; }
.mv-header__subtitle { font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin: 2px 0 0; }

.mv-body {
  flex: 1;
  overflow-y: auto;
  scrollbar-width: none;
  padding: var(--mv-space-4);
  padding-bottom: calc(100px + var(--mv-safe-bottom));
}
.mv-body::-webkit-scrollbar { display: none; }

.mv-nav {
  position: absolute;
  left: 0; right: 0; bottom: 0;
  display: flex;
  justify-content: space-around;
  align-items: center;
  padding: var(--mv-space-2) 0 calc(var(--mv-space-2) + var(--mv-safe-bottom));
  background: color-mix(in srgb, var(--mv-bg) 88%, transparent);
  backdrop-filter: blur(12px);
  border-top: 1px solid var(--mv-border);
  z-index: 10;
}
.mv-nav__item {
  display: flex; flex-direction: column; align-items: center; gap: 2px;
  min-width: 44px; min-height: 44px; justify-content: center;
  background: none; border: none; color: var(--mv-text-tertiary);
  font-size: 10px; cursor: pointer;
}
.mv-nav__item.active { color: var(--mv-text-primary); font-weight: 600; }

.mv-fab {
  position: absolute;
  right: var(--mv-space-4);
  bottom: calc(76px + var(--mv-safe-bottom));
  width: 52px; height: 52px; border-radius: 50%;
  background: var(--mv-accent); color: var(--mv-accent-text);
  border: none; box-shadow: var(--mv-shadow-fab);
  display: flex; align-items: center; justify-content: center;
  font-size: 22px; cursor: pointer; z-index: 9;
  transition: transform var(--mv-motion-fast) var(--mv-ease-standard);
}
.mv-fab:active { transform: scale(0.93); }

.mv-card {
  background: var(--mv-surface);
  border: 1px solid var(--mv-border);
  border-radius: var(--mv-radius-xl);
  padding: var(--mv-space-4);
}
.mv-card:active { transform: scale(0.98); }
.mv-card--hero { background: var(--mv-surface-inverted); color: var(--mv-text-inverted); border: none; }
.mv-card--hero .mv-card__label { color: color-mix(in srgb, var(--mv-text-inverted) 65%, transparent); }

@keyframes mv-shake {
  10%, 90% { transform: translateX(-1px); }
  20%, 80% { transform: translateX(2px); }
  30%, 50%, 70% { transform: translateX(-4px); }
  40%, 60% { transform: translateX(4px); }
}
.mv-shake { animation: mv-shake var(--mv-motion-slow) var(--mv-ease-standard); }

@keyframes mv-slide-down {
  from { transform: translateY(-12px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}
@keyframes mv-scale-in {
  from { transform: scale(0.96); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
.mv-slide-down { animation: mv-slide-down var(--mv-motion-normal) var(--mv-ease-standard); }
.mv-scale-in { animation: mv-scale-in var(--mv-motion-normal) var(--mv-ease-spring); }

.mv-offline-banner {
  background: var(--mv-warning-bg); color: var(--mv-warning-text);
  font-size: var(--mv-fs-sm); text-align: center; padding: var(--mv-space-2);
}

.mv-sr-only {
  position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px;
  overflow: hidden; clip: rect(0,0,0,0); white-space: nowrap; border: 0;
}
```

---

## 3. Layouts

### `Layouts/MainLayout.razor`

```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

<div class="mv-shell">
    @if (isOffline)
    {
        <div class="mv-offline-banner">You're offline. Changes will sync when reconnected.</div>
    }
    @Body
    <ToastContainer />
</div>

@code {
    private bool isOffline;
    private DotNetObjectReference<MainLayout>? selfRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("monivise.connectivity.register", selfRef);
        }
    }

    [JSInvokable]
    public void OnConnectivityChanged(bool online)
    {
        isOffline = !online;
        StateHasChanged();
    }
}
```

Add this small helper to `wwwroot/index.html` (before the Blazor script tag) so `MainLayout` can react to connectivity:

```html
<script>
  window.monivise = window.monivise || {};
  window.monivise.connectivity = {
    register: function (dotnetRef) {
      const notify = () => dotnetRef.invokeMethodAsync('OnConnectivityChanged', navigator.onLine);
      window.addEventListener('online', notify);
      window.addEventListener('offline', notify);
      notify();
    }
  };
</script>
```

### `Layouts/AppShell.razor`

The shell used by every authenticated page: fixed header, scrollable body, fixed nav, optional FAB.

```razor
@inherits LayoutComponentBase
@inject NavigationManager Nav

<div class="mv-header">
    <p class="mv-header__brand">Monivise</p>
    <p class="mv-header__title">@Title</p>
    @if (!string.IsNullOrWhiteSpace(Subtitle))
    {
        <p class="mv-header__subtitle">@Subtitle</p>
    }
</div>

<div class="mv-body">
    @Body
</div>

@if (ShowFab)
{
    <button class="mv-fab" aria-label="Quick add" @onclick="OnFabClick">+</button>
}

<nav class="mv-nav" aria-label="Primary">
    <NavItem Href="/dashboard" Icon="home" Label="Home" Active="@IsActive("/dashboard")" />
    <NavItem Href="/buckets" Icon="wallet" Label="Buckets" Active="@IsActive("/buckets")" />
    <NavItem Href="/weekly" Icon="chart" Label="Weekly" Active="@IsActive("/weekly")" />
    <NavItem Href="/settings" Icon="settings" Label="Settings" Active="@IsActive("/settings")" />
</nav>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public bool ShowFab { get; set; } = true;
    [Parameter] public EventCallback OnFab { get; set; }

    private bool IsActive(string path) => Nav.Uri.EndsWith(path, StringComparison.OrdinalIgnoreCase);

    private Task OnFabClick() => OnFab.HasDelegate ? OnFab.InvokeAsync() : Task.CompletedTask;
}
```

`AppShell` expects a small `NavItem.razor` helper (place it under `Components/Organisms`):

```razor
@* Components/Organisms/NavItem.razor *@
<a href="@Href" class="mv-nav__item @(Active ? "active" : "")">
    <Icon Name="@Icon" Size="20" />
    <span>@Label</span>
</a>

@code {
    [Parameter] public string Href { get; set; } = "#";
    [Parameter] public string Icon { get; set; } = "home";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public bool Active { get; set; }
}
```

### `Layouts/AuthLayout.razor`

```razor
@inherits LayoutComponentBase

<div class="mv-header" style="border-bottom:none;">
    <p class="mv-header__brand">Monivise</p>
</div>
<div class="mv-body" style="display:flex; flex-direction:column; justify-content:center; padding-bottom: var(--mv-space-4);">
    @Body
</div>
```

---

## 4. Atoms

A shared `Icon.razor` first, since several atoms use it — inline SVG paths, no emoji, no external icon font dependency:

### `Components/Atoms/Icon.razor`

```razor
@switch (Name)
{
    case "home": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M3 11l9-7 9 7v9a1 1 0 01-1 1h-5v-6H9v6H4a1 1 0 01-1-1v-9z"/></svg> break;
    case "wallet": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><rect x="3" y="6" width="18" height="13" rx="2"/><path d="M3 10h18M16 14h2"/></svg> break;
    case "chart": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M4 20V10M12 20V4M20 20v-7"/></svg> break;
    case "settings": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 11-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 11-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 112.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 112.83 2.83l-.06.06A1.65 1.65 0 0019.32 9c.14.32.44.55.79.66.35.11.6.4.71.75.11.35.02.73-.24 1a1.65 1.65 0 00-.33 1.82z"/></svg> break;
    case "check": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M20 6L9 17l-5-5"/></svg> break;
    case "close": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M18 6L6 18M6 6l12 12"/></svg> break;
    case "alert": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M12 9v4M12 17h.01"/><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/></svg> break;
    case "chevron-down": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M6 9l6 6 6-6"/></svg> break;
    case "backspace": <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M21 4H8l-7 8 7 8h13a1 1 0 001-1V5a1 1 0 00-1-1z"/><path d="M14 10l-4 4M10 10l4 4"/></svg> break;
    default: <svg width="@Size" height="@Size" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><circle cx="12" cy="12" r="9"/></svg> break;
}

@code {
    [Parameter] public string Name { get; set; } = "";
    [Parameter] public int Size { get; set; } = 18;
}
```

### `Components/Atoms/Button.razor`

```razor
<button type="@Type"
        class="mv-btn mv-btn--@Variant"
        disabled="@(Disabled || IsLoading)"
        @onclick="HandleClick">
    @if (IsLoading)
    {
        <span class="mv-btn__spinner" aria-hidden="true"></span>
        <span class="mv-sr-only">Loading</span>
    }
    else
    {
        @ChildContent
    }
</button>

<style>
    .mv-btn {
        width: 100%; min-height: 48px; border-radius: var(--mv-radius-lg);
        font-size: var(--mv-fs-base); font-weight: 600; cursor: pointer;
        display: flex; align-items: center; justify-content: center; gap: 8px;
        transition: transform var(--mv-motion-fast) var(--mv-ease-standard), opacity var(--mv-motion-fast);
        border: 1.5px solid transparent;
    }
    .mv-btn:active:not(:disabled) { transform: scale(0.97); }
    .mv-btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .mv-btn--primary { background: var(--mv-accent); color: var(--mv-accent-text); }
    .mv-btn--secondary { background: transparent; border-color: var(--mv-border-strong); color: var(--mv-text-primary); }
    .mv-btn--ghost { background: transparent; color: var(--mv-text-secondary); }
    .mv-btn--danger { background: var(--mv-danger); color: #fff; }
    .mv-btn__spinner {
        width: 18px; height: 18px; border-radius: 50%;
        border: 2px solid color-mix(in srgb, currentColor 30%, transparent);
        border-top-color: currentColor; animation: mv-spin 0.7s linear infinite;
    }
    @@keyframes mv-spin { to { transform: rotate(360deg); } }
</style>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Variant { get; set; } = "primary"; // primary | secondary | ghost | danger
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private Task HandleClick() => OnClick.HasDelegate ? OnClick.InvokeAsync() : Task.CompletedTask;
}
```

### `Components/Atoms/Input.razor`

```razor
<div class="mv-field">
    @if (!string.IsNullOrWhiteSpace(Label))
    {
        <label class="mv-field__label" for="@id">@Label</label>
    }
    <input id="@id"
           type="@Type"
           class="mv-field__input @(ShakeError ? "mv-shake" : "") @(HasError ? "mv-field__input--error" : "")"
           placeholder="@Placeholder"
           value="@Value"
           @oninput="HandleInput"
           inputmode="@InputMode"
           aria-invalid="@HasError"
           aria-describedby="@(HasError ? id + "-error" : null)" />
    @if (HasError)
    {
        <p id="@(id)-error" class="mv-field__error" role="alert">@ErrorMessage</p>
    }
    else if (!string.IsNullOrWhiteSpace(Help))
    {
        <p class="mv-field__help">@Help</p>
    }
</div>

<style>
    .mv-field { display: flex; flex-direction: column; gap: 6px; margin-bottom: var(--mv-space-4); }
    .mv-field__label { font-size: var(--mv-fs-sm); font-weight: 600; color: var(--mv-text-secondary); }
    .mv-field__input {
        min-height: 48px; padding: 0 var(--mv-space-4);
        border: 1.5px solid var(--mv-border); border-radius: var(--mv-radius-md);
        font-size: var(--mv-fs-base); background: var(--mv-surface); color: var(--mv-text-primary);
        transition: border-color var(--mv-motion-fast), box-shadow var(--mv-motion-fast);
    }
    .mv-field__input:focus {
        outline: none; border-color: var(--mv-text-primary);
        box-shadow: 0 0 0 3px color-mix(in srgb, var(--mv-text-primary) 10%, transparent);
    }
    .mv-field__input--error { border-color: var(--mv-danger); }
    .mv-field__input--error:focus { box-shadow: 0 0 0 3px color-mix(in srgb, var(--mv-danger) 12%, transparent); }
    .mv-field__error { font-size: var(--mv-fs-xs); color: var(--mv-danger); margin: 0; }
    .mv-field__help { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 0; }
</style>

@code {
    private readonly string id = "f_" + Guid.NewGuid().ToString("N")[..8];
    private bool ShakeError;

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string InputMode { get; set; } = "text";
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? Help { get; set; }

    private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected override void OnParametersSet()
    {
        if (HasError) { ShakeError = true; _ = ResetShake(); }
    }

    private async Task ResetShake()
    {
        await Task.Delay(350);
        ShakeError = false;
    }

    private Task HandleInput(ChangeEventArgs e) =>
        ValueChanged.InvokeAsync(e.Value?.ToString() ?? "");
}
```

### `Components/Atoms/Badge.razor`

```razor
<span class="mv-badge mv-badge--@Variant">@ChildContent</span>

<style>
    .mv-badge { display: inline-flex; align-items: center; padding: 3px 9px; border-radius: var(--mv-radius-sm); font-size: 11px; font-weight: 600; }
    .mv-badge--positive { background: var(--mv-positive-bg); color: var(--mv-positive-text); }
    .mv-badge--danger { background: var(--mv-danger-bg); color: var(--mv-danger-text); }
    .mv-badge--warning { background: var(--mv-warning-bg); color: var(--mv-warning-text); }
    .mv-badge--info { background: var(--mv-info-bg); color: var(--mv-info-text); }
    .mv-badge--neutral { background: var(--mv-surface-sunken); color: var(--mv-text-secondary); }
</style>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Variant { get; set; } = "neutral";
}
```

### `Components/Atoms/ProgressBar.razor`

```razor
<div class="mv-progress" style="height:@(Height)px;">
    <div class="mv-progress__fill" style="width:@(clamped)%; background:@Color;"></div>
</div>

<style>
    .mv-progress { width: 100%; background: var(--mv-surface-sunken); border-radius: var(--mv-radius-pill); overflow: hidden; }
    .mv-progress__fill { height: 100%; border-radius: var(--mv-radius-pill); transition: width var(--mv-motion-slow) var(--mv-ease-standard); }
</style>

@code {
    [Parameter] public double Percent { get; set; }
    [Parameter] public int Height { get; set; } = 5;
    [Parameter] public string Color { get; set; } = "var(--mv-positive)";
    private double clamped => Math.Clamp(Percent, 0, 100);
}
```

### `Components/Atoms/Numpad.razor`

```razor
<div class="mv-numpad" role="group" aria-label="Amount keypad">
    @foreach (var key in keys)
    {
        <button type="button" class="mv-numpad__key" aria-label="@(key == "back" ? "Backspace" : key)" @onclick="() => Press(key)">
            @if (key == "back")
            {
                <Icon Name="backspace" Size="20" />
            }
            else
            {
                @key
            }
        </button>
    }
</div>

<style>
    .mv-numpad { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
    .mv-numpad__key {
        min-height: 56px; border-radius: var(--mv-radius-lg); border: 1px solid var(--mv-border);
        background: var(--mv-surface); font-size: var(--mv-fs-lg); font-weight: 500;
        display: flex; align-items: center; justify-content: center; cursor: pointer;
    }
    .mv-numpad__key:active { background: var(--mv-surface-sunken); transform: scale(0.96); }
</style>

@code {
    private readonly string[] keys = { "1","2","3","4","5","6","7","8","9","000","0","back" };

    [Parameter] public string Value { get; set; } = "0";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public decimal MaxValue { get; set; } = 999_999_999;

    private void Press(string key)
    {
        var digits = Value == "0" ? "" : Value;
        var next = key switch
        {
            "back" => digits.Length > 0 ? digits[..^1] : "",
            _ => digits + key
        };
        if (next.Length == 0) next = "0";
        if (decimal.TryParse(next, out var num) && num > MaxValue) return;
        Value = next;
        ValueChanged.InvokeAsync(Value);
    }
}
```

### `Components/Atoms/Toast.razor`

```razor
<div class="mv-toast mv-toast--@Variant mv-slide-down" role="@(Variant == "error" ? "alert" : "status")">
    <Icon Name="@(Variant == "error" ? "alert" : Variant == "success" ? "check" : "alert")" Size="18" />
    <span class="mv-toast__message">@Message</span>
    <button type="button" class="mv-toast__dismiss" aria-label="Dismiss" @onclick="OnDismiss">
        <Icon Name="close" Size="16" />
    </button>
</div>

<style>
    .mv-toast {
        display: flex; align-items: center; gap: 10px;
        padding: 12px 14px; border-radius: var(--mv-radius-md);
        box-shadow: var(--mv-shadow-toast); font-size: var(--mv-fs-sm);
        backdrop-filter: blur(10px);
    }
    .mv-toast--success { background: var(--mv-positive-bg); color: var(--mv-positive-text); }
    .mv-toast--error { background: var(--mv-danger-bg); color: var(--mv-danger-text); }
    .mv-toast--info { background: var(--mv-info-bg); color: var(--mv-info-text); }
    .mv-toast__message { flex: 1; }
    .mv-toast__dismiss { background: none; border: none; color: inherit; cursor: pointer; min-width: 24px; min-height: 24px; }
</style>

@code {
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public string Variant { get; set; } = "info"; // success | error | info
    [Parameter] public EventCallback OnDismiss { get; set; }
}
```

---

## 5. Molecules

### `Components/Molecules/BucketRow.razor`

```razor
<div class="mv-bucket-row" @onclick="HandleClick">
    <div class="mv-bucket-row__icon" style="background:@BgTint(Bucket.Type); color:@TextTint(Bucket.Type);">
        <Icon Name="wallet" Size="18" />
    </div>
    <div class="mv-bucket-row__body">
        <p class="mv-bucket-row__name">@Bucket.Name</p>
        <p class="mv-bucket-row__meta">@Bucket.Type &middot; @(Bucket.Allocated > 0 ? $"{(Bucket.Spent / Bucket.Allocated * 100):0}% used" : "no allocation")</p>
    </div>
    <p class="mv-bucket-row__amount">@Bucket.Balance.ToString("C0")</p>
</div>

<style>
    .mv-bucket-row { display: flex; align-items: center; gap: 10px; border: 1px solid var(--mv-border); border-radius: var(--mv-radius-md); padding: 10px 12px; cursor: pointer; }
    .mv-bucket-row:active { background: var(--mv-surface-sunken); }
    .mv-bucket-row__icon { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .mv-bucket-row__body { flex: 1; min-width: 0; }
    .mv-bucket-row__name { font-size: var(--mv-fs-md); margin: 0; }
    .mv-bucket-row__meta { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 2px 0 0; }
    .mv-bucket-row__amount { font-size: var(--mv-fs-md); font-weight: 600; margin: 0; }
</style>

@code {
    [Parameter] public BucketDto Bucket { get; set; } = default!;
    [Parameter] public EventCallback<BucketDto> OnSelect { get; set; }

    private Task HandleClick() => OnSelect.HasDelegate ? OnSelect.InvokeAsync(Bucket) : Task.CompletedTask;

    private static string BgTint(string type) => type switch
    {
        "HardFixed" or "Fixed" => "var(--mv-bucket-fixed-bg)",
        "Soft" or "Flexible" => "var(--mv-bucket-flexible-bg)",
        "Savings" => "var(--mv-bucket-savings-bg)",
        _ => "var(--mv-bucket-reserve-bg)"
    };
    private static string TextTint(string type) => type switch
    {
        "HardFixed" or "Fixed" => "var(--mv-bucket-fixed-text)",
        "Soft" or "Flexible" => "var(--mv-bucket-flexible-text)",
        "Savings" => "var(--mv-bucket-savings-text)",
        _ => "var(--mv-bucket-reserve-text)"
    };
}
```

### `Components/Molecules/TransactionRow.razor`

```razor
<div class="mv-tx-row">
    <span class="mv-tx-row__dot" style="background:@(Tx.Kind == "Income" ? "var(--mv-positive)" : "var(--mv-text-tertiary)");"></span>
    <div class="mv-tx-row__body">
        <p class="mv-tx-row__name">@(Tx.BucketName ?? Tx.Note ?? Tx.Kind)</p>
        <p class="mv-tx-row__date">@Tx.Date.ToString("MMM d")@(string.IsNullOrWhiteSpace(Tx.Note) ? "" : $" · {Tx.Note}")</p>
    </div>
    <p class="mv-tx-row__amount" style="color:@(Tx.Kind == "Income" ? "var(--mv-positive)" : "var(--mv-text-primary)");">
        @(Tx.Kind == "Income" ? "+" : "-")@Math.Abs(Tx.Amount).ToString("C0")
    </p>
</div>

<style>
    .mv-tx-row { display: flex; align-items: center; gap: 10px; padding: 10px 0; border-bottom: 1px solid var(--mv-border); }
    .mv-tx-row:last-child { border-bottom: none; }
    .mv-tx-row__dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
    .mv-tx-row__body { flex: 1; min-width: 0; }
    .mv-tx-row__name { font-size: var(--mv-fs-md); margin: 0; }
    .mv-tx-row__date { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 2px 0 0; }
    .mv-tx-row__amount { font-size: var(--mv-fs-md); font-weight: 600; margin: 0; }
</style>

@code {
    [Parameter] public TransactionDto Tx { get; set; } = default!;
}
```

### `Components/Molecules/ConsequenceCard.razor`

```razor
<div class="mv-card mv-consequence">
    <div class="mv-consequence__grid">
        <div>
            <p class="mv-consequence__label">Before</p>
            <p class="mv-consequence__value">@Preview.CurrentBalance.ToString("C0")</p>
        </div>
        <div>
            <p class="mv-consequence__label">After</p>
            <p class="mv-consequence__value" style="color:@DeltaColor;">@Preview.PostSpendBalance.ToString("C0")</p>
        </div>
    </div>
    <ProgressBar Percent="@remainingPercent" Color="@DeltaColor" Height="5" />
    <p class="mv-consequence__note">Leaves @remainingPercent.ToString("0")% of this bucket for the next @Preview.DaysRemaining days.</p>
    @if (Preview.RegretSignals is { Count: > 0 })
    {
        <details class="mv-consequence__regret">
            <summary>@Preview.RegretSignals.Count regret signal(s)</summary>
            <ul>
                @foreach (var signal in Preview.RegretSignals)
                {
                    <li>@signal</li>
                }
            </ul>
        </details>
    }
</div>

<style>
    .mv-consequence__grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 12px; }
    .mv-consequence__label { font-size: 11px; color: var(--mv-text-tertiary); margin: 0; }
    .mv-consequence__value { font-size: var(--mv-fs-md); font-weight: 600; margin: 2px 0 0; }
    .mv-consequence__note { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 8px 0 0; }
    .mv-consequence__regret { margin-top: 10px; font-size: var(--mv-fs-xs); color: var(--mv-warning-text); }
</style>

@code {
    [Parameter] public SimulateResponse Preview { get; set; } = default!;

    private double remainingPercent => Preview.CurrentBalance <= 0 ? 0 :
        (double)(Preview.PostSpendBalance / Preview.CurrentBalance * 100);

    private string DeltaColor => Preview.CanAfford
        ? (Preview.PaceScore >= 0.7m ? "var(--mv-warning)" : "var(--mv-positive)")
        : "var(--mv-danger)";
}
```

### `Components/Molecules/ChoiceCard.razor`

```razor
<div class="mv-choice @(Selected ? "mv-choice--selected" : "")" role="radio" aria-checked="@Selected" tabindex="0"
     @onclick="HandleClick" @onkeydown="HandleKeyDown">
    <p class="mv-choice__title">@Title</p>
    <p class="mv-choice__desc">@Description</p>
    @ChildContent
</div>

<style>
    .mv-choice { border: 1.5px solid var(--mv-border); border-radius: var(--mv-radius-md); padding: 12px; cursor: pointer; }
    .mv-choice--selected { border-color: var(--mv-info); background: var(--mv-info-bg); }
    .mv-choice--selected .mv-choice__title { color: var(--mv-info-text); }
    .mv-choice--selected .mv-choice__desc { color: var(--mv-info-text); }
    .mv-choice__title { font-size: var(--mv-fs-md); font-weight: 600; margin: 0; }
    .mv-choice__desc { font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: 2px 0 0; }
</style>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Description { get; set; } = "";
    [Parameter] public bool Selected { get; set; }
    [Parameter] public EventCallback OnSelect { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private Task HandleClick() => OnSelect.HasDelegate ? OnSelect.InvokeAsync() : Task.CompletedTask;
    private Task HandleKeyDown(KeyboardEventArgs e) =>
        e.Key is "Enter" or " " ? HandleClick() : Task.CompletedTask;
}
```

### `Components/Molecules/HintBox.razor`

```razor
<div class="mv-hint">
    <Icon Name="alert" Size="16" />
    <p>@ChildContent</p>
</div>

<style>
    .mv-hint { display: flex; gap: 8px; background: var(--mv-warning-bg); color: var(--mv-warning-text); border-radius: var(--mv-radius-md); padding: 10px 12px; font-size: var(--mv-fs-xs); }
    .mv-hint p { margin: 0; }
    .mv-hint svg { flex-shrink: 0; margin-top: 1px; }
</style>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

### `Components/Molecules/ChipList.razor`

Referenced by the onboarding flow for "added so far" items — not in the original component list explicitly broken out, but needed since chips appear in two places (onboarding expenses and income sources), so it's factored out rather than duplicated.

```razor
<div class="mv-chip-list">
    @foreach (var chip in Items)
    {
        <button type="button" class="mv-chip" @onclick="() => OnChipClick.InvokeAsync(chip)">
            @chip.Label
        </button>
    }
</div>

<style>
    .mv-chip-list { display: flex; flex-wrap: wrap; gap: 6px; }
    .mv-chip { background: var(--mv-surface-sunken); border: none; border-radius: var(--mv-radius-pill); padding: 6px 12px; font-size: var(--mv-fs-xs); cursor: pointer; }
</style>

@code {
    public record Chip(string Id, string Label);
    [Parameter] public List<Chip> Items { get; set; } = new();
    [Parameter] public EventCallback<Chip> OnChipClick { get; set; }
}
```

---

## 6. Organisms

### `Components/Organisms/ToastContainer.razor`

Reads from `AppState.Messages` (a queue, not a single string — see State section — so multiple toasts can stack without clobbering each other).

```razor
@inherits FluxorComponent
@inject IState<AppState> AppState
@inject IDispatcher Dispatcher

<div class="mv-toast-container">
    @foreach (var msg in AppState.Value.Messages)
    {
        <Toast Message="@msg.Text" Variant="@msg.Variant" OnDismiss="() => Dismiss(msg.Id)" />
    }
</div>

<style>
    .mv-toast-container {
        position: absolute; top: calc(var(--mv-safe-top) + 8px); left: 12px; right: 12px;
        display: flex; flex-direction: column; gap: 8px; z-index: 50; pointer-events: none;
    }
    .mv-toast-container > * { pointer-events: auto; }
</style>

@code {
    private void Dismiss(Guid id) => Dispatcher.Dispatch(new DismissMessageAction(id));

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AppState.StateChanged += async (_, _) =>
        {
            foreach (var msg in AppState.Value.Messages.ToList())
            {
                if (msg.Variant != "error")
                {
                    _ = AutoDismiss(msg.Id);
                }
            }
        };
    }

    private async Task AutoDismiss(Guid id)
    {
        await Task.Delay(4000);
        Dispatcher.Dispatch(new DismissMessageAction(id));
    }
}
```

### `Components/Organisms/AppHeader.razor`

Folded into `AppShell.razor` above for most pages (header is part of the shell), but exposed standalone for pages that need a custom header (e.g. Simulator's contextual bucket header):

```razor
<div class="mv-header">
    <p class="mv-header__brand">@Brand</p>
    <p class="mv-header__title">@Title</p>
    @if (!string.IsNullOrWhiteSpace(Subtitle))
    {
        <p class="mv-header__subtitle">@Subtitle</p>
    }
    @ChildContent
</div>

@code {
    [Parameter] public string Brand { get; set; } = "Monivise";
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`BottomNav` is implemented inline inside `AppShell.razor` above via `NavItem` — no separate file needed since it has no independent state.

---

## 7. Error Handling — API Layer

This is the piece that didn't exist before. Every API client routes through one exception type and one "ensure success" method, so no call site can silently swallow a failure.

### `API/ApiException.cs`

```csharp
namespace Monivise.App.API;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? FieldErrors { get; }

    public ApiException(int statusCode, string message, string? errorCode = null,
        Dictionary<string, string[]>? fieldErrors = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        FieldErrors = fieldErrors;
    }
}
```

### `API/ApiClient.cs`

```csharp
using System.Net.Http.Json;
using System.Text.Json;

namespace Monivise.App.API;

public class ApiClient
{
    protected readonly HttpClient Http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http) => Http = http;

    protected async Task<T> GetAsync<T>(string url)
    {
        var response = await SendWithNetworkGuard(() => Http.GetAsync(url));
        await EnsureSuccessOrThrowAsync(response);
        return await DeserializeAsync<T>(response);
    }

    protected async Task<T> PostAsync<T>(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PostAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
        return await DeserializeAsync<T>(response);
    }

    protected async Task PostAsync(string url, object? body = null)
    {
        var response = await SendWithNetworkGuard(() => Http.PostAsJsonAsync(url, body, JsonOptions));
        await EnsureSuccessOrThrowAsync(response);
    }

    private static async Task<HttpResponseMessage> SendWithNetworkGuard(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(0, "Network error. Please check your connection.", "network_error");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(0, "The request timed out. Please try again.", "timeout");
        }
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            if (result is null)
                throw new ApiException((int)response.StatusCode, "The server returned an empty response.", "empty_response");
            return result;
        }
        catch (JsonException)
        {
            throw new ApiException((int)response.StatusCode, "The server returned an unexpected response.", "parse_error");
        }
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var status = (int)response.StatusCode;
        string message;
        string? errorCode = null;
        Dictionary<string, string[]>? fieldErrors = null;

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var problem = JsonSerializer.Deserialize<ApiErrorBody>(body, JsonOptions);
            message = problem?.Message ?? DefaultMessageFor(status);
            errorCode = problem?.Code;
            fieldErrors = problem?.Errors;
        }
        catch
        {
            message = DefaultMessageFor(status);
        }

        throw new ApiException(status, message, errorCode, fieldErrors);
    }

    private static string DefaultMessageFor(int status) => status switch
    {
        400 => "That request wasn't valid. Check your input and try again.",
        401 => "Your session has expired. Please sign in again.",
        403 => "You don't have permission to do that.",
        404 => "We couldn't find what you were looking for.",
        409 => "That already exists.",
        422 => "Some of the information provided isn't valid.",
        >= 500 => "Something went wrong on our end. Please try again.",
        _ => "Something went wrong. Please try again."
    };

    private record ApiErrorBody(string? Message, string? Code, Dictionary<string, string[]>? Errors);
}
```

Every downstream API client extends `ApiClient` and calls only `GetAsync<T>` / `PostAsync<T>` / `PostAsync` — there is nowhere left in the codebase for a bare `catch { return false; }` to hide.

### `API/AuthApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class AuthApiClient : ApiClient
{
    public AuthApiClient(HttpClient http) : base(http) { }

    public Task<AuthResponse> LoginAsync(LoginRequest req) => PostAsync<AuthResponse>("api/auth/login", req);
    public Task<AuthResponse> RegisterAsync(RegisterRequest req) => PostAsync<AuthResponse>("api/auth/register", req);
    public Task<AuthResponse> RefreshAsync() => PostAsync<AuthResponse>("api/auth/refresh");
    public Task LogoutAsync() => PostAsync("api/auth/logout");
}
```

### `API/DashboardApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class DashboardApiClient : ApiClient
{
    public DashboardApiClient(HttpClient http) : base(http) { }
    public Task<DashboardSummaryDto> GetSummaryAsync() => GetAsync<DashboardSummaryDto>("api/dashboard/summary");
}
```

### `API/BucketApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class BucketApiClient : ApiClient
{
    public BucketApiClient(HttpClient http) : base(http) { }
    public Task<List<BucketDto>> GetBucketsAsync() => GetAsync<List<BucketDto>>("api/buckets");
}
```

### `API/SimulatorApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class SimulatorApiClient : ApiClient
{
    public SimulatorApiClient(HttpClient http) : base(http) { }
    public Task<SimulateResponse> PreviewAsync(SimulateRequest req) => PostAsync<SimulateResponse>("api/simulator/preview", req);
    public Task<TransactionDto> CommitAsync(SimulateRequest req) => PostAsync<TransactionDto>("api/simulator/commit", req);
}
```

### `API/IncomeApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class IncomeApiClient : ApiClient
{
    public IncomeApiClient(HttpClient http) : base(http) { }
    public Task<List<AllocationResult>> AllocateAsync(AllocateIncomeRequest req) =>
        PostAsync<List<AllocationResult>>("api/income/allocate", req);
}
```

### `API/TransactionApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class TransactionApiClient : ApiClient
{
    public TransactionApiClient(HttpClient http) : base(http) { }
    public Task<List<TransactionDto>> RecordIncomeAsync(RecordIncomeRequest req) =>
        PostAsync<List<TransactionDto>>("api/transactions/income", req);
    public Task<TransactionDto> RecordExpenseAsync(RecordExpenseRequest req) =>
        PostAsync<TransactionDto>("api/transactions/expense", req);
    public Task<List<TransactionDto>> GetCurrentCycleAsync() =>
        GetAsync<List<TransactionDto>>("api/transactions/current-cycle");
}
```

### `API/OnboardingApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class OnboardingApiClient : ApiClient
{
    public OnboardingApiClient(HttpClient http) : base(http) { }
    public Task<List<PathwayPreview>> IntakeAsync(OnboardingIntakeRequest req) =>
        PostAsync<List<PathwayPreview>>("api/onboarding/intake", req);
    public Task CommitAsync(OnboardingCommitRequest req) => PostAsync("api/onboarding/commit", req);
}
```

### `API/GoalApiClient.cs` and `API/ReviewApiClient.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.API;

public class GoalApiClient : ApiClient
{
    public GoalApiClient(HttpClient http) : base(http) { }
    public Task<List<GoalDto>> GetGoalsAsync() => GetAsync<List<GoalDto>>("api/goals");
    public Task<GoalDto> CreateAsync(CreateGoalRequest req) => PostAsync<GoalDto>("api/goals", req);
    public Task<GoalDto> ActivateAsync(Guid id) => PostAsync<GoalDto>($"api/goals/{id}/activate");
}

public class ReviewApiClient : ApiClient
{
    public ReviewApiClient(HttpClient http) : base(http) { }
    public Task<WeeklyReviewDto> GetWeeklyAsync() => GetAsync<WeeklyReviewDto>("api/review/weekly");
    public Task SweepAsync(SweepRequest req) => PostAsync("api/review/sweep", req);
}
```

---

## 8. Authentication

### `Authentication/TokenStore.cs`

In-memory only — never `localStorage`, so a page refresh always re-authenticates via the refresh cookie rather than trusting a token sitting in browser storage.

```csharp
namespace Monivise.App.Authentication;

public class TokenStore
{
    public string? AccessToken { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public event Action? Changed;

    public void Set(string token, DateTimeOffset expiresAt)
    {
        AccessToken = token;
        ExpiresAt = expiresAt;
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        ExpiresAt = null;
        Changed?.Invoke();
    }

    public bool IsExpired => ExpiresAt is null || ExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(10);
}
```

### `Authentication/JwtAuthStateProvider.cs`

```csharp
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Monivise.App.Authentication;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore tokenStore;

    public JwtAuthStateProvider(TokenStore tokenStore)
    {
        this.tokenStore = tokenStore;
        tokenStore.Changed += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrWhiteSpace(tokenStore.AccessToken) || tokenStore.IsExpired)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = ParseClaims(tokenStore.AccessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = jwt.Split('.')[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();
        return dict.Select(kv => new Claim(kv.Key, kv.Value.ToString()));
    }
}
```

### `Authentication/RefreshTokenHandler.cs`

A single refresh attempt on 401, then a hard logout if it fails — no silent retry loops.

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Monivise.App.Authentication;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly TokenStore tokenStore;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly Action onRefreshFailed;
    private bool isRefreshing;

    public RefreshTokenHandler(TokenStore tokenStore, IHttpClientFactory httpClientFactory, Action onRefreshFailed)
    {
        this.tokenStore = tokenStore;
        this.httpClientFactory = httpClientFactory;
        this.onRefreshFailed = onRefreshFailed;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(tokenStore.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
        }

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized || isRefreshing)
        {
            return response;
        }

        isRefreshing = true;
        try
        {
            var refreshClient = httpClientFactory.CreateClient("MoniviseRawApi");
            var refreshResponse = await refreshClient.PostAsync("api/auth/refresh", null, ct);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                tokenStore.Clear();
                onRefreshFailed();
                return response;
            }

            var auth = await refreshResponse.Content.ReadFromJsonAsync<Monivise.App.DTOs.AuthResponse>(cancellationToken: ct);
            if (auth is null)
            {
                tokenStore.Clear();
                onRefreshFailed();
                return response;
            }

            tokenStore.Set(auth.AccessToken, auth.ExpiresAt);

            var retryRequest = await CloneRequestAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            return await base.SendAsync(retryRequest, ct);
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return clone;
    }
}
```

`MoniviseRawApi` (registered in `Program.cs` below) is a plain named client with no auth handler attached, used only to call `/api/auth/refresh` without recursing into the handler that triggered the refresh.

---

## 9. State (Fluxor)

### `State/AppState.cs`

```csharp
using Fluxor;
using Monivise.App.DTOs;

namespace Monivise.App.State;

public record AppMessage(Guid Id, string Text, string Variant); // Variant: success | error | info

[FeatureState]
public record AppState
{
    public bool IsLoading { get; init; }
    public DashboardSummaryDto? Dashboard { get; init; }
    public List<BucketDto> Buckets { get; init; } = new();
    public List<TransactionDto> Transactions { get; init; } = new();
    public List<AppMessage> Messages { get; init; } = new();

    // Simulator sub-state
    public Guid? SimulatorBucketId { get; init; }
    public decimal SimulatorAmount { get; init; }
    public SimulateResponse? SimulatorPreview { get; init; }
    public bool SimulatorIsLoading { get; init; }
    public string? SimulatorError { get; init; }

    private AppState() { }
    public static AppState CreateInitial() => new();
}
```

### `State/AuthState.cs`

```csharp
using Fluxor;

namespace Monivise.App.State;

[FeatureState]
public record AuthState
{
    public bool IsAuthenticated { get; init; }
    public bool IsLoading { get; init; }
    public string? DisplayName { get; init; }
    public string? Currency { get; init; }
    public string? ErrorMessage { get; init; }

    private AuthState() { }
    public static AuthState CreateInitial() => new();
}
```

### `State/Actions/AuthActions.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record LoginAction(string Email, string Password);
public record LoginSuccessAction(AuthResponse Response);
public record LoginFailureAction(string Message);

public record RegisterAction(string DisplayName, string Email, string Password, string Currency);
public record RegisterSuccessAction(AuthResponse Response);
public record RegisterFailureAction(string Message);

public record LogoutAction;
public record LogoutSuccessAction;

public record ClearAuthErrorAction;
```

### `State/Actions/AppActions.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record LoadDashboardAction;
public record LoadDashboardSuccessAction(DashboardSummaryDto Summary);
public record LoadDashboardFailureAction(string Message);

public record ShowSuccessAction(string Message);
public record ShowErrorAction(string Message);
public record ShowInfoAction(string Message);
public record DismissMessageAction(Guid Id);
```

### `State/Actions/SimulatorActions.cs`

```csharp
using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record SetSimulatorBucketAction(Guid BucketId);
public record SetSimulatorAmountAction(decimal Amount);

public record PreviewSimulationAction;
public record PreviewSimulationSuccessAction(SimulateResponse Preview);
public record PreviewSimulationFailureAction(string Message);

public record CommitSimulationAction;
public record CommitSimulationSuccessAction(TransactionDto Transaction);
public record CommitSimulationFailureAction(string Message);

public record ClearSimulatorAction;
```

### `State/Reducers/AuthReducer.cs`

```csharp
using Fluxor;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Reducers;

public static class AuthReducer
{
    [ReducerMethod]
    public static AuthState ReduceLogin(AuthState state, LoginAction action) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState ReduceLoginSuccess(AuthState state, LoginSuccessAction action) =>
        state with { IsLoading = false, IsAuthenticated = true, DisplayName = action.Response.DisplayName, Currency = action.Response.Currency, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState ReduceLoginFailure(AuthState state, LoginFailureAction action) =>
        state with { IsLoading = false, IsAuthenticated = false, ErrorMessage = action.Message };

    [ReducerMethod]
    public static AuthState ReduceRegister(AuthState state, RegisterAction action) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState ReduceRegisterSuccess(AuthState state, RegisterSuccessAction action) =>
        state with { IsLoading = false, IsAuthenticated = true, DisplayName = action.Response.DisplayName, Currency = action.Response.Currency, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState ReduceRegisterFailure(AuthState state, RegisterFailureAction action) =>
        state with { IsLoading = false, ErrorMessage = action.Message };

    [ReducerMethod]
    public static AuthState ReduceLogoutSuccess(AuthState state, LogoutSuccessAction action) =>
        AuthState.CreateInitial();

    [ReducerMethod]
    public static AuthState ReduceClearError(AuthState state, ClearAuthErrorAction action) =>
        state with { ErrorMessage = null };
}
```

### `State/Reducers/AppReducer.cs`

```csharp
using Fluxor;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Reducers;

public static class AppReducer
{
    [ReducerMethod]
    public static AppState ReduceLoadDashboard(AppState state, LoadDashboardAction action) =>
        state with { IsLoading = true };

    [ReducerMethod]
    public static AppState ReduceLoadDashboardSuccess(AppState state, LoadDashboardSuccessAction action) =>
        state with { IsLoading = false, Dashboard = action.Summary, Buckets = action.Summary.Buckets, Transactions = action.Summary.RecentTransactions };

    [ReducerMethod]
    public static AppState ReduceLoadDashboardFailure(AppState state, LoadDashboardFailureAction action) =>
        state with { IsLoading = false };

    [ReducerMethod]
    public static AppState ReduceShowSuccess(AppState state, ShowSuccessAction action) =>
        state with { Messages = Append(state.Messages, action.Message, "success") };

    [ReducerMethod]
    public static AppState ReduceShowError(AppState state, ShowErrorAction action) =>
        state with { Messages = Append(state.Messages, action.Message, "error") };

    [ReducerMethod]
    public static AppState ReduceShowInfo(AppState state, ShowInfoAction action) =>
        state with { Messages = Append(state.Messages, action.Message, "info") };

    [ReducerMethod]
    public static AppState ReduceDismissMessage(AppState state, DismissMessageAction action) =>
        state with { Messages = state.Messages.Where(m => m.Id != action.Id).ToList() };

    [ReducerMethod]
    public static AppState ReduceSetSimulatorBucket(AppState state, SetSimulatorBucketAction action) =>
        state with { SimulatorBucketId = action.BucketId, SimulatorPreview = null, SimulatorError = null };

    [ReducerMethod]
    public static AppState ReduceSetSimulatorAmount(AppState state, SetSimulatorAmountAction action) =>
        state with { SimulatorAmount = action.Amount };

    [ReducerMethod]
    public static AppState ReducePreviewSimulation(AppState state, PreviewSimulationAction action) =>
        state with { SimulatorIsLoading = true, SimulatorError = null };

    [ReducerMethod]
    public static AppState ReducePreviewSimulationSuccess(AppState state, PreviewSimulationSuccessAction action) =>
        state with { SimulatorIsLoading = false, SimulatorPreview = action.Preview };

    [ReducerMethod]
    public static AppState ReducePreviewSimulationFailure(AppState state, PreviewSimulationFailureAction action) =>
        state with { SimulatorIsLoading = false, SimulatorError = action.Message };

    [ReducerMethod]
    public static AppState ReduceClearSimulator(AppState state, ClearSimulatorAction action) =>
        state with { SimulatorBucketId = null, SimulatorAmount = 0, SimulatorPreview = null, SimulatorError = null };

    private static List<AppMessage> Append(List<AppMessage> messages, string text, string variant) =>
        messages.Append(new AppMessage(Guid.NewGuid(), text, variant)).ToList();
}
```

### `State/Effects/AuthEffects.cs`

Every effect follows the same shape: try the API call, dispatch success; on `ApiException`, map the status code to a message and dispatch failure; on anything else, dispatch a generic network failure. This is where the "no silent catch" requirement actually lives.

```csharp
using Fluxor;
using Microsoft.AspNetCore.Components;
using Monivise.App.API;
using Monivise.App.Authentication;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class AuthEffects
{
    private readonly AuthApiClient api;
    private readonly TokenStore tokenStore;
    private readonly NavigationManager nav;

    public AuthEffects(AuthApiClient api, TokenStore tokenStore, NavigationManager nav)
    {
        this.api = api;
        this.tokenStore = tokenStore;
        this.nav = nav;
    }

    [EffectMethod]
    public async Task HandleLogin(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await api.LoginAsync(new(action.Email, action.Password));
            tokenStore.Set(response.AccessToken, response.ExpiresAt);
            dispatcher.Dispatch(new LoginSuccessAction(response));
            nav.NavigateTo("/dashboard");
        }
        catch (ApiException ex)
        {
            var message = ex.StatusCode switch
            {
                401 => "Invalid email or password.",
                >= 500 => "Something went wrong. Please try again.",
                0 => "Network error. Please check your connection.",
                _ => ex.Message
            };
            dispatcher.Dispatch(new LoginFailureAction(message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new LoginFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleRegister(RegisterAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await api.RegisterAsync(new(action.DisplayName, action.Email, action.Password, action.Currency));
            tokenStore.Set(response.AccessToken, response.ExpiresAt);
            dispatcher.Dispatch(new RegisterSuccessAction(response));
            nav.NavigateTo("/onboarding");
        }
        catch (ApiException ex)
        {
            var message = ex.StatusCode switch
            {
                409 => "An account with this email already exists.",
                422 when ex.FieldErrors is not null => string.Join(" ", ex.FieldErrors.Values.SelectMany(v => v)),
                >= 500 => "Something went wrong. Please try again.",
                0 => "Network error. Please check your connection.",
                _ => ex.Message
            };
            dispatcher.Dispatch(new RegisterFailureAction(message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new RegisterFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleLogout(LogoutAction action, IDispatcher dispatcher)
    {
        try
        {
            await api.LogoutAsync();
        }
        catch (ApiException)
        {
            // Logging out locally still proceeds even if the server call fails —
            // the user's intent is to leave, and we must not trap them in the app.
        }
        finally
        {
            tokenStore.Clear();
            dispatcher.Dispatch(new LogoutSuccessAction());
            nav.NavigateTo("/login");
        }
    }
}
```

### `State/Effects/DashboardEffects.cs`

```csharp
using Fluxor;
using Monivise.App.API;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class DashboardEffects
{
    private readonly DashboardApiClient api;
    public DashboardEffects(DashboardApiClient api) => this.api = api;

    [EffectMethod]
    public async Task HandleLoadDashboard(LoadDashboardAction action, IDispatcher dispatcher)
    {
        try
        {
            var summary = await api.GetSummaryAsync();
            dispatcher.Dispatch(new LoadDashboardSuccessAction(summary));
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new LoadDashboardFailureAction(ex.Message));
            dispatcher.Dispatch(new ShowErrorAction(ex.Message));
        }
        catch (Exception)
        {
            const string message = "Network error. Please check your connection.";
            dispatcher.Dispatch(new LoadDashboardFailureAction(message));
            dispatcher.Dispatch(new ShowErrorAction(message));
        }
    }
}
```

### `State/Effects/SimulatorEffects.cs`

```csharp
using Fluxor;
using Monivise.App.API;
using Monivise.App.DTOs;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class SimulatorEffects
{
    private readonly SimulatorApiClient api;
    private readonly IState<AppState> state;

    public SimulatorEffects(SimulatorApiClient api, IState<AppState> state)
    {
        this.api = api;
        this.state = state;
    }

    [EffectMethod]
    public async Task HandlePreview(PreviewSimulationAction action, IDispatcher dispatcher)
    {
        var s = state.Value;
        if (s.SimulatorBucketId is null || s.SimulatorAmount <= 0) return;

        try
        {
            var preview = await api.PreviewAsync(new SimulateRequest(s.SimulatorBucketId.Value, s.SimulatorAmount));
            dispatcher.Dispatch(new PreviewSimulationSuccessAction(preview));
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new PreviewSimulationFailureAction(ex.Message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new PreviewSimulationFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleCommit(CommitSimulationAction action, IDispatcher dispatcher)
    {
        var s = state.Value;
        if (s.SimulatorBucketId is null || s.SimulatorAmount <= 0) return;

        try
        {
            var tx = await api.CommitAsync(new SimulateRequest(s.SimulatorBucketId.Value, s.SimulatorAmount));
            dispatcher.Dispatch(new CommitSimulationSuccessAction(tx));
            dispatcher.Dispatch(new ShowSuccessAction("Spend recorded."));
            dispatcher.Dispatch(new ClearSimulatorAction());
            dispatcher.Dispatch(new LoadDashboardAction());
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new CommitSimulationFailureAction(ex.Message));
            dispatcher.Dispatch(new ShowErrorAction(ex.Message));
        }
        catch (Exception)
        {
            const string message = "Network error. Please check your connection.";
            dispatcher.Dispatch(new CommitSimulationFailureAction(message));
            dispatcher.Dispatch(new ShowErrorAction(message));
        }
    }
}
```

---

## 10. DTOs

Flat records matching the backend contracts exactly. Grouped by folder as in the file structure, shown together here for brevity — split into the listed files (`DTOs/Auth/AuthResponse.cs`, `DTOs/Buckets/BucketDto.cs`, etc.) when copying into the project.

```csharp
namespace Monivise.App.DTOs;

// Auth
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string DisplayName, string Email, string Password, string Currency);
public record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, Guid UserId, string DisplayName, string Currency);

// Buckets
public record BucketDto(Guid Id, string Name, string Icon, string Color, string Type, decimal AllocationPercent,
    bool IsActive, int DisplayOrder, decimal Allocated, decimal Spent, decimal Balance);

// Transactions
public record TransactionDto(Guid Id, string Kind, Guid? BucketId, string? BucketName, string? BucketIcon,
    string? BucketColor, decimal Amount, DateTimeOffset Date, string? Note);
public record RecordIncomeRequest(decimal Amount, string Source, bool IsPrimary);
public record RecordExpenseRequest(Guid BucketId, decimal Amount, string? Note);

// Simulator
public record SimulateRequest(Guid BucketId, decimal Amount);
public record SimulateResponse(bool CanAfford, decimal CurrentBalance, decimal PostSpendBalance, decimal PaceScore,
    decimal AverageDailySpend, int DaysRemaining, List<string> RegretSignals);

// Income
public record AllocateIncomeRequest(decimal Amount, string Source, bool IsPrimary);
public record AllocationResult(Guid BucketId, string BucketName, decimal Amount);

// Dashboard
public record DashboardSummaryDto(decimal TotalIncome, decimal TotalSpent, decimal SafeToSpend, decimal DailyLimit,
    decimal PaceScore, string PaceLabel, int DaysRemaining, int DaysElapsed, int DaysInCycle, bool ActiveCycle,
    List<BucketDto> Buckets, List<TransactionDto> RecentTransactions);

// Onboarding
public record ExpenseIntake(string Name, string Nature, decimal? Amount); // Nature: HardFixed | Soft | Unpriced
public record OnboardingIntakeRequest(decimal MonthlyIncome, List<ExpenseIntake> Expenses);
public record PathwayPreview(string Pathway, decimal DailyLimit, decimal WeeklyLimit, decimal SavingsRate,
    string Example, bool IsAffordable, decimal? Gap, List<string>? SuggestedCuts);
public record OnboardingCommitRequest(decimal MonthlyIncome, List<ExpenseIntake> Expenses, string Pathway);

// Goals
public record GoalDto(Guid Id, string Name, decimal TargetAmount, decimal CurrentAmount, bool IsActive);
public record CreateGoalRequest(string Name, decimal TargetAmount);

// Review
public record WeeklyReviewDto(decimal ProjectedEndBalance, decimal DailyAverage, string PaceLabel,
    List<BucketDto> BucketBreakdown, decimal Underspend);
public record SweepRequest(decimal Amount, Guid TargetGoalId);
```

---

## 11. Wiring — `Program.cs`

```csharp
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Monivise.App;
using Monivise.App.API;
using Monivise.App.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.HostEnvironment.BaseAddress; // adjust if the API is on a different origin

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Raw client for refresh calls only — no auth handler attached, avoids recursive 401 handling.
builder.Services.AddHttpClient("MoniviseRawApi", client => client.BaseAddress = new Uri(apiBaseAddress));

builder.Services.AddScoped<RefreshTokenHandler>(sp =>
{
    var tokenStore = sp.GetRequiredService<TokenStore>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new RefreshTokenHandler(tokenStore, factory, () => nav.NavigateTo("/login"));
});

builder.Services.AddHttpClient("MoniviseApi", client => client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<RefreshTokenHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MoniviseApi"));

builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<BucketApiClient>();
builder.Services.AddScoped<SimulatorApiClient>();
builder.Services.AddScoped<IncomeApiClient>();
builder.Services.AddScoped<TransactionApiClient>();
builder.Services.AddScoped<OnboardingApiClient>();
builder.Services.AddScoped<GoalApiClient>();
builder.Services.AddScoped<ReviewApiClient>();

builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(Program).Assembly));

await builder.Build().RunAsync();
```

### `App.razor`

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(Layouts.MainLayout)">
                <NotAuthorized>
                    @{ Nav.NavigateTo("/login", replace: true); }
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(Layouts.MainLayout)">
                <p style="padding:24px;">Page not found.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    [Inject] private Microsoft.AspNetCore.Components.NavigationManager Nav { get; set; } = default!;
}
```

### `Utilities/CurrencyFormatter.cs`

```csharp
using System.Globalization;

namespace Monivise.App.Utilities;

public static class CurrencyFormatter
{
    public static string Format(decimal amount, string currencyCode)
    {
        var culture = currencyCode switch
        {
            "NGN" => new CultureInfo("en-NG"),
            "USD" => new CultureInfo("en-US"),
            "GBP" => new CultureInfo("en-GB"),
            _ => CultureInfo.InvariantCulture
        };
        return amount.ToString("C0", culture);
    }
}
```

---

## 12. Pages

### `Pages/Auth/Login.razor`

```razor
@page "/login"
@layout Layouts.AuthLayout
@inject IState<AuthState> AuthState
@inject IDispatcher Dispatcher

<h1 class="mv-header__title" style="margin-bottom: var(--mv-space-6);">Sign in</h1>

<Input Label="Email" Type="email" InputMode="email" Value="@email" ValueChanged="v => email = v"
       ErrorMessage="@(fieldErrors.GetValueOrDefault("email"))" />
<Input Label="Password" Type="password" Value="@password" ValueChanged="v => password = v"
       ErrorMessage="@(fieldErrors.GetValueOrDefault("password"))" />

@if (!string.IsNullOrWhiteSpace(AuthState.Value.ErrorMessage))
{
    <p class="mv-field__error mv-shake" role="alert" style="margin-bottom: var(--mv-space-4);">@AuthState.Value.ErrorMessage</p>
}

<Button Variant="primary" IsLoading="@AuthState.Value.IsLoading" OnClick="Submit">Sign in</Button>

<p style="text-align:center; font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin-top: var(--mv-space-4);">
    New here? <a href="/register">Create an account</a>
</p>

@code {
    private string email = "";
    private string password = "";
    private Dictionary<string, string> fieldErrors = new();

    private void Submit()
    {
        fieldErrors.Clear();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            fieldErrors["email"] = "Enter a valid email address.";
        if (string.IsNullOrWhiteSpace(password))
            fieldErrors["password"] = "Enter your password.";

        if (fieldErrors.Count > 0) return;

        Dispatcher.Dispatch(new LoginAction(email, password));
    }
}
```

### `Pages/Auth/Register.razor`

```razor
@page "/register"
@layout Layouts.AuthLayout
@inject IState<AuthState> AuthState
@inject IDispatcher Dispatcher

<h1 class="mv-header__title" style="margin-bottom: var(--mv-space-6);">Create your account</h1>

<Input Label="Display name" Value="@displayName" ValueChanged="v => displayName = v"
       ErrorMessage="@(fieldErrors.GetValueOrDefault("displayName"))" />
<Input Label="Email" Type="email" InputMode="email" Value="@email" ValueChanged="v => email = v"
       ErrorMessage="@(fieldErrors.GetValueOrDefault("email"))" />
<Input Label="Password" Type="password" Value="@password" ValueChanged="v => password = v"
       Help="At least 8 characters." ErrorMessage="@(fieldErrors.GetValueOrDefault("password"))" />

<div class="mv-field">
    <label class="mv-field__label">Currency</label>
    <select class="mv-field__input" @bind="currency">
        <option value="NGN">NGN — Nigerian Naira</option>
        <option value="USD">USD — US Dollar</option>
        <option value="GBP">GBP — British Pound</option>
    </select>
</div>

@if (!string.IsNullOrWhiteSpace(AuthState.Value.ErrorMessage))
{
    <p class="mv-field__error mv-shake" role="alert" style="margin-bottom: var(--mv-space-4);">@AuthState.Value.ErrorMessage</p>
}

<Button Variant="primary" IsLoading="@AuthState.Value.IsLoading" OnClick="Submit">Create account</Button>

<p style="text-align:center; font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin-top: var(--mv-space-4);">
    Already have an account? <a href="/login">Sign in</a>
</p>

@code {
    private string displayName = "";
    private string email = "";
    private string password = "";
    private string currency = "NGN";
    private Dictionary<string, string> fieldErrors = new();

    private void Submit()
    {
        fieldErrors.Clear();
        if (string.IsNullOrWhiteSpace(displayName))
            fieldErrors["displayName"] = "Enter your name.";
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            fieldErrors["email"] = "Enter a valid email address.";
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            fieldErrors["password"] = "Password must be at least 8 characters.";

        if (fieldErrors.Count > 0) return;

        Dispatcher.Dispatch(new RegisterAction(displayName, email, password, currency));
    }
}
```

### `Pages/Onboarding/Onboarding.razor`

The three-step conversational flow: income, then one expense at a time, then pathway choice. All state lives in the component itself (it's a self-contained wizard, not shared app state) and posts once at the end.

```razor
@page "/onboarding"
@layout Layouts.AuthLayout
@inject OnboardingApiClient Api
@inject IDispatcher Dispatcher
@inject NavigationManager Nav

<div style="display:flex; gap:4px; margin-bottom: var(--mv-space-5);">
    <div style="height:3px; flex:1; border-radius:2px; background:@(step >= 1 ? "var(--mv-positive)" : "var(--mv-border)");"></div>
    <div style="height:3px; flex:1; border-radius:2px; background:@(step >= 2 ? "var(--mv-positive)" : "var(--mv-border)");"></div>
    <div style="height:3px; flex:1; border-radius:2px; background:@(step >= 3 ? "var(--mv-positive)" : "var(--mv-border)");"></div>
</div>

@if (step == 1)
{
    <p style="font-size: var(--mv-fs-lg); font-weight:600; margin:0 0 var(--mv-space-4);">What's your monthly income?</p>
    <p style="text-align:center; font-size: var(--mv-fs-2xl); font-weight:600; margin: var(--mv-space-4) 0;">@FormatAmount(incomeInput)</p>
    <Numpad Value="@incomeInput" ValueChanged="v => incomeInput = v" />
    <HintBox>Use only money you can count on every month.</HintBox>
    <div style="height: var(--mv-space-5);"></div>
    <Button Variant="primary" Disabled="@(!decimal.TryParse(incomeInput, out var inc) || inc <= 0)" OnClick="GoToStep2">Next</Button>
}
else if (step == 2)
{
    <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin:0 0 6px;">Expense @(expenses.Count + 1)</p>
    <p style="font-size: var(--mv-fs-lg); font-weight:600; margin:0 0 var(--mv-space-4);">What do you spend on?</p>
    <Input Placeholder="e.g. Netflix subscription" Value="@currentName" ValueChanged="v => currentName = v" />

    <p style="font-size: var(--mv-fs-sm); font-weight:600; margin:0 0 8px;">Is the amount fixed?</p>
    <div style="display:flex; flex-direction:column; gap:8px; margin-bottom: var(--mv-space-4);">
        <ChoiceCard Title="Hard fixed" Description="Same amount, every month. Like rent." Selected="@(currentNature == "HardFixed")" OnSelect="() => currentNature = \"HardFixed\"" />
        <ChoiceCard Title="Soft" Description="Roughly the same, but it can shift." Selected="@(currentNature == "Soft")" OnSelect="() => currentNature = \"Soft\"" />
        <ChoiceCard Title="Unpriced" Description="You don't know the amount yet." Selected="@(currentNature == "Unpriced")" OnSelect="() => { currentNature = \"Unpriced\"; currentAmount = \"0\"; }" />
    </div>

    @if (currentNature != "Unpriced" && !string.IsNullOrEmpty(currentNature))
    {
        <p style="font-size: var(--mv-fs-sm); font-weight:600; margin:0 0 8px;">How much per month?</p>
        <p style="text-align:center; font-size: var(--mv-fs-xl); font-weight:600; margin: var(--mv-space-3) 0;">@FormatAmount(currentAmount)</p>
        <Numpad Value="@currentAmount" ValueChanged="v => currentAmount = v" />
    }

    <div style="height: var(--mv-space-4);"></div>
    <HintBox>Hard fixed expenses are protected first, before anything flexible.</HintBox>

    @if (expenses.Count > 0)
    {
        <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: var(--mv-space-4) 0 8px;">Added so far</p>
        <ChipList Items="@(expenses.Select(e => new ChipList.Chip(e.Name, $"{e.Name} · {(e.Amount is null ? "unpriced" : FormatAmount(e.Amount.Value.ToString()))}")).ToList())"
                  OnChipClick="RemoveExpense" />
    }

    <div style="height: var(--mv-space-5);"></div>
    <Button Variant="secondary" Disabled="@(!CanAddCurrentExpense())" OnClick="AddExpense">+ Add another</Button>
    <div style="height: var(--mv-space-3);"></div>
    <Button Variant="primary" Disabled="@(expenses.Count == 0)" OnClick="GoToStep3">Done, continue</Button>
}
else
{
    <p style="font-size: var(--mv-fs-lg); font-weight:600; margin:0 0 var(--mv-space-4);">Choose your pathway</p>

    @if (isLoadingPathways)
    {
        <p style="color: var(--mv-text-secondary); text-align:center;">Working out your options…</p>
    }
    else if (pathwayError is not null)
    {
        <p class="mv-field__error" role="alert">@pathwayError</p>
        <Button Variant="secondary" OnClick="LoadPathways">Try again</Button>
    }
    else
    {
        @foreach (var pathway in pathways)
        {
            <div class="mv-card" style="margin-bottom: var(--mv-space-3);">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">
                    <p style="font-size: var(--mv-fs-md); font-weight:600; margin:0;">@pathway.Pathway</p>
                    @if (!pathway.IsAffordable)
                    {
                        <Badge Variant="warning">Gap: @FormatAmount(pathway.Gap?.ToString() ?? "0")</Badge>
                    }
                </div>
                <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin:0 0 4px;">
                    @FormatAmount(pathway.DailyLimit.ToString())/day &middot; @FormatAmount(pathway.WeeklyLimit.ToString())/week
                </p>
                <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin:0 0 12px;">@pathway.Example</p>
                @if (pathway.SuggestedCuts is { Count: > 0 })
                {
                    <HintBox>Suggested cuts: @string.Join(", ", pathway.SuggestedCuts)</HintBox>
                }
                <div style="height: var(--mv-space-3);"></div>
                <Button Variant="@(pathway.Pathway == "Moderate" ? "primary" : "secondary")" OnClick="() => Commit(pathway.Pathway)">Choose @pathway.Pathway</Button>
            </div>
        }
    }
}

@code {
    private int step = 1;
    private string incomeInput = "0";
    private string currentName = "";
    private string currentNature = "";
    private string currentAmount = "0";
    private List<ExpenseIntake> expenses = new();
    private List<PathwayPreview> pathways = new();
    private bool isLoadingPathways;
    private string? pathwayError;

    private void GoToStep2() => step = 2;

    private bool CanAddCurrentExpense() =>
        !string.IsNullOrWhiteSpace(currentName) &&
        !string.IsNullOrEmpty(currentNature) &&
        (currentNature == "Unpriced" || (decimal.TryParse(currentAmount, out var a) && a > 0));

    private void AddExpense()
    {
        decimal? amount = currentNature == "Unpriced" ? null : decimal.Parse(currentAmount);
        expenses.Add(new ExpenseIntake(currentName, currentNature, amount));
        currentName = "";
        currentNature = "";
        currentAmount = "0";
    }

    private void RemoveExpense(ChipList.Chip chip) =>
        expenses = expenses.Where(e => e.Name != chip.Id).ToList();

    private async Task GoToStep3()
    {
        if (CanAddCurrentExpense()) AddExpense();
        step = 3;
        await LoadPathways();
    }

    private async Task LoadPathways()
    {
        isLoadingPathways = true;
        pathwayError = null;
        try
        {
            pathways = await Api.IntakeAsync(new OnboardingIntakeRequest(decimal.Parse(incomeInput), expenses));
        }
        catch (ApiException ex)
        {
            pathwayError = ex.Message;
        }
        catch (Exception)
        {
            pathwayError = "Network error. Please check your connection.";
        }
        finally
        {
            isLoadingPathways = false;
        }
    }

    private async Task Commit(string pathway)
    {
        try
        {
            await Api.CommitAsync(new OnboardingCommitRequest(decimal.Parse(incomeInput), expenses, pathway));
            Nav.NavigateTo("/dashboard");
        }
        catch (ApiException ex)
        {
            Dispatcher.Dispatch(new ShowErrorAction(ex.Message));
        }
        catch (Exception)
        {
            Dispatcher.Dispatch(new ShowErrorAction("Network error. Please check your connection."));
        }
    }

    private static string FormatAmount(string raw) =>
        decimal.TryParse(raw, out var val) ? val.ToString("C0") : raw;
}
```

### `Pages/Dashboard/Dashboard.razor`

```razor
@page "/dashboard"
@layout Layouts.AppShell
@attribute [Authorize]
@inject IState<AppState> AppState
@inject IDispatcher Dispatcher
@inject NavigationManager Nav

@if (AppState.Value.IsLoading && AppState.Value.Dashboard is null)
{
    <div class="mv-card" style="text-align:center; color: var(--mv-text-secondary);">Loading your dashboard…</div>
}
else if (AppState.Value.Dashboard is null)
{
    <div class="mv-card" style="text-align:center;">
        <p style="font-weight:600; margin:0 0 4px;">No active cycle</p>
        <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin:0 0 var(--mv-space-4);">Start a cycle to see your safe-to-spend amount.</p>
        <Button Variant="primary" OnClick='() => Nav.NavigateTo("/onboarding")'>Start cycle</Button>
    </div>
}
else
{
    var d = AppState.Value.Dashboard;
    <div class="mv-card mv-card--hero mv-scale-in">
        <p class="mv-card__label" style="font-size:12px; margin:0 0 4px;">Safe to spend</p>
        <p style="font-size: var(--mv-fs-2xl); font-weight:600; margin:0;">@d.SafeToSpend.ToString("C0")</p>
        <p class="mv-card__label" style="font-size:12px; margin:8px 0 0;">@d.DailyLimit.ToString("C0")/day &middot; @d.PaceLabel</p>
    </div>

    <div style="display:grid; grid-template-columns:1fr 1fr; gap:10px; margin-top: var(--mv-space-3);">
        <div style="background:var(--mv-surface); border:1px solid var(--mv-border); border-radius:10px; padding:12px;">
            <p style="font-size:12px; color:var(--mv-text-tertiary); margin:0;">Income</p>
            <p style="font-size: var(--mv-fs-md); font-weight:600; margin:2px 0 0;">@d.TotalIncome.ToString("C0")</p>
        </div>
        <div style="background:var(--mv-surface); border:1px solid var(--mv-border); border-radius:10px; padding:12px;">
            <p style="font-size:12px; color:var(--mv-text-tertiary); margin:0;">Spent</p>
            <p style="font-size: var(--mv-fs-md); font-weight:600; margin:2px 0 0;">@d.TotalSpent.ToString("C0")</p>
        </div>
    </div>

    <p style="font-size: var(--mv-fs-sm); font-weight:600; margin: var(--mv-space-5) 0 8px;">Buckets</p>
    @if (d.Buckets.Count == 0)
    {
        <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-tertiary);">No buckets yet.</p>
    }
    else
    {
        <div style="display:flex; flex-direction:column; gap:8px;">
            @foreach (var bucket in d.Buckets)
            {
                <BucketRow Bucket="bucket" OnSelect='b => Nav.NavigateTo($"/simulator?bucket={b.Id}")' />
            }
        </div>
    }

    <p style="font-size: var(--mv-fs-sm); font-weight:600; margin: var(--mv-space-5) 0 8px;">Recent activity</p>
    @if (d.RecentTransactions.Count == 0)
    {
        <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-tertiary);">No transactions yet.</p>
    }
    else
    {
        <div class="mv-card">
            @foreach (var tx in d.RecentTransactions.Take(5))
            {
                <TransactionRow Tx="tx" />
            }
        </div>
    }
}

@code {
    protected override void OnInitialized()
    {
        Dispatcher.Dispatch(new LoadDashboardAction());
    }
}
```

### `Pages/Simulator/Simulator.razor`

Single screen, state-driven — select bucket, enter amount, see the live consequence, approve.

```razor
@page "/simulator"
@layout Layouts.AppShell
@attribute [Authorize]
@inject IState<AppState> AppState
@inject IDispatcher Dispatcher
@inject NavigationManager Nav

@if (AppState.Value.SimulatorBucketId is null)
{
    <p style="font-size: var(--mv-fs-lg); font-weight:600; margin:0 0 var(--mv-space-4);">Which bucket?</p>
    <div style="display:flex; flex-direction:column; gap:8px;">
        @foreach (var bucket in AppState.Value.Buckets)
        {
            <BucketRow Bucket="bucket" OnSelect="SelectBucket" />
        }
    </div>
}
else
{
    var bucket = AppState.Value.Buckets.FirstOrDefault(b => b.Id == AppState.Value.SimulatorBucketId);
    <div style="display:flex; align-items:center; gap:10px; margin-bottom: var(--mv-space-4);">
        <Button Variant="ghost" OnClick="() => Dispatcher.Dispatch(new ClearSimulatorAction())">&larr; Back</Button>
    </div>
    <p style="font-size: var(--mv-fs-md); font-weight:600; margin:0;">@bucket?.Name</p>
    <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin:2px 0 var(--mv-space-4);">Balance: @(bucket?.Balance.ToString("C0")) left</p>

    <p style="text-align:center; font-size: var(--mv-fs-2xl); font-weight:600; margin: var(--mv-space-3) 0;">@amountInput.ToString("C0")</p>
    <Numpad Value="@amountText" ValueChanged="OnAmountChanged" />

    <div style="height: var(--mv-space-4);"></div>
    <Button Variant="secondary" Disabled="@(amountInput <= 0)" IsLoading="@AppState.Value.SimulatorIsLoading" OnClick="Preview">See consequence</Button>

    @if (AppState.Value.SimulatorError is not null)
    {
        <p class="mv-field__error mv-shake" role="alert" style="margin-top: var(--mv-space-3);">@AppState.Value.SimulatorError</p>
    }

    @if (AppState.Value.SimulatorPreview is { } preview)
    {
        <div style="height: var(--mv-space-4);"></div>
        <Badge Variant="@(preview.CanAfford ? (preview.PaceScore >= 0.7m ? "warning" : "positive") : "danger")">
            @(preview.CanAfford ? (preview.PaceScore >= 0.7m ? "Tight pace" : "On pace") : "Not affordable")
        </Badge>
        <div style="height: var(--mv-space-3);"></div>
        <ConsequenceCard Preview="preview" />
        <div style="height: var(--mv-space-4);"></div>
        <div style="display:flex; gap:10px;">
            <Button Variant="secondary" OnClick="() => Dispatcher.Dispatch(new ClearSimulatorAction())">Cancel</Button>
            <Button Variant="@(preview.CanAfford ? "primary" : "danger")" OnClick="Commit">Approve</Button>
        </div>
    }
}

@code {
    private string amountText = "0";
    private decimal amountInput => decimal.TryParse(amountText, out var v) ? v : 0;

    protected override void OnInitialized()
    {
        var uri = new Uri(Nav.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("bucket", out var bucketId) && Guid.TryParse(bucketId, out var id))
        {
            Dispatcher.Dispatch(new SetSimulatorBucketAction(id));
        }
    }

    private void SelectBucket(BucketDto bucket) => Dispatcher.Dispatch(new SetSimulatorBucketAction(bucket.Id));

    private void OnAmountChanged(string value)
    {
        amountText = value;
        Dispatcher.Dispatch(new SetSimulatorAmountAction(amountInput));
    }

    private void Preview() => Dispatcher.Dispatch(new PreviewSimulationAction());
    private void Commit() => Dispatcher.Dispatch(new CommitSimulationAction());
}
```

### `Pages/Income/Income.razor`

```razor
@page "/income"
@layout Layouts.AppShell
@attribute [Authorize]
@inject IncomeApiClient Api
@inject TransactionApiClient TxApi
@inject IDispatcher Dispatcher

<div style="display:flex; gap:8px; margin-bottom: var(--mv-space-4);">
    <button type="button" class="mv-btn mv-btn--@(isPrimary ? "primary" : "secondary")" style="width:auto; padding:0 16px; min-height:38px;" @onclick="() => isPrimary = true">Primary</button>
    <button type="button" class="mv-btn mv-btn--@(!isPrimary ? "primary" : "secondary")" style="width:auto; padding:0 16px; min-height:38px;" @onclick="() => isPrimary = false">Extra</button>
</div>

<p style="text-align:center; font-size: var(--mv-fs-2xl); font-weight:600; margin: var(--mv-space-3) 0;">@amountInput.ToString("C0")</p>
<Numpad Value="@amountText" ValueChanged="v => amountText = v" />

<div class="mv-field" style="margin-top: var(--mv-space-4);">
    <label class="mv-field__label">Source</label>
    <select class="mv-field__input" @bind="source">
        <option value="Salary">Salary</option>
        <option value="Freelance">Freelance</option>
        <option value="Business">Business</option>
        <option value="Gift">Gift</option>
        <option value="Other">Other</option>
    </select>
</div>

@if (errorMessage is not null)
{
    <p class="mv-field__error mv-shake" role="alert">@errorMessage</p>
}

<Button Variant="primary" Disabled="@(amountInput <= 0)" IsLoading="isSubmitting" OnClick="Allocate">Allocate income</Button>

@if (results.Count > 0)
{
    <p style="font-size: var(--mv-fs-sm); font-weight:600; margin: var(--mv-space-5) 0 8px;">Allocated to</p>
    <div class="mv-card">
        @foreach (var r in results)
        {
            <div style="display:flex; justify-content:space-between; padding:8px 0; border-bottom:1px solid var(--mv-border);">
                <span style="font-size: var(--mv-fs-sm);">@r.BucketName</span>
                <span style="font-size: var(--mv-fs-sm); font-weight:600;">@r.Amount.ToString("C0")</span>
            </div>
        }
    </div>
}

@code {
    private bool isPrimary = true;
    private string amountText = "0";
    private decimal amountInput => decimal.TryParse(amountText, out var v) ? v : 0;
    private string source = "Salary";
    private bool isSubmitting;
    private string? errorMessage;
    private List<AllocationResult> results = new();

    private async Task Allocate()
    {
        isSubmitting = true;
        errorMessage = null;
        try
        {
            results = await Api.AllocateAsync(new AllocateIncomeRequest(amountInput, source, isPrimary));
            await TxApi.RecordIncomeAsync(new RecordIncomeRequest(amountInput, source, isPrimary));
            Dispatcher.Dispatch(new ShowSuccessAction("Income allocated."));
            Dispatcher.Dispatch(new LoadDashboardAction());
            amountText = "0";
        }
        catch (ApiException ex)
        {
            errorMessage = ex.Message;
        }
        catch (Exception)
        {
            errorMessage = "Network error. Please check your connection.";
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### `Pages/Buckets/Buckets.razor`

```razor
@page "/buckets"
@layout Layouts.AppShell
@attribute [Authorize]
@inject IState<AppState> AppState
@inject NavigationManager Nav

<HintBox>Allocations are set by your chosen pathway. Re-run onboarding to change them.</HintBox>
<div style="height: var(--mv-space-4);"></div>

@if (AppState.Value.Buckets.Count == 0)
{
    <div class="mv-card" style="text-align:center; color: var(--mv-text-secondary);">No buckets yet.</div>
}
else
{
    <div style="display:flex; flex-direction:column; gap:8px;">
        @foreach (var bucket in AppState.Value.Buckets)
        {
            <BucketRow Bucket="bucket" OnSelect='b => Nav.NavigateTo($"/simulator?bucket={b.Id}")' />
            <ProgressBar Percent="@(bucket.Allocated > 0 ? (double)(bucket.Spent / bucket.Allocated * 100) : 0)"
                         Color="@(bucket.Spent > bucket.Allocated ? "var(--mv-danger)" : "var(--mv-positive)")" />
        }
    </div>
}
```

### `Pages/History/History.razor`

```razor
@page "/history"
@layout Layouts.AppShell
@attribute [Authorize]
@inject TransactionApiClient Api
@inject IDispatcher Dispatcher

@if (isLoading)
{
    <p style="text-align:center; color: var(--mv-text-secondary);">Loading transactions…</p>
}
else if (errorMessage is not null)
{
    <div class="mv-card" style="text-align:center;">
        <p class="mv-field__error">@errorMessage</p>
        <Button Variant="secondary" OnClick="Load">Retry</Button>
    </div>
}
else if (grouped.Count == 0)
{
    <div class="mv-card" style="text-align:center; color: var(--mv-text-secondary);">No transactions yet.</div>
}
else
{
    @foreach (var group in grouped)
    {
        <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin: var(--mv-space-4) 0 8px;">@group.Key.ToString("MMMM d")</p>
        <div class="mv-card">
            @foreach (var tx in group)
            {
                <TransactionRow Tx="tx" />
            }
        </div>
    }
}

@code {
    private bool isLoading = true;
    private string? errorMessage;
    private List<IGrouping<DateTime, TransactionDto>> grouped = new();

    protected override async Task OnInitializedAsync() => await Load();

    private async Task Load()
    {
        isLoading = true;
        errorMessage = null;
        try
        {
            var txs = await Api.GetCurrentCycleAsync();
            grouped = txs.GroupBy(t => t.Date.Date).OrderByDescending(g => g.Key).ToList();
        }
        catch (ApiException ex)
        {
            errorMessage = ex.Message;
        }
        catch (Exception)
        {
            errorMessage = "Network error. Please check your connection.";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### `Pages/Weekly/Weekly.razor`

```razor
@page "/weekly"
@layout Layouts.AppShell
@attribute [Authorize]
@inject ReviewApiClient Api
@inject IDispatcher Dispatcher

@if (isLoading)
{
    <p style="text-align:center; color: var(--mv-text-secondary);">Loading your weekly review…</p>
}
else if (errorMessage is not null)
{
    <div class="mv-card" style="text-align:center;">
        <p class="mv-field__error">@errorMessage</p>
        <Button Variant="secondary" OnClick="Load">Retry</Button>
    </div>
}
else if (review is { } r)
{
    <div class="mv-card mv-card--hero">
        <p class="mv-card__label" style="font-size:12px; margin:0 0 4px;">Projected end-of-cycle balance</p>
        <p style="font-size: var(--mv-fs-2xl); font-weight:600; margin:0;">@r.ProjectedEndBalance.ToString("C0")</p>
    </div>

    <div style="display:grid; grid-template-columns:1fr 1fr; gap:10px; margin-top: var(--mv-space-3);">
        <div style="background:var(--mv-surface); border:1px solid var(--mv-border); border-radius:10px; padding:12px;">
            <p style="font-size:12px; color:var(--mv-text-tertiary); margin:0;">Daily average</p>
            <p style="font-size: var(--mv-fs-md); font-weight:600; margin:2px 0 0;">@r.DailyAverage.ToString("C0")</p>
        </div>
        <div style="background:var(--mv-surface); border:1px solid var(--mv-border); border-radius:10px; padding:12px;">
            <p style="font-size:12px; color:var(--mv-text-tertiary); margin:0;">Pace</p>
            <p style="font-size: var(--mv-fs-md); font-weight:600; margin:2px 0 0;">@r.PaceLabel</p>
        </div>
    </div>

    @if (r.Underspend > 0)
    {
        <div style="height: var(--mv-space-4);"></div>
        <div class="mv-card" style="text-align:center;">
            <p style="margin:0 0 4px;">You underspent @r.Underspend.ToString("C0") this week.</p>
            <p style="font-size: var(--mv-fs-xs); color: var(--mv-text-tertiary); margin:0 0 var(--mv-space-3);">Sweep it into a goal?</p>
            <Button Variant="primary" OnClick="OpenSweep">Sweep to goal</Button>
        </div>
    }

    <p style="font-size: var(--mv-fs-sm); font-weight:600; margin: var(--mv-space-5) 0 8px;">Bucket breakdown</p>
    <div style="display:flex; flex-direction:column; gap:8px;">
        @foreach (var bucket in r.BucketBreakdown)
        {
            <BucketRow Bucket="bucket" />
        }
    </div>
}

@code {
    private bool isLoading = true;
    private string? errorMessage;
    private WeeklyReviewDto? review;

    protected override async Task OnInitializedAsync() => await Load();

    private async Task Load()
    {
        isLoading = true;
        errorMessage = null;
        try
        {
            review = await Api.GetWeeklyAsync();
        }
        catch (ApiException ex)
        {
            errorMessage = ex.Message;
        }
        catch (Exception)
        {
            errorMessage = "Network error. Please check your connection.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenSweep() => Dispatcher.Dispatch(new ShowInfoAction("Pick a goal from Settings to sweep into."));
}
```

### `Pages/Settings/Settings.razor`

```razor
@page "/settings"
@layout Layouts.AppShell
@attribute [Authorize]
@inject IState<AuthState> AuthState
@inject IDispatcher Dispatcher
@inject IJSRuntime JS

<div class="mv-card" style="margin-bottom: var(--mv-space-4);">
    <p style="font-weight:600; margin:0 0 4px;">@AuthState.Value.DisplayName</p>
    <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin:0;">Currency: @AuthState.Value.Currency</p>
</div>

<div class="mv-card" style="display:flex; justify-content:space-between; align-items:center; margin-bottom: var(--mv-space-4);">
    <span>Dark mode</span>
    <label class="mv-switch">
        <input type="checkbox" checked="@isDark" @onchange="ToggleDarkMode" />
    </label>
</div>

<Button Variant="danger" OnClick="() => showLogoutConfirm = true">Log out</Button>

@if (showLogoutConfirm)
{
    <div style="min-height:400px; background:rgba(0,0,0,0.45); display:flex; align-items:center; justify-content:center; position:absolute; inset:0; z-index:40;">
        <div class="mv-card mv-scale-in" style="width:80%; background:var(--mv-surface);">
            <p style="font-weight:600; margin:0 0 4px;">Log out?</p>
            <p style="font-size: var(--mv-fs-sm); color: var(--mv-text-secondary); margin:0 0 var(--mv-space-4);">You'll need to sign in again to access your dashboard.</p>
            <div style="display:flex; gap:10px;">
                <Button Variant="secondary" OnClick="() => showLogoutConfirm = false">Cancel</Button>
                <Button Variant="danger" OnClick="ConfirmLogout">Log out</Button>
            </div>
        </div>
    </div>
}

<style>
    .mv-switch input { width: 44px; height: 26px; }
</style>

@code {
    private bool showLogoutConfirm;
    private bool isDark;

    protected override async Task OnInitializedAsync()
    {
        isDark = await JS.InvokeAsync<bool>("monivise.theme.isDark");
    }

    private async Task ToggleDarkMode(ChangeEventArgs e)
    {
        isDark = (bool)(e.Value ?? false);
        await JS.InvokeVoidAsync("monivise.theme.set", isDark);
    }

    private void ConfirmLogout()
    {
        showLogoutConfirm = false;
        Dispatcher.Dispatch(new LogoutAction());
    }
}
```

Add the matching theme helper to `wwwroot/index.html` alongside the connectivity script:

```html
<script>
  window.monivise.theme = {
    isDark: () => document.documentElement.classList.contains('dark'),
    set: (dark) => document.documentElement.classList.toggle('dark', dark)
  };
</script>
```

---

## 13. Migration Notes (Revamp, Not Rebuild)

What to delete from the current codebase:
- Every hardcoded hex value (`#07090F`, `#00CFA8`, `#101420`, etc.) across all `.razor` and `.css` files — replace with the `--mv-*` tokens above.
- The `height: 860px` phone-shell rule and any related fixed-pixel container styles.
- Any `catch { return false; }` or empty `catch { }` blocks in API-calling code — every one of them should now be a call into an `ApiClient` subclass that throws `ApiException`, caught explicitly in the effect that calls it.
- The inline per-item DOM-generation pattern in the old onboarding page — replaced entirely by `Pages/Onboarding/Onboarding.razor` above.
- The three separate Simulator pages/routes — replaced entirely by the single `Pages/Simulator/Simulator.razor` state machine.

What carries over unchanged:
- Backend routes, DTO field names and shapes (Section 10 matches the backend exactly — verify against the actual backend doc before wiring, since minor field naming drift is the most common source of runtime 422s).
- JWT auth model (access token + refresh cookie) — only the transport (`RefreshTokenHandler`) and storage (`TokenStore`, in-memory) are formalized here, not replaced.
- Fluxor as the state management library — this guide extends it with proper actions/reducers/effects rather than introducing something new.

## 14. Suggested Build Order

1. `tokens.css` + `app.css` — get these compiling and visually correct on one page before touching anything else.
2. `Layouts/MainLayout.razor`, `AppShell.razor`, `AuthLayout.razor` — confirm the viewport-safe shell works on an actual phone (or Chrome device toolbar), not just desktop.
3. Atoms, then Molecules, then Organisms — each layer depends on the one before it.
4. `API/` layer + `Authentication/` — get login/refresh working against the real backend before wiring pages.
5. `State/` — actions, reducers, effects, in that order.
6. Pages, in this order: Login/Register → Dashboard → Onboarding → Simulator → Income/Buckets/History/Weekly/Settings.

Nothing above is a stub — every file listed compiles conceptually against the DTOs and routes given. The one thing to double check against your actual backend before wiring is field-name drift in the DTOs (Section 10), since that's the most common source of silent 422s once the explicit error handling above surfaces them for the first time.
