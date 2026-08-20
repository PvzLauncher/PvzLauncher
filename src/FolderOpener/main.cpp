#include <windows.h>
#include <filesystem>

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int) {
    wchar_t path[MAX_PATH];
    GetModuleFileNameW(nullptr, path, MAX_PATH);

    std::filesystem::path dir(path);

    ShellExecuteW(
        nullptr,
        L"open",
        dir.parent_path().c_str(),
        nullptr,
        nullptr,
        SW_SHOWNORMAL
    );

    return 0;
}