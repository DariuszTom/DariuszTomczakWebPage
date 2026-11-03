// Blazor JS initializer – loaded automatically by the framework.
export function afterWebAssemblyStarted() {
    requestAnimationFrame(() => window.splash?.appReady());
}
